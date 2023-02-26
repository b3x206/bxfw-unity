using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// An fps camera, that rotates the target transform horizontally and rotates itself vertically.
    /// </summary>
    public class PlayerFPSCamera : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private Transform playerTransform;

        [Header("Camera Settings")]
        /// Note about RawMouseLook : Keep it true as default, otherwise mouse movement is jerky.
        [SerializeField] private bool rawMouseLook = true;
        public bool SensitivityMouseRawInput { get => rawMouseLook; set => rawMouseLook = value; }

        [SerializeField] private float sensitivityMouse = 100f;
        public float SensitivityMouseCamera { get => sensitivityMouse; set => sensitivityMouse = value; }

        [Header("Camera Constraints")]
        private float xRotation = 0f;
        [Tooltip("The limit is splitted to 2 before acting as input")]
        [SerializeField] private float headRotationLimit = 180f;
        public InputAxis currentAxes = InputAxis.MouseX | InputAxis.MouseY;

        private void Update()
        {
            if (currentAxes != InputAxis.None)
            {
                CameraLookUpdate();
            }
        }

        private void CameraLookUpdate()
        {
            // Mouse input calc
            float mouseX = (rawMouseLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X")) * sensitivityMouse * Time.smoothDeltaTime;
            float mouseY = (rawMouseLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y")) * sensitivityMouse * Time.smoothDeltaTime;

            // Mouse up looking
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -headRotationLimit / 2f, headRotationLimit / 2f);

            // Rotate player with camera
            switch (currentAxes)
            {
                case InputAxis.MouseX | InputAxis.MouseY:
                    // You can probably use euler function method as well, it's unity being unity.
                    transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
                    playerTransform.Rotate(Vector3.up * mouseX);
                    break;

                case InputAxis.MouseX:
                    playerTransform.Rotate(Vector3.up * mouseX);
                    break;
                case InputAxis.MouseY:
                    transform.localRotation = Quaternion.AngleAxis(xRotation, Vector3.right);
                    break;

                case InputAxis.None:
                default:
                    break;
            }
        }
    }
}