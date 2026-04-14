using UnityEngine;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Header("Image References")]
    public Image BurnEffect;
    public Image HealthEffect;

    [Header("Burn Settings")]
    public float burnEffectMaxAlpha = 0.6f;
    public float burnFadeInSpeed = 10f;

    public float BurnPulseSpeed = 8f;
    public float BurnPulseAmount = 0.05f;

    [Header("Health Settings")]
    public float healthEffectMaxAlpha = 0.5f;

    public float HealthPulseSpeed = 8f;
    public float HealthPulseAmount = 0.1f;

    [Header("Pulse Settings")]
    

    private float burnCurrentAlpha = 0f;

    private void Update()
    {
        if (playerHealth == null) return;

        HandleBurnEffect();
        HandleHealthEffect();
    }

    private void HandleBurnEffect()
    {
        if (BurnEffect == null) return;

        if (playerHealth.Burning)
        {
            burnCurrentAlpha = Mathf.Lerp(
                burnCurrentAlpha,
                burnEffectMaxAlpha,
                Time.deltaTime * burnFadeInSpeed
            );
        }
        else
        {
            float fadeSpeed = 1f / playerHealth.burnPersistTime;

            burnCurrentAlpha = Mathf.Lerp(
                burnCurrentAlpha,
                0f,
                Time.deltaTime * fadeSpeed
            );
        }

        float pulse = 0f;

        if (playerHealth.Burning)
        {
            pulse = Mathf.Sin(Time.time * BurnPulseSpeed) * BurnPulseAmount;
        }

        float finalAlpha = Mathf.Clamp01(burnCurrentAlpha + pulse);

        Color c = BurnEffect.color;
        c.a = finalAlpha;
        BurnEffect.color = c;
    }

    private void HandleHealthEffect()
    {
        if (HealthEffect == null) return;

        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;

        float t = 1f - healthPercent;

        float baseAlpha = t * healthEffectMaxAlpha;

        float pulse = 0f;

        if (t > 0.001f) // only pulse when damaged
        {
            float intensity = t;
            pulse = Mathf.Sin(Time.time * HealthPulseSpeed) * HealthPulseAmount * intensity;
        }

        float finalAlpha = Mathf.Clamp01(baseAlpha + pulse);

        Color c = HealthEffect.color;
        c.a = finalAlpha;
        HealthEffect.color = c;
    }
}