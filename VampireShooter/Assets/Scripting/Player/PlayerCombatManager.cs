using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatManager : MonoBehaviour
{
    public WeaponSO EquippedWeapon;

    public int MaxWeapons = 3;
    public List<WeaponRuntimeData> WeaponLoadout;
    private int currentWeaponIndex = 0;
    private WeaponRuntimeData currentWeaponData;

    public LayerMask hitMask;

    [Header("References")]
    public GameObject weaponSocket;// where the equipped weapon is placed
    public Collider MeleeHitbox;

    private GameObject currentWeaponFirePoint;// Needs to change to be whatever can be found named "FirePoint" under the active weapons children

    public Image EquippedWeaponIcon;// cahnges to be the equipped weapons Icon
    private GameObject currentWeaponModel;

    private float nextFireTime = 0f;

    private bool isReloading = false;

    [Header("VFX")]
    public LineRenderer tracer;
    public float tracerDuration = 0.05f;
    public GameObject tracerPrefab;
    public int maxShotgunTracers = 12;

    private Coroutine tracerRoutine;

    [Header("Punching (Fallback if no weapons")]
    public float PunchDamage = 5f;
    public float PunchSpeed = 1.5f;
    public Vector3 PunchHitboxSize = new Vector3 (2f, 2f, 2f);

    private void Awake()
    {
        ResetEquippedWeapon();
    }

    public void ResetEquippedWeapon()
    {
        if (WeaponLoadout.Count == 0) return;

        currentWeaponIndex = 0;

        for (int i = 0; i < WeaponLoadout.Count; i++)
        {
            WeaponLoadout[i].currentAmmo = WeaponLoadout[i].weapon.MaxAmmo;
            WeaponLoadout[i].currentMagazines = WeaponLoadout[i].weapon.MaxMagazines;
        }

        currentWeaponData = WeaponLoadout[currentWeaponIndex];
        EquippedWeapon = currentWeaponData.weapon;

        EquipWeaponVisuals();
    }

    public void UseWeapon()
    {
        if (isReloading) return;

        if (Time.time < nextFireTime) return;

        if (EquippedWeapon == null)
        {
            Punch();
            nextFireTime = Time.time + (1f / PunchSpeed);
            return;
        }

        // CHECK AMMO (skip for melee)
        if (EquippedWeapon.weaponType == WeaponSO.WeaponType.Ranged || EquippedWeapon.weaponType == WeaponSO.WeaponType.Throwable)
        {
            if (currentWeaponData.currentAmmo <= 0)
            {
                TryReload();
                return;
            }
        }

        nextFireTime = Time.time + (1f / EquippedWeapon.FireRate);

        switch (EquippedWeapon.weaponType)
        {
            case WeaponSO.WeaponType.Ranged:
                RangedAttack();
                break;

            case WeaponSO.WeaponType.Melee:
                MeleeAttack();
                break;

            case WeaponSO.WeaponType.Throwable:
                ThrowAttack();
                break;
        }
    }

    public void Punch()
    {
        Collider[] hits = Physics.OverlapBox(
            transform.position + transform.forward * 1.5f,
            PunchHitboxSize * 0.5f,
            transform.rotation
        );

        foreach (Collider col in hits)
        {
            IDamageable dmg = col.GetComponentInParent<IDamageable>();
            if (dmg == null) continue;

            bool isVampire = col.GetComponentInParent<IVampire>() != null;

            float damage = PunchDamage;

            if (isVampire)
            {
                // Cannot kill vampires
                VampireBase vamp = col.GetComponentInParent<VampireBase>();
                if (vamp != null && vamp.currentHealth - damage <= 0f)
                {
                    vamp.currentHealth = 1f;
                    continue;
                }
            }

            dmg.TakeDamage(damage);
        }
    }

    public void HoldFire()
    {
        if (EquippedWeapon == null) return;

        if (EquippedWeapon.fireType == WeaponSO.FireType.Automatic)
        {
            UseWeapon();
        }
    }



    // RANGED WEAPON LOGIC ///////////////////////////////////////////////////////////////////////////////////////
    public void RangedAttack()
    {
        // all attacks need to deal different damage to vampires and humans dependant on the guns relevant multiplier, and do the base damage if its preferred target is "any"
        // determine vampires as anything with IVampire and human as anything with IHuman

        if (EquippedWeapon.fireType == WeaponSO.FireType.SingleFire)
        {
            // Single fire logic
            if (IsProjectileWeapon())
            {
                RangedSingleProjectile();
            }
            else
            {
                RangedSingleHitscan();
            }
        }
        else if (EquippedWeapon.fireType == WeaponSO.FireType.BurstFire)
        {
            // Burst fire logic
            if (IsProjectileWeapon())
            {
                RangedBurstProjectile();
            }
            else
            {
                RangedBurstHitscan();
            }
        }
        else if (EquippedWeapon.fireType == WeaponSO.FireType.Automatic)
        {
            // Burst fire logic
            if (IsProjectileWeapon())
            {
                RangedAutomaticProjectile();
            }
            else
            {
                RangedAutomaticHitscan();
            }
        }
        else if (EquippedWeapon.fireType == WeaponSO.FireType.Shotgun)
        {
            // Burst fire logic
            if (IsProjectileWeapon())
            {
                RangedShotgunProjectile();
            }
            else
            {
                RangedShotgunHitscan();
            }
        }

    }

    
    // Projectile attacks
    public void RangedSingleProjectile()
    {
        if (!ConsumeAmmo()) return;

        SpawnProjectile(GetSpreadDirection());// Basically just the ThrowAttack
    }


    public void RangedBurstProjectile()
    {

        StartCoroutine(BurstProjectileRoutine());// Throws multiple Projectiles in quick succession in bursts
    }
    private IEnumerator BurstProjectileRoutine()
    {
        for (int i = 0; i < EquippedWeapon.BurstFireAmount; i++)
        {
            if (!ConsumeAmmo()) yield break;

            SpawnProjectile(GetSpreadDirection());
            yield return new WaitForSeconds(EquippedWeapon.BurstFireRate);
        }
    }


    public void RangedAutomaticProjectile()
    {
        if (!ConsumeAmmo()) return;

        SpawnProjectile(GetSpreadDirection());// throws projectiles repeatedly while firing
    }


    public void RangedShotgunProjectile()
    {
        if (!ConsumeAmmo()) return;

        int pelletCount = EquippedWeapon.ShotgunPellets;

        for (int i = 0; i < pelletCount; i++)
        {
            SpawnProjectile(GetSpreadDirection());
        }
    }

    // Hitscan attacks
    public void RangedSingleHitscan()
    {
        if (currentWeaponFirePoint == null) return;
        if (!ConsumeAmmo()) return;

        Vector3 origin = currentWeaponFirePoint.transform.position;

        Ray camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit camHit, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore))
            targetPoint = camHit.point;
        else
            targetPoint = camRay.origin + camRay.direction * EquippedWeapon.MaxRange;

        Vector3 direction = (targetPoint - origin).normalized;

        float remainingPenetration = EquippedWeapon.PiercingLevel;

        Ray ray = new Ray(origin, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector3 endPoint = origin + direction * EquippedWeapon.MaxRange;

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;

            float distance01 = hit.distance / EquippedWeapon.MaxRange;
            float falloff = EquippedWeapon.WeaponFalloff.Evaluate(distance01);
            float damage = EquippedWeapon.Damage * falloff;

            DealDamage(obj, damage);

            endPoint = hit.point;

            PenetratableObject pen = obj.GetComponent<PenetratableObject>();

            if (pen != null)
                remainingPenetration -= pen.PenetrationBlockage;
            else
                break;

            if (remainingPenetration < 0)
                break;
        }

        SpawnSingleTracer(origin, endPoint);
    }
    public void RangedBurstHitscan()
    {
        StartCoroutine(BurstHitscanRoutine());
    }

    private IEnumerator BurstHitscanRoutine()
    {
        for (int i = 0; i < EquippedWeapon.BurstFireAmount; i++)
        {
            if (!ConsumeAmmo()) yield break;

            FireHitscanShot();
            yield return new WaitForSeconds(EquippedWeapon.BurstFireRate);
        }
    }
    public void RangedAutomaticHitscan()
    {
        if (!ConsumeAmmo()) return;

        FireHitscanShot();
    }
    public void RangedShotgunHitscan()
    {
        if (!ConsumeAmmo()) return;
        if (currentWeaponFirePoint == null) return;

        int pelletCount = EquippedWeapon.ShotgunPellets;

        Vector3 origin = currentWeaponFirePoint.transform.position;

        Ray camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        Vector3 baseTarget;

        if (Physics.Raycast(camRay, out RaycastHit camHit, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore))
            baseTarget = camHit.point;
        else
            baseTarget = camRay.origin + camRay.direction * EquippedWeapon.MaxRange;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 spreadDir = GetSpreadDirection();
            Vector3 direction = (baseTarget - origin).normalized + spreadDir * 0.1f;
            direction.Normalize();

            Ray ray = new Ray(origin, direction);
            RaycastHit[] hits = Physics.RaycastAll(ray, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            Vector3 endPoint = origin + direction * EquippedWeapon.MaxRange;

            float remainingPenetration = EquippedWeapon.PiercingLevel;

            foreach (RaycastHit hit in hits)
            {
                GameObject obj = hit.collider.gameObject;

                float distance01 = hit.distance / EquippedWeapon.MaxRange;
                float falloff = EquippedWeapon.WeaponFalloff.Evaluate(distance01);
                float damage = EquippedWeapon.Damage * falloff;

                DealDamage(obj, damage);

                endPoint = hit.point;

                PenetratableObject pen = obj.GetComponent<PenetratableObject>();

                if (pen != null)
                    remainingPenetration -= pen.PenetrationBlockage;
                else
                    break;

                if (remainingPenetration < 0)
                    break;
            }

            SpawnTracerPrefab(origin, endPoint);
        }
    }


    // MELEE WEAPON LOGIC /////////////////////////////////////////////////////////////////////////////////////
    public void MeleeAttack()
    {
        if (MeleeHitbox == null) return;

        Vector3 center = MeleeHitbox.bounds.center;
        Vector3 halfExtents = MeleeHitbox.bounds.extents;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);

        foreach (Collider col in hits)
        {
            DealDamage(col.gameObject, EquippedWeapon.Damage);
        }
    }



    // THROWN WEAPON LOGIC /////////////////////////////////////////////////////////////////////////////////////
    public void ThrowAttack()
    {
        if (EquippedWeapon.ProjectileModel == null || currentWeaponFirePoint == null) return;

        GameObject proj = Instantiate(
            EquippedWeapon.ProjectileModel,
            currentWeaponFirePoint.transform.position,
            Quaternion.identity
        );

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null) projectile = proj.AddComponent<Projectile>();

        projectile.InitializeProjectile(
            EquippedWeapon.WeaponMass,
            EquippedWeapon.Damage,
            EquippedWeapon.UseGravity,
            EquippedWeapon,
            gameObject.layer
        );

        if (EquippedWeapon.HasEffectOnCollide)
        {
            projectile.InitializeProjectileEffect(EquippedWeapon.ProjectileEffect);
        }

        Vector3 force = currentWeaponFirePoint.transform.forward * EquippedWeapon.ThrowForwardForce
                      + Vector3.up * EquippedWeapon.ThrowUpForce;

        rb.AddForce(force, ForceMode.Impulse);
    }






    // Assistance logic


    public bool IsProjectileWeapon()
    {
        if (EquippedWeapon.projectileType == WeaponSO.ProjectileType.Projectile)
        { return true; }
        else 
        { return false; }
    }

    private void FireHitscanShot()
    {
        if (currentWeaponFirePoint == null) return;

        Vector3 origin = currentWeaponFirePoint.transform.position;

        Ray camRay = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit camHit, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore))
            targetPoint = camHit.point;
        else
            targetPoint = camRay.origin + camRay.direction * EquippedWeapon.MaxRange;

        Vector3 direction = (targetPoint - origin).normalized;

        float remainingPenetration = EquippedWeapon.PiercingLevel;

        Ray ray = new Ray(origin, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, EquippedWeapon.MaxRange, hitMask, QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Vector3 endPoint = origin + direction * EquippedWeapon.MaxRange;

        foreach (RaycastHit hit in hits)
        {
            GameObject obj = hit.collider.gameObject;

            float distance01 = hit.distance / EquippedWeapon.MaxRange;
            float falloff = EquippedWeapon.WeaponFalloff.Evaluate(distance01);
            float damage = EquippedWeapon.Damage * falloff;

            DealDamage(obj, damage);

            endPoint = hit.point;

            PenetratableObject pen = obj.GetComponent<PenetratableObject>();

            if (pen != null)
                remainingPenetration -= pen.PenetrationBlockage;
            else
                break;

            if (remainingPenetration < 0)
                break;
        }

        SpawnSingleTracer(origin, endPoint);
    }
    private void SpawnProjectile(Vector3 direction)
    {
        if (EquippedWeapon.ProjectileModel == null || currentWeaponFirePoint == null) return;

        GameObject proj = Instantiate(
            EquippedWeapon.ProjectileModel,
            currentWeaponFirePoint.transform.position,
            Quaternion.LookRotation(direction)
        );

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile == null) projectile = proj.AddComponent<Projectile>();

        projectile.InitializeProjectile(
         EquippedWeapon.WeaponMass,
         EquippedWeapon.Damage,
         EquippedWeapon.UseGravity,
         EquippedWeapon,
         gameObject.layer
         );

        if (EquippedWeapon.HasEffectOnCollide)
            projectile.InitializeProjectileEffect(EquippedWeapon.ProjectileEffect);

        Vector3 force =
            direction * EquippedWeapon.ThrowForwardForce +
            Vector3.up * EquippedWeapon.ThrowUpForce;

        rb.AddForce(force, ForceMode.Impulse);
    }

    private void DealDamage(GameObject target, float baseDamage)
    {
        IDamageable dmg = target.GetComponentInParent<IDamageable>();
        if (dmg == null) return;

        float finalDamage = baseDamage;

        bool isVampire = target.GetComponentInParent<IVampire>() != null;
        bool isHuman = target.GetComponentInParent<IHuman>() != null;

        if (EquippedWeapon != null)
        {
            switch (EquippedWeapon.PreferredTarget)
            {
                case WeaponSO.EffectiveTarget.Vampire:
                    if (isVampire) finalDamage *= EquippedWeapon.VampireMultiplier;
                    break;

                case WeaponSO.EffectiveTarget.Humans:
                    if (isHuman) finalDamage *= EquippedWeapon.HumanMultiplier;
                    break;
            }
        }

        dmg.TakeDamage(finalDamage);
    }

    private Vector3 GetSpreadDirection()
    {
        Vector3 forward = Camera.main.transform.forward;

        float angle = EquippedWeapon.BaseSpreadAngle;

        Vector3 randomDir = Random.insideUnitSphere * Mathf.Tan(angle * Mathf.Deg2Rad);

        return (forward + randomDir).normalized;
    }

    private void SetupFirePoint()
    {
        currentWeaponFirePoint = null;

        Transform[] children = weaponSocket.GetComponentsInChildren<Transform>();

        foreach (Transform t in children)
        {
            if (t.name == "FirePoint")
            {
                currentWeaponFirePoint = t.gameObject;
                return;
            }
        }

        Debug.LogWarning("No FirePoint found on weapon!");
    }

    private void EquipWeaponVisuals()
    {
        // Clear old weapon
        if (currentWeaponModel != null)
            Destroy(currentWeaponModel);

        if (EquippedWeapon == null || EquippedWeapon.Model == null) return;

        // Spawn model
        currentWeaponModel = Instantiate(EquippedWeapon.Model, weaponSocket.transform);
        currentWeaponModel.transform.localPosition = Vector3.zero;
        currentWeaponModel.transform.localRotation = Quaternion.identity;

        // Set up firepoint
        SetupFirePoint();

        // Update UI
        if (EquippedWeaponIcon != null)
            EquippedWeaponIcon.sprite = EquippedWeapon.WeaponIcon;
    }

    private bool ConsumeAmmo(int amount = 1)
    {
        if (currentWeaponData.currentAmmo < amount)
        {
            TryReload();
            return false;
        }

        currentWeaponData.currentAmmo -= amount;
        return true;
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentWeaponData.currentMagazines <= 0) return;
        if (currentWeaponData.currentAmmo == EquippedWeapon.MaxAmmo) return;

        StartCoroutine(ReloadRoutine());
    }

    public void ReloadInput()
    {
        TryReload();
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(EquippedWeapon.ReloadTime);

        currentWeaponData.currentMagazines--;
        currentWeaponData.currentAmmo = EquippedWeapon.MaxAmmo;

        isReloading = false;
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= WeaponLoadout.Count) return;
        if (index == currentWeaponIndex) return;

        currentWeaponIndex = index;
        currentWeaponData = WeaponLoadout[currentWeaponIndex];
        EquippedWeapon = currentWeaponData.weapon;

        StopAllCoroutines(); // stops reloads/bursts
        isReloading = false;

        EquipWeaponVisuals();
    }

    public void SwitchWeaponScroll(int direction)
    {
        int newIndex = currentWeaponIndex + direction;

        if (newIndex < 0)
            newIndex = WeaponLoadout.Count - 1;
        else if (newIndex >= WeaponLoadout.Count)
            newIndex = 0;

        SwitchWeapon(newIndex);
    }

    private void SpawnSingleTracer(Vector3 start, Vector3 end)
    {
        if (tracer == null)
        {
            Debug.LogWarning("Tracer is NULL");
            return;
        }

        tracer.enabled = true;
        tracer.positionCount = 2;

        tracer.SetPosition(0, start);
        tracer.SetPosition(1, end);

        // Properly stop previous coroutine
        if (tracerRoutine != null)
            StopCoroutine(tracerRoutine);

        tracerRoutine = StartCoroutine(DisableTracer());
    }


    private IEnumerator DisableTracer()
    {
        yield return new WaitForSeconds(tracerDuration);

        if (tracer != null)
            tracer.enabled = false;
    }

    private void SpawnTracerPrefab(Vector3 start, Vector3 end)
    {
        if (tracerPrefab == null) return;

        GameObject obj = Instantiate(tracerPrefab);
        LineRenderer lr = obj.GetComponent<LineRenderer>();

        if (lr == null)
        {
            Debug.LogWarning("Tracer prefab missing LineRenderer!");
            Destroy(obj);
            return;
        }

        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Destroy(obj, tracerDuration);
    }

}

[System.Serializable]
public class WeaponRuntimeData
{
    public WeaponSO weapon;

    public int currentAmmo;
    public int currentMagazines;

    public WeaponRuntimeData(WeaponSO weapon)
    {
        this.weapon = weapon;
        currentAmmo = weapon.MaxAmmo;
        currentMagazines = weapon.MaxMagazines;
    }
}
