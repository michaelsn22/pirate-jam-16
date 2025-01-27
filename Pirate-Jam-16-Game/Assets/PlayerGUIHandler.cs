using Hertzole.ScriptableValues;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerGUIHandler : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI ammoText, weaponText, healthText;

    [SerializeField] private ScriptableStringEvent onWeaponChanged;
    [SerializeField] private ScriptableStringEvent onAmmoChanged;
    [SerializeField] private ScriptableStringEvent onHealthChanged;

    private void Awake()
    {
        onWeaponChanged.OnInvoked += SetCurrentWeapon;
        onAmmoChanged.OnInvoked += SetCurrentAmmo;
        onHealthChanged.OnInvoked += SetCurrentHealth;
    }

    private void OnEnable()
    {
        onWeaponChanged.OnInvoked += SetCurrentWeapon;
        onAmmoChanged.OnInvoked += SetCurrentAmmo;
        onHealthChanged.OnInvoked += SetCurrentHealth;
    }
    private void OnDisable()
    {
        onWeaponChanged.OnInvoked -= SetCurrentWeapon;
        onAmmoChanged.OnInvoked -= SetCurrentAmmo;
        onHealthChanged.OnInvoked -= SetCurrentHealth;
    }

    private void SetCurrentAmmo(object sender, string e)
    {
        ammoText.SetText($"{e}");
    }

    private void SetCurrentWeapon(object sender, string e)
    {
        weaponText.SetText($"{e}");
    }
    private void SetCurrentHealth(object sender, string e)
    {
        healthText.SetText($"{e}");
    }



}