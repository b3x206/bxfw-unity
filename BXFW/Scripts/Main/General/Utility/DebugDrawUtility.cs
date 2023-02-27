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
        public static void DrawSphere(Vector4 pos, float radius, Color color)
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

                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }

        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context.
        /// <br>Draw color by default is <see cref="Color.white"/>.</br>
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            DrawArrow(pos, direction, Color.white, arrowHeadLength, arrowHeadAngle);
        }
        /// <summary>
        /// Draws an arrow in <see cref="Debug"/> context again, but with changeable color (a revolutionary function).
        /// </summary>
        public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction, color);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
            Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
        }
    }
}
