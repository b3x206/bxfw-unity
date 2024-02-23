using System;

namespace BXFW.SceneManagement
{
    /// <summary>
    /// An entry for the scene.
    /// <br>Scene index of the entry is the index of target in <see cref="UnitySceneReferenceList.entries"/>.</br>
    /// </summary>
    [Serializable]
    public sealed class SceneEntry : IEquatable<string>
    {
        /// <summary>
        /// GUID of the scene in the editor (as string).
        /// </summary>
        public string editorGUID;
        /// <summary>
        /// Path of the scene in the editor.
        /// </summary>
        public string editorPath;

        public SceneEntry() { }
        public SceneEntry(string path) { editorPath = path; }
        public SceneEntry(string path, string guid) { editorPath = path; editorGUID = guid; }

        public bool Equals(string other)
        {
            return editorGUID == other;
        }
        public override string ToString()
        {
            return string.Format("guid=0x{0}, path={1}", editorGUID, editorPath);
        }
    }

    /// <summary>
    /// List of the scenes, accessible from the runtime, but not from the editor. (Instance == null, regardless of play mode enabled or not)
    /// <br>For editor, use <see cref="UnityEditor.EditorBuildSettings"/>.</br>
    /// </summary>
    public sealed class UnitySceneReferenceList : ScriptableObjectSingleton<UnitySceneReferenceList>
    {
        public SceneEntry[] entries;
    }
}
