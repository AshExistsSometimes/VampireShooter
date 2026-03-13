using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInterface : MonoBehaviour
{
    public static UserInterface Instance;

    [Header("Ammo")]
    public TextMeshProUGUI AmmoText;

    private void Awake()
    {
        UserInterface.Instance = this;
    }

    public void UpdateBulletCounter(int ammoCount, int maxAmmo)
    {
        AmmoText.text = ammoCount + " / " + maxAmmo;    
    }
}
