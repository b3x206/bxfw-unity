#if UNITY_EDITOR
// This is an editor exclusive namespace.
using BXFW.Tools.Editor;
#endif

using System;
using System.Collections;
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
        /// Boolean to check if whether a <see cref="SnappableCubeTransform"/> is setup.
        /// </summary>
        public bool Snappable_IsSetup { get; private set; } = false;

        [SerializeField, HideInInspector] private MeshFilter m_CurrentMeshFilter;
        /// <summary>Current mesh filter on this object that the script is attached to.</summary>
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

        private List<Vector3> m_Snappable_VertPoints;
        /// <summary>
        /// Initilazed vertice points on the mesh filter.
        /// </summary>
        public List<Vector3> Snappable_VertPoints
        {
            get
            {
                if (transform.hasChanged || !Snappable_IsSetup)
                {
                    UpdateSnapPoints();
                }

                return m_Snappable_VertPoints;
            }
        }

        /// <summary>Helper method for getting the world position of the vertices.</summary>
        public Vector3[] VerticesToWorldPos(MeshFilter filter)
        {
            Mesh mesh;
#if UNITY_EDITOR
            mesh = !Application.isPlaying ? filter.sharedMesh : filter.mesh;
#else
            mesh = filter.mesh;
#endif

            if (mesh == null)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::VerticesToWorldPos] Mesh on MeshFilter '{0}' is null!", filter.GetPath()));
                return null;
            }

            Matrix4x4 localToWorld = filter.transform.localToWorldMatrix;
            Vector3[] vertPos = new Vector3[mesh.vertices.Length];

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                vertPos[i] = localToWorld.MultiplyPoint3x4(mesh.vertices[i]);
            }

            return vertPos;
        }

        /// <summary>Updates the snap points.</summary>
        /// TODO : This probably runs terrible on certain occasions.
        public void UpdateSnapPoints()
        {
            var verts = VerticesToWorldPos(CurrentMeshFilter);

            if (verts == null)
            {
                Debug.LogError("[MBVertSnappableTransform::UpdateSnapPoints] Given verts from local method 'VerticesToWorldPos' is null. Make sure the mesh filter's mesh isn't null.");
                return;
            }

            if (m_Snappable_VertPoints == null)
            {
                m_Snappable_VertPoints = new List<Vector3>();
            }
            m_Snappable_VertPoints.Clear();

            var listDrawnVerts = new List<Vector3>(verts.Length / 3); // There are always 2 excess verts.

            for (int i = 0; i < verts.Length; i++)
            {
                var currentVert = verts[i];
                bool duplicateVert = listDrawnVerts.Contains(currentVert);

                if (duplicateVert)
                { continue; }

                listDrawnVerts.Add(currentVert);
                m_Snappable_VertPoints.Add(currentVert);
            }
        }

        protected Action OnSnapTransformCall;
        protected Action OnAlignTransformCall;

        #region Extension Functions
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <param name="pointTarget">Snap point for target.</param>
        /// <param name="snapTarget">Given Snap. Change this to swap the object to move.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(MBVertSnappableTransform transformTarget, int pointThis, int pointTarget, bool snapTarget = false)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null) return false;

            // there is no better way to check whether the scale is valid for snapping.
            if (transformTarget.transform.localScale.x == 0f || transformTarget.transform.localScale.y == 0f || transformTarget.transform.localScale.z == 0f ||
                transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, transformTarget.name));
                return false;
            }

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            if (snapTarget)
            {
                var PrevParent = transformTarget.transform.parent;

                SnapHelper.position = transformTarget.Snappable_VertPoints[pointTarget];
                transformTarget.transform.SetParent(SnapHelper);
                SnapHelper.position = Snappable_VertPoints[pointThis];
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = Snappable_VertPoints[pointThis];
                transform.SetParent(SnapHelper);
                SnapHelper.position = transformTarget.Snappable_VertPoints[pointTarget];
                transform.SetParent(PrevParent);
            }

            Destroy(SnapHelper.gameObject);
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
            if (transformTarget == null) return false;

            if (transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, transformTarget.name));
                return false;
            }

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            if (snapTarget)
            {
                var PrevParent = transformTarget.transform.parent;

                SnapHelper.position = transformTarget.Snappable_SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(SnapHelper);
                SnapHelper.position = Snappable_VertPoints[pointThis];
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = Snappable_VertPoints[pointThis];
                transform.SetParent(SnapHelper);
                SnapHelper.position = transformTarget.Snappable_SnapPoints[pointTarget].position;
                transform.SetParent(PrevParent);
            }

            Destroy(SnapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }
        /// <summary>
        /// Snaps the given transform to this transform. (Depending on the <paramref name="SnapGiven"/>)
        /// </summary>
        /// <param name="transformTarget">Transform target. The default object to move.</param>
        /// <param name="pointThis">Snap point for object that calls this method.</param>
        /// <returns>Whether if the SnapTransform operation was successful.</returns>
        public bool SnapTransform(Transform transformTarget, int pointThis, Vector3 transformTargetPosOffset = default)
        {
            // Check target. (if null do nothing)
            if (transformTarget == null) return false;
            if (transform.localScale.x == 0f || transform.localScale.y == 0f || transform.localScale.z == 0f)
            {
                Debug.LogError(string.Format("[MBVertSnappableTransform::SnapTransform] Scale is invalid for snapping. Objects requested for snap : \"{0}->{1}\"", name, transformTarget.name));
                return false;
            }

            /// -- Create snap helper --
            /// --> So here's the way snap helper works:
            /// 1: Create the gameobject,
            /// 2: Put this gameobject to the same place as the corner of the platform,
            /// 3: Parent the platform to this gameobject,
            /// 4: Place this gameobject to the target corner,
            /// 5: Unparent the platform.
            /// Rinse and repeat. 
            var SnapHelper = new GameObject("SnapHelper").transform;

            // Difference here is that we snap the target object instead of this object.
            var PrevParent = transformTarget.transform.parent;

            SnapHelper.position = transformTarget.position + transformTargetPosOffset;
            transformTarget.transform.SetParent(SnapHelper);
            SnapHelper.position = Snappable_VertPoints[pointThis];
            transformTarget.transform.SetParent(PrevParent);

            Destroy(SnapHelper.gameObject);
            OnSnapTransformCall?.Invoke();

            return true;
        }
        #endregion

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (transform.hasChanged)
            {
                UpdateSnapPoints();
            }

            var prevGColor = Gizmos.color;

            for (int i = 0; i < m_Snappable_VertPoints.Count; i++)
            {
                // Draw with offset for duplicate vert.
                GizmoUtility.DrawText($"V:{i}", m_Snappable_VertPoints[i], Color.green, true);
                Gizmos.color = new Color(1f, 0f, 0f, .4f);
                // Gizmos
                Gizmos.DrawSphere(m_Snappable_VertPoints[i], .05f);
            }

            Gizmos.color = prevGColor;
        }
#endif
    }
}