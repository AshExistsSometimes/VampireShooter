using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    protected AIMovement movement;

    private Renderer[] renderers;
    private Color[] originalColors;
    public float flashDuration = 0.1f;

    public bool HeardPlayer { get; private set; }
    public Vector3 lastHeardPosition;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        movement = GetComponent<AIMovement>();
        if (movement == null)
            movement = gameObject.AddComponent<AIMovement>();

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;

        StartCoroutine(DamageFlash());

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

    private IEnumerator DamageFlash()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
