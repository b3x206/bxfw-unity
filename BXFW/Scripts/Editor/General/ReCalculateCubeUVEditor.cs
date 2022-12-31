using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ReCalculateCubeUV))]
    public class ReCalculateCubeUVEditor : Editor
    {
        /// <summary>
        /// Directory to save.
        /// </summary>
        private readonly string DirSave = $"{System.IO.Directory.GetCurrentDirectory()}/Assets/Prefabs/GenMeshes";
        /// <summary>
        /// Returns the unity save directory.
        /// Like : <c>Assets/Directory/GenMeshes</c>, does not include the <c>C:/</c> stuff on the start.
        /// </summary>
        private string UDirSave
        {
            get
            {
                // Remove the extension
                var UFilePath = DirSave.Split('.')[0];
                // Remove the junk.
                UFilePath = UFilePath.Substring(UFilePath.IndexOf('/', System.IO.Directory.GetCurrentDirectory().Length) + 1 /*(omit character '/')*/);

                return UFilePath;
            }
        }
        private ReCalculateCubeUV Target
        {
            get
            {
                return (ReCalculateCubeUV)target;
            }
        }

        #region Utility
        /// <summary>
        /// Creates & puts the uv cube into specified assets folder.
        /// </summary>
        private Mesh CreateUVCubeToAssets()
        {
            var TargetMesh = Target.GetCalculateMesh();

            AssetDatabase.CreateAsset(TargetMesh, $"{UDirSave}/{Target.CubeMeshName}.asset");
            AssetDatabase.SaveAssets();

            return TargetMesh;
        }
        /// <summary>
        /// Gets the UV Cube created.
        /// <br>If there is no UV cube, it also creates one using <see cref="CreateUVCube()"/>.</br>
        /// </summary>
        private Mesh GetUVCubeFromAssets()
        {
            // Get Directory
            if (!System.IO.Directory.Exists(DirSave))
                System.IO.Directory.CreateDirectory(DirSave);

            if (string.IsNullOrEmpty(Target.CubeMeshName))
            {
                Target.CubeMeshName = "Cube_InstUV";
            }

            string[] FileNames = System.IO.Directory.GetFiles(DirSave, $"{Target.CubeMeshName}.*");

            // At least one matching file exists, the file name is files[0].
            // Use that cube as mesh.
            if (FileNames.Length > 0)
            {
                var MeshAssign = AssetDatabase.LoadAssetAtPath<Mesh>($"{UDirSave}/{Target.CubeMeshName}.asset");

                if (MeshAssign != null)
                {
                    MeshAssign = CreateUVCubeToAssets();
                    AssetDatabase.SaveAssets();
                    return MeshAssign;
                }
                else
                {
                    // This error theoretically shouldn't happen.
                    Debug.LogError(string.Format("[ReCalcCubeTexture] The loaded mesh was null. There's no such asset named as {0}??", Target.CubeMeshName));
                    return null;
                }
            }
            else // Create and save the cube.
            {
                var TargetMesh = CreateUVCubeToAssets();

                return TargetMesh;
            }
        }
        #endregion

        public override void OnInspectorGUI()
        {
            // m_Script & Mesh Name Field
            base.OnInspectorGUI();

            // Draw line
            GUIAdditionals.DrawUILineLayout(new Color(.5f, .5f, .5f));

            // Button to recalculate
            if (GUILayout.Button("Re-Calculate Cube texture."))
            {
                // Get References
                var TargetMeshFilter = Target.GetComponent<MeshFilter>();
                var TargetRenderer = Target.GetComponent<Renderer>();

                // If it's not a prefab.
                // if (PrefabUtility.GetCorrespondingObjectFromSource(Target.gameObject) == null)
                if (!Target.IsPrefab)
                {
                    // Set mesh & texture
                    TargetMeshFilter.mesh = GetUVCubeFromAssets();
                    var tex = TargetRenderer.sharedMaterial.mainTexture;
                    if (tex != null)
                    {
                        if (tex.wrapMode != TextureWrapMode.Repeat)
                        {
                            tex.wrapMode = TextureWrapMode.Repeat;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[ReCalcCubeTexture::Calculate<in editor>] Make sure the texture you added has the wrapMode property set to TextureWrapMode.Repeat.");
                    }
                }
                else // It's a prefab, do a different procedure
                {
                    if (EditorUtility.DisplayDialog("[ReCalcCubeTexture]",
                        "Warning : You are about to modify a prefab. Are you sure you want to do it?",
                        "Yes", "No"))
                    {
                        // Set Prefab specific flags
                        PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMeshFilter);
                        EditorUtility.SetDirty(Target.gameObject);

                        // Set mesh & texture
                        TargetMeshFilter.mesh = GetUVCubeFromAssets();
                        var tex = TargetRenderer.sharedMaterial.mainTexture;
                        if (tex != null)
                        {
                            if (tex.wrapMode != TextureWrapMode.Repeat)
                            {
                                tex.wrapMode = TextureWrapMode.Repeat;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[ReCalcCubeTexture::Calculate<in editor>] Make sure the texture you added has the wrapMode property set to TextureWrapMode.Repeat.");
                        }

                        // Apply Prefab
                        PrefabUtility.ApplyObjectOverride
                            (TargetMeshFilter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Target), InteractionMode.UserAction);
                    }
                }
            }
        }
    }
}