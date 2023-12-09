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
    public sealed class BezierPoint : IEnumerable<BezierPoint>
    {
        public Vector3 position;
        public Vector3 handle;

        /// <summary>
        /// Parent path reference set when the point is added into the <see cref="BezierPath"/>.
        /// </summary>
        [HideInInspector, SerializeField] internal BezierPath parentPath;
        
        public BezierPoint()
        { }

        /// <summary>
        /// Constructs a bezier point with points.
        /// </summary>
        /// <param name="pos">Base Position</param>
        /// <param name="hnd">Handle Position</param>
        public BezierPoint(Vector3 pos, Vector3 hnd)
        {
            position = pos;
            handle = hnd;

            parentPath = null;
        }

        /// <summary>
        /// Index of the point. (If it's owned / added to a <see cref="BezierPath"/>)
        /// </summary>
        public int Index()
        {
            if (parentPath == null)
            {
                Debug.LogWarning("[BezierPath] Tried to get bezier path count without parent. Please assign into a parent.");
                return -1;
            }

            return parentPath.ControlPoints.IndexOf(this);
        }

        /// <summary>
        /// Count of the <see cref="BezierPoint"/>'s before this point.
        /// <br>Dependent on the <see cref="Index"/> method.</br>
        /// </summary>
        public int CountBefore()
        {
            if (parentPath == null)
            {
                Debug.LogWarning("[BezierPath] Tried to get bezier path count without parent. Please assign into a parent.");
                return -1;
            }

            return parentPath.ControlPoints.Count - (Index() + 1);
        }

        public IEnumerator<BezierPoint> GetEnumerator()
        {
            if (parentPath == null)
            {
                Debug.LogWarning("[BezierPath] Tried to iterate bezier path without parent. Please assign into a parent.");
                yield break;
            }

            // Subtract 1 as we add up to this point
            int startIndex = Index() - 1;

            for (int i = startIndex; i < parentPath.ControlPoints.Count; i++)
            {
                yield return parentPath.ControlPoints[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public static bool operator ==(BezierPoint p1, BezierPoint p2)
        {
            return (p1.position == p2.position && p1.handle == p2.handle);
        }
        public static bool operator !=(BezierPoint p1, BezierPoint p2)
        {
            return !(p1 == p2);
        }
        public override int GetHashCode()
        {
            int hashCode = 69728019;
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            hashCode = hashCode * -1521134295 + handle.GetHashCode();

            return hashCode;
        }

        public static implicit operator (Vector3, Vector3)(BezierPoint p)
        {
            return (p.position, p.handle);
        }
    }

    // TODO : Use this as an scriptable object to hold a proper reference.
    // TODO 2 : Iterate using a KeyValuePair because tuples are too new
    /// <summary>
    /// Represents a bezier path.
    /// </summary>
    [Serializable]
    public class BezierPath : IEnumerable<(Vector3, Vector3)>, IEquatable<List<BezierPoint>>
    {
        /// <summary>
        /// List of the generated path points.
        /// <br>Can iterate and interpolate between for path following.</br>
        /// </summary>
        [HideInInspector] public List<Vector3> PathPoints = new List<Vector3>();
        /// <summary>
        /// Target list of the generated path points.
        /// <br>Pass the positions of the objects for bezier here.</br>
        /// </summary>
        [SerializeField] private List<BezierPoint> m_ControlPoints = new List<BezierPoint>();
        /// <summary>
        /// Target list of the generated path points.
        /// <br>Pass the positions of the objects for bezier here.</br>
        /// </summary>
        public List<BezierPoint> ControlPoints
        {
            get
            {
                // Initialize generation path points
                // (becuase delegates / events are not serialized)
                if (m_ControlPoints != null)
                {
                    for (int i = 0; i < m_ControlPoints.Count; i++)
                    {
                        m_ControlPoints[i].parentPath = this;
                    }

                    // UpdateCurve();

                    //mControlPoints.Changed += (int i, BezierPoint prev, BezierPoint set) => { if (prev != set) { UpdateCurve(); } };
                    //mControlPoints.Updated += UpdateCurve;
                }

                return m_ControlPoints;
            }
            private set
            {
                m_ControlPoints = value;

                // Initialize generation path points
                if (m_ControlPoints != null)
                {
                    for (int i = 0; i < m_ControlPoints.Count; i++)
                    {
                        m_ControlPoints[i].parentPath = this;
                    }

                    //mControlPoints.Changed += (int i, BezierPoint prev, BezierPoint set) => { if (prev != set) { UpdateCurve(); } };
                    //mControlPoints.Updated += UpdateCurve;                
                }
            }
        }

        /// <summary>
        /// The amount of segments.
        /// <br>Equal to <see cref="ControlPoints"/>.Count / 2</br>
        /// <br>Use <see cref="ControlPoints"/>.Count / 3 if you want <see cref="Vector3"/> only control points.</br>
        /// </summary>
        public int Segments => ControlPoints.Count / 2;
        /// <summary>
        /// The amount of points. (verts)
        /// </summary>
        public int pointCount = 100;

        public BezierPath()
        {
            PathPoints = new List<Vector3>(pointCount);
        }
        public BezierPath(List<BezierPoint> controlPoints)
        {
            PathPoints = new List<Vector3>(pointCount);

            ControlPoints = new List<BezierPoint>(controlPoints);
        }

        public void DeletePath()
        {
            PathPoints.Clear();
            ControlPoints.Clear();
        }

        /// <summary>
        /// Calculate bezier path.
        /// <br><sub><sup>i have no idea how this works</sup></sub></br>
        /// </summary>
        /// <returns>The calculated bezier? idk.</returns>
        private Vector3 BezierPathCalculation(Vector3 p0, Vector3 h0, Vector3 p1, Vector3 h1, float t)
        {
            // You could change this for Vector2's, but
            // Vector3 can convert to Vector2 and the other way around so
            
            // Some necessary math
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            // Point 1
            Vector3 B = uuu * p0;
            // Handle(s)
            B += 3.0f * uu * t * h0;
            B += 3.0f * u * tt * h1;
            // Point 2
            B += ttt * p1;

            return B;
        }

        /// TODO : Make use of the 'BezierPathCalculation' method instead with control fn
        /// <summary>
        /// Returns a interpolation at time <paramref name="t"/>.
        /// <br>This is clamped between 0~1.</br>
        /// </summary>
        public Vector3 Interpolate(float t)
        {
            // Clamp
            if (t <= 0f)
            { return PathPoints[0]; }
            if (t >= 1f)
            { return PathPoints[PathPoints.Count - 1]; }

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
            float targetIndexFloat = (t / 1f) * (PathPoints.Count - 1); // Subtract from actual path point count for full lerp
            int targetIndex = Mathf.FloorToInt(targetIndexFloat);
            float targetInterpBetweenPoints = Mathf.Abs(targetIndex - targetIndexFloat);

            Vector3 startv = PathPoints[targetIndex];
            Vector3 endv = PathPoints[targetIndex + 1];

            return Vector3.Lerp(startv, endv, targetInterpBetweenPoints);
        }
        /// <summary>
        /// Creates a curve from a set of control points.
        /// </summary>
        /// <param name="controlPoints"></param>
        public void CreateCurve(List<BezierPoint> controlPoints)
        {
            ControlPoints = new List<BezierPoint>(controlPoints);

            UpdateCurve();
        }
        public void UpdateCurve()
        {
            PathPoints.Clear();

            // Nothing to generate if there's only 1 control point.
            if (ControlPoints.Count <= 1)
            {
                return;
            }

            for (int s = 0; s < ControlPoints.Count - 1; s++)
            {
                Vector3 p0 = ControlPoints[s].position;
                Vector3 h0 = ControlPoints[s].handle;

                Vector3 p1 = ControlPoints[s + 1].position;
                Vector3 h1 = ControlPoints[s + 1].handle;

                if (s == 0)
                {
                    PathPoints.Add(BezierPathCalculation(p0, h0, p1, h1, 0.0f));
                }

                // p should be <= for last point check
                // Generate beizered points.
                if (PathPoints.Capacity < pointCount * s)
                {
                    PathPoints.Capacity = pointCount * s; // lazily add more point capacity
                }

                for (int p = 0; p <= (pointCount / Segments); p++)
                {
                    float t = (1.0f / (pointCount / Segments)) * p;

                    Vector3 point = BezierPathCalculation(p0, h0, p1, h1, t);
                    PathPoints.Add(point);
                }
            }
        }

        /// <summary>
        /// Returns a iterable tuple thing. (for plotting)
        /// <br>First parameter : Start vertex | Last Parameter : End vertex</br>
        /// </summary>
        public IEnumerator<(Vector3, Vector3)> GetEnumerator()
        {
            // Use a for loop instead of a (while - int loop)
            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                yield return (PathPoints[i], PathPoints[i + 1]);
            }
        }

        /// <summary>
        /// Returns the internal enumerator.
        /// <br>This is NOT meant to be used. <c><see langword="foreach"/></c> the one that returns a tuple.</br>
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
        /// Returns whether if the <see cref="BezierPath"/> has any control points.
        /// </summary>
        public static implicit operator bool(BezierPath p)
        {
            return p.ControlPoints.Count > 0;
        }
    }
}