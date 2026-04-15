using UnityEngine;

/// <summary>
/// Handles ALL player combat input and sends commands to PlayerCombatManager
/// Keeps input separate from combat logic (clean architecture)
/// </summary>
public class PlayerCombatInput : MonoBehaviour
{
    public PlayerCombatManager combat;

    private void Update()
    {
        if (combat == null) return;

        HandleFireInput();
        HandleReloadInput();
        HandleWeaponSwitchInput();
    }

    /// <summary>
    /// Handles firing input (single + automatic)
    /// </summary>
    private void HandleFireInput()
    {
        // Left Click (single fire)
        if (Input.GetMouseButtonDown(0))
        {
            combat.UseWeapon();
        }

        // Hold fire (automatic)
        if (Input.GetMouseButton(0))
        {
            combat.HoldFire();
        }
    }

    /// <summary>
    /// Handles reload input
    /// </summary>
    private void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            combat.ReloadInput();
        }
    }

    /// <summary>
    /// Handles weapon switching (scroll + number keys)
    /// </summary>
    private void HandleWeaponSwitchInput()
    {
        // Scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            combat.SwitchWeaponScroll(1);
        }
        else if (scroll < 0f)
        {
            combat.SwitchWeaponScroll(-1);
        }

        // Number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
            combat.SwitchWeapon(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            combat.SwitchWeapon(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            combat.SwitchWeapon(2);
    }
}
