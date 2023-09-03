using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// An fps camera, that rotates the target transform horizontally and rotates itself vertically.
    /// </summary>
    public class PlayerFPSCamera : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private Transform m_playerTransform;

        [Header("Camera Settings")]
        /// Note about RawMouseLook : Keep it true as default, otherwise mouse movement could be jerky.
        [SerializeField] private bool m_UseRawInputLook = true;
        public bool UseRawInputLook { get => m_UseRawInputLook; set => m_UseRawInputLook = value; }
        [SerializeField] private float m_LookSensitivity = 100f;
        public float LookSensitivity { get => m_LookSensitivity; set => m_LookSensitivity = value; }

        [Header("Camera Constraints")]
        [Tooltip("The limit is splitted to 2 before acting as input")]
        [SerializeField] private float m_headRotationLimit = 180f;
        public MouseInputAxis currentAxes = MouseInputAxis.MouseX | MouseInputAxis.MouseY;
        private float m_xRotation = 0f;

        private void Update()
        {
            if (currentAxes != MouseInputAxis.None)
            {
                CameraLookUpdate();
            }
        }

        private void CameraLookUpdate()
        {
            // - Mouse up looking
            if ((currentAxes & MouseInputAxis.MouseX) == MouseInputAxis.MouseX)
            {
                float mouseX = (m_UseRawInputLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X")) * m_LookSensitivity * Time.smoothDeltaTime;

                // Rotate with 'mouseX' delta
                m_playerTransform.Rotate(Vector3.up * mouseX);
            }
            if ((currentAxes & MouseInputAxis.MouseY) == MouseInputAxis.MouseY)
            {
                float mouseY = (m_UseRawInputLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y")) * m_LookSensitivity * Time.smoothDeltaTime;

                m_xRotation -= mouseY;
                m_xRotation = Mathf.Clamp(m_xRotation, -m_headRotationLimit / 2f, m_headRotationLimit / 2f);

                transform.localRotation = Quaternion.AngleAxis(m_xRotation, Vector3.right);
            }
        }
    }
}