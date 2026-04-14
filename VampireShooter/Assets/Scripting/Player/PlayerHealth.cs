using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable, IVampire
{
    public PlayerMovement pMovement;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    [Space]
    [Header("Burning")]
    public bool Burning = false;
    public float burnPersistTime = 0.25f;
    private float burnTimer = 0f;

    [Header("UV Light Logic")]
    public bool InUVLight = false;

    [Header("Cross Logic")]
    public bool InCrossArea = false;
    [HideInInspector] public Vector3 crossCenter;
    [HideInInspector] public float crossRadius;

    private void Awake() // Initializes health
    {
        currentHealth = maxHealth;
    }

    private void Update() // Handles burn persistence
    {
        if (!InUVLight && !InCrossArea && !Burning) { return; } 

        if (InUVLight || InCrossArea)
        {
            Burning = true;
            burnTimer = burnPersistTime;
        }
        else
        {
            burnTimer -= Time.deltaTime;

            if (burnTimer <= 0f)
            {
                Burning = false;
            }
        }
    }

    public void TakeDamage(float damage) // Applies damage
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Die() // Handles death
    {
        Debug.Log("Player Dead");
    }

    public void SetInUVLight(bool state) // Called by UV zones
    {
        InUVLight = state;
    }

    public void SetCrossZone(Vector3 center, float radius, bool state)
    {
        InCrossArea = state;

        if (state)
        {
            crossCenter = center;
            crossRadius = radius;
        }
    }

    public void DrainHuman(IHuman Target)
    {

    }
}
