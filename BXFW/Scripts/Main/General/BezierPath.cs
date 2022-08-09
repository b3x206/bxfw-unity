using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Represents a bezier path.
    /// </summary>
    [Serializable]
    public sealed class BezierPath : IEnumerable<(Vector3, Vector3)>, IEquatable<List<Vector3>>
    {
        /// <summary>
        /// List of the generated path points.
        /// </summary>
        public List<Vector3> PathPoints = new List<Vector3>();
        /// <summary>
        /// Target list of the generated path points.
        /// <br>Pass the positions of the objects for bezier here.</br>
        /// </summary>
        [SerializeField] private ObservedList<Vector3> generatePathPoints = new ObservedList<Vector3>();
        /// <summary>
        /// Target list of the generated path points.
        /// <br>Pass the positions of the objects for bezier here.</br>
        /// </summary>
        public ObservedList<Vector3> GeneratePathPoints
        {
            get
            {
                return generatePathPoints;
            }
            private set
            {
                generatePathPoints = value;

                // Initialize generation path points
                if (generatePathPoints != null)
                {
                    generatePathPoints.Updated += () =>
                    {
                        UpdateCurve();
                    };
                }
            }
        }

        /// <summary>
        /// The amount of segments.
        /// <br>Equal to <see cref="GeneratePathPoints"/>.Count / 3</br>
        /// </summary>
        public int Segments => GeneratePathPoints.Count / 3;
        /// <summary>
        /// The amount of points. (verts)
        /// </summary>
        public int pointCount = 100;

        public BezierPath()
        {
            PathPoints = new List<Vector3>(pointCount);
        }
        public BezierPath(List<Vector3> controlPoints)
        {
            GeneratePathPoints = new ObservedList<Vector3>(controlPoints);
        }

        public void DeletePath()
        {
            PathPoints.Clear();
            GeneratePathPoints.Clear();
        }

        /// <summary>
        /// Calculate bezier path.
        /// <br><sub><sup>i have no idea how this works</sup></sub></br>
        /// </summary>
        /// <returns>The calculated bezier? idk.</returns>
        private Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            Vector3 B = uuu * p0;
            B += 3.0f * uu * t * p1;
            B += 3.0f * u * tt * p2;
            B += ttt * p3;

            return B;
        }

        /// TODO : Make use of the 'BezierPathCalculation' method instead with control fn
        /// <summary>
        /// Returns a interpolation at time <paramref name="t"/>.
        /// <br>This is clamped between 0~1.</br>
        /// </summary>
        public Vector3 Interpolated(float t)
        {
            // Clamp
            if (t <= 0f)
            { return PathPoints[0]; }
            if (t >= 1f)
            { return PathPoints[PathPoints.Count - 1]; }

            // TODO proof of concept
            // int targetControlSetIndex = Mathf.CeilToInt((3 / segments) * t) - 1;
            // 
            // Vector3 p0 = generatePathPoints[targetControlSetIndex];
            // Vector3 p1 = generatePathPoints[targetControlSetIndex + 1];
            // Vector3 p2 = generatePathPoints[targetControlSetIndex + 2];
            // Vector3 p3 = generatePathPoints[targetControlSetIndex + 3];
            // 
            // Vector3 vTarget = BezierPathCalculation(p0, p1, p2, p3, t);
            // return vTarget;

            // (meh, this will do for now, even though it's not ""particularly efficient"")

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
        public void CreateCurve(List<Vector3> controlPoints)
        {
            GeneratePathPoints = new ObservedList<Vector3>(controlPoints);

            UpdateCurve();
        }
        public void UpdateCurve()
        {
            PathPoints.Clear();

            if (GeneratePathPoints.Count <= 0)
                return;

            // segments = GeneratePathPoints.Count / 3;

            for (int s = 0; s < GeneratePathPoints.Count - 3; s += 3)
            {
                Vector3 p0 = GeneratePathPoints[s];
                Vector3 p1 = GeneratePathPoints[s + 1];
                Vector3 p2 = GeneratePathPoints[s + 2];
                Vector3 p3 = GeneratePathPoints[s + 3];

                if (s == 0)
                {
                    PathPoints.Add(BezierPathCalculation(p0, p1, p2, p3, 0.0f));
                }

                // p should be <= for last point check
                for (int p = 0; p <= (pointCount / Segments); p++)
                {
                    float t = (1.0f / (pointCount / Segments)) * p;

                    Vector3 point = BezierPathCalculation(p0, p1, p2, p3, t);
                    PathPoints.Add(point);
                }
            }
        }

        /// <summary>
        /// Returns a iterable tuple thing.
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
        public bool Equals(List<Vector3> other)
        {
            if (other is null)
                return false;

            return generatePathPoints.SequenceEqual(other);
        }
    }
}