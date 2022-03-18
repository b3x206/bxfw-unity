using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Following camera.
    /// <br>Tracks the <see cref="FollowTransform"/> smoothly.</br>
    /// </summary>
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
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
        /// <summary>The <see cref="UnityEngine.Events.UnityEvent{T0}"/> based setter.</summary>
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

        #region Camera Shake
        /// <summary>
        /// Externally applied movement.
        /// </summary>
        private Vector3 externLerpPosMove;
        private Quaternion externLerpRotateMove;
        /// Current shake duration (total duration for easing in-out), set by <see cref="ShakeCamera(Vector2, float)"/>.
        private float currentShakeDuration;
        /// Current shake elapsed, set by <see cref="ShakeCamTick"/>.
        private float currentShakeElapsed;
        /// Current shake magnitude, set by <see cref="ShakeCamera(Vector2, float)"/>.
        private Vector2 currentShakeMagnitude;
        /// Frames 'Fixed frames' after a random variable is generated.
        private int currentTotalShakeGraceFrames = 1;
        private int currentShakeGraceFrames = 1;
        private Vector2 currentRandomVector = Vector2.zero;
        private bool currentShakeRotation = false;
        /// <summary>
        /// Ticks the <see cref="ShakeCamera(Vector2, float)"/> method.
        /// </summary>
        private void ShakeCamTick()
        {
            if (currentShakeElapsed > 0f)
            {
                currentShakeElapsed -= Time.fixedDeltaTime / currentShakeDuration;
            }
            else
            {
                externLerpPosMove = Vector3.zero;
                externLerpRotateMove = Quaternion.identity;
                return;
            }

            // Doesn't have to be converted to local x/y, for some reason rotation doesn't modify global x/y ?
            currentShakeGraceFrames--;
            if (currentShakeGraceFrames <= 0)
            {
                currentRandomVector = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                currentShakeGraceFrames = currentTotalShakeGraceFrames;
            }

            externLerpPosMove = transform.InverseTransformPoint(Vector3.Scale(Vector2.Lerp(currentRandomVector, Vector2.zero, currentShakeElapsed), currentShakeMagnitude) + transform.position);
            
            if (currentShakeRotation)
            {
                externLerpRotateMove = Quaternion.LookRotation(externLerpPosMove + Vector3.forward, transform.up);
            }
            else
            {
                externLerpRotateMove = Quaternion.identity;
            }
        }
        /// <summary>
        /// Shakes the camera on the local x/y position.
        /// </summary>
        public void ShakeCamera(Vector2 shakeMagnitude, float duration, bool shakeRotation = false, int shakeGraceFrames = 3)
        {
            if (shakeMagnitude == Vector2.zero || Mathf.Approximately(0f, duration))
                return; // Do nothing if we are requested to do nothing.
                        // NOTE : The modular 'CameraShake' component (TODO) will wait for duration to set 'IsShaking'.

            currentShakeRotation = shakeRotation;
            currentShakeGraceFrames = currentTotalShakeGraceFrames = Mathf.Clamp(shakeGraceFrames, 0, 16);
            currentShakeElapsed = 1f;
            currentShakeDuration = duration;
            currentShakeMagnitude = shakeMagnitude;
        }
        #endregion

        // ** Follow Target Transform
        // FIX : FixedUpdate fixes movement jitter.
        /// <summary>
        /// The camera movement method.
        /// <br>Always call this method when you override it.</br>
        /// </summary>
        protected virtual void FixedUpdate()
        {
            ShakeCamTick();

            var followPos = (FollowTransform == null || UseFollowVecInstead) ? FollowVector3 : FollowTransform.position;
            var lerpPos = CurrentCameraOffset.UseCameraPosClamp ? new Vector3(
                Mathf.Clamp(followPos.x + CurrentCameraOffset.Position.x, CurrentCameraOffset.CameraPosXClamp.x, CurrentCameraOffset.CameraPosXClamp.y),
                Mathf.Clamp(followPos.y + CurrentCameraOffset.Position.y, CurrentCameraOffset.CameraPosYClamp.x, CurrentCameraOffset.CameraPosYClamp.y),
                Mathf.Clamp(followPos.z + CurrentCameraOffset.Position.z, CurrentCameraOffset.CameraPosZClamp.x, CurrentCameraOffset.CameraPosZClamp.y))
                : new Vector3(followPos.x + CurrentCameraOffset.Position.x,
                        followPos.y + CurrentCameraOffset.Position.y,
                        followPos.z + CurrentCameraOffset.Position.z)
                + externLerpPosMove;
            var rotatePos = Quaternion.Euler(CurrentCameraOffset.EulerRotation.x,
                        CurrentCameraOffset.EulerRotation.y,
                        CurrentCameraOffset.EulerRotation.z)
                * externLerpRotateMove;

            if (CanFollow)
            {
                transform.SetPositionAndRotation(
                    // Position
                    Vector3.Lerp(transform.position, lerpPos, Time.fixedDeltaTime * Move_Damp),
                    // Rotation
                    Quaternion.Slerp(transform.rotation, rotatePos, Time.fixedDeltaTime * Rotation_Damp)
                );
            }

            OnFixedUpdate();
        }
        protected virtual void OnFixedUpdate() { }
        #endregion

#if UNITY_EDITOR
        private static Color[] CacheColor;
        /// <summary>
        /// Draw gizmos on selection. This draws the camera positions in <see cref="CameraOffsetTargets"/>.
        /// <br>Always call this method when you override it.</br>
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            // Generate persistent unique colors. (dumb method, we should use the editor script instead).
            static Color GetRandColor()
            {
                return new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f));
            }
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