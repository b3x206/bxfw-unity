using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ReCalculateCubeUV)), CanEditMultipleObjects]
    public class ReCalculateCubeUVEditor : Editor
    {
        /// <summary>
        /// Directory to save.
        /// </summary>
        private readonly string SAVE_DIRECTORY = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Prefabs/GenMeshes");
        /// <summary>
        /// The absolute dir + file name to save.
        /// </summary>
        private string GetFileSaveDir(ReCalculateCubeUV Target)
        {
            return string.Format("{0}.asset", Path.Combine(SAVE_DIRECTORY, Target.cubeMeshName));
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
                var UFilePath = SAVE_DIRECTORY.Split('.')[0];
                // Remove the junk. (Add 1 to omit '/', Subtract 7 to include the 'Assets' directory on the start)
                UFilePath = UFilePath.Substring(UFilePath.IndexOf('/', Directory.GetCurrentDirectory().Length) - 6);

                return UFilePath;
            }
        }
        /// <summary>
        /// The relative dir + file name to save. (with .asset appended for full file name)
        /// </summary>
        private string GetUFileSave(ReCalculateCubeUV Target)
        {
            return string.Format("{0}.asset", Path.Combine(UDirSave, Target.cubeMeshName));
        }

        #region Utility
        /// <summary>
        /// Creates & puts the uv cube into specified assets folder.
        /// </summary>
        private Mesh CreateUVCubeToAssets(ReCalculateCubeUV Target)
        {
            var TargetMesh = Target.GetCalculateMesh();

            AssetDatabase.CreateAsset(TargetMesh, GetUFileSave(Target));
            AssetDatabase.SaveAssets();

            return TargetMesh;
        }
        /// <summary>
        /// Gets the UV Cube created.
        /// <br>If there is no UV cube, it also creates one (using <see cref="CreateUVCubeToAssets(ReCalculateCubeUV)"/>).</br>
        /// </summary>
        private Mesh GetUVCube(ReCalculateCubeUV Target)
        {
            // Get Directory
            if (!Directory.Exists(SAVE_DIRECTORY))
                Directory.CreateDirectory(SAVE_DIRECTORY);

            if (string.IsNullOrEmpty(Target.cubeMeshName))
            {
                Target.cubeMeshName = "Cube_InstUV";
            }

            string[] FileNames = Directory.GetFiles(SAVE_DIRECTORY, string.Format("{0}.*", Target.cubeMeshName));

            // At least one matching file exists, the file name is files[0].
            // Use that cube as mesh.
            if (FileNames.Length > 0)
            {
                Mesh MeshAssign = AssetDatabase.LoadAssetAtPath<Mesh>(GetUFileSave(Target));

                if (MeshAssign != null)
                {
                    MeshAssign = CreateUVCubeToAssets(Target);
                    AssetDatabase.SaveAssets();
                    return MeshAssign;
                }
                else
                {
                    // This error shouldn't happen.
                    Debug.LogError(string.Format("[ReCalculcateCubeUV] The loaded mesh was null. There's no such asset named as '{0}'?", Target.cubeMeshName));
                    return null;
                }
            }
            else // Create and save the cube.
            {
                return CreateUVCubeToAssets(Target);
            }
        }
        #endregion

        private const int MAX_UNIQUE_NAME_ITERS = ushort.MaxValue;
        public override void OnInspectorGUI()
        {
            var targets = base.targets.Cast<ReCalculateCubeUV>().ToArray();

            // m_Script & Mesh Name Field
            base.OnInspectorGUI();

            // Draw line
            GUIAdditionals.DrawUILineLayout(new Color(.5f, .5f, .5f));

            bool drawnMeshAssetExistsWarning = false;
            bool drawnWrapModePropertyInfo = false;
            foreach (var target in targets)
            {
                var TargetRenderer = target.GetComponent<Renderer>();
                var TargetMeshFilter = target.GetComponent<MeshFilter>();

                if (TargetRenderer == null || TargetMeshFilter == null)
                    continue;

                // Only show this HelpBox if the target mesh wasn't applied to this object also.
                if (File.Exists(GetFileSaveDir(target)) && 
                    TargetMeshFilter.sharedMesh != null && TargetMeshFilter.sharedMesh.name != target.cubeMeshName &&
                    !drawnMeshAssetExistsWarning)
                {
                    EditorGUILayout.HelpBox(string.Format("[ReCalculateCubeUV] A mesh asset with the same name {0} exists. Please change the name to prevent data loss.", target.cubeMeshName), MessageType.Warning);
                    drawnMeshAssetExistsWarning = true;
                }
                if (TargetRenderer.sharedMaterial != null && TargetRenderer.sharedMaterial.mainTexture != null &&
                    TargetRenderer.sharedMaterial.mainTexture.wrapMode != TextureWrapMode.Repeat && 
                    !drawnWrapModePropertyInfo)
                {
                    EditorGUILayout.HelpBox("[ReCalculateCubeUV] Ensure the texture you added has the wrapMode property set to TextureWrapMode.Repeat.", MessageType.Warning);
                    drawnWrapModePropertyInfo = true;
                }
            }

            if (drawnMeshAssetExistsWarning)
            {
                int uniqueIndex = 0;
                if (GUILayout.Button("Fix Warning"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("fix warnings on uv editor");
                    int undoID = Undo.GetCurrentGroup();
                    foreach (var target in targets)
                    {
                        var TargetMeshFilter = target.GetComponent<MeshFilter>();

                        while (File.Exists(GetFileSaveDir(target)) && TargetMeshFilter.sharedMesh.name != target.cubeMeshName)
                        {
                            Undo.RecordObject(target, string.Empty);
                            target.cubeMeshName += string.Format("_{0}", uniqueIndex + 1);

                            if (uniqueIndex >= MAX_UNIQUE_NAME_ITERS)
                            {
                                throw new System.OperationCanceledException(string.Format("[ReCalculateCubeUVEditor::OnInspectorGUI::FixWarning] Name uniqueifiying iterations exceeded maximum ({0}).", MAX_UNIQUE_NAME_ITERS));
                            }

                            uniqueIndex++;
                        }
                    }
                    Undo.CollapseUndoOperations(undoID);
                }
            }

            // Button to recalculate
            if (GUILayout.Button("Re-Calculate Cube texture."))
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var target = targets[i];
                    var TargetRenderer = target.GetComponent<Renderer>();
                    var TargetMeshFilter = target.GetComponent<MeshFilter>();

                    if (TargetRenderer == null || TargetMeshFilter == null)
                    {
                        Debug.LogWarning(string.Format("[ReCalculateCubeUV::ReCalculate] Failed to recalculate on object \"{0}\". This object does not have a MeshFilter or a MeshRenderer.", target.GetPath()));
                        continue;
                    }

                    // Changing multiple objects?
                    if (i >= 1)
                    {
                        // Make name unique if it exists
                        if (File.Exists(GetFileSaveDir(target)))
                        {
                            target.cubeMeshName += string.Format("_{0}", i);
                        }
                    }

                    // Prefab check (if no prefab object)
                    if (PrefabUtility.GetCorrespondingObjectFromSource(target.gameObject) == null)
                    {
                        // Set mesh & texture
                        TargetMeshFilter.mesh = GetUVCube(target);
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
                        TargetMeshFilter.mesh = GetUVCube(target);
                        var tex = TargetRenderer.sharedMaterial.mainTexture;
                        if (tex != null)
                        {
                            if (tex.wrapMode != TextureWrapMode.Repeat)
                            {
                                tex.wrapMode = TextureWrapMode.Repeat;
                            }
                        }

                        // Apply Prefab
                        EditorUtility.SetDirty(target.gameObject);
                        PrefabUtility.ApplyObjectOverride(TargetMeshFilter, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(target), InteractionMode.UserAction);
                    }
                }
            }
        }
    }
}