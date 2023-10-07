using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Camera that rotates around a pivot object.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PivotRotatingCamera : MonoBehaviour
    {
        [Header("Positional Setting")]
        public Transform pivotObj = null;
        public float distanceBetweenPivot = 10f;

        [Header("Camera Clamping")]
        public bool clampCameraRotation = true;
        [InspectorConditionalDraw(nameof(clampCameraRotation))] public MinMaxValue xRotationRange = new MinMaxValue(-75f, 75f); 
        [InspectorConditionalDraw(nameof(clampCameraRotation))] public MinMaxValue yRotationRange = new MinMaxValue(-20f, 75f); 
        public float lookSensitivity = 180f;

        [Header("Camera Zooming")]
        [Range(.5f, 200f)] public float cameraScrollZoomSensitivity = 2f;
        public MinMaxValue distanceZoomLimits = new MinMaxValue(5f, 50f);
        private float m_currentZoom = 0f;

        public Camera CameraComponent { get; private set; }

        private void Awake()
        {
            CameraComponent = GetComponent<Camera>();
            transform.position = pivotObj.position;
            transform.Translate(new Vector3(0f, 0f, -distanceBetweenPivot));
        }

        private Vector3 m_prevMousePos;
        private void Update()
        {
            // Get the starting mouse position.
            if (Input.GetMouseButtonDown(0))
            {
                m_prevMousePos = CameraComponent.ScreenToViewportPoint(Input.mousePosition);
            }

            // Set the current zoom
            // It is clamped inbetween the maximum zoom limit - starting distance
            float scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0f)
            {
                m_currentZoom = distanceZoomLimits.ClampBetween(m_currentZoom + (scrollAxis * cameraScrollZoomSensitivity));
            }

            if (Input.GetMouseButton(0))
            {
                transform.position = pivotObj.position;

                Vector3 dragDir = m_prevMousePos - CameraComponent.ScreenToViewportPoint(Input.mousePosition);

                // Clamping
                // FIXME : For some reason in this clamping this doesn't work as intended in LateUpdate
                // Unlike the PlayerTPSCamera script
                if (clampCameraRotation)
                {
                    // Get Rotation to apply
                    // This is the exact same euler you see on editor view.
                    // This fixes the dumb unity issue. (Clamp values being inconsistent with editor values)
                    Vector3 camRotationEuler = MathUtility.EditorEulerRotation(transform.eulerAngles);

                    // X, Y and Z Clamps
                    camRotationEuler.x = xRotationRange.ClampBetween(camRotationEuler.x);
                    camRotationEuler.y = yRotationRange.ClampBetween(camRotationEuler.y);
                    camRotationEuler.z = 0f;

                    // Apply Rotation
                    transform.localRotation = Quaternion.Euler(camRotationEuler);
                }

                // Rotating
                transform.Rotate(new Vector3(1f, 0f, 0f), dragDir.y * lookSensitivity);
                transform.Rotate(new Vector3(0f, 1f, 0f), -dragDir.x * lookSensitivity, Space.World);

                // Distance inbetween 
                // Current zoom is added because scroll wheel is inverted (negative mouse scroll delta)
                transform.Translate(new Vector3(0f, 0f, -distanceBetweenPivot + m_currentZoom));

                m_prevMousePos = CameraComponent.ScreenToViewportPoint(Input.mousePosition);
            }
            // Zooming while not clicking the screen..
            // Also works for applying the positioning preferences
            else if (scrollAxis != 0f)
            {
                transform.position = pivotObj.position;
                transform.Translate(new Vector3(0f, 0f, -distanceBetweenPivot + m_currentZoom));
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (pivotObj == null)
                return;

            var gColor = Gizmos.color;
            Gizmos.DrawWireSphere(pivotObj.position, distanceBetweenPivot);

            // Draw a portion of sphere to show possible positions of the camera.
            // Note : Since i'm stupid, this will only show a good approximation instead of showing actually where is possible
            if (clampCameraRotation)
            {
                // Base arc rotation for X axis
                Gizmos.color = Color.red;
                Quaternion xRangeArcRotation = Quaternion.AngleAxis(90f, Vector3.up) * // Rotate towards inverse direction the camera is looking
                    // Rota-te sideways so that it's X axis
                    Quaternion.AngleAxis(-90f, Vector3.right) *
                    // Arc difference rotation (as the given DrawArc is center aligned)
                    Quaternion.AngleAxis((xRotationRange.Min + xRotationRange.Max) / 2f, Vector3.forward);
                // Upper arc rotation = AngleAxis(yRotation.Max, Vector3.down)
                GizmoUtility.DrawArc(
                    origin: pivotObj.position, 
                    rotation: xRangeArcRotation * Quaternion.AngleAxis(yRotationRange.Max, Vector3.down), 
                    distance: distanceBetweenPivot,
                    arcAngle: xRotationRange.Size()
                );
                // Lower arc rotation = AngleAxis(yRotation.Min, Vector3.down)
                GizmoUtility.DrawArc(
                    origin: pivotObj.position,
                    rotation: xRangeArcRotation * Quaternion.AngleAxis(yRotationRange.Min, Vector3.down),
                    distance: distanceBetweenPivot,
                    arcAngle: xRotationRange.Size()
                );

                // Actually won't show the Y rotation because the X was hard enough
            }

            Gizmos.color = gColor;
        }
    }
}