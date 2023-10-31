using System;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Contains mesh related utility.
    /// </summary>
    public static class MeshUtility
    {
        /// <summary>
        /// Converts the vertices of <paramref name="mesh"/> into world space using <paramref name="matrixSpace"/>.
        /// <br>Values are assigned into the 'vertsArray', the 'vertsArray' will be overwritten by <see cref="Mesh.GetVertices(List{Vector3})"/>.</br>
        /// </summary>
        public static void VerticesToMatrixSpaceNoAlloc(Mesh mesh, Matrix4x4 matrixSpace, List<Vector3> vertsArray)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh), "[MeshUtility::VerticesToWorldSpaceNoAlloc] Passed 'mesh' argument is null.");
            }

            // This method throws anyway if the 'vertsArray' is null, no need to check.
            mesh.GetVertices(vertsArray);

            // Modify all elements
            for (int i = 0; i < vertsArray.Count; i++)
            {
                vertsArray[i] = matrixSpace.MultiplyPoint3x4(vertsArray[i]);
            }
        }
        /// <summary>
        /// Converts the vertices of <paramref name="mesh"/> into world space using <paramref name="matrixSpace"/>.
        /// <br>Allocates a new <see cref="List{T}"/> every time it's called.</br>
        /// </summary>
        public static List<Vector3> VerticesToMatrixSpace(Mesh mesh, Matrix4x4 matrixSpace)
        {
            List<Vector3> array = new List<Vector3>(mesh.vertexCount);
            VerticesToMatrixSpaceNoAlloc(mesh, matrixSpace, array);
            return array;
        }

        /// <summary>
        /// Converts the <paramref name="filter"/>'s vertices to the world space, using the same transform attached to the <paramref name="filter"/>.
        /// <br>The result is outputted in <paramref name="vertsArray"/>.</br>
        /// </summary>
        public static void VerticesToWorldSpaceNoAlloc(this MeshFilter filter, List<Vector3> vertsArray)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "[MeshUtility::VerticesToWorldSpaceNoAlloc] Passed 'filter' argument is null.");
            }

            Mesh mesh;
#if UNITY_EDITOR
            mesh = !Application.isPlaying ? filter.sharedMesh : filter.mesh;
#else
            mesh = filter.mesh;
#endif
            VerticesToMatrixSpaceNoAlloc(mesh, filter.transform.localToWorldMatrix, vertsArray);
        }
        /// <summary>
        /// Converts vertex position to world position on the mesh.
        /// <br>Applies matrix transformations of <paramref name="filter"/>.transform, so rotations / scale / other stuff are also calculated.</br>
        /// </summary>
        public static List<Vector3> VerticesToWorldSpace(this MeshFilter filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "[MeshUtility::VerticesToWorldSpace] Passed 'filter' argument is null.");
            }

            Mesh mesh;
#if UNITY_EDITOR
            mesh = !Application.isPlaying ? filter.sharedMesh : filter.mesh;
#else
            mesh = filter.mesh;
#endif
            return VerticesToMatrixSpace(mesh, filter.transform.localToWorldMatrix);
        }
        /// <summary>
        /// Converts vertex position to world position on the mesh.
        /// <br>Applies matrix transformations of <paramref name="coll"/>.transform, so rotations / scale / other stuff are also calculated.</br>
        /// </summary>
        public static Vector3[] VerticesToWorldSpace(this BoxCollider coll)
        {
            if (coll == null)
            {
                throw new ArgumentNullException(nameof(coll), "[MeshUtility::VerticesToWorldSpace] Passed 'coll' argument is null.");
            }

            Vector3[] vertices = new Vector3[8];
            Matrix4x4 thisMatrix = coll.transform.localToWorldMatrix;
            Quaternion storedRotation = coll.transform.rotation;
            coll.transform.rotation = Quaternion.identity;

            Vector3 extents = coll.bounds.extents;
            vertices[0] = thisMatrix.MultiplyPoint3x4(extents);
            vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, extents.z));
            vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, extents.y, -extents.z));
            vertices[3] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, extents.y, -extents.z));
            vertices[4] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
            vertices[5] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
            vertices[6] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
            vertices[7] = thisMatrix.MultiplyPoint3x4(-extents);

            coll.transform.rotation = storedRotation;
            return vertices;
        }
        /// <summary>
        /// Converts world vertices to local mesh space.
        /// <br>Useful for chaning vertex position on the world space using <see cref="VerticesToWorldSpace(MeshFilter)"/>, and re-applying it back to the target mesh.</br>
        /// </summary>
        public static Vector3[] WorldVertsToLocalSpace(this MeshFilter filter, Vector3[] worldV)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "[MeshUtility::WorldVertsToLocalSpace] Passed 'filter' argument is null.");
            }

            Mesh vertsMesh = Application.isPlaying ? filter.mesh : filter.sharedMesh;

            if (vertsMesh == null)
            {
                throw new ArgumentException("[MeshUtility::WorldVertsToLocalSpace] Passed 'filter's mesh value is null.", nameof(filter.mesh));
            }

            if (vertsMesh.vertexCount != worldV.Length)
            {
                throw new ArgumentException("[MeshUtility::WorldVertsToLocalSpace] The vertex count of passed array is not equal with mesh's vertex count.", nameof(worldV));
            }

            Matrix4x4 worldToLocal = filter.transform.worldToLocalMatrix;
            Vector3[] localV = new Vector3[worldV.Length];

            for (int i = 0; i < worldV.Length; i++)
            {
                localV[i] = worldToLocal.MultiplyPoint3x4(worldV[i]);
            }

            return localV;
        }
    }
}
