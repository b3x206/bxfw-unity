using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Draws shapes using a line drawing function.
    /// <br>Useful for things like <see cref="DebugDrawUtility"/> and <see cref="GizmoUtility"/></br>
    /// </summary>
    public abstract class LineShapeDrawer
    {
        /// <summary>
        /// Used to define a line drawing function.
        /// <br>Takes 2 3D Vector points and draws a line from-to that.</br>
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public delegate void LineMethod(Vector3 start, Vector3 end);

        /// <summary>
        /// The matrix that is the default.
        /// </summary>
        public static readonly Matrix4x4 DefaultMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        /// <summary>
        /// The matrix where the line drawing positions are transformed.
        /// </summary>
        public Matrix4x4 matrix = DefaultMatrix;
        /// <summary>
        /// Whether to use the <see cref="Matrix4x4.MultiplyPoint(Vector3)"/> instead of <see cref="Matrix4x4.MultiplyPoint3x4(Vector3)"/>.
        /// <br>This is required if you are transforming things like camera projection matrices, but for normal 3D transforms it is not required.</br>
        /// <br>It is faster to leave this option at <see langword="false"/>.</br>
        /// </summary>
        public bool transformPointGeneric = false;

        protected abstract void DoDrawLine(Vector3 start, Vector3 end);
        /// <summary>
        /// Draws a line using <see cref="DrawLineMethod"/>.
        /// </summary>
        /// <param name="start">Start of the line.</param>
        /// <param name="end">End of the line.</param>
        protected void DrawLine(Vector3 start, Vector3 end)
        {
            if (matrix != DefaultMatrix)
            {
                start = transformPointGeneric ? matrix.MultiplyPoint(start) : matrix.MultiplyPoint3x4(start);
                end = transformPointGeneric ? matrix.MultiplyPoint(end) : matrix.MultiplyPoint3x4(end);
            }

            DoDrawLine(start, end);
        }
        /// <summary>
        /// Draws a ray using the <see cref="DrawLine(Vector3, Vector3)"/>
        /// </summary>
        /// <param name="start">The starting point of the ray.</param>
        /// <param name="direction">The direction that the ray will move by.</param>
        protected void DrawRay(Vector3 start, Vector3 direction)
        {
            // Move from by direction
            DrawLine(start, start + direction);
        }

        /// <summary>
        /// Draws a square.
        /// </summary>
        /// <param name="pos">Position of the square.</param>
        /// <param name="size">Size of the square.</param>
        /// <param name="rotation">The rotation to draw the square.</param>
        public void DrawSquare(Vector3 pos, Vector2 size, Quaternion rotation)
        {
            Span<Vector3> verts = stackalloc Vector3[4];

            // 2-----3
            // |     |
            // |     |
            // 0-----1
            verts[0] = rotation * new Vector3(pos.x - (size.x / 2f), pos.y - (size.y / 2f), pos.z);
            verts[1] = rotation * new Vector3(pos.x + (size.x / 2f), pos.y - (size.y / 2f), pos.z);
            verts[2] = rotation * new Vector3(pos.x - (size.x / 2f), pos.y + (size.y / 2f), pos.z);
            verts[3] = rotation * new Vector3(pos.x + (size.x / 2f), pos.y + (size.y / 2f), pos.z);
            DrawLine(verts[0], verts[1]);
            DrawLine(verts[0], verts[2]);
            DrawLine(verts[1], verts[3]);
            DrawLine(verts[2], verts[3]);
        }
        /// <inheritdoc cref="DrawSquare(Vector3, Vector2, Quaternion)"/>
        /// <param name="direction">The direction of the square. Leave <see cref="Vector3.zero"/> to look towards <c>forward</c>.</param>
        public void DrawSquare(Vector3 pos, Vector2 size, Vector3 direction)
        {
            DrawSquare(pos, size, direction != Vector3.zero ? Quaternion.LookRotation(direction, Vector3.up) : Quaternion.identity);
        }
        /// <inheritdoc cref="DrawSquare(Vector3, Vector2, Quaternion)"/>
        public void DrawSquare(Vector3 pos, Vector2 size)
        {
            DrawSquare(pos, size, Quaternion.identity);
        }
        /// <summary>Draws an arrow.</summary>
        /// <param name="pos">Start position of the arrow.</param>
        /// <param name="direction">Direction point of the arrow. If this is <see cref="Vector3.zero"/> nothing is drawn.</param>
        /// <param name="arrowHeadLength">Head side rays length.</param>
        /// <param name="arrowHeadAngle">Head side rays angle.</param>
        public void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero)
            {
                return;
            }

            DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * Vector3.forward;
            DrawRay(pos + direction, right * arrowHeadLength);
            DrawRay(pos + direction, left * arrowHeadLength);
        }

        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="rotation">Rotation of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public void DrawCircle(Vector3 pos, float radius, Quaternion rotation)
        {
            const int CircleLength = 16;
            Span<Vector3> v = stackalloc Vector3[CircleLength];
            for (int i = 0; i < CircleLength; i++)
            {
                float fl = i / (float)CircleLength; // current lerp
                float c = Mathf.Cos(fl * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(fl * (float)(Math.PI * 2.0));

                // Rotate using 'direction'.
                Vector3 setVector = new Vector3(c, s, 0f);
                if (rotation != Quaternion.identity)
                {
                    setVector = rotation * setVector;
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
                DrawLine(start, end);
            }
        }
        /// <summary>
        /// Draws a circle.
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="direction">Direction that the circle looks towards. Set to Vector3.zero to look towards <c>forward</c>.</param>
        /// <param name="radius">Radius of the circle.</param>
        public void DrawCircle(Vector3 pos, float radius, Vector3 direction)
        {
            DrawCircle(pos, radius, direction != Vector3.zero ? Quaternion.LookRotation(direction, Vector3.up) : Quaternion.identity);
        }
        /// <summary>
        /// Draws a hemisphere.
        /// <br>Use the <see cref="Matrix"/> to rotate this hemisphere.</br>
        /// </summary>
        /// <param name="pos">Position of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        public void DrawHemiSphere(Vector3 pos, float radius)
        {
            const int SphereSegments = 32 - 1; // + 1 segment for ending
            Vector3 prevX = default,
                prevY = default,
                prevZ = default;

            for (int i = 0; i < SphereSegments + 1; i++)
            {
                float fL = i / (float)SphereSegments; // current lerp
                // 'H' prefixes half
                float cH = Mathf.Cos(fL * (float)Math.PI), c = Mathf.Cos(fL * (float)(Math.PI * 2.0));
                float sH = Mathf.Sin(fL * (float)Math.PI), s = Mathf.Sin(fL * (float)(Math.PI * 2.0));

                if (i == 0)
                {
                    // x = new Vector3(c, s, 0);
                    // y = new Vector3(0, c, s);
                    // z = new Vector3(s, 0, c);
                    // --
                    // the 'Y' axis formula has been slightly altered to be rotated towards Vector3.up
                    prevX = pos + (radius * new Vector3(cH, sH, 0f)); // X
                    prevY = pos + (radius * new Vector3(0f, sH, cH)); // Y
                    prevZ = pos + (radius * new Vector3(s, 0f, c));   // Z
                    continue;
                }

                // Draw
                Vector3 currentX = pos + (radius * new Vector3(cH, sH, 0f)),
                    currentY = pos + (radius * new Vector3(0f, sH, cH)),
                    currentZ = pos + (radius * new Vector3(s, 0f, c));

                DrawLine(prevX, currentX);
                DrawLine(prevY, currentY);
                DrawLine(prevZ, currentZ);

                prevX = pos + (radius * new Vector3(cH, sH, 0f));
                prevY = pos + (radius * new Vector3(0f, sH, cH));
                prevZ = pos + (radius * new Vector3(s, 0f, c));
            }
        }
        /// <summary>
        /// Draws a sphere.
        /// </summary>
        /// <param name="pos">The position of the sphere. Can take normal 3D positions.</param>
        /// <param name="radius">Radius of the sphere.</param>
        public void DrawSphere(Vector4 pos, float radius)
        {
            // Make unit sphere.
            const int SphereSegments = 16;
            Span<Vector4> v = stackalloc Vector4[SphereSegments * 3]; // Sphere vector
            for (int i = 0; i < SphereSegments; i++)
            {
                float f = i / (float)SphereSegments;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[(0 * SphereSegments) + i] = new Vector4(c, s, 0, 1);
                v[(1 * SphereSegments) + i] = new Vector4(0, c, s, 1);
                v[(2 * SphereSegments) + i] = new Vector4(s, 0, c, 1);
            }

            int length = v.Length / 3;
            for (int i = 0; i < length; i++)
            {
                Vector4 sX = pos + (radius * v[(0 * length) + i]);
                Vector4 eX = pos + (radius * v[(0 * length) + ((i + 1) % length)]);
                Vector4 sY = pos + (radius * v[(1 * length) + i]);
                Vector4 eY = pos + (radius * v[(1 * length) + ((i + 1) % length)]);
                Vector4 sZ = pos + (radius * v[(2 * length) + i]);
                Vector4 eZ = pos + (radius * v[(2 * length) + ((i + 1) % length)]);

                DrawLine(sX, eX);
                DrawLine(sY, eY);
                DrawLine(sZ, eZ);
            }
        }

        /// <summary>
        /// Draws an arc.
        /// </summary>
        /// <param name="origin">World point origin point of the arc.</param>
        /// <param name="rotation">Rotation of this arc.</param>
        /// <param name="distance">Distance relative to the origin.</param>
        /// <param name="arcAngle">The size of the arc, in degrees. Converted to radians by <see cref="Mathf.Deg2Rad"/>.</param>
        /// <param name="drawLinesFromOrigin">Draws 2 lines towards the starting and ending position from <paramref name="origin"/>.</param>
        public void DrawArc(Vector3 origin, Quaternion rotation, float distance, float arcAngle, bool drawLinesFromOrigin = true)
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
                DrawLine(origin, prevPosition);
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
                DrawLine(prevPosition, arcNextPosition);

                prevPosition = arcNextPosition;
            }

            // Final line
            Vector3 lastPosition = origin + (rotation * new Vector3(
                Mathf.Cos(arcAngle / 2f * Mathf.Deg2Rad),
                Mathf.Sin(arcAngle / 2f * Mathf.Deg2Rad)
            ) * distance);

            // Draw normal + origin line for the last time
            DrawLine(prevPosition, lastPosition);
            if (drawLinesFromOrigin)
            {
                DrawLine(origin, lastPosition);
            }
        }
        /// <inheritdoc cref="DrawArc(Vector3, Quaternion, float, float, bool)"/>
        /// <param name="direction">
        /// Direction relative to the origin point.
        /// Converted to a look rotation with upwards of <see cref="Vector3.up"/> and rotated in <see cref="Vector3.up"/> axis by 90f degrees.
        /// </param>
        public void DrawArc(Vector3 origin, Vector3 direction, float distance, float arcAngle, bool drawLinesFromOrigin = true)
        {
            Quaternion rotation = direction == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(90f, Vector3.up);
            DrawArc(origin, rotation, distance, arcAngle, drawLinesFromOrigin);
        }

        /// <summary>
        /// Base constructor for LineShapeDrawer. Does nothing.
        /// </summary>
        public LineShapeDrawer()
        { }
        /// <summary>
        /// Sets the LineShapeDrawers values.
        /// </summary>
        public LineShapeDrawer(Matrix4x4 transformMatrix, bool transformPointGeneric)
        {
            matrix = transformMatrix;
            this.transformPointGeneric = transformPointGeneric;
        }
    }
}
