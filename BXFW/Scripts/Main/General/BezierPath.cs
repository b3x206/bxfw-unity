using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Represents a bezier point.
    /// </summary>
    [Serializable]
    public struct BezierPoint : IEquatable<BezierPoint>
    {
        public Vector3 position;
        public Vector3 handle;

        /// <summary>
        /// Constructs a bezier point with points.
        /// </summary>
        /// <param name="pos">Base Position</param>
        /// <param name="hnd">Handle Position</param>
        public BezierPoint(Vector3 pos, Vector3 hnd)
        {
            position = pos;
            handle = hnd;
        }

        /// <summary>
        /// Deconstructs the point.
        /// </summary>
        public void Deconstruct(out Vector3 position, out Vector3 handle)
        {
            position = this.position;
            handle = this.handle;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public bool Equals(BezierPoint other)
        {
            if (other == null)
            {
                return false;
            }

            return this == other;
        }
        public static bool operator ==(BezierPoint p1, BezierPoint p2)
        {
            return p1.position == p2.position && p1.handle == p2.handle;
        }
        public static bool operator !=(BezierPoint p1, BezierPoint p2)
        {
            return !(p1 == p2);
        }

        public override int GetHashCode()
        {
            int hashCode = 69728019;
            unchecked
            {
                hashCode = (hashCode * -1521134295) + position.GetHashCode();
                hashCode = (hashCode * -1521134295) + handle.GetHashCode();
            }

            return hashCode;
        }
        public override string ToString()
        {
            return $"position={position}, handle={handle}";
        }
    }

    /// <summary>
    /// Represents a bezier path.
    /// </summary>
    [Serializable]
    public class BezierPath : ICollection<BezierPoint>, IEnumerable<BezierPath.FromToValues>, IEquatable<List<BezierPoint>>, IEquatable<BezierPath>
    {
        /// <summary>
        /// Contains a path enumeration from-&gt;to value.
        /// </summary>
        public struct FromToValues : IEquatable<FromToValues>
        {
            public Vector3 from;
            public Vector3 to;

            public FromToValues(Vector3 fromPoint, Vector3 toPoint)
            {
                from = fromPoint;
                to = toPoint;
            }

            public bool Equals(FromToValues other)
            {
                return from == other.from && to == other.to;
            }

            /// <summary>
            /// Returns a offseted value.
            /// </summary>
            public FromToValues Offset(Vector3 offset)
            {
                return new FromToValues(from + offset, to + offset);
            }
        }

        /// <summary>
        /// List of the generated path points.
        /// </summary>
        [HideInInspector] public List<Vector3> PathPoints = new List<Vector3>();
        /// <summary>
        /// Target list of the generated path points.
        /// <br>Pass the positions of the objects for bezier here.</br>
        /// </summary>
        [SerializeField] private List<BezierPoint> m_ControlPoints = new List<BezierPoint>();
        /// <summary>
        /// The amount of <see cref="Vector3"/>'s to use for the <see cref="PathPoints"/>.
        /// </summary>
        [SerializeField, Clamp(0, int.MaxValue)]
        private int m_GeneratePointCount = 100;
        /// <inheritdoc cref="m_GeneratePointCount"/>
        public int GeneratePointCount
        {
            get { return m_GeneratePointCount; }
            set
            {
                m_GeneratePointCount = Mathf.Max(0, value);
                UpdatePath();
            }
        }

        /// <summary>
        /// The amount of segments.
        /// <br>Equal to <see cref="m_ControlPoints"/>.Count / 2 because of the <see cref="BezierPoint"/> type containing 2 values.</br>
        /// <br>Use <see cref="m_ControlPoints"/>.Count / 3 if you want <see cref="Vector3"/> typed control points.</br>
        /// </summary>
        public int PerPointSegments => m_ControlPoints.Count / 2;
        /// <summary>
        /// Count of the control points that this path has.
        /// </summary>
        public int Count => m_ControlPoints.Count;
        /// <summary>
        /// Is always <see langword="false"/>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates a blank path, with no generation.
        /// </summary>
        public BezierPath()
        {
            PathPoints = new List<Vector3>(GeneratePointCount);
        }
        /// <summary>
        /// Creates a bezier path with control points <paramref name="controlPoints"/>.
        /// </summary>
        public BezierPath(List<BezierPoint> controlPoints)
        {
            PathPoints = new List<Vector3>(GeneratePointCount);

            m_ControlPoints = new List<BezierPoint>(controlPoints);
        }

        /// <summary>
        /// Returns whether if the <see cref="BezierPath"/> has any control points.
        /// </summary>
        public static implicit operator bool(BezierPath path)
        {
            return path.Count > 0;
        }

        /// <summary>
        /// Returns a interpolation at time <paramref name="t"/>.
        /// <br><paramref name="t"/> is clamped between 0~1.</br>
        /// </summary>
        public Vector3 Interpolate(float t)
        {
            // Clamp
            if (t <= 0f)
            {
                return PathPoints[0];
            }
            if (t >= 1f)
            {
                return PathPoints[PathPoints.Count - 1];
            }

            // TODO : proof of concept
            // int targetControlSetIndex = Mathf.CeilToInt((3 / segments) * t) - 1;
            // 
            // Vector3 p0 = generatePathPoints[targetControlSetIndex];
            // Vector3 p1 = generatePathPoints[targetControlSetIndex + 1];
            // Vector3 p2 = generatePathPoints[targetControlSetIndex + 2];
            // Vector3 p3 = generatePathPoints[targetControlSetIndex + 3];
            // 
            // Vector3 vTarget = BezierPathCalculation(p0, p1, p2, p3, t);
            // return vTarget;

            // (meh, this will do for now, even though it's not ""particularly efficient"" + normalized)

            // Lerp
            float targetIndexLerp = (t / 1f) * (PathPoints.Count - 1); // Subtract from actual path point count for full lerp
            int targetIndex = Mathf.FloorToInt(targetIndexLerp);
            float targetInterpBetweenPoints = Mathf.Abs(targetIndex - targetIndexLerp);

            Vector3 startv = PathPoints[targetIndex];
            Vector3 endv = PathPoints[targetIndex + 1];

            return Vector3.Lerp(startv, endv, targetInterpBetweenPoints);
        }
        /// <summary>
        /// Creates a curve from a set of control points.
        /// </summary>
        public void CreatePath(List<BezierPoint> controlPoints)
        {
            m_ControlPoints = new List<BezierPoint>(controlPoints);

            UpdatePath();
        }
        /// <summary>
        /// Updates the given <see cref="PathPoints"/>.
        /// </summary>
        public void UpdatePath()
        {
            PathPoints.Clear();

            // Nothing to generate if there's only 1 control point.
            if (m_ControlPoints.Count <= 1)
            {
                return;
            }

            // No points to generate per segment
            if (GeneratePointCount <= 0)
            {
                return;
            }

            for (int s = 0; s < m_ControlPoints.Count - 1; s++)
            {
                Vector3 p0 = m_ControlPoints[s].position;
                Vector3 h0 = m_ControlPoints[s].handle;

                Vector3 p1 = m_ControlPoints[s + 1].position;
                Vector3 h1 = m_ControlPoints[s + 1].handle;

                if (s == 0)
                {
                    PathPoints.Add(MathUtility.BezierInterpolate(p0, h0, p1, h1, 0.0f));
                }

                // p should be <= for last point check
                // Generate bezier path.
                if (PathPoints.Capacity < GeneratePointCount * s)
                {
                    PathPoints.Capacity = GeneratePointCount * s; // lazily add more point capacity
                }

                for (int p = 0; p <= (GeneratePointCount / PerPointSegments); p++)
                {
                    float t = (1.0f / (GeneratePointCount / PerPointSegments)) * p;

                    Vector3 point = MathUtility.BezierInterpolate(p0, h0, p1, h1, t);
                    PathPoints.Add(point);
                }
            }
        }

        /// <summary>
        /// This compares the list to the target path points to generate from.
        /// </summary>
        public bool Equals(List<BezierPoint> other)
        {
            if (other is null)
            {
                return false;
            }

            return m_ControlPoints.SequenceEqual(other);
        }
        /// <summary>
        /// This compares the list to the target path points to generate from.
        /// <br>If this returns true then the paths are identical.</br>
        /// </summary>
        public bool Equals(BezierPath path)
        {
            return Equals(path.m_ControlPoints);
        }

        /// <summary>
        /// Allows for accessing and setting a bezier point.
        /// </summary>
        public BezierPoint this[int index]
        {
            get { return m_ControlPoints[index]; }
            set
            {
                m_ControlPoints[index] = value;
                UpdatePath();
            }
        }

        public void Add(BezierPoint item)
        {
            m_ControlPoints.Add(item);
            UpdatePath();
        }
        public void Clear()
        {
            m_ControlPoints.Clear();
            PathPoints.Clear();
        }
        public bool Contains(BezierPoint item)
        {
            return m_ControlPoints.Contains(item);
        }
        public void CopyTo(BezierPoint[] array, int arrayIndex)
        {
            m_ControlPoints.CopyTo(array, arrayIndex);
        }
        public bool Remove(BezierPoint item)
        {
            bool result = m_ControlPoints.Remove(item);
            if (result)
            {
                UpdatePath();
            }
            return result;
        }

        /// <summary>
        /// The iterator that iterates the containing bezier points.
        /// </summary>
        IEnumerator<BezierPoint> IEnumerable<BezierPoint>.GetEnumerator()
        {
            return m_ControlPoints.GetEnumerator();
        }
        /// <summary>
        /// The iterator that is the evaluated line.
        /// </summary>
        public IEnumerator<FromToValues> GetEnumerator()
        {
            // Use a for loop instead of a (while - int loop)
            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                yield return new FromToValues(PathPoints[i], PathPoints[i + 1]);
            }
        }
        /// <summary>
        /// Returns the internal enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
