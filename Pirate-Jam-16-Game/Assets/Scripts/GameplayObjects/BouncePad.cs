using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float m_BounceForce = 20f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement characterController = other.GetComponent<PlayerMovement>();
            if (characterController != null)
            {
                characterController.m_PlayerVelocity.y = m_BounceForce;
            }
        }
    }
}
