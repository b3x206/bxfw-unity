using UnityEngine;

namespace BXFW
{
    /// Most of the work for this script is done on the editor script <see cref="ReCalcCubeTexEditor"/> anyway.
    /// Note that it can be recalculated in runtime too.
    /// <summary>
    /// Calculates the cube texture uv so it's tiled properly.
    /// </summary>
    [ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(Renderer))]
    public class ReCalculateCubeUV : MonoBehaviour
    {
        // -- Variables
        private Vector3 currentCalcScale;
        public string CubeMeshName = "Cube_InstUV";

        #region Utility
        /// <summary>
        /// Generates and sets up the mesh. (NOTE : This method should be called only in tempoary objects)
        /// <br>Use this to fix uv wrapping of a cube.</br>
        /// </summary>
        public void SetupCalculateMesh()
        {
            // Check whether re-calculation is necessary.
            if (currentCalcScale == transform.localScale) return;
            if (CheckForDefaultSize()) return;

            // Get the mesh filter.
            var filter = GetComponent<MeshFilter>();
            var tex = GetComponent<Renderer>().sharedMaterial.mainTexture;

            if (tex != null)
            {
                if (tex.wrapMode != TextureWrapMode.Repeat)
                {
                    tex.wrapMode = TextureWrapMode.Repeat;
                }
            }
            else
            {
                Debug.LogWarning("[ReCalculateCubeUV::Calculate] Make sure the texture you added has the wrapMode property set to TextureWrapMode.Repeat.");
            }

            filter.mesh = GetCalculateMesh();
        }

        /// <summary>
        /// Returns the mesh inside this object with extra checks.
        /// </summary>
        private Mesh GetMesh()
        {
            Mesh mesh;

#if UNITY_EDITOR
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
                Debug.LogWarning(string.Format("[ReCalculateCubeUV::GetMesh] The mesh was null on object \"{0}\". Assigned the default unity cube.", name));
            }
            var meshCopy = Instantiate(meshFilter.sharedMesh);
            mesh = meshFilter.mesh = meshCopy;
#else
            mesh = GetComponent<MeshFilter>().mesh;
#endif

            return mesh;
        }
        #endregion

        #region Main Calculation
        /// <summary>
        /// Generate a cube mesh, and get it back.
        /// </summary>
        /// <returns>Generated cube.</returns>
        public Mesh GetCalculateMesh()
        {
            currentCalcScale = transform.lossyScale;
            var mesh = GetMesh();
            mesh.uv = SetupUvMap(mesh.uv);
            mesh.name = CubeMeshName;

            return mesh;
        }
        /// <summary>
        /// Setup the uv's of the mesh.
        /// </summary>
        /// <param name="meshUVs">Mesh uv's of the default unity cube.</param>
        /// <returns>The appopriate mesh uv's for uniform texture scaling through the cube.</returns>
        private Vector2[] SetupUvMap(Vector2[] meshUVs)
        {
            var width = currentCalcScale.x;
            var depth = currentCalcScale.z;
            var height = currentCalcScale.y;

            if (meshUVs.Length != 24)
            {
                throw new System.ArgumentOutOfRangeException(
                    "[ReCalculateCubeUV::SetupUvMap] You are using a mesh with uv's different than the default unity cube."
                );
            }

            // Front
            meshUVs[2] = new Vector2(0, height);
            meshUVs[3] = new Vector2(width, height);
            meshUVs[0] = new Vector2(0, 0);
            meshUVs[1] = new Vector2(width, 0);

            // Back
            meshUVs[7] = new Vector2(0, 0);
            meshUVs[6] = new Vector2(width, 0);
            meshUVs[11] = new Vector2(0, height);
            meshUVs[10] = new Vector2(width, height);

            // Left
            meshUVs[19] = new Vector2(depth, 0);
            meshUVs[17] = new Vector2(0, height);
            meshUVs[16] = new Vector2(0, 0);
            meshUVs[18] = new Vector2(depth, height);

            // Right
            meshUVs[23] = new Vector2(depth, 0);
            meshUVs[21] = new Vector2(0, height);
            meshUVs[20] = new Vector2(0, 0);
            meshUVs[22] = new Vector2(depth, height);

            // Top
            meshUVs[4] = new Vector2(width, 0);
            meshUVs[5] = new Vector2(0, 0);
            meshUVs[8] = new Vector2(width, depth);
            meshUVs[9] = new Vector2(0, depth);

            // Bottom
            meshUVs[13] = new Vector2(width, 0);
            meshUVs[14] = new Vector2(0, 0);
            meshUVs[12] = new Vector2(width, depth);
            meshUVs[15] = new Vector2(0, depth);

            return meshUVs;
        }
        /// <summary>
        /// Creates a reference cube to check this object's size.
        /// </summary>
        /// <returns>Whether we have a default object.</returns>
        private bool CheckForDefaultSize()
        {
            if (currentCalcScale != Vector3.one) return false;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            DestroyImmediate(GetComponent<MeshFilter>());
            gameObject.AddComponent<MeshFilter>();
            GetComponent<MeshFilter>().sharedMesh = cube.GetComponent<MeshFilter>().sharedMesh;

            DestroyImmediate(cube);

            return true;
        }
        #endregion
    }
}