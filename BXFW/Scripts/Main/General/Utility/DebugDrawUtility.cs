using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Extended shape drawing for unity class <see cref="Debug"/>.
    /// </summary>
    public static class DebugDrawUtility
    {
        /// <summary>
        /// Draws a sphere in <see cref="Debug"/> context.
        /// </summary>
        /// <param name="pos">Position of the circle. Can use normal 3d positions.</param>
        /// <param name="radius">Radius of the circle.</param> 
        /// <param name="duration">The duration that this sphere will appear for.</param>
        /// <param name="depthTest">Should the sphere lines be obscured by other objects in scene?</param>
        public static void DrawSphere(Vector4 pos, float radius, Color color, float duration, bool depthTest)
        {
            // Make unit sphere.
            int lenSphere = 16;
            Vector4[] v = new Vector4[lenSphere * 3]; // Sphere vector
            for (int i = 0; i < lenSphere; i++)
            {
                float f = i / (float)lenSphere;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[(0 * lenSphere) + i] = new Vector4(c, s, 0, 1);
                v[(1 * lenSphere) + i] = new Vector4(0, c, s, 1);
                v[(2 * lenSphere) + i] = new Vector4(s, 0, c, 1);
            }

            int len = v.Length / 3;
            for (int i = 0; i < len; i++)
            {
                Vector4 sX = pos + (radius * v[(0 * len) + i]);
                Vector4 eX = pos + (radius * v[(0 * len) + ((i + 1) % len)]);
                Vector4 sY = pos + (radius * v[(1 * len) + i]);
                Vector4 eY = pos + (radius * v[(1 * len) + ((i + 1) % len)]);
                Vector4 sZ = pos + (radius * v[(2 * len) + i]);
                Vector4 eZ = pos + (radius * v[(2 * len) + ((i + 1) % len)]);

                Debug.DrawLine(sX, eX, color, duration, depthTest);
                Debug.DrawLine(sY, eY, color, duration, depthTest);
                Debug.DrawLine(sZ, eZ, color, duration, depthTest);
            }
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
                Debug.DrawLine(origin, prevPosition, color, duration);
            }

            for (int i = -halfSegments + 1; i < halfSegments; i++)
            {
                // initial line is already drawn
                // since lerp only goes from -0.49.. -> 0.5, the arc angle will be completed.
                float lerp = (float)i / (segments - 1); // lerp that goes from -0.49.. -> 0.49..
                float c = Mathf.Cos(arcAngle * lerp * Mathf.Deg2Rad); // x axis
                float s = Mathf.Sin(arcAngle * lerp * Mathf.Deg2Rad); // y axis
                Vector3 arcToPosition = origin + (rotation * new Vector3(c, s) * distance);

                // Primary line
                Debug.DrawLine(prevPosition, arcToPosition, color, duration);

                prevPosition = arcToPosition;
            }

            // Final line
            Vector3 lastPosition = origin + (rotation * new Vector3(
                Mathf.Cos(arcAngle / 2f * Mathf.Deg2Rad),
                Mathf.Sin(arcAngle / 2f * Mathf.Deg2Rad)
            ) * distance);

            // Draw normal + origin line for the last time
            Debug.DrawLine(prevPosition, lastPosition, color, duration);
            if (drawLinesFromOrigin)
            {
                Debug.DrawLine(origin, lastPosition, color, duration);
            }
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
                Vector3 sX = pos + (radius * v[i]);
                Vector3 eX = pos + (radius * v[(i + 1) % len]);

                Debug.DrawLine(sX, eX, color, duration, depthTest);
            }
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
            Vector3[] verts = new Vector3[4];

            // 2-----3
            // |     |
            // |     |
            // 0-----1
            verts[0] = new Vector3(pos.x - (size.x / 2f), pos.y - (size.y / 2f), pos.z);
            verts[1] = new Vector3(pos.x + (size.x / 2f), pos.y - (size.y / 2f), pos.z);
            verts[2] = new Vector3(pos.x - (size.x / 2f), pos.y + (size.y / 2f), pos.z);
            verts[3] = new Vector3(pos.x + (size.x / 2f), pos.y + (size.y / 2f), pos.z);
            Debug.DrawLine(verts[0], verts[1], color, duration, depthTest);
            Debug.DrawLine(verts[0], verts[2], color, duration, depthTest);
            Debug.DrawLine(verts[1], verts[3], color, duration, depthTest);
            Debug.DrawLine(verts[2], verts[3], color, duration, depthTest);
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
            Debug.DrawRay(pos, direction, color, duration);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color, duration);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color, duration);
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
    }
}
