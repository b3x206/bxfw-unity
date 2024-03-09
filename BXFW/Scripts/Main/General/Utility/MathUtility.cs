using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Transformation axis, can be used for positioning, (euler) rotation or scaling.
    /// <br>This class is used to define a direction in 3D, without needing the Vector3 (and it's potential unwanted inbetween values).</br>
    /// </summary>
    [Flags]
    public enum TransformAxis
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,
        ZAxis = 1 << 2,

        // This will exist because 'TransformAxis2D' also has it.
        XYZAxis = XAxis | YAxis | ZAxis
    }

    /// <summary>
    /// Transform axis for positioning, used in 2D space.
    /// </summary>
    [Flags]
    public enum TransformAxis2D
    {
        None = 0,

        XAxis = 1 << 0,
        YAxis = 1 << 1,

        XYAxis = XAxis | YAxis
    }

    /// <summary>
    /// Contains general purpose math utility.
    /// <br>This class both contains float and vector math utils.</br>
    /// </summary>
    public static class MathUtility
    {
        // -------------------
        // -  Float Utility  -
        // -------------------
        /// <summary>
        /// A clamped mapped linear interpolation.
        /// <br>Can be used to interpolate values according to other values.</br>
        /// </summary>
        /// <param name="from">Returned value start. This is the value you will get if <paramref name="value"/> is equal to <paramref name="valueFrom"/>.</param>
        /// <param name="to">Returned value end. This is the value you will get if <paramref name="value"/> is equal to <paramref name="valueTo"/>.</param>
        /// <param name="valueFrom">Mapping Range value start. The <paramref name="value"/> is assumed to be started from here.</param>
        /// <param name="valueTo">Mapping Range value end. The <paramref name="value"/> is assumed to be ending in here.</param>
        /// <param name="value">The value that is the 'time' parameter. Goes between <paramref name="to"/>-&gt;<paramref name="valueTo"/>.</param>
        public static float Map(float from, float to, float valueFrom, float valueTo, float value)
        {
            if (value <= valueFrom)
            {
                return from;
            }
            else if (value >= valueTo)
            {
                return to;
            }

            // a + ((b - a) * t) but goofy
            return from + ((to - from) * ((value - valueFrom) / (valueTo - valueFrom)));
        }

        // Well, not blaming myself for being unable to figure out 'wrap' as it is unintuitive.
        // Or maybe i am just stupid.
        /// <summary>
        /// Clamps with <paramref name="value"/> rollback between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        public static int Wrap(int value, int min, int max)
        {
            int minMaxRange = max - min;
            if (minMaxRange == 0)
            {
                return min;
            }

            return min + ((((value - min) % minMaxRange) + minMaxRange) % minMaxRange);
        }
        /// <summary>
        /// Clamps with <paramref name="value"/> rollback between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        public static float Wrap(float value, float min, float max)
        {
            float minMaxRange = max - min;
            if (Approximately(minMaxRange, 0f))
            {
                return min;
            }

            float result = value - (minMaxRange * (float)Math.Floor((value - min) / minMaxRange));
            if (Approximately(result, max))
            {
                return min;
            }

            return result;
        }

        /// <summary>
        /// Moves towards a value with a <paramref name="maxDelta"/> constraint.
        /// <br>This version uses double precision floats.</br>
        /// </summary>
        /// <param name="current">Value to start from.</param>
        /// <param name="target">Value to move into.</param>
        /// <param name="maxDelta">Maximum change in <paramref name="current"/> to <paramref name="target"/> allowed.</param>
        public static double MoveTowards(double current, double target, double maxDelta)
        {
            // Delta (between values to move) is smaller than the max delta
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + (Math.Sign(target - current) * maxDelta);
        }

        /// <summary>
        /// Checks whether if given <paramref name="x"/> is close to given <paramref name="y"/>.
        /// </summary>
        public static bool Approximately(float x, float y)
        {
            return Math.Abs(y - x) < Math.Max(1E-06f * Math.Max(Math.Abs(x), Math.Abs(y)), float.Epsilon * 8f);
        }
        /// <summary>
        /// Checks whether if given <paramref name="x"/> is close to given <paramref name="y"/>.
        /// </summary>
        public static bool Approximately(double x, double y)
        {
            return Math.Abs(y - x) < Math.Max(1E-06d * Math.Max(Math.Abs(x), Math.Abs(y)), double.Epsilon * 8d);
        }

        // ------------------
        // - Vector Utility -
        // ------------------
        /// <summary>
        /// An AABB collision check.
        /// <br>If the rects are colliding this will return true.</br>
        /// </summary>
        /// <param name="lhs">First rect to check</param>
        /// <param name="rhs">Second rect to check</param>
        public static bool RectsCollide(Rect lhs, Rect rhs)
        {
            return lhs.x < (rhs.x + rhs.width) &&
                (lhs.x + lhs.width) > rhs.x &&
                lhs.y < (rhs.y + rhs.height) &&
                (lhs.y + lhs.height) > rhs.y;
        }

        /// <summary>
        /// Returns the center of an virtual circle, between the <paramref name="cPoint0"/>, <paramref name="cPoint1"/> and <paramref name="cPoint2"/>.
        /// <br>Normal of the circle is <paramref name="cPointNormal"/>.</br>
        /// </summary>
        public static Vector3 CircleCenter(Vector3 cPoint0, Vector3 cPoint1, Vector3 cPoint2, out Vector3 cPointNormal)
        {
            // two circle chords
            Vector3 v1 = cPoint1 - cPoint0;
            Vector3 v2 = cPoint2 - cPoint0;

            // Normal related stuff
            cPointNormal = Vector3.Cross(v1, v2);
            if (cPointNormal.sqrMagnitude < Mathf.Epsilon)
            {
                return Vector3.one * float.NaN;
            }

            cPointNormal.Normalize();

            // Perpendicular of both chords
            Vector3 pd1 = Vector3.Cross(v1, cPointNormal).normalized;
            Vector3 pd2 = Vector3.Cross(v2, cPointNormal).normalized;
            // Distance between the chord midpoints
            Vector3 r = (v1 - v2) * 0.5f;
            // Center angle between the two perpendiculars
            float c = Vector3.Angle(pd1, pd2);
            // Angle between first perpendicular and chord midpoint vector
            float a = Vector3.Angle(r, pd1);
            // Law of sine to calculate length of p2
            float d = (float)(r.magnitude * Math.Sin(a * Mathf.Deg2Rad) / Math.Sin(c * Mathf.Deg2Rad));

            if (Vector3.Dot(v1, cPoint2 - cPoint1) > 0f)
            {
                return cPoint0 + (v2 * 0.5f) - (pd2 * d);
            }

            return cPoint0 + (v2 * 0.5f) + (pd2 * d);
        }
        /// <summary>
        /// Gives a bezier interpolated value between <paramref name="point0"/> and <paramref name="point1"/> 
        /// influenced by <paramref name="handle0"/> and <paramref name="handle1"/>.
        /// </summary>
        /// <param name="point0">First main point of the bezier.</param>
        /// <param name="handle0">Handle for the first point.</param>
        /// <param name="point1">Second main point.</param>
        /// <param name="handle1">Handle for the second point.</param>
        /// <param name="t">The time value to interpolate by, unclamped.</param>
        /// <returns>The position value interpolated by <paramref name="t"/>.</returns>
        public static Vector3 BezierInterpolate(Vector3 point0, Vector3 handle0, Vector3 point1, Vector3 handle1, float t)
        {
            // Bezier is just a fancy combination of lerps
            // Powers of values
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            // Point 0
            Vector3 B = uuu * point0;
            // Handle(s)
            B += 3.0f * uu * t * handle0;
            B += 3.0f * u * tt * handle1;
            // Point 1
            B += ttt * point1;

            return B;
        }

        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector4 target)
        {
            // This is faster than 'for looping' the Vector4
            if (target.x > target.y && target.x > target.z && target.x > target.w)
            {
                return target.x;
            }

            if (target.y > target.x && target.y > target.z && target.y > target.w)
            {
                return target.y;
            }

            if (target.z > target.x && target.z > target.y && target.z > target.w)
            {
                return target.z;
            }

            if (target.w > target.x && target.w > target.y && target.w > target.z)
            {
                return target.w;
            }

            return target.x;
        }
        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector3 target)
        {
            if (target.x > target.y && target.x > target.z)
            {
                return target.x;
            }

            if (target.y > target.x && target.y > target.z)
            {
                return target.y;
            }

            if (target.z > target.y && target.z > target.x)
            {
                return target.z;
            }

            return target.x;
        }
        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector2 target)
        {
            if (target.x > target.y)
            {
                return target.x;
            }

            if (target.y > target.x)
            {
                return target.y;
            }

            return target.x;
        }

        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector4 target)
        {
            if (target.x < target.y && target.x < target.z && target.x < target.w)
            {
                return target.x;
            }

            if (target.y < target.x && target.y < target.z && target.y < target.w)
            {
                return target.y;
            }

            if (target.z < target.x && target.z < target.y && target.z < target.w)
            {
                return target.z;
            }

            if (target.w < target.x && target.w < target.y && target.w < target.z)
            {
                return target.w;
            }

            return target.x;
        }
        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector3 target)
        {
            if (target.x < target.y && target.x < target.z)
            {
                return target.x;
            }

            if (target.y < target.x && target.y < target.z)
            {
                return target.y;
            }

            if (target.z < target.y && target.z < target.x)
            {
                return target.z;
            }

            return target.x;
        }
        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector2 target)
        {
            if (target.x < target.y)
            {
                return target.x;
            }

            if (target.y < target.x)
            {
                return target.y;
            }

            return target.x;
        }

        /// <summary>
        /// Returns the given <see cref="Vector3"/> direction value from <paramref name="axis"/>.
        /// <br>The returned <see cref="Vector3"/> is not normalized, use <see cref="Vector3.normalized"/> to normalize it.</br>
        /// </summary>
        public static Vector3 GetDirection(this TransformAxis axis)
        {
            Vector3 directionAxis = Vector3.zero;

            if ((axis & TransformAxis.XAxis) == TransformAxis.XAxis)
            {
                directionAxis += Vector3.right;
            }
            if ((axis & TransformAxis.YAxis) == TransformAxis.YAxis)
            {
                directionAxis += Vector3.up;
            }
            if ((axis & TransformAxis.ZAxis) == TransformAxis.ZAxis)
            {
                directionAxis += Vector3.forward;
            }

            return directionAxis;
        }
        /// <summary>
        /// Returns the given <see cref="Vector2"/> direction value from <paramref name="axis"/>.
        /// </summary>
        public static Vector2 GetDirection(this TransformAxis2D axis)
        {
            switch (axis)
            {
                case TransformAxis2D.None:
                    return Vector2.zero;
                case TransformAxis2D.XAxis:
                    return Vector2.right;
                case TransformAxis2D.YAxis:
                    return Vector2.up;

                default:
                case TransformAxis2D.XYAxis:
                    return Vector2.one;
            }
        }

        /// <summary>
        /// Get the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector3 AxisVector(this Vector3 target, TransformAxis axisConstraint)
        {
            Vector3 v = Vector3.zero;

            if ((axisConstraint & TransformAxis.XAxis) == TransformAxis.XAxis)
            {
                v.x = target.x;
            }
            if ((axisConstraint & TransformAxis.YAxis) == TransformAxis.YAxis)
            {
                v.y = target.y;
            }
            if ((axisConstraint & TransformAxis.ZAxis) == TransformAxis.ZAxis)
            {
                v.z = target.z;
            }

            return v;
        }
        /// <summary>
        /// Sets or removes the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector3 SettedAxisVector(this Vector3 target, TransformAxis axisConstraint, Vector3 valueSet)
        {
            Vector3 v = target;

            if ((axisConstraint & TransformAxis.XAxis) == TransformAxis.XAxis)
            {
                v.x = valueSet.x;
            }
            if ((axisConstraint & TransformAxis.YAxis) == TransformAxis.YAxis)
            {
                v.y = valueSet.y;
            }
            if ((axisConstraint & TransformAxis.ZAxis) == TransformAxis.ZAxis)
            {
                v.z = valueSet.z;
            }

            return v;
        }

        /// <summary>
        /// Get the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector2 AxisVector(this Vector2 target, TransformAxis2D axisConstraint)
        {
            Vector3 v = target;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    v.x = 0f;
                    v.y = 0f;
                    break;

                case TransformAxis2D.XAxis:
                    v.y = 0f;
                    break;
                case TransformAxis2D.YAxis:
                    v.x = 0f;
                    break;

                default:
                case TransformAxis2D.XYAxis:
                    break;
            }

            return v;
        }
        /// <summary>
        /// Sets or removes the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector2 SettedAxisVector(this Vector2 target, TransformAxis2D axisConstraint, Vector2 valueSet)
        {
            Vector3 v = target;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    v.x = 0f;
                    v.y = 0f;
                    break;

                case TransformAxis2D.XAxis:
                    v.x = valueSet.x;
                    break;
                case TransformAxis2D.YAxis:
                    v.y = valueSet.y;
                    break;

                default:
                case TransformAxis2D.XYAxis:
                    v.x = valueSet.x;
                    v.y = valueSet.y;
                    break;
            }

            return v;
        }

        /// <summary>
        /// Returns the sign vector of this vector.
        /// <br>This calls <see cref="Math.Sign(float)"/> for all axis and returns a new vector of that.</br>
        /// <br>Can be used to easily create a vector (that is defining an axis 
        /// but inbetween axis are ignored) to multiply with.</br>
        /// </summary>
        public static Vector3 SignVector(this Vector3 target)
        {
            return new Vector3(Math.Sign(target.x), Math.Sign(target.y), Math.Sign(target.z));
        }
        /// <summary>
        /// Returns the sign vector of this vector.
        /// <br>This calls <see cref="Math.Sign(float)"/> for all axis and returns a new vector of that.</br>
        /// <br>Can be used to easily create a vector (that is defining an axis 
        /// but inbetween axis are ignored) to multiply with.</br>
        /// </summary>
        public static Vector2 SignVector(this Vector2 target)
        {
            return new Vector2(Math.Sign(target.x), Math.Sign(target.y));
        }

        /// <summary>
        /// Creates a <see cref="Vector2"/> with positive values from given vector <paramref name="v"/>.
        /// </summary>
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Math.Abs(v.x), Math.Abs(v.y));
        }
        /// <summary>
        /// Creates a <see cref="Vector3"/> with positive values from given vector <paramref name="v"/>.
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
        }
        /// <summary>
        /// Creates a <see cref="Vector4"/> with positive values from given vector <paramref name="v"/>.
        /// </summary>
        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z), Math.Abs(v.w));
        }

        /// <summary>
        /// Rounds the indiviual components of the given vector <paramref name="v"/>.
        /// <br>The resulting vector is the rounded vector, this method does not mutate the given vector
        /// (like the rest of the Mathf shorthands for Vectors)</br>
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Rounded axis Vector2.</returns>
        public static Vector2 RoundAxis(this Vector2 v)
        {
            return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
        }
        /// <summary>
        /// Rounds the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Rounded axis Vector3.</returns>
        public static Vector3 RoundAxis(this Vector3 v)
        {
            return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
        }
        /// <summary>
        /// Rounds the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Rounded axis Vector4.</returns>
        public static Vector4 RoundAxis(this Vector4 v)
        {
            return new Vector4(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z), Mathf.Round(v.w));
        }

        /// <summary>
        /// Floors the indiviual components of the given vector <paramref name="v"/>.
        /// <br>The resulting vector is the floored vector, this method does not mutate the given vector
        /// (like the rest of the Mathf shorthands for Vectors)</br>
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Floored axis Vector2.</returns>
        public static Vector2 FloorAxis(this Vector2 v)
        {
            return new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
        }
        /// <summary>
        /// Floors the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Floored axis Vector3.</returns>
        public static Vector3 FloorAxis(this Vector3 v)
        {
            return new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
        }
        /// <summary>
        /// Floors the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to round it's axis.</param>
        /// <returns>Floored axis Vector4.</returns>
        public static Vector4 FloorAxis(this Vector4 v)
        {
            return new Vector4(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z), Mathf.Floor(v.w));
        }

        /// <summary>
        /// Ceils the indiviual components of the given vector <paramref name="v"/>.
        /// <br>The resulting vector is the ceiled vector, this method does not mutate the given vector
        /// (like the rest of the Mathf shorthands for Vectors)</br>
        /// </summary>
        /// <param name="v">Vector to ceil it's axis.</param>
        /// <returns>Ceiled axis Vector2.</returns>
        public static Vector2 CeilAxis(this Vector2 v)
        {
            return new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
        }
        /// <summary>
        /// Ceils the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to ceil it's axis.</param>
        /// <returns>Ceiled axis Vector3.</returns>
        public static Vector3 CeilAxis(this Vector3 v)
        {
            return new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
        }
        /// <summary>
        /// Ceils the indiviual components of the given vector <paramref name="v"/>.
        /// </summary>
        /// <param name="v">Vector to ceil it's axis.</param>
        /// <returns>Ceiled axis Vector4.</returns>
        public static Vector4 CeilAxis(this Vector4 v)
        {
            return new Vector4(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z), Mathf.Ceil(v.w));
        }

        /// <summary>
        /// Fixes euler rotation to Unity Editor type of values instead of the code based setting values.
        /// </summary>
        public static Vector3 EditorEulerRotation(Vector3 euler)
        {
            // The editor view for the rotation constraints are rolled between -180f~180f
            // Instead of going between 0f~360f if you use transform.Rotate or anything similar
            Vector3 editorEuler = new Vector3(
                euler.x > 180f ? euler.x - 360f : euler.x,
                euler.y > 180f ? euler.y - 360f : euler.y,
                euler.z > 180f ? euler.z - 360f : euler.z
            );

            return editorEuler;
        }
        /// <summary>
        /// Returns the <paramref name="quaternion"/> with the constrainted euler angles of given axis <paramref name="axisConstraint"/>.
        /// </summary>
        public static Quaternion AxisEulerQuaternion(this Quaternion quaternion, TransformAxis axisConstraint)
        {
            return Quaternion.Euler(quaternion.eulerAngles.AxisVector(axisConstraint));
        }

        /// <summary>
        /// Gives a random position inside given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to get a random position inside.</param>
        /// <param name="axis">
        /// The bound extent axis to consider.
        /// This controls which components contain a position in the returned <see cref="Vector3"/>.
        /// </param>
        public static Vector3 RandomInside(this Bounds bounds, TransformAxis axis)
        {
            Vector3 result = Vector3.zero;
            if ((axis & TransformAxis.XAxis) == TransformAxis.XAxis)
            {
                result.x = bounds.center.x + UnityEngine.Random.Range(-bounds.extents.x, bounds.extents.x);
            }
            if ((axis & TransformAxis.YAxis) == TransformAxis.YAxis)
            {
                result.y = bounds.center.y + UnityEngine.Random.Range(-bounds.extents.y, bounds.extents.y);
            }
            if ((axis & TransformAxis.ZAxis) == TransformAxis.ZAxis)
            {
                result.z = bounds.center.z + UnityEngine.Random.Range(-bounds.extents.z, bounds.extents.z);
            }

            return result;
        }
        /// <inheritdoc cref="RandomInside(Bounds, TransformAxis)"/>
        public static Vector3 RandomInside(this Bounds bounds)
        {
            return RandomInside(bounds, TransformAxis.XYZAxis);
        }

        // ------------------
        // - Matrix Utility -
        // ------------------
