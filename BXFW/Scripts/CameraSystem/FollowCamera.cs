using UnityEngine;

/// <summary>
/// The player camera.
/// <br>Tracks the player smoothly.</br>
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
        public bool UseCameraPosClamp;
        public Vector2 CameraPosXClamp;
        public Vector2 CameraPosYClamp;
        public Vector2 CameraPosZClamp;
        public Vector3 Position;
        public Vector3 EulerRotation;
    }
    [Header("State Positions Camera")]
    public CameraOffset[] CameraOffsetTargets = new CameraOffset[1];
    [SerializeField, HideInInspector] private int _CurrentCameraOffsetIndex = 0;
    public int CurrentCameraOffsetIndex
    {
        get { return _CurrentCameraOffsetIndex; }
        set { _CurrentCameraOffsetIndex = Mathf.Clamp(value, 0, CameraOffsetTargets.Length - 1); }
    }
    /// <summary>The <see cref="UnityEngine.Events.UnityEvent"/> setter.</summary>
    public void SetCurrentCameraOffsetIndex(int Offset)
    { CurrentCameraOffsetIndex = Offset; }

    // ** Variables (Hidden)
    public Camera CameraComponent { get; private set; }

    // * Private-Internal
    private Plane[] CameraFrustumPlanes;

    // ** Initilaze
    private void Awake()
    {
        CameraComponent = GetComponent<Camera>();
    }

    #region Camera Mechanics
    // ** Follow Target Transform
	// FIX : FixedUpdate fixes movement jitter.
    private void FixedUpdate()
    {
        var followPos = (FollowTransform == null || UseFollowVecInstead) ? FollowVector3 : FollowTransform.position;
        var lerpPos = CurrentCameraOffset.UseCameraPosClamp ? new Vector3(
            Mathf.Clamp(followPos.x + CurrentCameraOffset.Position.x, CurrentCameraOffset.CameraPosXClamp.x, CurrentCameraOffset.CameraPosXClamp.y),
            Mathf.Clamp(followPos.y + CurrentCameraOffset.Position.y, CurrentCameraOffset.CameraPosYClamp.x, CurrentCameraOffset.CameraPosYClamp.y),
            Mathf.Clamp(followPos.z + CurrentCameraOffset.Position.z, CurrentCameraOffset.CameraPosZClamp.x, CurrentCameraOffset.CameraPosZClamp.y))
            : new Vector3(followPos.x + CurrentCameraOffset.Position.x,
                    followPos.y + CurrentCameraOffset.Position.y,
                    followPos.z + CurrentCameraOffset.Position.z);

        if (CanFollow)
        {
            transform.SetPositionAndRotation(
                // Position
                Vector3.Lerp(transform.position, lerpPos, Time.deltaTime * Move_Damp),
                // Rotation
                Quaternion.Slerp(transform.rotation,
                    Quaternion.Euler(CurrentCameraOffset.EulerRotation.x,
                    CurrentCameraOffset.EulerRotation.y,
                    CurrentCameraOffset.EulerRotation.z),
                Time.deltaTime * Rotation_Damp)
            );
        }
    }
    #endregion

#if UNITY_EDITOR
    private static Color[] CacheColor;
    private void OnDrawGizmosSelected()
    {
        // Generate persistent unique colors.
        static Color GetRandColor()
        {
            return new Color(
                Random.Range(.5f, 1f),
                Random.Range(.5f, 1f),
                Random.Range(.5f, 1f));
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
        if (FollowTransform != null)
        {
            for (int i = 0; i < CameraOffsetTargets.Length; i++)
            {
                Gizmos.color = CacheColor[i];
                Gizmos.DrawSphere(FollowTransform.position + CameraOffsetTargets[i].Position, 1f);
            }

            Gizmos.color = CacheColor[CacheColor.Length - 1];
            Gizmos.DrawCube(FollowVector3, Vector3.one);
        }
    }
#endif
}
