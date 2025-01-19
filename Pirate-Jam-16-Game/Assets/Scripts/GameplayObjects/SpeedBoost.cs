using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [SerializeField] private float m_SpeedForce = 20f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement characterController = other.GetComponent<PlayerMovement>();
            if (characterController != null)
            {
                characterController.m_PlayerSettings.m_GroundSettings.MaxSpeed += m_SpeedForce;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement characterController = other.GetComponent<PlayerMovement>();
            if (characterController != null)
            {
                characterController.m_PlayerSettings.m_GroundSettings.MaxSpeed -= m_SpeedForce;
            }
        }
    }
}
