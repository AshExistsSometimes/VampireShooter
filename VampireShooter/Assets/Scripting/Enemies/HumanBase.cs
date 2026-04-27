using System.Collections;
using UnityEngine;

public class HumanBase : EnemyBase, IHuman
{
    public bool DebugMode = false;

    [Header("Drain")]
    public Transform DrainPosPivot;
    public Transform MyDrainPos;
    public float drainDuration = 1.5f;

    private bool isBeingDrained = false;

    private Renderer rend;
    private Color originalColor;

    [Header("Hearing")]
    public float hearingCheckInterval = 0.2f;

    private float hearingTimer;

    [Header("<b><size=110%>AI")]
    public Transform player;
    public LayerMask visionMask;
    public float viewRange = 15f;

    [Header("Idle")]
    [Tooltip("Idle = does nothing")]
    public bool canIdle = true;
    public Vector2 idleTime = new Vector2(2f, 7f);
    public float idleChancePerSecond = 0.2f;

    private float idleTimer;
    private HumanState previousState;

    [Header("Waiting")]
    [Tooltip("Waiting = stands still until player detected")]
    public bool canWait = true;
    public float roamRadius = 5f;
    public float roamInterval = 3f;

    [Header("Searching")]
    [Tooltip("Searching = goes to last known player position")]
    public bool canSearch = true;

    [Header("Attacking")]
    [Tooltip("Attacking = moves toward player")]
    public bool canAttack = true;
    public bool hasRangedWeapon = false;
    public float AttackDamage = 10f;

    [Header("Fleeing")]
    [Tooltip("Flee when health drops below this percentage")]
    [Range(0f, 1f)]
    public float fleeHealthThreshold = 0.3f;

    public float fleeDistance = 10f;
    public float fleeRepathTime = 1f;

    private float fleeTimer;


    private Vector3 waitAnchor;
    private float roamTimer;

    public enum HumanState
    {
        Idle,
        Waiting,
        Searching,
        Attacking,
        Fleeing
    }

    public HumanState currentState;

    private Vector3 lastKnownPlayerPos;
    private bool hasLastKnownPos = false;

