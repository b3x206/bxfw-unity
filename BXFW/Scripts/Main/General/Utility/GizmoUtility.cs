#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Utility to draw gizmos.
    /// </summary>
    public static class GizmoUtility
    {
        /// <summary>
        /// Draws box collider gizmo according to the rotation of the parent transform.
        /// </summary>
        public static void DrawBoxCollider(this Transform transform, Color gizmoColor, BoxCollider boxCollider, float alphaForInsides = 0.3f)
        {
            // Save the color in a temporary variable to not overwrite changes in the inspector (if the sent-in color is a serialized variable).
            var color = gizmoColor;

            // Change the gizmo matrix to the relative space of the boxCollider.
            // This makes offsets with rotation work
            // Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, transform.lossyScale);

            // Draws the edges of the BoxCollider
            // Center is Vector3.zero, since we've transformed the calculation space in the previous step.
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

            // Draws the sides/insides of the BoxCollider, with a tint to the original color.
            color.a *= alphaForInsides;
            Gizmos.color = color;
            Gizmos.DrawCube(Vector3.zero, boxCollider.size);
        }
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrowGizmos(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }
        public static void DrawArrowDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }

        public static void DrawText(string text, Vector3 worldPos, Color? color = null, bool cullText = true, float oX = 0f, float oY = 0f)
        {
#if UNITY_EDITOR
            Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (color.HasValue) GUI.color = color.Value;

            var view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (cullText)
            {
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
                {
                    GUI.color = restoreColor;
                    Handles.EndGUI();
                    return;
                }
            }

            Handles.Label(TransformByPixel(worldPos, oX, oY), text);

            GUI.color = restoreColor;
            Handles.EndGUI();
#else
            Debug.LogWarning("[GizmoUtility::DrawText] DrawText only works in unity editor.");
#endif
        }
        internal static Vector3 TransformByPixel(Vector3 position, float x, float y)
        {
            return TransformByPixel(position, new Vector3(x, y));
        }
        internal static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
        {
#if UNITY_EDITOR
            Camera cam = SceneView.currentDrawingSceneView.camera;

            return cam != null ? cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy) : position;
#else
            return Vector3.zero;
#endif
        }
    }
}
