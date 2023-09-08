using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// TPS camera for <see cref="PlayerMovement"/>.
    /// <br>Note : This script is loosely based of <see cref="PivotRotatingCamera"/>.</br>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlayerTPSCamera : MonoBehaviour
    {
        [Header("Camera Reference")]
        public Transform playerTransform;
        private Vector3 m_followTargetPos;
        public BehaviourUpdateMode lookUpdateMode = BehaviourUpdateMode.Update;
        public BehaviourUpdateMode moveUpdateMode = BehaviourUpdateMode.FixedUpdate;

        [Header("Camera Settings")] // Input settings
        [SerializeField] private bool m_UseRawInputLook = true;
        public bool UseRawInputLook { get => m_UseRawInputLook; set => m_UseRawInputLook = value; }
        [SerializeField] private float m_LookSensitivity = 100f;
        public float LookSensitivity { get => m_LookSensitivity; set => m_LookSensitivity = value; }
        [Space]

        [Tooltip("Camera rotation axes. X only makes horizontal rotate and Y does vertical.")]
        public MouseInputAxis currentAxes = MouseInputAxis.MouseX | MouseInputAxis.MouseY;
        public MouseInputAxis invertAxes;
        [Tooltip("Camera offset, for the player follow.")]
        public Vector3 playerTransformPositionOffset;
        [Tooltip("Distance between the 'PlayerTransform' and the camera.")]
        public float distanceFromTarget = 4f;
        [Tooltip("Follow dampening. Set to 0 or lower for no smooth follow.")]
        public float followDamp = 4f;
        [Tooltip("Camera clamp for vertical looking.")]
        [Clamp(-89.9f, 89.9f)] public MinMaxValue lookVerticalAngleRange = MinMaxValue.Zero;

        private void Awake()
        {
            m_followTargetPos = playerTransform.position;
        }

        private void Update()
        {
            if (lookUpdateMode != BehaviourUpdateMode.FixedUpdate)
                CameraLookUpdate(Time.deltaTime);

            if (moveUpdateMode != BehaviourUpdateMode.FixedUpdate)
                CameraMoveUpdate(Time.deltaTime);
        }
        private void FixedUpdate()
        {
            if (lookUpdateMode == BehaviourUpdateMode.FixedUpdate)
                CameraLookUpdate(Time.fixedDeltaTime);

            if (moveUpdateMode == BehaviourUpdateMode.FixedUpdate)
                CameraMoveUpdate(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Update camera position into required rotation + position
        /// </summary>
        private void CameraLookUpdate(float deltaTime)
        {
            // Place camera into the same position as 'fake origin that follows PlayerTransform'
            transform.position = m_followTargetPos;

            // Rotate using transform.Rotate
            float xAxis = m_UseRawInputLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X");
            float yAxis = m_UseRawInputLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y");
            xAxis *= m_LookSensitivity * deltaTime;
            yAxis *= m_LookSensitivity * deltaTime;

            if ((invertAxes & MouseInputAxis.MouseX) == MouseInputAxis.MouseX)
                xAxis *= -1;
            if ((invertAxes & MouseInputAxis.MouseY) == MouseInputAxis.MouseY)
                yAxis *= -1;

            // Rotating
            if ((currentAxes & MouseInputAxis.MouseX) == MouseInputAxis.MouseX)
                transform.Rotate(Vector3.up, -xAxis, Space.World);
            if ((currentAxes & MouseInputAxis.MouseY) == MouseInputAxis.MouseY)
                transform.Rotate(Vector3.right, yAxis);

            transform.Translate(0f, 0f, -distanceFromTarget, Space.Self);
        }

        private void LateUpdate()
        {
            // Do clamping here because the 'transform.Rotate' is not very fond of doing rotate after setting transform.rotation
            if (lookVerticalAngleRange != MinMaxValue.Zero)
            {
                // Get Rotation to apply
                Vector3 currentRotationEuler = Additionals.EditorEulerRotation(transform.eulerAngles);
                // Clamp vertical look
                currentRotationEuler.x = lookVerticalAngleRange.ClampBetween(currentRotationEuler.x);
                currentRotationEuler.z = 0f;
                // Apply clamped Rotation
                transform.localRotation = Quaternion.Euler(currentRotationEuler);
            }
        }

        /// <summary>
        /// Updates the follow position of the camera.
        /// </summary>
        private void CameraMoveUpdate(float deltaTime)
        {
            // Offseted position
            Vector3 offsetPos = playerTransform.position + (transform.TransformPoint(playerTransformPositionOffset) - transform.position);
            // Lerp origin position
            m_followTargetPos = followDamp >= 0f ? Vector3.Lerp(
                m_followTargetPos,
                offsetPos,
                followDamp * deltaTime
            ) : offsetPos;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (playerTransform != null && lookVerticalAngleRange != MinMaxValue.Zero)
            {
                Vector3 lookDir = (transform.position - playerTransform.position).normalized;
                if (lookDir == Vector3.zero)
                {
                    lookDir = Vector3.forward;
                }
                lookDir.y = 0f;

                // Look towards inverted target transform
                Quaternion centerRotation = Quaternion.LookRotation(lookDir, Vector3.up) * Quaternion.AngleAxis(-90f, Vector3.up);
                // Move rotation to be centered
                centerRotation *= Quaternion.AngleAxis((lookVerticalAngleRange.Min + lookVerticalAngleRange.Max) / 2f, Vector3.forward);

                GizmoUtility.DrawArc(playerTransform.position, centerRotation, distanceFromTarget, lookVerticalAngleRange.Size());
            }
        }
#endif
    }
}