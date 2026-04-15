using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    private float baseDamage;
    private WeaponSO weaponData;

    private Rigidbody rb;
    private GameObject myEffect;

    public bool StickToTarget = false;

    private int ownerLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    /// <summary>
    /// Initializes projectile physics and stores weapon data for damage calculation
    /// </summary>
    public void InitializeProjectile(float mass, float damage, bool useGravity, WeaponSO weapon, int shooterLayer)
    {
        rb.mass = mass;
        rb.useGravity = useGravity;

        baseDamage = damage;
        weaponData = weapon;
        ownerLayer = shooterLayer;

        IgnoreOwnerLayers();
    }

    /// <summary>
    /// Assigns impact effect
    /// </summary>
    public void InitializeProjectileEffect(GameObject effect)
    {
        myEffect = effect;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (myEffect != null)
        {
            Instantiate(myEffect, transform.position, Quaternion.identity);
        }

        IDamageable dmg = collision.collider.GetComponentInParent<IDamageable>();
        if (dmg != null)
        {
            float finalDamage = CalculateDamage(collision.gameObject);
            dmg.TakeDamage(finalDamage);
        }

        Destroy(gameObject);// change later
        // If stick to target is true, it should become a child of the object it hit and stay attatched in the relative position it hit for a short time
        // else, it bounces, cant run OnCollisionEnter again when it does so, then despawns
    }

    /// <summary>
    /// Applies weapon multipliers based on target type
    /// </summary>
    private float CalculateDamage(GameObject target)
    {
        float finalDamage = baseDamage;

        bool isVampire = target.GetComponentInParent<IVampire>() != null;
        bool isHuman = target.GetComponentInParent<IHuman>() != null;

        if (weaponData != null)
        {
            switch (weaponData.PreferredTarget)
            {
                case WeaponSO.EffectiveTarget.Vampire:
                    if (isVampire)
                        finalDamage *= weaponData.VampireMultiplier;
                    break;

                case WeaponSO.EffectiveTarget.Humans:
                    if (isHuman)
                        finalDamage *= weaponData.HumanMultiplier;
                    break;
            }
        }

        return finalDamage;
    }

    private void IgnoreOwnerLayers()
    {
        int projectileLayer = gameObject.layer;

        if (ownerLayer == LayerMask.NameToLayer("Player"))
        {
            Physics.IgnoreLayerCollision(projectileLayer, LayerMask.NameToLayer("Player"), true);
        }
        else
        {
            Physics.IgnoreLayerCollision(projectileLayer, LayerMask.NameToLayer("Enemy/Human"), true);
            Physics.IgnoreLayerCollision(projectileLayer, LayerMask.NameToLayer("Enemy/Vampire"), true);
        }
    }
}

