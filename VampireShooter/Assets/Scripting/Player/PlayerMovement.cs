using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public bool canMove = true;

    public CharacterController controller;
    public Transform HeadPosition;
    public Transform cameraHolder;

    [Header("Movement Speeds")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float crouchSpeed = 1.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    private float xRotation = 0f;

    [Header("Crouch")]
    public float standingScale = 1.0f;
    public float crouchingScale = 0.5f;
    public float crouchCheckDistance = 1.2f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundedForce = -2f;
    private float verticalVelocity;

    [Header("Super Speed Ability")]
    private bool superSpeedActive = false;
    public float superSpeedTimeScale = 0.35f;
    private float superSpeedMovementMultiplier = 1f;
    public float superSpeedLerpTime = 0.2f;

    [Header("Cross Effect")]
    public float crossSlowMultiplier = 1f;
    public AnimationCurve crossFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [Range(0.05f, 1f)]
    public float crossSlowCap = 0.5f;
    [Range(0.1f, 0.8f)]
    public float crossEscapeDifficulty = 0.3f;

    private PlayerHealth pHealth;



    public PlayerState currentState;

    private Vector2 moveInput;
    private bool sprintHeld;
    private bool crouchHeld;

    private float currentSpeed;

    private void Awake() // Setup references
    {
        controller = GetComponent<CharacterController>();

        if (TryGetComponent<PlayerHealth>(out PlayerHealth ph))
        {
            pHealth = ph;
        }
    }

    private void Update() // Main update loop
    {
        HandleInput();
        HandleMouseLook();
        HandleState();
        HandleMovement();
        HandleGravity();

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            superSpeedActive = !superSpeedActive;

            if (GameManager.Instance == null) return;

            if (superSpeedActive)
                GameManager.Instance.SetTimeScale(superSpeedTimeScale);
            else
                GameManager.Instance.ResetTimeScale();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.mouseState == MouseState.Locked || GameManager.Instance.mouseState == MouseState.Confined)
                GameManager.Instance.UpdateMouseLock(MouseState.Unlocked);
            else
                GameManager.Instance.UpdateMouseLock(MouseState.Locked);
        }

        if (superSpeedActive)
        {
            superSpeedMovementMultiplier = 1f / Time.timeScale;
        }


        if (Input.GetKeyDown(KeyCode.G))
        {
            TryDrain();
        }
    }

    private void LateUpdate() // Keeps camera aligned after movement
    {
        UpdateCameraPosition();
    }

    private void HandleInput() // Reads player input
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        sprintHeld = Input.GetKey(KeyCode.LeftShift);
        crouchHeld = Input.GetKey(KeyCode.LeftControl);
    }

    private void HandleMouseLook() // Handles camera rotation
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.mouseState != MouseState.Locked) return;

        float multiplier = superSpeedActive ? superSpeedMovementMultiplier : 1f;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100f * multiplier * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100f * multiplier * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Vertical rotation on camera holder
        cameraHolder.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);

        // Horizontal rotation on player
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleState() // Determines movement state and scale
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        // Handle crouch first
        if (crouchHeld)
        {
            SetScale(crouchingScale);

            if (!isMoving)
            {
                currentState = PlayerState.CrouchingStationary;
                currentSpeed = 0f;
            }
            else
            {
                currentState = PlayerState.Crouching;
                currentSpeed = crouchSpeed;
            }

            return;
        }
        else
        {
            // Attempt to stand up
            if (!CanStand())
            {
                SetScale(crouchingScale);

                if (!isMoving)
                {
                    currentState = PlayerState.CrouchingStationary;
                    currentSpeed = 0f;
                }
                else
                {
                    currentState = PlayerState.Crouching;
                    currentSpeed = crouchSpeed;
                }

                return;
            }

            SetScale(standingScale);
        }

        if (!isMoving)
        {
            currentState = PlayerState.Stationary;
            currentSpeed = 0f;
            return;
        }

        if (sprintHeld)
        {
            currentState = PlayerState.Sprinting;
            currentSpeed = sprintSpeed;
            return;
        }

        currentState = PlayerState.Walking;
        currentSpeed = walkSpeed;
    }

    private void HandleMovement() // Moves the player
    {
        if (!canMove) return;

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move);

        float multiplier = superSpeedActive ? superSpeedMovementMultiplier : 1f;

        // Cross logic
        if (TryGetComponent<PlayerHealth>(out PlayerHealth health))
        {
            if (health.InCrossArea && move.sqrMagnitude > 0.01f)
            {
                Vector3 toCenter = health.crossCenter - transform.position;
                float distance = toCenter.magnitude;

                if (distance < health.crossRadius)
                {
                    Vector3 dirToCenter = toCenter.normalized;
                    Vector3 moveDir = move.normalized;

                    float dot = Vector3.Dot(moveDir, dirToCenter);

                    if (dot > crossEscapeDifficulty)
                    {
                        // 0 = edge or radius, 1 = center of radius
                        float normalizedDistance = 1f - (distance / health.crossRadius);

                        float curveValue = crossFalloff.Evaluate(normalizedDistance);

                        float t = curveValue * dot;

                        // Final mult
                        crossSlowMultiplier = Mathf.Lerp(1f, crossSlowCap, t);
                    }
                }
            }
            else if (!health.InCrossArea)
            {
                crossSlowMultiplier = 1f;
            }
        }
        

        Vector3 finalMove = move * currentSpeed * multiplier * crossSlowMultiplier;

        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void HandleGravity() // Applies gravity
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = groundedForce;
        }
        else
        {
            float gravityMultiplier = superSpeedActive ? 1f / Time.timeScale : 1f;
            verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    private void SetScale(float scale) // Adjusts player scale
    {
        transform.localScale = new Vector3(1f, scale, 1f);
    }

    private bool CanStand() // Checks if player can stand up
    {
        Ray ray = new Ray(HeadPosition.position, Vector3.up);

        if (Physics.Raycast(ray, crouchCheckDistance))
        {
            return false;
        }

        return true;
    }

    private void UpdateCameraPosition() // Locks camera to head position
    {
        if (cameraHolder == null || HeadPosition == null) return;

        cameraHolder.position = HeadPosition.position;
    }

    private void TryDrain()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 2f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IHuman>(out IHuman human))
            {
                HumanBase hb = hit.GetComponent<HumanBase>();

                if (hb != null && hb.currentHealth <= hb.maxHealth * 0.5f)
                {
                    StartCoroutine(DrainPlayerRoutine(hb));
                    break;
                }
            }
        }
    }

    private IEnumerator DrainPlayerRoutine(HumanBase target)
    {
        canMove = false;

        target.Drain(GetComponent<IVampire>());

        float duration = 1.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            // Force player to keep looking at drain pivot
            if (target.DrainPosPivot != null)
            {
                Vector3 dir = target.DrainPosPivot.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation,
                        targetRot,
                        Time.deltaTime * 10f
                    );
                }
            }

            yield return null;
        }

        // Restore health
        if (TryGetComponent<PlayerHealth>(out PlayerHealth ph))
        {
            ph.currentHealth = ph.maxHealth;
        }

        canMove = true;
    }
}



public enum PlayerState
{
    Stationary,
    Walking,
    Crouching,
    CrouchingStationary,
    Sprinting
}