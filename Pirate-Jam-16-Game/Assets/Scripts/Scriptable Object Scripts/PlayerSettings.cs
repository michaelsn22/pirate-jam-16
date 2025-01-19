using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MovementSettings
{
    public float MaxSpeed;
    public float Acceleration;
    public float Deceleration;

    public MovementSettings(float maxSpeed, float accel, float decel)
    {
        MaxSpeed = maxSpeed;
        Acceleration = accel;
        Deceleration = decel;
    }
}

[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Scriptable Object/Player Settings")]

public class PlayerSettings : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] public float m_Friction = 6;
    [SerializeField] public float m_Gravity = 20;
    [SerializeField] public float m_JumpForce = 8;
    [Tooltip("Automatically jump when holding jump button")]
    [SerializeField] public bool m_AutoBunnyHop = false;
    [Tooltip("How precise air control is")]
    [SerializeField] public float m_AirControl = 0.3f;
    [SerializeField] public MovementSettings m_GroundSettings = new MovementSettings(7, 14, 10);
    [SerializeField] public MovementSettings m_AirSettings = new MovementSettings(7, 2, 2);
    [SerializeField] public MovementSettings m_StrafeSettings = new MovementSettings(1, 50, 50);
}
