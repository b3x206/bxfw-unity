﻿using UnityEngine;

namespace BXFW
{
    public class PlayerFPSCamera : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private Transform PlayerTransform = null;

        [Header("Camera Settings")]
        /// Note about RawMouseLook : Keep it true as default, otherwise mouse movement is jerky.
        [SerializeField] private bool RawMouseLook = true;
        public bool SensitivityMouseRawInput { get => RawMouseLook; set => RawMouseLook = value; }

        [SerializeField] private float SensitivityMouse = 100f;
        public float SensitivityMouseCamera { get => SensitivityMouse; set => SensitivityMouse = value; }

        [Header("Camera Constraints")]
        private float xRotation = 0f;
        [Tooltip("The limit is splitted to 2 before acting as input")]
        [SerializeField] private float HeadRotationLimit = 180f;
        public CameraRotationAxes CurrentAxes = CameraRotationAxes.MouseX | CameraRotationAxes.MouseY;

        private void Update()
        {
            if (CurrentAxes != CameraRotationAxes.None)
            {
                CameraLookUpdate();
            }
        }

        private void CameraLookUpdate()
        {
            //Mouse input calc
            float mouseX = (RawMouseLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X")) * SensitivityMouse * Time.smoothDeltaTime;
            float mouseY = (RawMouseLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y")) * SensitivityMouse * Time.smoothDeltaTime;

            // Mouse up looking
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -HeadRotationLimit / 2f, HeadRotationLimit / 2f);

            // Rotate player with camera
            switch (CurrentAxes)
            {
                case CameraRotationAxes.MouseX | CameraRotationAxes.MouseY:
                    // You can probably use euler function method as well, it's unity being unity.
                    transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
                    PlayerTransform.Rotate(Vector3.up * mouseX);
                    break;

                case CameraRotationAxes.MouseX:
                    PlayerTransform.Rotate(Vector3.up * mouseX);
                    break;
                case CameraRotationAxes.MouseY:
                    transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
                    break;

                case CameraRotationAxes.None:
                default:
                    break;
            }
        }
    }
}