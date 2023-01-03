using System.IO;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ReCalculateCubeUV))]
    public class ReCalculateCubeUVEditor : Editor
    {
        private ReCalculateCubeUV Target
        {
            get
            {
                return (ReCalculateCubeUV)target;
            }
        }
        /// <summary>
        /// Directory to save.
        /// </summary>
        private readonly string DirSave = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Prefabs/GenMeshes");
        /// <summary>
        /// The absolute dir + file name to save.
        /// </summary>
        private string FileSave
        {
            get { return string.Format("{0}.asset", Path.Combine(DirSave, Target.CubeMeshName)); }
        }
        /// <summary>
        /// Returns the unity (relative) save directory.
        /// Like : <c>Assets/Directory/GenMeshes</c>, does not include the <c>C:/</c> stuff on the start.
        /// </summary>
        private string UDirSave
        {
            get
            {
                // Remove the extension
                var UFilePath = DirSave.Split('.')[0];
                // Remove the junk. (Add 1 to omit '/', Subtract 7 to include the 'Assets' directory on the start)
                UFilePath = UFilePath.Substring(UFilePath.IndexOf('/', Directory.GetCurrentDirectory().Length) - 6);

                return UFilePath;
            }
        }
        /// <summary>
        /// The relative dir + file name to save. (with .asset appended for full file name)
        /// </summary>
        private string UFileSave
        {
            get { return string.Format("{0}.asset", Path.Combine(UDirSave, Target.CubeMeshName)); }
        }

        #region Utility
        /// <summary>
        /// Creates & puts the uv cube into specified assets folder.
        /// </summary>
        private Mesh CreateUVCubeToAssets()
        {
            var TargetMesh = Target.GetCalculateMesh();

            AssetDatabase.CreateAsset(TargetMesh, UFileSave);
            AssetDatabase.SaveAssets();

            return TargetMesh;
        }
        /// <summary>
        /// Gets the UV Cube created.
        /// <br>If there is no UV cube, it also creates one using <see cref="CreateUVCubeToAssets()"/>.</br>
        /// </summary>
        private Mesh GetUVCubeFromAssets()
        {
            // Get Directory
            if (!Directory.Exists(DirSave))
                Directory.CreateDirectory(DirSave);

            if (string.IsNullOrEmpty(Target.CubeMeshName))
            {
                Target.CubeMeshName = "Cube_InstUV";
            }

            string[] FileNames = Directory.GetFiles(DirSave, string.Format("{0}.*", Target.CubeMeshName));

            // At least one matching file exists, the file name is files[0].
            // Use that cube as mesh.
            if (FileNames.Length > 0)
            {
                Mesh MeshAssign = AssetDatabase.LoadAssetAtPath<Mesh>(UFileSave);

                if (MeshAssign != null)
                {
                    MeshAssign = CreateUVCubeToAssets();
                    AssetDatabase.SaveAssets();
                    return MeshAssign;
                }
                else
                {
                    // This error shouldn't happen.
                    Debug.LogError(string.Format("[ReCalculcateCubeUV] The loaded mesh was null. There's no such asset named as '{0}'?", Target.CubeMeshName));
                    return null;
                }
            }
            else // Create and save the cube.
            {
                return CreateUVCubeToAssets();
            }
        }
        #endregion

        public override void OnInspectorGUI()
        {
            // m_Script & Mesh Name Field
            base.OnInspectorGUI();

            // Draw line
            GUIAdditionals.DrawUILineLayout(new Color(.5f, .5f, .5f));

            var TargetRenderer = Target.GetComponent<Renderer>();
            var TargetMeshFilter = Target.GetComponent<MeshFilter>();

            // Only show this HelpBox if the target mesh wasn't applied to this object also.
            if (File.Exists(FileSave) && TargetMeshFilter.sharedMesh.name != Target.CubeMeshName)
            {
                EditorGUILayout.HelpBox(string.Format("[ReCalculateCubeUV] A mesh asset with the same name {0} exists. Please change the name to prevent data loss.", Target.CubeMeshName), MessageType.Warning);
            }
            if (TargetRenderer.sharedMaterial != null && TargetRenderer.sharedMaterial.mainTexture != null && TargetRenderer.sharedMaterial.mainTexture.wrapMode != TextureWrapMode.Repeat)
            {
                EditorGUILayout.HelpBox("[ReCalculateCubeUV] Make sure the texture you added has the wrapMode property set to TextureWrapMode.Repeat.", MessageType.Warning);
            }

            // Button to recalculate
            if (GUILayout.Button("Re-Calculate Cube texture."))
            {
                // Prefab check (if no prefab object)
                if (PrefabUtility.GetCorrespondingObjectFromSource(Target.gameObject) == null)
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
                }
                else
                {
                    // Set Prefab specific flags
                    PrefabUtility.RecordPrefabInstancePropertyModifications(TargetMeshFilter);

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

                    // Apply Prefab
                    EditorUtility.SetDirty(Target.gameObject);
                    PrefabUtility.ApplyObjectOverride(TargetMeshFilter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(Target), InteractionMode.UserAction);  
                }
            }
        }
    }
}