#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Extended draw shapes for unity class <see cref="Gizmos"/>.
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

        /// <summary>Draws an arrow to the unity scene using <see cref="Gizmos"/> class.</summary>
        /// <param name="pos">Start position of the arrow.</param>
        /// <param name="direction">Direction point of the arrow.</param>
        /// <param name="arrowHeadLength">Head side rays length.</param>
        /// <param name="arrowHeadAngle">Head side rays angle.</param>
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        /// <summary>
        /// Draws an arrow to the scene using <see cref="Gizmos"/> with switchable color.
        /// <br>Modifies <see cref="Gizmos.color"/> and resets it to it's previous value.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            var gColor = Gizmos.color;
            Gizmos.color = color;
            DrawArrow(pos, direction, arrowHeadLength, arrowHeadAngle);
            Gizmos.color = gColor;
        }

        /// <summary>
        /// Draws a circle to the scene using <see cref="Gizmos"/>.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="direction">Direction that the circle looks towards. Set to Vector3.zero to look towards <c>forward</c>.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void DrawWireCircle(Vector3 pos, float radius, Vector3 direction)
        {
            int lenSphere = 16;
            Vector3[] v = new Vector3[lenSphere]; // Sphere points (normalized)
            for (int i = 0; i < lenSphere; i++)
            {
                float fl = i / (float)lenSphere; // current lerp
                float c = Mathf.Cos(fl * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(fl * (float)(Math.PI * 2.0));

                // Rotate using 'direction'.
                Vector3 setVector = new Vector3(c, s, 0f);
                if (direction != Vector3.zero)
                    setVector = Quaternion.LookRotation(direction, Vector3.up) * setVector;

                v[i] = setVector;
            }

            int len = v.Length;
            for (int i = 0; i < len; i++)
            {
                // Calculate sphere points using radius
                Vector3 sX = pos + (radius * v[(0 * len) + i]);
                Vector3 eX = pos + (radius * v[(0 * len) + ((i + 1) % len)]);

                Gizmos.DrawLine(sX, eX);
            }
        }
        /// <summary>
        /// Draws a circle to the scene using <see cref="Gizmos"/> with switchable color.
        /// <br>Modifies <see cref="Gizmos.color"/> and resets it to it's previous value.</br>
        /// </summary>
        public static void DrawWireCircle(Vector3 pos, float radius, Vector3 direction, Color color)
        {
            var gColor = Gizmos.color;
            Gizmos.color = color;
            DrawWireCircle(pos, radius, direction);
            Gizmos.color = gColor;
        }

        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="textStyle">Style of the drawn text.</param>
        /// <param name="oX">X offset.</param>
        /// <param name="oY">Y offset.</param>
        /// <param name="cullText">Should the text be culled if it's not visible by the camera?</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, GUIStyle textStyle, float oX, float oY, bool cullText = true)
        {
#if UNITY_EDITOR
            Handles.BeginGUI();

            var restoreColor = GUI.color;
            GUI.color = color;
            if (textStyle == null)
                textStyle = GUI.skin.label;

            var view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (cullText)
            {
                if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0 || 
                    (textStyle != null && textStyle.fontSize <= 0))
                {
                    GUI.color = restoreColor;
                    Handles.EndGUI();
                    return;
                }
            }

            Handles.Label(TransformByPixel(worldPos, oX, oY), text, textStyle);

            GUI.color = restoreColor;

            Handles.EndGUI();
#else
            Debug.LogWarning("[GizmoUtility::DrawText] DrawText only works in unity editor.");
#endif
        }
        // -- textSize
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="textSize">Font size of the drawn text. You can also change this by using a <see cref="GUIStyle"/>.</param>
        /// <param name="oX">X offset.</param>
        /// <param name="oY">Y offset.</param>
        /// <param name="cullText">Should the text be culled if it's not visible by the camera?</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, int textSize, float oX, float oY, bool cullText)
        {
            DrawText(text, worldPos, color, new GUIStyle(GUI.skin.label) { fontSize = textSize }, oX, oY, cullText);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="textSize">Font size of the drawn text. You can also change this by using a <see cref="GUIStyle"/>.</param>
        /// <param name="oX">X offset.</param>
        /// <param name="oY">Y offset.</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, int textSize, float oX, float oY)
        {
            DrawText(text, worldPos, color, new GUIStyle(GUI.skin.label) { fontSize = textSize }, oX, oY, true);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="textSize">Font size of the drawn text. You can also change this by using a <see cref="GUIStyle"/>.</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, int textSize)
        {
            DrawText(text, worldPos, color, new GUIStyle(GUI.skin.label) { fontSize = textSize }, 0f, 0f, true);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="textSize">Font size of the drawn text. You can also change this by using a <see cref="GUIStyle"/>.</param>
        public static void DrawText(string text, Vector3 worldPos, int textSize)
        {
            DrawText(text, worldPos, Color.white, new GUIStyle(GUI.skin.label) { fontSize = textSize }, 0f, 0f, true);
        }
        // -- non-textSize
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="oX">X offset.</param>
        /// <param name="oY">Y offset.</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, float oX, float oY, bool cullText = true)
        {
            DrawText(text, worldPos, color, null, oX, oY, cullText);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        /// <param name="cullText">Should the text be culled if it's not visible by the camera?</param>
        public static void DrawText(string text, Vector3 worldPos, Color color, bool cullText)
        {
            DrawText(text, worldPos, color, null, 0f, 0f, cullText);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        /// <param name="color">Color of the text. </param>
        public static void DrawText(string text, Vector3 worldPos, Color color)
        {
            DrawText(text, worldPos, color, null, 0f, 0f, true);
        }
        /// <summary>
        /// Draws a text into the gizmos context.
        /// </summary>
        /// <param name="text">Text to display and draw.</param>
        /// <param name="worldPos">Position in the world.</param>
        public static void DrawText(string text, Vector3 worldPos)
        {
            DrawText(text, worldPos, Color.white, null, 0f, 0f, true);
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
