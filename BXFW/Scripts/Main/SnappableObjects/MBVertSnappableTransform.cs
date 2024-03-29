﻿using System;
using System.Collections.Generic;

using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Snappable transform that uses mesh vertices.
    /// <br>Working principle is same as <see cref="MBCubeSnappableTransform"/>.</br>
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class MBVertSnappableTransform : MonoBehaviour
    {
        /// <summary>
        /// Boolean to check if this snappable transform is setup.
        /// </summary>
        public bool IsSetup { get; private set; } = false;
        /// <summary>
        /// The previous transformation matrix.
        /// <br>Used to check if the transform was changed in position, scale or rotation.</br>
        /// </summary>
        protected Matrix4x4 PrevTrsMatrix { get; private set; } = Matrix4x4.zero;

        [SerializeField, HideInInspector] private MeshFilter m_CurrentMeshFilter;
        /// <summary>
        /// Current mesh filter on this object that the script is also attached to.
        /// </summary>
        public MeshFilter CurrentMeshFilter
        {
            get
            {
                if (m_CurrentMeshFilter == null)
                {
                    m_CurrentMeshFilter = GetComponent<MeshFilter>();
                }

                return m_CurrentMeshFilter;
            }
        }

        /// <inheritdoc cref="VertPoints"/>
        private List<Vector3> m_VertPoints = new List<Vector3>(32);
        /// <summary>
        /// Vertex points of given <see cref="CurrentMeshFilter"/>'s mesh transformed into
        /// this <see cref="Component.transform"/>'s matrix.
        /// </summary>
        public List<Vector3> VertPoints
        {
            get
            {
                // S was never used to be setup
                // So because of this, even though the transform never changed we always update it's snap points
                // (oh and also the transform can move during gameplay so i made a no-alloc method to avoid gc.alloc)
                if (transform.localToWorldMatrix != PrevTrsMatrix || !IsSetup)
                {
                    UpdateSnapPoints();
                    PrevTrsMatrix = transform.localToWorldMatrix;
                }

                return m_VertPoints;
            }
        }

        /// <summary>
        /// List of the local vertices for mesh.
        /// </summary>
        private List<Vector3> m_MeshVerts;
        /// <summary>
        /// Vertices of the current mesh filter. (<see cref="CurrentMeshFilter"/>.mesh.vertices but cached)
        /// </summary>
        public List<Vector3> LocalVertPoints
        {
            get
            {
                if (transform.localToWorldMatrix != PrevTrsMatrix || !IsSetup)
                {
                    UpdateSnapPoints();
                    PrevTrsMatrix = transform.localToWorldMatrix;
                }

                return m_MeshVerts;
            }
        }

        private Mesh updatedPrevMesh;
        /// <summary>
        /// Updates the snap points.
        /// </summary>
        public void UpdateSnapPoints()
        {
            // Mesh used in the mesh filter
            Mesh mesh;
#if UNITY_EDITOR
            mesh = Application.isPlaying ? CurrentMeshFilter.mesh : CurrentMeshFilter.sharedMesh;
#else
            mesh = CurrentMeshFilter.mesh;
#endif
            if (mesh == null)
            {
                return;
            }

            m_VertPoints ??= new List<Vector3>(mesh.vertexCount / 3);  // There are always 2 excess verts.
            // Check existance of the vertex list or if the mesh updated.
            if (m_MeshVerts == null || updatedPrevMesh != mesh)
            {
                // Now this is the local space vertices array.
                m_VertPoints.Clear();
                
                // Get locals into m_VertPoints
                m_MeshVerts = new List<Vector3>(mesh.vertexCount);
                mesh.GetVertices(m_MeshVerts);
                // Remove from duplicates
                for (int i = 0; i < m_MeshVerts.Count; i++)
                {
                    // Remove duplicates
                    Vector3 vert = m_MeshVerts[i];
                    bool isDuplicate = m_VertPoints.Contains(vert);
                    
                    if (isDuplicate)
                    {
                        continue;
                    }

                    m_VertPoints.Add(vert);
                }

                // Clear + add the list values
                // And no need to regenerate the local verts again (unless the mesh was changed)
                m_MeshVerts.Clear();
                m_MeshVerts.AddRange(m_VertPoints);
            }

            // Found out that my mistake is that i apply the same matrix
            // transformations to the points that were already transformed.
            // --
            // Clear m_VertPoints from the values, now it's time for this array to be the world matrix points
            // Clearing this array is fine as it already chunks the list.
            m_VertPoints.Clear();
            // Apply the transformation points for the local 
            for (int i = 0; i < m_MeshVerts.Count; i++)
            {
                m_VertPoints.Add(transform.localToWorldMatrix.MultiplyPoint3x4(m_MeshVerts[i]));
            }

            IsSetup = true;
            updatedPrevMesh = mesh;
        }

        /// <summary>
        /// An action called when any of the <see cref="SnapTransform"/> methods were called.
        /// </summary>
        protected Action OnSnapTransformCall;
        
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="snappableTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <param name="pointTarget">Snap point for target.</param>
        /// <param name="snapTarget">Given Snap. Change this to swap the object to move.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(MBVertSnappableTransform snappableTarget, int pointThis, int pointTarget, bool snapTarget = false)
        {
            // Check target. (if null do nothing)
            if (snappableTarget == null)
            {
                return false;
            }

            // there is no better way to check whether the scale is valid for snapping.
            if (snappableTarget.transform.localScale.MinAxis() <= 0f ||
                transform.localScale.MinAxis() <= 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, snappableTarget.name));
                return false;
            }

            // nvm this works fine. No touchy. (matrix and vector math always betrays you)
            Transform snapHelper = new GameObject("SnapHelper").transform;

            if (!snapTarget)
            {
                var prevParent = transform.parent;

                snapHelper.position = VertPoints[pointThis];
                transform.SetParent(snapHelper, true);
                snapHelper.position = snappableTarget.VertPoints[pointTarget];
                transform.SetParent(prevParent, true);
            }
            // Difference here is that we snap the target object instead of this object.
            else
            {
                var prevParent = snappableTarget.transform.parent;

                snapHelper.position = snappableTarget.VertPoints[pointTarget];
                snappableTarget.transform.SetParent(snapHelper, true);
                snapHelper.position = VertPoints[pointThis];
                snappableTarget.transform.SetParent(prevParent, true);
            }

            Destroy(snapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <param name="pointTarget">Snap point for target.</param>
        /// <param name="snapTarget">Given Snap. Change this to swap the object to move.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(MBCubeSnappableTransform transformTarget, int pointThis, SnapPoint pointTarget, bool snapTarget = false)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null)
            {
                return false;
            }

            if (transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, transformTarget.name));
                return false;
            }

            var snapHelper = new GameObject("SnapHelper").transform;

            if (snapTarget)
            {
                var prevParent = transformTarget.transform.parent;

                snapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(snapHelper, true);
                snapHelper.position = VertPoints[pointThis];
                transformTarget.transform.SetParent(prevParent, true);
            }
            else
            {
                var prevParent = transform.parent;

                snapHelper.position = VertPoints[pointThis];
                transform.SetParent(snapHelper, true);
                snapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transform.SetParent(prevParent, true);
            }

            Destroy(snapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(Transform transformTarget, int pointThis, Vector3 pointOffset = default)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null)
            {
                return false;
            }

            if (transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, transformTarget.name));
                return false;
            }

            var snapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            var prevParent = transformTarget.transform.parent;

            snapHelper.position = transformTarget.position + pointOffset;
            transformTarget.transform.SetParent(snapHelper, true);
            snapHelper.position = VertPoints[pointThis];
            transformTarget.transform.SetParent(prevParent, true);

            Destroy(snapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (transform.hasChanged)
            {
                UpdateSnapPoints();
            }

            var prevGColor = Gizmos.color;

            for (int i = 0; i < m_VertPoints.Count; i++)
            {
                // Draw with offset for duplicate vert.
                GizmoUtility.DrawText($"V:{i}", m_VertPoints[i], Color.green, true);
                Gizmos.color = new Color(1f, 0f, 0f, .4f);
                // Gizmos
                Gizmos.DrawSphere(m_VertPoints[i], .05f);
            }

            // Debug only
            //for (int i = 0; i < m_MeshVerts.Count; i++)
            //{
            //    // Draw with offset for duplicate vert.
            //    GizmoUtility.DrawText($"LOCAL:{i}", Vector3.Scale(transform.rotation * m_MeshVerts[i], transform.lossyScale), Color.green, true);
            //    Gizmos.color = new Color(0f, 1f, 0f, .4f);
            //    // Gizmos
            //    Gizmos.DrawSphere(Vector3.Scale(transform.rotation * m_MeshVerts[i], transform.lossyScale), .07f);
            //}

            Gizmos.color = prevGColor;
        }
#endif
    }
}
