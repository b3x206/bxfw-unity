using System;

namespace BXFW.SceneManagement
{
    [Serializable]
    public sealed class SceneEntry
    {
        public string editorPath;

        public SceneEntry() { }
        public SceneEntry(string path) { editorPath = path; }
    }

    public sealed class UnitySceneReferenceList : ScriptableObjectSingleton<UnitySceneReferenceList>
    {
        public SceneEntry[] entries;
    }
}