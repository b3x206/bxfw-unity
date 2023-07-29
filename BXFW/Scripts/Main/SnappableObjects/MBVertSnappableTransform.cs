using System;
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
        // 'SIsSetup' is not set properly 
        // Because of this, the mesh vertices were always generated when we wanted to get 'VertPoints'
        // And that allocates a lotta garbo (it's mesh related but the array that we create also has it's issues.

        /// <summary>
        /// Boolean to check if whether a <see cref="SnappableCubeTransform"/> is setup.
        /// </summary>
        public bool SIsSetup { get; private set; } = false;
        protected Matrix4x4 PrevTrsMatrix { get; private set; } = Matrix4x4.zero;

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

        private List<Vector3> m_VertPoints = new List<Vector3>(32);
        /// <summary>
        /// Initilazed vertice points on the mesh filter.
        /// </summary>
        public List<Vector3> VertPoints
        {
            get
            {
                //if (transform.hasChanged || !SIsSetup)
                if (transform.localToWorldMatrix != PrevTrsMatrix ||
                    !SIsSetup)
                {
                    UpdateSnapPoints();
                    PrevTrsMatrix = transform.localToWorldMatrix;
                }

                return m_VertPoints;
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

        private List<Vector3> meshVertsList;
        /// <summary>Updates the snap points.</summary>
        public void UpdateSnapPoints()
        {
#if UNITY_EDITOR
            if ((Application.isPlaying && CurrentMeshFilter.mesh == null) || CurrentMeshFilter.sharedMesh == null)
                return;
#else
            if (CurrentMeshFilter.mesh == null)
                return;
#endif

            meshVertsList ??= new List<Vector3>(CurrentMeshFilter.mesh.vertexCount);
            CurrentMeshFilter.VerticesToWorldSpaceNoAlloc(meshVertsList); // Generate verts without allocating garbage

            m_VertPoints ??= new List<Vector3>(meshVertsList.Count / 3);  // There are always 2 excess verts.
            m_VertPoints.Clear();

            for (int i = 0; i < meshVertsList.Count; i++)
            {
                Vector3 currentVert = meshVertsList[i];
                bool isDuplicateVert = m_VertPoints.Contains(currentVert);
                
                // Do not add if it exists in the array.
                if (isDuplicateVert)
                { 
                    continue;
                }

                m_VertPoints.Add(currentVert);
            }

            SIsSetup = true;
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

                SnapHelper.position = transformTarget.VertPoints[pointTarget];
                transformTarget.transform.SetParent(SnapHelper);
                SnapHelper.position = VertPoints[pointThis];
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = VertPoints[pointThis];
                transform.SetParent(SnapHelper);
                SnapHelper.position = transformTarget.VertPoints[pointTarget];
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

                SnapHelper.position = transformTarget.SnapPoints[pointTarget].position;
                transformTarget.transform.SetParent(SnapHelper);
                SnapHelper.position = VertPoints[pointThis];
                transformTarget.transform.SetParent(PrevParent);
            }
            else
            {
                var PrevParent = transform.parent;

                SnapHelper.position = VertPoints[pointThis];
                transform.SetParent(SnapHelper);
                SnapHelper.position = transformTarget.SnapPoints[pointTarget].position;
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
            SnapHelper.position = VertPoints[pointThis];
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

            for (int i = 0; i < m_VertPoints.Count; i++)
            {
                // Draw with offset for duplicate vert.
                GizmoUtility.DrawText($"V:{i}", m_VertPoints[i], Color.green, true);
                Gizmos.color = new Color(1f, 0f, 0f, .4f);
                // Gizmos
                Gizmos.DrawSphere(m_VertPoints[i], .05f);
            }

            Gizmos.color = prevGColor;
        }
#endif
    }
}