using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponControls : MonoBehaviour
{
    public static Action ShootingHeld;
    public static Action ShootingReleased;
    public static Action Reload;


    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            ShootingHeld?.Invoke();
            //Debug.Log("Shooting");
        }
            

        if (Input.GetMouseButtonUp(0))
        {
            ShootingReleased?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.R)){
            Reload?.Invoke();;
        }
        
    }
}
