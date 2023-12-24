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
        // Note about RawMouseLook : Keep it true as default, otherwise mouse movement could be jerky.
        [SerializeField] private bool m_UseRawInputLook = true;
        public bool UseRawInputLook { get => m_UseRawInputLook; set => m_UseRawInputLook = value; }
        [SerializeField] private float m_LookSensitivity = 100f;
        public float LookSensitivity { get => m_LookSensitivity; set => m_LookSensitivity = value; }

        [Header("Camera Constraints")]
        public MinMaxValue headXRotationLimit = new MinMaxValue(-85f, 85f);
        public MouseInputAxis currentAxes = MouseInputAxis.MouseX | MouseInputAxis.MouseY;

        private float m_currentXRotation = 0f;
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

                m_currentXRotation = headXRotationLimit.ClampBetween(m_currentXRotation - mouseY);
                transform.localRotation = Quaternion.AngleAxis(m_currentXRotation, Vector3.right);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (headXRotationLimit != MinMaxValue.Zero)
            {
                Quaternion centerRotation = transform.rotation.AxisEulerQuaternion(TransformAxis.YAxis) * Quaternion.AngleAxis(-90f, Vector3.up);
                // Move rotation to be centered
                centerRotation *= Quaternion.AngleAxis((headXRotationLimit.Min + headXRotationLimit.Max) / 2f, Vector3.forward);

                GizmoUtility.DrawArc(transform.position, centerRotation, 1f, headXRotationLimit.Size());
            }
        }
#endif
    }
}
