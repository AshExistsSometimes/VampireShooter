using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    protected AIMovement movement;

    public bool HeardPlayer { get; private set; }
    public Vector3 lastHeardPosition;

    protected virtual void Awake() // Initializes health and movement
    {
        currentHealth = maxHealth;

        movement = GetComponent<AIMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<AIMovement>();
        }
    }

    public virtual void TakeDamage(float damage) // Applies damage
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public virtual void Die() // Handles death
    {
        Destroy(gameObject);// will be overwritten, vampires disintegrate into dust, humans ragdoll
    }

    

    public virtual void OnHeardNoise(Vector3 position)
    {
        HeardPlayer = true;
        lastHeardPosition = position;
    }

    public void ClearHeardPlayer()
    {
        HeardPlayer = false;
    }
}
