using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    public bool GizmosOn = true;

    public PlayerMovement movement;

    [Header("Noise Radius")]
    public float humanRadius;
    public float vampireRadius;

    [Header("Detection")]
    public LayerMask enemyMask;
    private float noiseTimer;


    [Header("Sphere References")]
    public Transform humanNoiseSphere;
    public Transform vampireNoiseSphere;
    [Space]
    [Header("<b><size=110%>Detection Ranges")]

    [Header("Stationary")]

    public float StationaryHumanRadius = 0f;
    public float StationaryVampireRadius = 0f;
    [Space]
    [Header("Walking")]
    public float WalkingHumanRadius = 10f;
    public float WalkingVampireRadius = 20f;
    [Space]
    [Header("Crouching")]
    public float CrouchingHumanRadius = 2f;
    public float CrouchingVampireRadius = 5f;
    [Space]
    [Header("Sprinting")]
    public float SprintingHumanRadius = 15f;
    public float SprintingVampireRadius = 30f;
    [Space]
    [Header("Shooting")]
    public float ShootingHumanRadius = 50f;
    public float ShootingVampireRadius = 75f;


    private void Update() // Updates noise values and scales spheres
    {
        UpdateNoiseValues();
        UpdateSphereScales();

        noiseTimer -= Time.deltaTime;

        if (noiseTimer <= 0f)
        {
            noiseTimer = 0.2f;
            EmitNoise();
        }
    }

    private void UpdateNoiseValues() // Sets noise based on player state
    {
        switch (movement.currentState)
        {
            case PlayerState.Stationary:
            case PlayerState.CrouchingStationary:

                humanRadius = StationaryHumanRadius;
                vampireRadius = StationaryVampireRadius;

                break;

            case PlayerState.Walking:

                humanRadius = WalkingHumanRadius;
                vampireRadius = WalkingVampireRadius;

                break;

            case PlayerState.Crouching:

                humanRadius = CrouchingHumanRadius;
                vampireRadius = CrouchingVampireRadius;

                break;

            case PlayerState.Sprinting:

                humanRadius = SprintingHumanRadius;
                vampireRadius = SprintingVampireRadius;

                break;
        }
    }

    private void UpdateSphereScales() // Scales visual spheres to match radius
    {
        if (humanNoiseSphere != null)
        {
            float scale = humanRadius * 2f;
            humanNoiseSphere.localScale = new Vector3(scale, scale, scale);
        }

        if (vampireNoiseSphere != null)
        {
            float scale = vampireRadius * 2f;
            vampireNoiseSphere.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void OnShoot() // Overrides noise when firing
    {
        humanRadius = ShootingHumanRadius;
        vampireRadius = ShootingVampireRadius;
    }

    private void EmitNoise()
    {
        float maxRadius = Mathf.Max(humanRadius, vampireRadius);

        Collider[] hits = Physics.OverlapSphere(transform.position, maxRadius, enemyMask);

        foreach (Collider col in hits)
        {
            EnemyBase enemy = col.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            // Humans
            if (enemy is HumanBase)
            {
                if (dist <= humanRadius)
                {
                    enemy.OnHeardNoise(transform.position);
                }
            }
            // Vampires
            else if (enemy is VampireBase)
            {
                if (dist <= vampireRadius)
                {
                    enemy.OnHeardNoise(transform.position);
                }
            }
        }
    }

    private void OnDrawGizmos() // Draws debug spheres
    {
        if (!GizmosOn) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, humanRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, vampireRadius);
    }
}