    protected override void Awake()
    {
        base.Awake();

        waitAnchor = transform.position;

        rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            var mat = rend.material;

            if (mat.HasProperty("_BaseColor"))
                originalColor = mat.GetColor("_BaseColor");
            else
                originalColor = mat.color;
        }
    }

    private void Update()
    {
        if (isBeingDrained) return;

        HandleHearing();

        HandleIdleInterrupt();

        RunStateMachine();

        if (!isBeingDrained && currentHealth / maxHealth <= fleeHealthThreshold)
        {
            currentState = HumanState.Fleeing;
        }
    }

    private void HandleHearing()
    {
        if (!HeardPlayer) return;

        lastKnownPlayerPos = lastHeardPosition;
        hasLastKnownPos = true;

        // Only force into searching from passive states
        if (currentState == HumanState.Waiting || currentState == HumanState.Idle)
        {
            currentState = HumanState.Searching;
        }

        ClearHeardPlayer();
    }
    private void RunStateMachine()
    {
        switch (currentState)
        {
            case HumanState.Idle:
                StateIdle();
                break;

            case HumanState.Waiting:
                StateWaiting();
                break;

            case HumanState.Searching:
                StateSearching();
                break;

            case HumanState.Attacking:
                StateAttacking();
                break;
            case HumanState.Fleeing:
                StateFleeing();
                break;
        }
    }

    // =========================
    // STATES
    // =========================


    // IDLE //
    private void StateIdle()
    {
        movement.Stop();

        // Interrupt if player seen
        if (CanSeePlayer() && canAttack)
        {
            currentState = HumanState.Attacking;
            return;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            currentState = previousState;
        }
    }

    private void HandleIdleInterrupt()
    {
        // Never idle if player visible
        if (CanSeePlayer()) return;

        // Don't interrupt certain states
        if (currentState == HumanState.Idle || currentState == HumanState.Searching)
            return;

        // Random chance per second
        if (Random.value < idleChancePerSecond * Time.deltaTime)
        {
            previousState = currentState;
            currentState = HumanState.Idle;

            idleTimer = Random.Range(idleTime.x, idleTime.y);
        }
    }

    // WAITING //

    private void StateWaiting()
    {
        if (CanSeePlayer())
        {
            currentState = HumanState.Attacking;
            return;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer <= 0f)
        {
            roamTimer = roamInterval;

            Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
            Vector3 roamTarget = waitAnchor + new Vector3(randomCircle.x, 0f, randomCircle.y);

            movement.MoveTo(roamTarget);
        }
    }

    // SEARCHING //
    private void StateSearching()
    {
        if (!hasLastKnownPos)
        {
            currentState = HumanState.Idle;
            return;
        }

        movement.MoveTo(lastKnownPlayerPos);

        if (CanSeePlayer())
        {
            currentState = HumanState.Attacking;
            return;
        }

        if (movement.ReachedDestination())
        {
            if (!CanSeePlayer())
            {
                hasLastKnownPos = false;
                currentState = HumanState.Waiting;
            }
        }
    }

    // ATTACKING //
    private void StateAttacking()
    {
        if (player == null) return;

        movement.MoveTo(player.position);

        if (CanSeePlayer())
        {
            lastKnownPlayerPos = player.position;
            hasLastKnownPos = true;
        }
        else
        {
            currentState = HumanState.Searching;
        }
    }

    // FLEEING //

    private void StateFleeing()
    {
        if (player == null) return;

        fleeTimer -= Time.deltaTime;

        if (fleeTimer <= 0f)
        {
            fleeTimer = fleeRepathTime;

            Vector3 awayDir = (transform.position - player.position).normalized;
            Vector3 target = transform.position + awayDir * fleeDistance;

            movement.MoveTo(target);
        }

        // Escape condition (10s no LoS comes later, keep simple for now)
        if (!CanSeePlayer())
        {
            currentState = HumanState.Waiting;
        }
    }

    // =========================
    // PERCEPTION
    // =========================

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dir = (player.position - origin);

        if (dir.magnitude > viewRange) return false;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, viewRange, visionMask))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    // =========================
    // DRAIN
    // =========================

    public void Drain(IVampire attacker)
    {
        if (isBeingDrained) return;
        if (currentHealth > maxHealth * 0.5f) return;

        StartCoroutine(DrainRoutine(attacker));
    }

    private IEnumerator DrainRoutine(IVampire attacker)
    {
        isBeingDrained = true;

        movement.Stop();

        Transform attackerTransform = ((MonoBehaviour)attacker).transform;

        float t = 0f;
        Vector3 startPos = attackerTransform.position;

        while (t < 1f)
        {
            // --- ROTATE HUMAN PIVOT TO FACE PLAYER (Y ONLY) ---
            if (DrainPosPivot != null)
            {
                Vector3 dir = attackerTransform.position - DrainPosPivot.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    DrainPosPivot.rotation = targetRot;
                }
            }

            // --- GET UPDATED TARGET POSITION AFTER ROTATION ---
            Vector3 targetPos = MyDrainPos.position;

            // --- MOVE PLAYER ---
            t += Time.deltaTime * 5f;
            attackerTransform.position = Vector3.Lerp(startPos, targetPos, t);

            // --- ROTATE PLAYER TO FACE DRAIN PIVOT (Y ONLY) ---
            if (DrainPosPivot != null)
            {
                Vector3 lookDir = DrainPosPivot.position - attackerTransform.position;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);

                    attackerTransform.rotation = Quaternion.Slerp(
                        attackerTransform.rotation,
                        targetRot,
                        Time.deltaTime * 10f
                    );
                }
            }

            yield return null;
        }

        float fadeT = 0f;
        while (fadeT < drainDuration)
        {
            fadeT += Time.deltaTime;

            if (rend != null)
            {
                float lerp = fadeT / drainDuration;
                var mat = rend.material;

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.Lerp(originalColor, Color.gray, lerp));
                else
                    mat.color = Color.Lerp(originalColor, Color.gray, lerp);
            }

            yield return null;
        }

        Die();
    }

    public override void Die()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!DebugMode) { return; }

        if (player == null) return;

        // View range circle
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRange);

        // Direction to player
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 dir = (player.position - origin).normalized;

        bool hasLoS = false;

        if (Application.isPlaying)
        {
            hasLoS = CanSeePlayer();
        }

        Gizmos.color = hasLoS ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + dir * viewRange);
    }
}