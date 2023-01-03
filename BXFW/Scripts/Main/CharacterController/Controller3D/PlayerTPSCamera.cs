using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// TPS camera for <see cref="PlayerMovement"/>.
    /// <br>Note : This script is based of <see cref="PivotRotatingCamera"/>.</br>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlayerTPSCamera : MonoBehaviour
    {
        [Header("Camera Reference")]
        public Transform PlayerTransform;
        private Vector3 followTargetPos;

        [Header("Camera Settings")] // Input settings
        [SerializeField] private bool RawMouseLook = true;
        public bool SensitivityMouseRawInput { get => RawMouseLook; set => RawMouseLook = value; }
        [SerializeField] private float SensitivityMouse = 100f;
        public float SensitivityMouseCamera { get => SensitivityMouse; set => SensitivityMouse = value; }
        [Space] // In-game settings
        [Tooltip("Uses FixedUpdate() tick method to update the camera position.\nUseful when following objects moving in FixedUpdate().")]
        public bool MoveInFixedUpdate = true;
        [Tooltip("Uses FixedUpdate() tick method to update the camera rotation.\nUseful when the game is locked / has lower than FixedUpdate() fps.")]
        public bool LookInFixedUpdate = false;

        [Tooltip("Camera rotation axes. X only makes horizontal rotate and Y does vertical.")]
        public InputAxis CurrentAxes = InputAxis.MouseX | InputAxis.MouseY;
        public InputAxis InvertAxes;
        [Tooltip("Camera offset, for the player follow.")]
        public Vector3 PlayerTransformPositionOffset;
        [Tooltip("Distance between the 'PlayerTransform' and the camera.")]
        public float DistanceFromTarget = 4f;
        [Tooltip("Follow dampening. Set to 0 or lower for no smooth follow.")]
        public float FollowDamp = 2f;
        [Tooltip("Camera clamp for vertical looking.")]
        public Vector2 LookVerticalAngleClamp = Vector2.zero;

        private void Awake()
        {
            followTargetPos = PlayerTransform.position;
        }

        private void Update()
        {
            if (!LookInFixedUpdate)
                CameraLookUpdate(Time.deltaTime);

            if (!MoveInFixedUpdate)
                CameraMoveUpdate(Time.deltaTime);
        }
        private void FixedUpdate()
        {
            if (LookInFixedUpdate)
                CameraLookUpdate(Time.fixedDeltaTime);

            if (MoveInFixedUpdate)
                CameraMoveUpdate(Time.fixedDeltaTime);
        }

        /// <summary>
        /// Update camera position into required rotation + position
        /// </summary>
        private void CameraLookUpdate(float deltaTime)
        {
            // Place camera into the same position as 'fake origin that follows PlayerTransform'
            transform.position = followTargetPos;

            // Rotate using transform.Rotate
            float xAxis = RawMouseLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X");
            float yAxis = RawMouseLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y");
            xAxis *= SensitivityMouse * deltaTime;
            yAxis *= SensitivityMouse * deltaTime;

            if ((InvertAxes & InputAxis.MouseX) == InputAxis.MouseX)
                xAxis *= -1;
            if ((InvertAxes & InputAxis.MouseY) == InputAxis.MouseY)
                yAxis *= -1;

            // Clamping
            if (LookVerticalAngleClamp != Vector2.zero)
            {
                // Get Rotation to apply
                Vector3 CurrentRotationEuler = Additionals.FixEulerRotation(transform.eulerAngles);
                // Clamp vertical look
                CurrentRotationEuler.x = Mathf.Clamp(CurrentRotationEuler.x, LookVerticalAngleClamp.x, LookVerticalAngleClamp.y);
                CurrentRotationEuler.z = 0f;
                // Apply clamped Rotation
                transform.localRotation = Quaternion.Euler(CurrentRotationEuler);
            }

            // Rotating
            if ((CurrentAxes & InputAxis.MouseX) == InputAxis.MouseX)
                transform.Rotate(Vector3.up, -xAxis, Space.World);
            if ((CurrentAxes & InputAxis.MouseY) == InputAxis.MouseY)
                transform.Rotate(Vector3.right, yAxis);

            transform.Translate(0f, 0f, -DistanceFromTarget, Space.Self);
        }
        
        /// <summary>
        /// Updates the follow position of the camera.
        /// </summary>
        private void CameraMoveUpdate(float deltaTime)
        {
            // Offseted position
            Vector3 offsetPos = PlayerTransform.position + (transform.TransformPoint(PlayerTransformPositionOffset) - transform.position);
            // Lerp origin position
            followTargetPos = FollowDamp >= 0f ? Vector3.Lerp(followTargetPos,
                offsetPos,
                FollowDamp * deltaTime) : offsetPos;
        }
    }
}