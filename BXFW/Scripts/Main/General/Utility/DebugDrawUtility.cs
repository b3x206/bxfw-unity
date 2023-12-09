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
                {
                    setVector = Quaternion.LookRotation(direction, Vector3.up) * setVector;
                }

                v[i] = setVector;
            }

            int len = v.Length;
            for (int i = 0; i < len; i++)
            {
                // Calculate sphere points using radius
                Vector3 sX = pos + (radius * v[(0 * len) + i]);
                Vector3 eX = pos + (radius * v[(0 * len) + ((i + 1) % len)]);

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
        /// Draws a cube to the debug context.
        /// <br>This cube is not rotated by function.</br>
        /// </summary>
        public static void DrawCube(Vector3 pos, Vector2 size, Color color, float duration, bool depthTest)
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
        public static void DrawCube(Vector3 pos, Vector2 size, Color color, float duration)
        {
            DrawCube(pos, size, color, duration, true);
        }
        public static void DrawCube(Vector3 pos, Vector2 size, Color color)
        {
            DrawCube(pos, size, color, 0f, true);
        }
        public static void DrawCube(Vector3 pos, Vector2 size)
        {
            DrawCube(pos, size, Color.white, 0f, true);
        }

        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// <br>Draw color by default is <see cref="Color.white"/>.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction)
        {
            DrawArrow(pos, direction, Color.white, 0f, 0.25f, 20.0f);
        }
        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// <br>Draw duration is 0.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color)
        {
            DrawArrow(pos, direction, color, 0f, 0.25f, 20.0f);
        }
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float duration)
        {
            DrawArrow(pos, direction, color, duration, 0.25f, 20.0f);
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
    }
}
