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
            // Save the color in a temporary variable to not overwrite other Gizmos
            Color prevColor = Gizmos.color;

            // Change the gizmo matrix to the relative space of the boxCollider.
            // This makes offsets with rotation work
            // Source: https://forum.unity.com/threads/gizmo-rotation.4817/#post-3242447
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCollider.center), transform.rotation, transform.lossyScale);

            // Draws the edges of the BoxCollider
            // Center is Vector3.zero, since we've transformed the calculation space in the previous step.
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(Vector3.zero, boxCollider.size);

            // Draws the sides/insides of the BoxCollider, with a tint to the original color.
            prevColor.a *= alphaForInsides;
            Gizmos.color = gizmoColor;
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
                {
                    setVector = Quaternion.LookRotation(direction, Vector3.up) * setVector;
                }

                v[i] = setVector;
            }

            // Draw loop
            int len = v.Length;
            for (int i = 0; i < len; i++)
            {
                // Calculate sphere points with radius and relative positioning
                Vector3 start = pos + (radius * v[i]);
                Vector3 end = pos + (radius * v[(i + 1) % len]);
                // Draw line
                Gizmos.DrawLine(start, end);
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
        /// Draws an arc.
        /// </summary>
        /// <param name="origin">World point origin point of the arc.</param>
        /// <param name="rotation">Rotation of this arc.</param>
        /// <param name="distance">Distance relative to the origin.</param>
        /// <param name="arcAngle">The size of the arc, in degrees. Converted to radians by <see cref="Mathf.Deg2Rad"/>.</param>
        /// <param name="drawLinesFromOrigin">Draws 2 lines towards the starting and ending position from <paramref name="origin"/>.</param>
        public static void DrawArc(Vector3 origin, Quaternion rotation, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            // rotate direction by that so it actually looks towards
            // The center of the arc is the (direction * origin) * distance), but it's not needed.
            
            // this number should be even, but an odd number of segments are drawn
            // because i am still stuck in how to do for loops
            int segments = 48;
            int halfSegments = segments / 2;

            // Draw origin line for initial
            Vector3 prevPosition = origin + (rotation * new Vector3(
                Mathf.Cos(-arcAngle / 2f * Mathf.Deg2Rad),
                Mathf.Sin(-arcAngle / 2f * Mathf.Deg2Rad)
            ) * distance);

            if (drawLinesFromOrigin)
            {
                Gizmos.DrawLine(origin, prevPosition);
            }

            for (int i = -halfSegments + 1; i < halfSegments; i++)
            {
                // initial line is already drawn
                // since lerp only goes from -0.49.. -> 0.49, the center arc lines will be drawn.
                float lerp = (float)i / (segments - 1); // lerp that goes from -0.49.. -> 0.49..
                float c = Mathf.Cos(arcAngle * lerp * Mathf.Deg2Rad); // x axis
                float s = Mathf.Sin(arcAngle * lerp * Mathf.Deg2Rad); // y axis
                Vector3 arcNextPosition = origin + (rotation * new Vector3(c, s) * distance);

                // Primary line
                Gizmos.DrawLine(prevPosition, arcNextPosition);

                prevPosition = arcNextPosition;
            }

            // Final line
            Vector3 lastPosition = origin + (rotation * new Vector3(
                Mathf.Cos(arcAngle / 2f * Mathf.Deg2Rad),
                Mathf.Sin(arcAngle / 2f * Mathf.Deg2Rad)
            ) * distance);

            // Draw normal + origin line for the last time
            Gizmos.DrawLine(prevPosition, lastPosition);
            if (drawLinesFromOrigin)
            { 
                Gizmos.DrawLine(origin, lastPosition);
            }
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, bool)"/>
        /// <param name="direction">
        /// Direction relative to the origin point.
        /// Converted to a look rotation with upwards of <see cref="Vector3.up"/> and rotated in <see cref="Vector3.up"/> axis by 90f degrees.
        /// </param>
        public static void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            Quaternion rotation = direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);
            DrawArc(origin, rotation, distance, arcAngle, drawLinesFromOrigin);
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
            textStyle ??= GUI.skin.label;

            var view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (cullText)
            {
                if (screenPos.y < 0 || screenPos.y > Screen.height ||
                    screenPos.x < 0 || screenPos.x > Screen.width ||
                    screenPos.z < 0 || 
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
            Debug.LogError("[GizmoUtility::DrawText] DrawText only works in unity editor.");
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

        private static Vector3 TransformByPixel(Vector3 position, float x, float y)
        {
            return TransformByPixel(position, new Vector3(x, y));
        }
        private static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
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
