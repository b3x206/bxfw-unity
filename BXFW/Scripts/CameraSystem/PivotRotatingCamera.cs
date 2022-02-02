using UnityEngine;

// TODO : Implement smooth camera movement
public class PivotRotatingCamera : MonoBehaviour
{
    [Header("Positional Setting")]
    [SerializeField] private Transform PivotObj = null;
    [SerializeField] private float DistanceBetweenObj = 15f;

    [Header("Camera Clamping")]
    [SerializeField] private bool ClampCameraRotation = true;
    [Tooltip("Euler rotation limit")]
    [SerializeField] private Vector2 RotLimitMin = new Vector2(-75f, -20f);
    [Tooltip("Euler rotation limit")]
    [SerializeField] private Vector2 RotLimitMax = new Vector2(75f, 75f);
    [SerializeField] private float RotSensitivity = 180f;

    [Header("Camera Zooming")]
    [Range(.5f, 200f)] [SerializeField] private float CameraScrollZoomSensitivity = 2f;
    // [Range(0.01f, 15f)][SerializeField] private float CameraZoomDampening = 2f;
    [SerializeField] private float DistanceZoomLimit = 5f;
    /* This is dumb. */
    private float CurrentZoom = 0f;

    private Vector3 prevPos;
    private Camera Cam;

    private void Awake()
    { 
        if (!TryGetComponent(out Cam)) { throw new System.Exception("This behaviour should be attached to a camera carrier!"); }
        transform.position = PivotObj.position;
        transform.Translate(new Vector3(0f, 0f, -DistanceBetweenObj));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            prevPos = Cam.ScreenToViewportPoint(Input.mousePosition);
        }
        /* Set the current zoom
         * It is clamped inbetween the maximum zoom limit - starting distance */
        CurrentZoom += Input.GetAxis("Mouse ScrollWheel") * CameraScrollZoomSensitivity;
        CurrentZoom = Mathf.Clamp(CurrentZoom, -DistanceZoomLimit, DistanceZoomLimit);

        if (Input.GetMouseButton(0))
        {
            /* This is the exact same euler you see on editor view...
             * This inconsistency is dumb... */
            Vector3 TransformEulerFixed = new Vector3(
                transform.eulerAngles.x > 180f ? transform.eulerAngles.x - 360f : transform.eulerAngles.x,
                transform.eulerAngles.y > 180f ? transform.eulerAngles.y - 360f : transform.eulerAngles.y,
                transform.eulerAngles.z > 180f ? transform.eulerAngles.z - 360f : transform.eulerAngles.z
                );

            transform.position = PivotObj.position;
            
            Vector3 dir = prevPos - Cam.ScreenToViewportPoint(Input.mousePosition);

            if (ClampCameraRotation)
            {
                /* Get Rotation to apply */
                Vector3 CurrentRotationEuler = TransformEulerFixed;

                /* X, Y and Z Clamps (we should not use euler for these but idk) */
                CurrentRotationEuler.x = Mathf.Clamp(CurrentRotationEuler.x, RotLimitMin.x, RotLimitMax.x);
                CurrentRotationEuler.y = Mathf.Clamp(CurrentRotationEuler.y, RotLimitMin.y, RotLimitMax.y);
                CurrentRotationEuler.z = 0f;

                /* Apply Rotation (goddamnit) */
                transform.localRotation = Quaternion.Euler(CurrentRotationEuler);
            }

            // Debug.Log($"Previous euler before transform.Rotate() : {transform.eulerAngles}");

            transform.Rotate(new Vector3(1f, 0f, 0f), dir.y * RotSensitivity);
            transform.Rotate(new Vector3(0f, 1f, 0f), -dir.x * RotSensitivity, Space.World);

            // Debug.Log($"Next euler after transform.Rotate() : {transform.eulerAngles}");

            /* Distance inbetween 
             * Current zoom is added because scroll wheel is inverse */
            transform.Translate(new Vector3(0f, 0f, 
                -DistanceBetweenObj + CurrentZoom));

            /* Z Axis should stay 0... */
            /* Causes bugs, DO NOT USE!!! */
            // transform.rotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y, 0f);

            prevPos = Cam.ScreenToViewportPoint(Input.mousePosition);
        }
        /* Zooming while not clicking the screen..
         * Also works for applying the position preferences */
        else
        {
            /* This works, but it is bad */
            transform.position = PivotObj.position;
            transform.Translate(new Vector3(0f, 0f,
                -DistanceBetweenObj + CurrentZoom));
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(PivotObj.position, DistanceBetweenObj);
        Gizmos.color = Color.red;

        // Eh, scrap it i am too dumb for maths...
        /* Max 
        Gizmos.DrawSphere(new Vector3(DistanceBetweenObj, DistanceBetweenObj, DistanceBetweenObj), 1f);
        Gizmos.DrawSphere(new Vector3(DistanceBetweenObj, DistanceBetweenObj, DistanceBetweenObj), 1f);

         * Min 
        Gizmos.DrawSphere(new Vector3(DistanceBetweenObj, DistanceBetweenObj, DistanceBetweenObj), 1f);
        Gizmos.DrawSphere(new Vector3(DistanceBetweenObj, DistanceBetweenObj, DistanceBetweenObj), 1f);
        */
    }
}
