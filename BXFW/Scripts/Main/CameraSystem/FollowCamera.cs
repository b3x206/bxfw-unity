using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Following camera.
    /// <br>Tracks the <see cref="followTransform"/> smoothly.</br>
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        /// <summary>
        /// An offset for the camera following.
        /// <br>Can also clamp the camera position.</br>
        /// </summary>
        [System.Serializable]
        public struct CameraOffset
        {
            // -- Position
            public Vector3 position;
            public Vector3 eulerRotation;

            // -- Clamp
            public bool usePositionClamp;
            public MinMaxValue posXClamp;
            public MinMaxValue posYClamp;
            public MinMaxValue posZClamp;

            public override string ToString() { return string.Format("[FollowCamera.CameraOffset] Pos={0}, Rot={1}", position, eulerRotation); }
        }

        // ** Variables (Inspector)
        [Header("Camera Settings")]
        public bool canFollow = true;
        public BehaviourUpdateMode updateMode = BehaviourUpdateMode.FixedUpdate;
        [Space(5)]
        public bool useFollowPositionInstead = false;
        public Vector3 followPosition = Vector3.zero;
        public Transform followTransform;
        [Range(.05f, 50f)] public float rotationDamp = 2f;
        [Range(.05f, 50f)] public float moveDamp = 2f;

        [Header("Offset Positions")]
        public CameraOffset[] cameraOffsetTargets = new CameraOffset[1];
        [SerializeField, HideInInspector] private int m_CurrentCameraOffsetIndex = 0;
        /// <summary>
        /// The <see cref="cameraOffsetTargets"/> that is being used by this FollowCamera.
        /// <br>Index is clamped for this value.</br>
        /// </summary>
        public int CurrentCameraOffsetIndex
        {
            get { return m_CurrentCameraOffsetIndex; }
            set { m_CurrentCameraOffsetIndex = Mathf.Clamp(value, 0, cameraOffsetTargets.Length - 1); }
        }
        /// <summary>
        /// Currently used camera offset.
        /// <br>To change this, use the <see cref="CurrentCameraOffsetIndex"/>.</br>
        /// </summary>
        public CameraOffset CurrentCameraOffset => cameraOffsetTargets[CurrentCameraOffsetIndex];

        /// <summary>
        /// The <see cref="UnityEngine.Events.UnityEvent{T0}"/> setter.
        /// </summary>
        public void SetCurrentCameraOffsetIndex(int offset)
        {
            CurrentCameraOffsetIndex = offset;
        }

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
                {
                    m_CamComponent = GetComponent<Camera>();
                }

                return m_CamComponent;
            }
        }

        #region Camera Mechanics
        /// <summary>
        /// Called on the selected update mode when the camera is going to be moved.
        /// <br>Respects the <see cref="canFollow"/> setting as it's being called by an update type.</br>
        /// <br>To not respect the <see cref="canFollow"/>, use the Update, FixedUpdate or LateUpdate overrides.</br>
        /// </summary>
        protected virtual void MoveCamera(float deltaTime)
        {
            // Get Position
            Vector3 followPos = (followTransform == null || useFollowPositionInstead) ? followPosition : followTransform.position;
            Vector3 lerpPos = CurrentCameraOffset.usePositionClamp ? new Vector3(
                CurrentCameraOffset.posXClamp.ClampBetween(followPos.x + CurrentCameraOffset.position.x),
                CurrentCameraOffset.posYClamp.ClampBetween(followPos.y + CurrentCameraOffset.position.y),
                CurrentCameraOffset.posZClamp.ClampBetween(followPos.z + CurrentCameraOffset.position.z)
            ) : new Vector3(
                followPos.x + CurrentCameraOffset.position.x,
                followPos.y + CurrentCameraOffset.position.y,
                followPos.z + CurrentCameraOffset.position.z
            );
            // Get Rotation
            Quaternion rotatePos = Quaternion.Euler(
                CurrentCameraOffset.eulerRotation.x,
                CurrentCameraOffset.eulerRotation.y,
                CurrentCameraOffset.eulerRotation.z
            );
            // Apply (with interpolation, using Mathf.MoveTowards doesn't work nice and smooth here)
            transform.SetPositionAndRotation(
                // Position
                Vector3.Lerp(transform.position, lerpPos, deltaTime * moveDamp),
                // Rotation
                Quaternion.Slerp(transform.rotation, rotatePos, deltaTime * rotationDamp)
            );
        }

        /// <summary>
        /// The camera Update method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="Update"/>) when you override it.</br>
        /// </summary>
        protected virtual void Update()
        {
            if (!canFollow || updateMode != BehaviourUpdateMode.Update)
            {
                return;
            }

            MoveCamera(Time.deltaTime);
        }

        /// <summary>
        /// The camera FixedUpdate method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="FixedUpdate"/>) when you override it.</br>
        /// </summary>
        protected virtual void FixedUpdate()
        {
            if (!canFollow || updateMode != BehaviourUpdateMode.FixedUpdate)
            {
                return;
            }

            MoveCamera(Time.fixedDeltaTime);
        }

        /// <summary>
        /// The camera LateUpdate method.
        /// <br>Always call this method (like <see langword="base"/>.<see cref="LateUpdate"/>) when you override it.</br>
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (!canFollow || updateMode != BehaviourUpdateMode.LateUpdate)
            {
                return;
            }

            MoveCamera(Time.deltaTime);
        }
        #endregion

#if UNITY_EDITOR
        private static Color[] m_cacheColor;
        private static Color GetRandColor(float alpha = 1f)
        {
            return new Color(
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                Random.Range(0.7f, 1f),
                alpha
            );
        }
        /// <summary>
        /// Draw gizmos on selection. This draws the camera positions in <see cref="cameraOffsetTargets"/>.
        /// <br>Always call this method when you override it.</br>
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            if (m_cacheColor == null)
            {
                m_cacheColor = new Color[cameraOffsetTargets.Length + 1];

                for (int i = 0; i < m_cacheColor.Length; i++)
                {
                    m_cacheColor[i] = GetRandColor(.6f);
                }
            }
            else if (m_cacheColor.Length != cameraOffsetTargets.Length + 1)
            {
                m_cacheColor = new Color[cameraOffsetTargets.Length + 1];

                for (int i = 0; i < m_cacheColor.Length; i++)
                {
                    m_cacheColor[i] = GetRandColor(.6f);
                }
            }

            // Draw spheres in camera offsets.
            var posOffset = followTransform == null || useFollowPositionInstead ? followPosition : followTransform.position;

            for (int i = 0; i < cameraOffsetTargets.Length; i++)
            {
                Gizmos.color = m_cacheColor[i];
                Gizmos.DrawSphere(cameraOffsetTargets[i].position + posOffset, 1f);
            }

            // Draw 'followVector3'
            if (followTransform == null || useFollowPositionInstead)
            {
                Gizmos.color = m_cacheColor[m_cacheColor.Length - 1];
                Gizmos.DrawCube(followPosition, Vector3.one);
            }
        }
#endif
    }
}
