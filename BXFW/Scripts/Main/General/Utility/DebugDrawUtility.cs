using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Extended shape drawing for unity class <see cref="Debug"/>.
    /// </summary>
    public static class DebugDrawUtility
    {
        /// <summary>
        /// Used to handle line drawing for the class <see cref="Debug"/>.
        /// </summary>
        public sealed class Drawer : LineShapeDrawer
        {
            // Extra arguments for 'Debug'
            public Color debugColor = Color.white;
            public float debugDuration = 0f;
            public bool debugDepthTest = true;

            protected override void DoDrawLine(Vector3 start, Vector3 end)
            {
                Debug.DrawLine(start, end, debugColor, debugDuration, debugDepthTest);
            }

            public void SetDebugArguments(Color color, float duration, bool depthTest)
            {
                debugColor = color;
                debugDuration = duration;
                debugDepthTest = depthTest;
            }
        }

        /// <summary>
        /// The primary DebugDrawUtility drawer. If the method isn't implemented in the variable's class you can use this.
        /// <br/>
        /// <br>To set the <see cref="Debug"/> class' drawing variables, use the <see cref="Drawer.SetDebugArguments(Color, float, bool)"/>.</br>
        /// </summary>
        private static readonly Drawer drawer = new Drawer();

        /// <inheritdoc cref="LineShapeDrawer.matrix"/>
        public static Matrix4x4 Matrix
        {
            get => drawer.matrix;
            set => drawer.matrix = value;
        }
        /// <inheritdoc cref="LineShapeDrawer.transformPointGeneric"/>
        public static bool TransformPointGeneric
        {
            get => drawer.transformPointGeneric;
            set => drawer.transformPointGeneric = value;
        }

        /// <inheritdoc cref="LineShapeDrawer.DrawSphere"/>
        /// <param name="duration">The duration that this sphere will appear for.</param>
        /// <param name="depthTest">Should the sphere lines be obscured by other objects in scene?</param>
        public static void DrawSphere(Vector4 pos, float radius, Color color, float duration, bool depthTest)
        {
            drawer.SetDebugArguments(color, duration, depthTest);
            drawer.DrawSphere(pos, radius);
        }
        /// <summary>
        /// Draws a sphere to the <see cref="Debug"/> context.
        /// <br>Contained in the depth test, meaning the drawn sphere lines can be obscured by other objects.</br>
        /// </summary>
        public static void DrawSphere(Vector3 pos, float radius, Color color, float duration)
        {
            DrawSphere(pos, radius, color, duration, true);
        }
        /// <summary>
        /// Draws a sphere to the <see cref="Debug"/> context.
        /// <br>Has no duration + contained in depth test.</br>
        /// </summary>
        public static void DrawSphere(Vector3 pos, float radius, Color color)
        {
            DrawSphere(pos, radius, color, 0f, true);
        }
        /// <summary>
        /// Draws a sphere to the <see cref="Debug"/> context.
        /// <br>Color is <see cref="Color.white"/>, no duration and contained in the depth test.</br>
        /// </summary>
        public static void DrawSphere(Vector3 pos, float radius)
        {
            DrawSphere(pos, radius, Color.white, 0f, true);
        }

        /// <summary>
        /// Draws an arc.
        /// </summary>
        /// <param name="origin">World point origin point of the arc.</param>
        /// <param name="rotation">Rotation of this arc.</param>
        /// <param name="distance">Distance relative to the origin.</param>
        /// <param name="arcAngle">The size of the arc, in degrees. Converted to radians by <see cref="Mathf.Deg2Rad"/>.</param>
        /// <param name="drawLinesFromOrigin">Draws 2 lines towards the starting and ending position from <paramref name="origin"/>.</param>
        public static void DrawArc(Vector3 origin, Quaternion rotation, float distance, float arcAngle, Color color, float duration, bool drawLinesFromOrigin = true)
        {
            drawer.SetDebugArguments(color, duration, true);
            drawer.DrawArc(origin, rotation, distance, arcAngle, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, Color, float, bool)"/>
        public static void DrawArc(Vector3 origin, Quaternion rotation, float distance, float arcAngle, Color color, bool drawLinesFromOrigin = true)
        {
            DrawArc(origin, rotation, distance, arcAngle, color, 0f, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, Color, float, bool)"/>
        public static void DrawArc(Vector3 origin, Quaternion rotation, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            DrawArc(origin, rotation, distance, arcAngle, Color.white, 0f, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, Color, float, bool)"/>
        /// <param name="direction">
        /// Direction relative to the origin point.
        /// Converted to a look rotation with upwards of <see cref="Vector3.up"/> and rotated in <see cref="Vector3.up"/> axis by 90f degrees.
        /// </param>
        public static void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, Color color, float duration, bool drawLinesFromOrigin = true)
        {
            Quaternion rotation = direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);
            DrawArc(origin, rotation, distance, arcAngle, color, duration, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Vector3, float, float, Color, float, bool)"/>
        public static void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, Color color, bool drawLinesFromOrigin = true)
        {
            DrawArc(origin, direction, distance, arcAngle, color, 0f, drawLinesFromOrigin);
        }
        /// <inheritdoc cref="DrawArc(Vector3, Vector3, float, float, Color, float, bool)"/>
        public static void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            DrawArc(origin, direction, distance, arcAngle, Color.white, 0f, drawLinesFromOrigin);
        }

        /// <summary>
        /// Draws a circle to the scene using <see cref="Debug"/>.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="direction">Direction that the circle looks towards. Set to Vector3.zero to look towards <c>-forward</c>.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="duration">The duration that this sphere will appear for.</param>
        /// <param name="depthTest">Should the sphere lines be obscured by other objects in scene?</param>
        public static void DrawCircle(Vector3 pos, float radius, Vector3 direction, Color color, float duration, bool depthTest)
        {
            drawer.SetDebugArguments(color, duration, depthTest);
            drawer.DrawCircle(pos, radius, direction);
        }
        /// <summary>
        /// Draws a circle to the <see cref="Debug"/> context.
        /// <br>Contained in the depth test, meaning the drawn circle lines can be obscured by other objects.</br>
        /// </summary>
        public static void DrawCircle(Vector3 pos, float radius, Vector3 direction, Color color, float duration)
        {
            DrawCircle(pos, radius, direction, color, duration, true);
        }
        /// <summary>
        /// Draws a circle to the <see cref="Debug"/> context.
        /// <br>Has no duration + contained in depth test.</br>
        /// </summary>
        public static void DrawCircle(Vector3 pos, float radius, Vector3 direction, Color color)
        {
            DrawCircle(pos, radius, direction, color, 0f, true);
        }
        /// <summary>
        /// Draws a circle to the <see cref="Debug"/> context.
        /// <br>Color is <see cref="Color.white"/>, no duration and contained in the depth test.</br>
        /// </summary>
        public static void DrawCircle(Vector3 pos, float radius, Vector3 direction)
        {
            DrawCircle(pos, radius, direction, Color.white, 0f, true);
        }
        /// <summary>
        /// Draws a circle to the <see cref="Debug"/> context.
        /// <br>Direction looks towards <c>-forward</c>, no duration and contained in the depth test.</br>
        /// </summary>
        public static void DrawCircle(Vector3 pos, float radius, Color color)
        {
            DrawCircle(pos, radius, Vector3.zero, color, 0f, true);
        }
        /// <summary>
        /// Draws a circle to the <see cref="Debug"/> context.
        /// <br>Direction looks towards <c>-forward</c>, color is <see cref="Color.white"/>, no duration and contained in the depth test.</br>
        /// </summary>
        public static void DrawCircle(Vector3 pos, float radius)
        {
            DrawCircle(pos, radius, Vector3.zero, Color.white, 0f, true);
        }

        /// <summary>
        /// Draws a square to the debug context.
        /// </summary>
        /// <param name="pos">Position of the square.</param>
        /// <param name="size">Size of the square.</param>
        /// <param name="color">Color of the square.</param>
        /// <param name="duration">How long the square will be visible after it's initial drawing call.</param>
        /// <param name="depthTest">Whether to test the depth of the drawn square (can be occluded by in front rendered objects)</param>
        public static void DrawSquare(Vector3 pos, Vector2 size, Color color, float duration, bool depthTest)
        {
            drawer.SetDebugArguments(color, duration, depthTest);
            drawer.DrawSquare(pos, size);
        }
        /// <inheritdoc cref="DrawSquare(Vector3, Vector2, Color, float, bool)"/>
        public static void DrawSquare(Vector3 pos, Vector2 size, Color color, float duration)
        {
            DrawSquare(pos, size, color, duration, true);
        }
        /// <inheritdoc cref="DrawSquare(Vector3, Vector2, Color, float, bool)"/>
        public static void DrawSquare(Vector3 pos, Vector2 size, Color color)
        {
            DrawSquare(pos, size, color, 0f, true);
        }
        /// <inheritdoc cref="DrawSquare(Vector3, Vector2, Color, float, bool)"/>
        public static void DrawSquare(Vector3 pos, Vector2 size)
        {
            DrawSquare(pos, size, Color.white, 0f, true);
        }

        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float duration, float arrowHeadLength, float arrowHeadAngle)
        {
            drawer.SetDebugArguments(color, duration, true);
            drawer.DrawArrow(pos, direction, arrowHeadLength, arrowHeadAngle);
        }
        /// <summary>
        /// Draws an arrow in debug context.
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float duration)
        {
            DrawArrow(pos, direction, color, duration, 0.25f, 20.0f);
        }
        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// <br>Draw duration is 0.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color)
        {
            DrawArrow(pos, direction, color, 0f, 0.25f, 20.0f);
        }
        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// <br>Draw color by default is <see cref="Color.white"/>.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction)
        {
            DrawArrow(pos, direction, Color.white, 0f, 0.25f, 20.0f);
        }

        /// <inheritdoc cref="LineShapeDrawer.DrawHemiSphere(Vector3, float)"/>
        public static void DrawHemiSphere(Vector3 pos, float radius, Color color, float duration, bool depthTest)
        {
            drawer.SetDebugArguments(color, duration, depthTest);
            drawer.DrawHemiSphere(pos, radius);
        }
        /// <inheritdoc cref="LineShapeDrawer.DrawHemiSphere(Vector3, float)"/>
        public static void DrawHemiSphere(Vector3 pos, float radius, Color color, float duration)
        {
            DrawHemiSphere(pos, radius, color, duration, true);
        }
        /// <inheritdoc cref="LineShapeDrawer.DrawHemiSphere(Vector3, float)"/>
        public static void DrawHemiSphere(Vector3 pos, float radius, Color color)
        {
            DrawHemiSphere(pos, radius, color, 0f, true);
        }
        /// <inheritdoc cref="LineShapeDrawer.DrawHemiSphere(Vector3, float)"/>
        public static void DrawHemiSphere(Vector3 pos, float radius)
        {
            DrawHemiSphere(pos, radius, Color.white, 0f, true);
        }
    }
}
