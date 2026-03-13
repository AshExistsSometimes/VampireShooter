using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public const string SHOOT = "Shoot";
    public const string RELOAD = "Reload";

    AudioSource audioSource;
    Animator weaponAnimator;// Please dont make me learn animations mr tutorial man

    public Transform muzzle;
    public WeaponData weaponData;

    public PlayerController myController { get; set; }
    public int ammoCount { get; private set; }

    bool reloading = false;
    bool shooting = false;
    bool readyToShoot = true;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        weaponAnimator = GetComponent<Animator>();

        ammoCount = weaponData.maxAmmo;
        UserInterface.Instance.UpdateBulletCounter(ammoCount, weaponData.maxAmmo);
    }



    // Shooting Behavior

    public void Shoot()
    {
        if (!readyToShoot || shooting || reloading || weaponData == null) return;

        if (ammoCount <= 0) { Reload(); return; }

        readyToShoot = false;
        shooting = true;

        UseAmmo();

        Invoke(nameof(ResetAttack), weaponData.fireRate);
        AttackRaycast();

        Instantiate(weaponData.fireEffect, muzzle);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(weaponData.fireSound);

        weaponAnimator.Play(SHOOT);
        myController.PlayAnimation(SHOOT);
    }

    public void Reload()
    {
        if (ammoCount == weaponData.maxAmmo || reloading) return;

        reloading = true;

        myController.PlayAnimation(RELOAD);
        weaponAnimator?.Play(RELOAD);
        Invoke(nameof(ResetReload), weaponData.reloadTime);
    } 
    
    public void UseAmmo()
    {
        ammoCount--;
        UserInterface.Instance.UpdateBulletCounter(ammoCount, weaponData.maxAmmo);
    }

    // Reset

    void ResetAttack()
    {
        shooting = false;
        readyToShoot = true;
    }

    void ResetReload()
    {
        reloading = false;
        ammoCount = weaponData.maxAmmo;
        UserInterface.Instance.UpdateBulletCounter(ammoCount, weaponData.maxAmmo);
    }

    // Bullet Behavior
    public void AttackRaycast()
    {
        RaycastHit hit;
        RaycastHit[] hits;

        switch (weaponData.type)
        {
            case WeaponType.Bullet:
                if (Physics.Raycast(myController.cam.transform.position,
                                    myController.cam.transform.forward,
                                    out hit,
                                    weaponData.weaponRange))
                { HitTarget(hit); }
                break;
            case WeaponType.Piercing:
                hits = Physics.RaycastAll(myController.cam.transform.position,
                                          myController.cam.transform.forward,
                                          weaponData.weaponRange);
                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                        { HitTarget(hits[i]); }
                }
                break;
        }
    }

    void HitTarget(RaycastHit hit)
    {

        GameObject gO = Instantiate(weaponData.hitEffect, hit.point, Quaternion.identity);
        Destroy(gO, 4);
    }
}
