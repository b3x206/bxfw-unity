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
        public sealed class Drawer : LineShapeDrawer
        {
            protected override void DoDrawLine(Vector3 start, Vector3 end)
            {
                Gizmos.DrawLine(start, end);
            }
        }

        /// <summary>
        /// The primary GizmoUtility drawer. If the method isn't implemented in the variable's class you can use this.
        /// </summary>
        public static readonly Drawer drawer = new Drawer();

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
            drawer.DrawArrow(pos, direction, arrowHeadLength, arrowHeadAngle);
        }
        /// <summary>
        /// Draws an arrow to the scene using <see cref="Gizmos"/> with switchable color.
        /// <br>Modifies <see cref="Gizmos.color"/> and resets it to it's previous value.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            DrawArrow(pos, direction, arrowHeadLength, arrowHeadAngle);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draws a circle to the scene using <see cref="Gizmos"/>.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="direction">Direction that the circle looks towards. Set to Vector3.zero to look towards <c>forward</c>.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void DrawWireCircle(Vector3 pos, float radius, Vector3 direction)
        {
            drawer.DrawCircle(pos, radius, direction);
        }
        /// <summary>
        /// Draws a circle to the scene using <see cref="Gizmos"/>.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="rotation">Rotation of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void DrawWireCircle(Vector3 pos, float radius, Quaternion rotation)
        {
            drawer.DrawCircle(pos, radius, rotation);
        }
        /// <summary>
        /// Draws a circle to the scene using <see cref="Gizmos"/> with switchable color.
        /// <br>Modifies <see cref="Gizmos.color"/> and resets it to it's previous value.</br>
        /// </summary>
        public static void DrawWireCircle(Vector3 pos, float radius, Vector3 direction, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            DrawWireCircle(pos, radius, direction);
            Gizmos.color = prevColor;
        }

        /// <summary>
        /// Draws a hemisphere to the scene using <see cref="Gizmos"/>.
        /// <br>Use the <see cref="Gizmos.matrix"/> to rotate this hemisphere.</br>
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public static void DrawWireHemiSphere(Vector3 pos, float radius)
        {
            drawer.DrawHemiSphere(pos, radius);
        }
        /// <summary>
        /// Draws a hemisphere to the scene using <see cref="Gizmos"/>.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="color">The color of the drawn gizmo.</param>
        public static void DrawWireHemiSphere(Vector3 pos, float radius, Color color)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = color;
            // this method should be noexcept?
            DrawWireHemiSphere(pos, radius);
            Gizmos.color = prevColor;
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
            drawer.DrawArc(origin, rotation, distance, arcAngle, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, bool)"/>
        /// <param name="direction">
        /// Direction relative to the origin point.
        /// Converted to a look rotation with upwards of <see cref="Vector3.up"/> and rotated in <see cref="Vector3.up"/> axis by 90f degrees.
        /// </param>
        public static void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            drawer.DrawArc(origin, direction, distance, arcAngle, drawLinesFromOrigin);
        }

        /// <inheritdoc cref="LineShapeDrawer.DrawSquare(Vector3, Vector2, Quaternion)"/>
        public static void DrawSquare(Vector3 pos, Vector2 size, Quaternion rotation)
        {
            drawer.DrawSquare(pos, size, rotation);
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
