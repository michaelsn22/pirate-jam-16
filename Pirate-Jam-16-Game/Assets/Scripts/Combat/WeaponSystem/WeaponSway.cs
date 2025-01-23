using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponSway : MonoBehaviour
{
    [SerializeField] private Transform weaponTransform;

    [SerializeField] private PlayerMovement playerController;

    [Header("Sway Properties")]
    [SerializeField] private float swayAmount = 0.01f;
    [SerializeField] public float maxSwayAmount = 0.1f;
    [SerializeField] public float swaySmooth = 9f;
    [SerializeField] public AnimationCurve swayCurve;

    [Range(0f, 1f)]
    [SerializeField] public float swaySmoothCounteraction = 1f;

    [Header("Rotation")]
    [SerializeField] public float rotationSwayMultiplier = 1f;

    [Header("Position")]
    [SerializeField] public float positionSwayMultiplier = -1f;

    [Header("Weapon Bob Properties")]
    [SerializeField] private float bobFrequency = 1.0f;
    [SerializeField] private float bobAmount = 0.1f;

    



    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector2 sway;

    private void Reset()
    {
        Keyframe[] ks = new Keyframe[] { new Keyframe(0, 0, 0, 2), new Keyframe(1, 1) };
        swayCurve = new AnimationCurve(ks);
    }

    private void Start()
    {
        if (!weaponTransform)
            weaponTransform = transform;
        initialPosition = weaponTransform.localPosition;
        initialRotation = weaponTransform.localRotation;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        sway = Vector2.MoveTowards(sway, Vector2.zero, swayCurve.Evaluate(Time.deltaTime * swaySmoothCounteraction * sway.magnitude * swaySmooth));
        sway = Vector2.ClampMagnitude(new Vector2(mouseX, mouseY) + sway, maxSwayAmount);

        float timeFactor = Time.deltaTime * swaySmooth;

        // Smoothly transition between weapon sway and view bobbing
        Vector3 swayPosition = new Vector3(sway.x, sway.y, 0) * positionSwayMultiplier + initialPosition;
        weaponTransform.localPosition = Vector3.Lerp(weaponTransform.localPosition, swayPosition, swayCurve.Evaluate(timeFactor));

        Quaternion swayRotation = initialRotation * Quaternion.Euler(Mathf.Rad2Deg * rotationSwayMultiplier * new Vector3(-sway.y, sway.x, 0));
        weaponTransform.localRotation = Quaternion.Slerp(weaponTransform.localRotation, swayRotation, swayCurve.Evaluate(timeFactor));
    }


}