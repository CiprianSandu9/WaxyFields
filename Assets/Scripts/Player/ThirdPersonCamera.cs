using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public PlayerController player;
    public Vector3 cameraOffset;
    public float MinPitch = 4f;
    public float MaxPitch = 4f;
    public float Sensitivity = .8f;


    float pitch, yaw;
    float distToPlayer = 9f;

    private PlayerController playerController;


    // Update is called once per frame
    void LateUpdate()
    {
        pitch -= player._lookInput.y * Sensitivity;
        yaw += player._lookInput.x * Sensitivity;

        // Clamp pitch
        pitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
        yaw = yaw % 360f;

        Quaternion camRot = Quaternion.Euler(pitch, yaw, 0f);

        transform.rotation = camRot;
        transform.position = player.transform.position - camRot * cameraOffset;


        var magnitude = player.rb.linearVelocity.magnitude;
        var p = Mathf.InverseLerp(0, 10, magnitude);
        var targetFov = Mathf.Lerp(85, 88, p);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFov, Time.deltaTime * 8f); ;
    }
}
