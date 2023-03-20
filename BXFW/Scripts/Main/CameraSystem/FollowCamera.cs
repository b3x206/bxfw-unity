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
        [Range(.05f, 50f)] public float Rotation_Damp = 2f;
        [Range(.05f, 50f)] public float Move_Damp = 2f;

        /// <summary>
        /// An offset for the camera following.
        /// </summary>
        [System.Serializable]
        public struct CameraOffset
        {
            // -- Position
            public Vector3 Position;
            public Vector3 EulerRotation;

            // -- Clamp
            public bool UseCameraPosClamp;
            public Vector2 CameraPosXClamp;
            public Vector2 CameraPosYClamp;
            public Vector2 CameraPosZClamp;

            public override string ToString() { return string.Format("[CameraOffset] Pos={0}, Rot={1}", Position, EulerRotation); }
        }
        [Header("State Positions Camera")]
        public CameraOffset[] CameraOffsetTargets = new CameraOffset[1];
        [SerializeField, HideInInspector] private int _CurrentCameraOffsetIndex = 0;
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
        private Camera _camComponent;
        public Camera CameraComponent
        {
            get
            {
                if (_camComponent == null)
                    _camComponent = GetComponent<Camera>();

                return _camComponent;
            }
        }

        #region Camera Mechanics
        /// <summary>
        /// The camera movement method.
        /// <br>Always call this method when you override it.</br>
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (!CanFollow)
                return;

            var followPos = (FollowTransform == null || UseFollowVecInstead) ? FollowVector3 : FollowTransform.position;
            var lerpPos = CurrentCameraOffset.UseCameraPosClamp ? new Vector3(
                Mathf.Clamp(followPos.x + CurrentCameraOffset.Position.x, CurrentCameraOffset.CameraPosXClamp.x, CurrentCameraOffset.CameraPosXClamp.y),
                Mathf.Clamp(followPos.y + CurrentCameraOffset.Position.y, CurrentCameraOffset.CameraPosYClamp.x, CurrentCameraOffset.CameraPosYClamp.y),
                Mathf.Clamp(followPos.z + CurrentCameraOffset.Position.z, CurrentCameraOffset.CameraPosZClamp.x, CurrentCameraOffset.CameraPosZClamp.y))
                : new Vector3(followPos.x + CurrentCameraOffset.Position.x,
                        followPos.y + CurrentCameraOffset.Position.y,
                        followPos.z + CurrentCameraOffset.Position.z);
            var rotatePos = Quaternion.Euler(CurrentCameraOffset.EulerRotation.x,
                        CurrentCameraOffset.EulerRotation.y,
                        CurrentCameraOffset.EulerRotation.z);

            transform.SetPositionAndRotation(
                // Position
                Vector3.Lerp(transform.position, lerpPos, Time.fixedDeltaTime * Move_Damp),
                // Rotation
                Quaternion.Slerp(transform.rotation, rotatePos, Time.fixedDeltaTime * Rotation_Damp)
            );
        }
        #endregion

#if UNITY_EDITOR
        private static Color[] CacheColor;
        // Generate persistent unique colors. (dumb method, we should use the editor script instead).
        private static Color GetRandColor()
        {
            return new Color(
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f),
                Random.Range(0.5f, 1f));
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
                    CacheColor[i] = GetRandColor();
                }
            }
            else if (CacheColor.Length != CameraOffsetTargets.Length + 1)
            {
                CacheColor = new Color[CameraOffsetTargets.Length + 1];

                for (int i = 0; i < CacheColor.Length; i++)
                {
                    CacheColor[i] = GetRandColor();
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