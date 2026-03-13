using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    ////////////// REFERENCES /////////////////////////////////////

    [Header("Weapon")]
    public Transform WeaponHolderPosition;
    public WeaponData DefaultWeapon;

    [Header("Controller")]
    public bool CanSprint = true;
    [Space]
    public float MoveSpeed = 5f;
    public float SprintSpeed = 7f;
    [Space]
    public float MaxStamina = 10f;
    public float StaminaDrainRate = 3f;
    [Space]
    public float StaminaRechargeCooldown = 3f;
    public float StaminaRechargeRate = 3f;
    [Space]
    [Range(0f, 1f)]
    public float StaminaRechargedBeforeSprint = 0.3f;

    private bool StaminaRecovering = false;
    private float remainingStamina;

    [Header("Camera")]
    public Camera cam;
    public float sensitivity = 2f;
    float xRotation;
    float yRotation;

    //////// INTERNAL /////////////////////////////////////////////

    private CharacterController controller;
    private float currentSpeed;
    private float rechargeTimer;

    //////// CODE /////////////////////////////////////////////////

    private void Start()
    {
        // Gets the CharacterController attached to the player object
        controller = GetComponent<CharacterController>();

        // Initializes stamina to maximum value
        remainingStamina = MaxStamina;

        // Locks and hides the cursor for first person camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Handles camera rotation input
        HandleLook();

        // Handles player movement input
        HandleMovement();

        // Handles stamina drain and regeneration
        HandleStamina();
    }

    void HandleMovement()
    {
        // Reads movement input from keyboard or controller
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Combines movement input into a direction vector
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // Determines if the player is attempting to sprint
        bool sprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Sprint");

        // Determines sprint state based on stamina and input
        bool isSprinting = sprintInput && CanSprint && remainingStamina > 0f && move.magnitude > 0.1f;

        // Selects movement speed based on sprint state
        currentSpeed = isSprinting ? SprintSpeed : MoveSpeed;

        // Applies movement using CharacterController
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Drains stamina if sprinting
        if (isSprinting)
        {
            remainingStamina -= StaminaDrainRate * Time.deltaTime;
            rechargeTimer = 0f;
            StaminaRecovering = false;

            // Prevents stamina from dropping below zero
            if (remainingStamina <= 0f)
            {
                remainingStamina = 0f;
                CanSprint = false;
                StaminaRecovering = true;
            }
        }
    }

    void HandleStamina()
    {
        // Skips stamina logic if stamina is full and sprint is available
        if (remainingStamina >= MaxStamina && CanSprint)
            return;

        // Increments recharge timer when not sprinting
        rechargeTimer += Time.deltaTime;

        // Starts stamina regeneration after cooldown period
        if (rechargeTimer >= StaminaRechargeCooldown)
        {
            remainingStamina += StaminaRechargeRate * Time.deltaTime;

            // Clamps stamina to maximum value
            if (remainingStamina > MaxStamina)
                remainingStamina = MaxStamina;
        }

        // Restores sprint capability after sufficient stamina recovery
        if (!CanSprint && remainingStamina >= MaxStamina * StaminaRechargedBeforeSprint)
        {
            CanSprint = true;
            StaminaRecovering = false;
        }
    }

    void HandleLook()
    {
        // Reads mouse and controller look input
        float mouseX = Input.GetAxis("Mouse X") + Input.GetAxis("RightStickX");
        float mouseY = Input.GetAxis("Mouse Y") + Input.GetAxis("RightStickY");

        // Applies sensitivity scaling
        mouseX *= sensitivity * 100f * Time.deltaTime;
        mouseY *= sensitivity * 100f * Time.deltaTime;

        // Accumulates horizontal and vertical rotation
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamps vertical camera rotation to prevent flipping
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // Applies vertical rotation to the camera
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Applies horizontal rotation to the player body
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }


    public void PlayAnimation(string WORD)
    {

    }


    //https://youtu.be/ZJ-RTtug7KY?si=kt4yF4Xam-YLxYPo&t=261
}
