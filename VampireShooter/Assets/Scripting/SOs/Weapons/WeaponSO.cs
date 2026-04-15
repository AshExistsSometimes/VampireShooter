using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Weapon")]
public class WeaponSO : ScriptableObject 
{
    [Header("Weapon Identity")]

    [Tooltip("The Weapons Name")]
    public string Name = "Gun";

    [Tooltip("The Weapons Model")]
    public GameObject Model;// What is shown in the players hand
    public Sprite WeaponIcon;

    [Tooltip("The projectiles model (NOTE: only important for Thrown and Projectile weapons")]
    public GameObject ProjectileModel;// what is insantiated for Projectile attacks and Thrown weapons

    [Tooltip("What type of attack the weapon will execute")]
    public WeaponType weaponType;
    public enum WeaponType
    {
        Ranged,// Raycast
        Melee,// Hitbox
        Throwable// Spawns object
    }

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Weapon Damage")]

    [Tooltip("Base Damage per Hit")]
    public float Damage = 5f;
    [Tooltip("How far the weapons raycast can travel")]
    public float MaxRange = 30f;

    [Tooltip("How much the weapons damage decreases as it approaches its maximum range")]
    public AnimationCurve WeaponFalloff;
    [Space]
    [Tooltip("Base angle of the weapons spread cone")]
    public float BaseSpreadAngle = 7f;
    [Space]
   

    [Tooltip("How much an attack can pierce through before being stopped - [0] : Can't Pierce (Melee/Throwable can't pierce)")]
    [Range(0f, 5f)]
    public int PiercingLevel = 0;

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Weapon Speed")]

    [Tooltip("Attacks Per Second")]
    public float FireRate = 5f;

    public FireType fireType;
    public enum FireType
    {
        SingleFire,// Fires once before cooldown (FireRate), needs to be clicked every time and cant be held down
        BurstFire,// Fires several shots consecutively (BurstFireRate), before needing to use regular fire rate, can be held down
        Shotgun,// Uses spread cone as a hitbox and hits anything in it
        Automatic// Fires repeatedly when fire button held down
    }
    public ProjectileType projectileType;
    public enum ProjectileType
    {
        Hitscan,// Uses raycasts
        Projectile// instantiates objects, use throw logic for projectiles
    }
    [Space]
    [Tooltip("Burst fire only: time between each bullet - not each attack")]
    public float BurstFireRate = 0.2f;
    [Tooltip("Burst fire only: Amount of bullets in burst fire")]
    public int BurstFireAmount = 5;

    public int ShotgunPellets = 8;

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Ammo")]
    [Header("<size=90%>Ammo is not applicable to Melee weapons")]

    [Tooltip("Maxiumum amount of ammo in weapon")]
    public int MaxAmmo = 18;

    [Tooltip("Maxiumum amount of magazines player can hold for this weapon")]// Magazines will restore ammo to full
    public int MaxMagazines = 10;

    [Tooltip("Seconds taken to reload weapon")]
    public float ReloadTime = 1f;

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Melee")]
    [Tooltip("Size of the melee attacks hitbox")]
    public Vector3 MeleeAttackSize = new Vector3(3f, 3f, 3f);

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Thrown Attacks")]
    [Header("<size=90%>NOTE: All thrown objects need a RigidBody on their model")]
    [Tooltip("Whether the weapon will use gravity or not")]
    public bool UseGravity = true;
    [Tooltip("Determines how much gravity effects the object, and how hard it hits objects with a RigidBody")]
    public float WeaponMass = 1f;
    [Tooltip("Determines how much forward force is applied to the thrown object")]
    public float ThrowForwardForce = 10f;
    [Tooltip("Determines how much up force is applied to the thrown object")]
    public float ThrowUpForce = 3f;

    public bool HasEffectOnCollide = false;
    public GameObject ProjectileEffect;

    // // // // // // // // // // // // // // // // // // // // //

    [Header("Effectivity")]
    public EffectiveTarget PreferredTarget;
    public enum EffectiveTarget
    {
        Vampire,// More Effective against Vampires + can have additional effects
        Humans,// More Effective against Humans + can have additional effects
        All// does not have differences between vampires and humans
    }

    public float VampireMultiplier = 1f;
    public float HumanMultiplier = 1f;
}
