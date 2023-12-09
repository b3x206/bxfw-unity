using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Camera that rotates around a pivot object.
    /// </summary>
    /// TODO : Implement smooth camera movement. (with lerp and movement damp)
    [RequireComponent(typeof(Camera))]
    public class PivotRotatingCamera : MonoBehaviour
    {
        [Header("Positional Setting")]
        public Transform PivotObj = null;
        public float DistanceBetweenObj = 15f;
        // public float CameraMoveDamp = 4f;

        [Header("Camera Clamping")]
        public bool ClampCameraRotation = true;
        [Tooltip("Euler rotation limit")]
        public Vector2 RotLimitMin = new Vector2(-75f, -20f);
        [Tooltip("Euler rotation limit")]
        public Vector2 RotLimitMax = new Vector2(75f, 75f);
        public float RotSensitivity = 180f;

        [Header("Camera Zooming")]
        [Range(.5f, 200f)] public float CameraScrollZoomSensitivity = 2f;
        public float DistanceZoomLimit = 5f;
        // [Range(0.01f, 15f)] public float CameraZoomDamp = 2f;
        private float CurrentZoom = 0f;

        private Vector3 prevPos;
        public Camera CameraComponent { get; private set; }

        private void Awake()
        {
            CameraComponent = GetComponent<Camera>();
            transform.position = PivotObj.position;
            transform.Translate(new Vector3(0f, 0f, -DistanceBetweenObj));
        }

        private void Update()
        {
            // Get the starting mouse position.
            if (Input.GetMouseButtonDown(0))
            {
                prevPos = CameraComponent.ScreenToViewportPoint(Input.mousePosition);
            }

            // Set the current zoom
            // It is clamped inbetween the maximum zoom limit - starting distance
            CurrentZoom += Input.GetAxis("Mouse ScrollWheel") * CameraScrollZoomSensitivity;
            CurrentZoom = Mathf.Clamp(CurrentZoom, -DistanceZoomLimit, DistanceZoomLimit);

            if (Input.GetMouseButton(0))
            {
                transform.position = PivotObj.position;

                Vector3 dir = prevPos - CameraComponent.ScreenToViewportPoint(Input.mousePosition);

                // Clamping
                if (ClampCameraRotation)
                {
                    // Get Rotation to apply
                    // This is the exact same euler you see on editor view.
                    // This fixes the dumb unity issue. (Clamp values being inconsistent with editor values)
                    Vector3 CurrentRotationEuler = Additionals.FixEulerRotation(transform.eulerAngles);

                    // X, Y and Z Clamps
                    CurrentRotationEuler.x = Mathf.Clamp(CurrentRotationEuler.x, RotLimitMin.x, RotLimitMax.x);
                    CurrentRotationEuler.y = Mathf.Clamp(CurrentRotationEuler.y, RotLimitMin.y, RotLimitMax.y);
                    CurrentRotationEuler.z = 0f;

                    // Apply Rotation
                    transform.localRotation = Quaternion.Euler(CurrentRotationEuler);
                }

                // Rotating
                transform.Rotate(new Vector3(1f, 0f, 0f), dir.y * RotSensitivity);
                transform.Rotate(new Vector3(0f, 1f, 0f), -dir.x * RotSensitivity, Space.World);

                // Distance inbetween 
                // Current zoom is added because scroll wheel is inverted (negative mouse scroll delta)
                transform.Translate(new Vector3(0f, 0f,
                    -DistanceBetweenObj + CurrentZoom));

                prevPos = CameraComponent.ScreenToViewportPoint(Input.mousePosition);
            }
            // Zooming while not clicking the screen..
            // Also works for applying the position preferences
            // This works, but it is bad
            else
            {
                transform.position = PivotObj.position;
                transform.Translate(new Vector3(0f, 0f,
                    -DistanceBetweenObj + CurrentZoom));
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (PivotObj == null)
            {
                return;
            }

            var gColor = Gizmos.color;
            Gizmos.DrawWireSphere(PivotObj.position, DistanceBetweenObj);
            Gizmos.color = Color.red;
            Gizmos.color = gColor;

            // maybe (TODO) : Draw a portion of sphere to show possible positions of the camera.
        }
    }
}