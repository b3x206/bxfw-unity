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
        public Transform playerTransform;
        private Vector3 followTargetPos;

        [Header("Camera Settings")] // Input settings
        [SerializeField] private bool rawMouseLook = true;
        public bool SensitivityMouseRawInput { get => rawMouseLook; set => rawMouseLook = value; }
        [SerializeField] private float sensitivityMouse = 100f;
        public float SensitivityMouseCamera { get => sensitivityMouse; set => sensitivityMouse = value; }
        [Space] // In-game settings
        [Tooltip("Uses FixedUpdate() tick method to update the camera position.\nUseful when following objects moving in FixedUpdate().")]
        public bool moveInFixedUpdate = true;
        [Tooltip("Uses FixedUpdate() tick method to update the camera rotation.\nUseful when the game is locked / has lower than FixedUpdate() fps.")]
        public bool lookInFixedUpdate = false;

        [Tooltip("Camera rotation axes. X only makes horizontal rotate and Y does vertical.")]
        public InputAxis currentAxes = InputAxis.MouseX | InputAxis.MouseY;
        public InputAxis invertAxes;
        [Tooltip("Camera offset, for the player follow.")]
        public Vector3 playerTransformPositionOffset;
        [Tooltip("Distance between the 'PlayerTransform' and the camera.")]
        public float distanceFromTarget = 4f;
        [Tooltip("Follow dampening. Set to 0 or lower for no smooth follow.")]
        public float followDamp = 4f;
        [Tooltip("Camera clamp for vertical looking.")]
        public Vector2 lookVerticalAngleClamp = Vector2.zero;

        private void Awake()
        {
            followTargetPos = playerTransform.position;
        }

        private void Update()
        {
            if (!lookInFixedUpdate)
                CameraLookUpdate(Time.deltaTime);

            if (!moveInFixedUpdate)
                CameraMoveUpdate(Time.deltaTime);
        }
        private void FixedUpdate()
        {
            if (lookInFixedUpdate)
                CameraLookUpdate(Time.fixedDeltaTime);

            if (moveInFixedUpdate)
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
            float xAxis = rawMouseLook ? Input.GetAxisRaw("Mouse X") : Input.GetAxis("Mouse X");
            float yAxis = rawMouseLook ? Input.GetAxisRaw("Mouse Y") : Input.GetAxis("Mouse Y");
            xAxis *= sensitivityMouse * deltaTime;
            yAxis *= sensitivityMouse * deltaTime;

            if ((invertAxes & InputAxis.MouseX) == InputAxis.MouseX)
                xAxis *= -1;
            if ((invertAxes & InputAxis.MouseY) == InputAxis.MouseY)
                yAxis *= -1;

            // Clamping
            if (lookVerticalAngleClamp != Vector2.zero)
            {
                // Get Rotation to apply
                Vector3 CurrentRotationEuler = Additionals.FixEulerRotation(transform.eulerAngles);
                // Clamp vertical look
                CurrentRotationEuler.x = Mathf.Clamp(CurrentRotationEuler.x, lookVerticalAngleClamp.x, lookVerticalAngleClamp.y);
                CurrentRotationEuler.z = 0f;
                // Apply clamped Rotation
                transform.localRotation = Quaternion.Euler(CurrentRotationEuler);
            }

            // Rotating
            if ((currentAxes & InputAxis.MouseX) == InputAxis.MouseX)
                transform.Rotate(Vector3.up, -xAxis, Space.World);
            if ((currentAxes & InputAxis.MouseY) == InputAxis.MouseY)
                transform.Rotate(Vector3.right, yAxis);

            transform.Translate(0f, 0f, -distanceFromTarget, Space.Self);
        }
        
        /// <summary>
        /// Updates the follow position of the camera.
        /// </summary>
        private void CameraMoveUpdate(float deltaTime)
        {
            // Offseted position
            Vector3 offsetPos = playerTransform.position + (transform.TransformPoint(playerTransformPositionOffset) - transform.position);
            // Lerp origin position
            followTargetPos = followDamp >= 0f ? Vector3.Lerp(followTargetPos,
                offsetPos,
                followDamp * deltaTime) : offsetPos;
        }
    }
}