using System;

namespace BXFW.SceneManagement
{
    /// <summary>
    /// An entry for the scene.
    /// <br>Scene index of the entry is the index of target in <see cref="UnitySceneReferenceList.entries"/>.</br>
    /// </summary>
    [Serializable]
    public sealed class SceneEntry
    {
        public string editorPath;

        public SceneEntry() { }
        public SceneEntry(string path) { editorPath = path; }
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