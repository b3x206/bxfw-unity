using System;
using UnityEngine;

namespace BXFW
{
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
        /// <param name="from">Returned value start. This is the value you will get if <paramref name="value"/> is equal to <paramref name="vFrom"/>.</param>
        /// <param name="to">Returned value end. This is the value you will get if <paramref name="value"/> is equal to <paramref name="vTo"/>.</param>
        /// <param name="vFrom">Mapping Range value start. The <paramref name="value"/> is assumed to be started from here.</param>
        /// <param name="vTo">Mapping Range value end. The <paramref name="value"/> is assumed to be ending in here.</param>
        /// <param name="value">The value that is the 'time' parameter. Goes between <paramref name="to"/>-&gt;<paramref name="vTo"/>.</param>
        public static float Map(float from, float to, float vFrom, float vTo, float value)
        {
            if (value <= vFrom)
                return from;
            else if (value >= vTo)
                return to;

            // a + ((b - a) * t) but goofy
            return from + ((to - from) * ((value - vFrom) / (vTo - vFrom)));
        }

        // ------------------
        // - Vector Utility -
        // ------------------
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
                return Vector3.one * float.NaN;
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
                return cPoint0 + (v2 * 0.5f) - (pd2 * d);

            return cPoint0 + (v2 * 0.5f) + (pd2 * d);
        }

        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector4 target)
        {
            // This is faster than 'for looping' the Vector4
            if (target.x > target.y && target.x > target.z && target.x > target.w)
                return target.x;
            if (target.y > target.x && target.y > target.z && target.y > target.w)
                return target.y;
            if (target.z > target.x && target.z > target.y && target.z > target.w)
                return target.z;
            if (target.w > target.x && target.w > target.y && target.w > target.z)
                return target.w;

            return target.x;
        }
        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector3 target)
        {
            if (target.x > target.y && target.x > target.z)
                return target.x;
            if (target.y > target.x && target.y > target.z)
                return target.y;
            if (target.z > target.y && target.z > target.x)
                return target.z;

            return target.x;
        }
        /// <summary>
        /// Returns the largest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MaxAxis(this Vector2 target)
        {
            if (target.x > target.y)
                return target.x;
            if (target.y > target.x)
                return target.y;

            return target.x;
        }

        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector4 target)
        {
            if (target.x < target.y && target.x < target.z && target.x < target.w)
                return target.x;
            if (target.y < target.x && target.y < target.z && target.y < target.w)
                return target.y;
            if (target.z < target.x && target.z < target.y && target.z < target.w)
                return target.z;
            if (target.w < target.x && target.w < target.y && target.w < target.z)
                return target.w;

            return target.x;
        }
        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector3 target)
        {
            if (target.x < target.y && target.x < target.z)
                return target.x;
            if (target.y < target.x && target.y < target.z)
                return target.y;
            if (target.z < target.y && target.z < target.x)
                return target.z;

            return target.x;
        }
        /// <summary>
        /// Returns the smallest axis in the <paramref name="target"/>.
        /// </summary>
        public static float MinAxis(this Vector2 target)
        {
            if (target.x < target.y)
                return target.x;
            if (target.y < target.x)
                return target.y;

            return target.x;
        }

        #region Obsolete
        /// <inheritdoc cref="MaxAxis"/>
        [Obsolete("Use 'value.MaxAxis();' instead.", false)]
        public static float GetBiggestAxis(this Vector3 target)
        {
            return target.MaxAxis();
        }
        /// <inheritdoc cref="MaxAxis"/>
        [Obsolete("Use 'value.MaxAxis();' instead.", false)]
        public static float GetBiggestAxis(this Vector2 target)
        {
            return target.MaxAxis();
        }
        /// <inheritdoc cref="MinAxis"/>
        [Obsolete("Use 'value.MinAxis();' instead.", false)]
        public static float GetSmallestAxis(this Vector3 target)
        {
            return target.MinAxis();
        }
        /// <inheritdoc cref="MinAxis"/>
        [Obsolete("Use 'value.MinAxis();' instead.", false)]
        public static float GetSmallestAxis(this Vector2 target)
        {
            return target.MinAxis();
        }
        #endregion

        /// <summary>
        /// Get the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector3 GetAxisVector(this TransformAxis axisConstraint, Vector3 current)
        {
            Vector3 v = current;
            switch (axisConstraint)
            {
                case TransformAxis.None:
                    return Vector3.zero;

                case TransformAxis.XAxis:
                    v.y = 0f;
                    v.z = 0f;
                    break;
                case TransformAxis.YAxis:
                    v.x = 0f;
                    v.z = 0f;
                    break;
                case TransformAxis.ZAxis:
                    v.x = 0f;
                    v.y = 0f;
                    break;
                case TransformAxis.XYAxis:
                    v.z = 0f;
                    break;
                case TransformAxis.YZAxis:
                    v.x = 0f;
                    break;
                case TransformAxis.XZAxis:
                    v.y = 0f;
                    break;

                default:
                case TransformAxis.XYZAxis:
                    return v;
            }

            return v;
        }
        /// <summary>
        /// Sets or removes the <see cref="Vector3"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector3 SetAxisVector(this TransformAxis axisConstraint, Vector3 current, Vector3 setCurrent)
        {
            Vector3 v = current;
            switch (axisConstraint)
            {
                case TransformAxis.None:
                    return Vector3.zero;

                case TransformAxis.XAxis:
                    v.x = setCurrent.x;
                    break;
                case TransformAxis.YAxis:
                    v.y = setCurrent.y;
                    break;
                case TransformAxis.ZAxis:
                    v.z = setCurrent.z;
                    break;
                case TransformAxis.XYAxis:
                    v.x = setCurrent.x;
                    v.y = setCurrent.y;
                    break;
                case TransformAxis.YZAxis:
                    v.y = setCurrent.y;
                    v.z = setCurrent.z;
                    break;
                case TransformAxis.XZAxis:
                    v.x = setCurrent.x;
                    v.z = setCurrent.z;
                    break;

                default:
                case TransformAxis.XYZAxis:
                    return setCurrent;
            }

            return v;
        }
        /// <summary>
        /// Get the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector2 GetAxisVector(this TransformAxis2D axisConstraint, Vector2 current)
        {
            Vector3 v = current;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    return Vector2.zero;

                default:
                case TransformAxis2D.XYAxis:
                    break;

                case TransformAxis2D.XAxis:
                    v.y = 0f;
                    break;
                case TransformAxis2D.YAxis:
                    v.x = 0f;
                    break;
            }

            return v;
        }
        /// <summary>
        /// Sets or removes the <see cref="Vector2"/> values according to <paramref name="axisConstraint"/>.
        /// </summary>
        public static Vector2 SetAxisVector(this TransformAxis2D axisConstraint, Vector2 current, Vector2 value)
        {
            Vector3 v = current;
            switch (axisConstraint)
            {
                case TransformAxis2D.None:
                    return Vector2.zero;

                case TransformAxis2D.XAxis:
                    v.x = value.x;
                    break;
                case TransformAxis2D.YAxis:
                    v.y = value.y;
                    break;
                default:
                case TransformAxis2D.XYAxis:
                    v.x = value.x;
                    v.y = value.y;
                    break;
            }

            return v;
        }

        /// <summary>
        /// Converts <see cref="Vector2"/> to positive values.
        /// </summary>
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Math.Abs(v.x), Math.Abs(v.y));
        }
        /// <summary>
        /// Converts <see cref="Vector3"/> to positive values.
        /// </summary>
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z));
        }
        /// <summary>
        /// Converts <see cref="Vector4"/> to positive values.
        /// </summary>
        public static Vector4 Abs(this Vector4 v)
        {
            return new Vector4(Math.Abs(v.x), Math.Abs(v.y), Math.Abs(v.z), Math.Abs(v.w));
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
                return Quaternion.identity; // Get identity rotation without logging message to the console
            Vector3 upwards = new Vector3(matrix.m01, matrix.m11, matrix.m21);

            return Quaternion.LookRotation(forward, upwards);
        }
        /// <summary>
        /// Returns the scaling values from the matrix.
        /// <br>Does not work properly with <b>negatively</b> scaled matrices.</br>
        /// </summary>
        public static Vector3 GetScale(this Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = matrix.GetColumn(0).magnitude;
            scale.y = matrix.GetColumn(1).magnitude;
            scale.z = matrix.GetColumn(2).magnitude;
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
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetMatrix(this Transform transform, Matrix4x4 matrix, Space space = Space.World)
        {
            matrix.Deconstruct(out Vector3 position, out Quaternion rotation, out Vector3 scale);

            switch (space)
            {
                case Space.World:
                    transform.SetPositionAndRotation(position, rotation);
                    Vector3 parentLossyScale = Vector3.one;
                    if (transform.parent != null)
                        parentLossyScale = transform.parent.lossyScale;

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
                    throw new ArgumentException(string.Format("[Additionals::SetMatrix] Failed setting matrix : Parameter 'space={0}' is invalid.", space));
            }
        }
    }
}