#if !UNITY_2021_2_OR_NEWER
        // This method exists since UNITY_2021_2
        /// <summary>
        /// Returns the transformation values from the matrix. (Position)
        /// </summary>
        public static Vector3 GetPosition(this Matrix4x4 matrix)
        {
            return new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }
#endif
        /// <summary>
        /// Returns the rotation values from the matrix.
        /// </summary>
        public static Quaternion GetRotation(this Matrix4x4 matrix)
        {
            Vector3 forward = new Vector3(matrix.m02, matrix.m12, matrix.m22);
            if (forward == Vector3.zero)
            {
                return Quaternion.identity; // Get identity rotation without logging message to the console
            }

            Vector3 upwards = new Vector3(matrix.m01, matrix.m11, matrix.m21);
            return Quaternion.LookRotation(forward, upwards);
        }
        /// <summary>
        /// Returns the scaling values from the matrix.
        /// <br>Does not work properly with <b>negatively</b> scaled matrices.</br>
        /// </summary>
        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            Vector3 scale = new Vector3(matrix.GetColumn(0).magnitude, matrix.GetColumn(1).magnitude, matrix.GetColumn(2).magnitude);
            return scale;
        }
        /// <summary>
        /// Deconstructs the given <paramref name="matrix"/> to it's <paramref name="position"/>, <paramref name="rotation"/> and <paramref name="scale"/>.
        /// </summary>
        public static void Deconstruct(this Matrix4x4 matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = matrix.GetPosition();
            rotation = matrix.GetRotation();
            scale = matrix.GetScale();
        }
        /// <summary>
        /// Sets a matrix to <paramref name="transform"/>.
        /// <br>Not 100% accurate.</br>
        /// </summary>
        /// <param name="space">
        /// The space to set the matrix on.
        /// If the matrix was gathered using <br/>
        /// <br><see cref="Transform.worldToLocalMatrix"/> = use the space mode <see cref="Space.World"/> as this is the matrix for the children.</br>
        /// <br><see cref="Transform.localToWorldMatrix"/> = use the space mode <see cref="Space.Self"/> as this is the world matrix.</br>
        /// <br><b>NOTE + TODO : </b> This could be inverted or completely wrong as i did not test it.</br>
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public static void SetMatrix(this Transform transform, Matrix4x4 matrix, Space space = Space.World)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform), "[MathUtility::SetMatrix] Passed parameter was null.");
            }

            matrix.Deconstruct(out Vector3 position, out Quaternion rotation, out Vector3 scale);

            switch (space)
            {
                case Space.World:
                    transform.SetPositionAndRotation(position, rotation);
                    Vector3 parentLossyScale = Vector3.one;
                    if (transform.parent != null)
                    {
                        parentLossyScale = transform.parent.lossyScale;
                    }

                    transform.localScale = Vector3.Scale(parentLossyScale, scale);
                    break;
                case Space.Self:
#if UNITY_2021_3_OR_NEWER
                    transform.SetLocalPositionAndRotation(position, rotation);
#else
                    transform.localPosition = position;
                    transform.localRotation = rotation;
#endif
                    transform.localScale = scale;
                    break;
                default:
                    throw new ArgumentException(string.Format("[Additionals::SetMatrix] Failed setting matrix : Argument 'space={0}' is invalid.", space), nameof(space));
            }
        }

        /// <summary>
        /// Interpolates a Matrix4x4.
        /// <br>This can be used for interpolating such things as <see cref="Camera.projectionMatrix"/> and others.</br>
        /// </summary>
        public static Matrix4x4 Lerp(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            return LerpUnclamped(src, dest, Math.Clamp(time, 0f, 1f));
        }
        /// <summary>
        /// Interpolates a Matrix4x4. (Unclamped)
        /// <br>This can be used for interpolating such things as <see cref="Camera.projectionMatrix"/> and others.</br>
        /// </summary>
        public static Matrix4x4 LerpUnclamped(Matrix4x4 src, Matrix4x4 dest, float time)
        {
            Matrix4x4 ret = new Matrix4x4();

            for (int i = 0; i < 16; i++)
            {
                ret[i] = src[i] + ((dest[i] - src[i]) * time);
            }

            return ret;
        }
    }
}
