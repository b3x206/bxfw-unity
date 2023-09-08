using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Following camera.
    /// <br>Tracks the <see cref="FollowTransform"/> smoothly.</br>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        // ** Variables (Inspector)
        [Header("Camera Fading Object Settings")]
        public bool CanFollow = true;
        public BehaviourUpdateMode UpdateMode = BehaviourUpdateMode.FixedUpdate;

        [Header("Camera Follow Settings")]
        public bool UseFollowVecInstead = false;
        public Vector3 FollowVector3 = Vector3.zero;
        public Transform FollowTransform;

        private CameraOffset CurrentCameraOffset
        {
            get
            {
                return CameraOffsetTargets[CurrentCameraOffsetIndex];
            }
        }
        [Range(.05f, 50f)] public float RotationDamp = 2f;
        [Range(.05f, 50f)] public float MoveDamp = 2f;

        /// <summary>
        /// An offset for the camera following.
        /// <br>Can also clamp the camera position.</br>
        /// </summary>
        [System.Serializable]
        public struct CameraOffset
        {
            // -- Position
            public Vector3 Position;
            public Vector3 EulerRotation;

            // -- Clamp
            public bool UseCameraPosClamp;
            public MinMaxValue CameraPosXClamp;
            public MinMaxValue CameraPosYClamp;
            public MinMaxValue CameraPosZClamp;

            public override string ToString() { return string.Format("[CameraOffset] Pos={0}, Rot={1}", Position, EulerRotation); }
        }
        [Header("State Positions Camera")]
        public CameraOffset[] CameraOffsetTargets = new CameraOffset[1];
        [SerializeField, HideInInspector] private int _CurrentCameraOffsetIndex = 0;
        /// <summary>
        /// The <see cref="CameraOffsetTargets"/> that is being used by this FollowCamera.
        /// <br>Index is clamped for this value.</br>
        /// </summary>
        public int CurrentCameraOffsetIndex
        {
            get { return _CurrentCameraOffsetIndex; }
            set { _CurrentCameraOffsetIndex = Mathf.Clamp(value, 0, CameraOffsetTargets.Length - 1); }
        }
        /// <summary>
        /// The <see cref="UnityEngine.Events.UnityEvent{T0}"/> setter.
        /// </summary>
        public void SetCurrentCameraOffsetIndex(int Offset)
        { CurrentCameraOffsetIndex = Offset; }

        // ** Variables (Hidden)
        private Camera m_CamComponent;
        /// <summary>
        /// Camera component attached to this 'FollowCamera'.
        /// </summary>
        public Camera CameraComponent
        {
            get
            {
                if (m_CamComponent == null)
                    m_CamComponent = GetComponent<Camera>();

                return m_CamComponent;
            }
        }

        #region Camera Mechanics
        /// <summary>
        /// Called on the selected update mode when the camera is going to be moved.
        /// <br>Respects the <see cref="CanFollow"/> setting as it's being called by an update type.</br>
        /// <br>To not respect the <see cref="CanFollow"/>, use the Update, FixedUpdate or LateUpdate overrides.</br>
        /// </summary>
        protected virtual void MoveCamera(float deltaTime)
        {
            // Get Position
            var followPos = (FollowTransform == null || UseFollowVecInstead) ? FollowVector3 : FollowTransform.position;
            var lerpPos = CurrentCameraOffset.UseCameraPosClamp ? new Vector3(
                CurrentCameraOffset.CameraPosXClamp.ClampBetween(followPos.x + CurrentCameraOffset.Position.x),
                CurrentCameraOffset.CameraPosYClamp.ClampBetween(followPos.y + CurrentCameraOffset.Position.y),
                CurrentCameraOffset.CameraPosZClamp.ClampBetween(followPos.z + CurrentCameraOffset.Position.z)
            ) : new Vector3(
                followPos.x + CurrentCameraOffset.Position.x,
                followPos.y + CurrentCameraOffset.Position.y,
                followPos.z + CurrentCameraOffset.Position.z
            );
            // Get Rotation
            var rotatePos = Quaternion.Euler(
                CurrentCameraOffset.EulerRotation.x,
                CurrentCameraOffset.EulerRotation.y,
                CurrentCameraOffset.EulerRotation.z
            );
            // Apply (with interpolation, using Mathf.MoveTowards doesn't work nice and smooth here)
            transform.SetPositionAndRotation(
                // Position
                Vector3.Lerp(transform.position, lerpPos, deltaTime * MoveDamp),
                // Rotation
                Quaternion.Slerp(transform.rotation, rotatePos, deltaTime * RotationDamp)
            );
        }

        /// <summary>
        /// The camera Update method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="Update"/>) when you override it.</br>
        /// </summary>
        protected virtual void Update()
        {
            if (!CanFollow || UpdateMode != BehaviourUpdateMode.Update)
                return;

            MoveCamera(Time.deltaTime);
        }

        /// <summary>
        /// The camera FixedUpdate method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="FixedUpdate"/>) when you override it.</br>
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (!CanFollow || UpdateMode != BehaviourUpdateMode.FixedUpdate)
                return;

            MoveCamera(Time.fixedDeltaTime);
        }

        /// <summary>
        /// The camera LateUpdate method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="LateUpdate"/>) when you override it.</br>
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!CanFollow || UpdateMode != BehaviourUpdateMode.LateUpdate)
                return;

            MoveCamera(Time.deltaTime);
        }
        #endregion

#if UNITY_EDITOR
        private static Color[] CacheColor;
        // Generate persistent unique colors. (dumb method, we should use the editor script instead).
        private static Color GetRandColor(float alpha = 1f)
        {
            return new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                alpha
            );
        }
        /// <summary>
        /// Draw gizmos on selection. This draws the camera positions in <see cref="CameraOffsetTargets"/>.
        /// <br>Always call this method when you override it.</br>
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (CacheColor == null)
            {
                CacheColor = new Color[CameraOffsetTargets.Length + 1];

                for (int i = 0; i < CacheColor.Length; i++)
                {
                    CacheColor[i] = GetRandColor(.6f);
                }
            }
            else if (CacheColor.Length != CameraOffsetTargets.Length + 1)
            {
                CacheColor = new Color[CameraOffsetTargets.Length + 1];

                for (int i = 0; i < CacheColor.Length; i++)
                {
                    CacheColor[i] = GetRandColor(.6f);
                }
            }

            // Draw spheres in camera offsets.
            var posOffset = FollowTransform == null ? FollowVector3 : FollowTransform.position;

            for (int i = 0; i < CameraOffsetTargets.Length; i++)
            {
                Gizmos.color = CacheColor[i];
                Gizmos.DrawSphere(CameraOffsetTargets[i].Position + posOffset, 1f);
            }

            Gizmos.color = CacheColor[CacheColor.Length - 1];
            Gizmos.DrawCube(FollowVector3, Vector3.one);
        }
#endif
    }
}