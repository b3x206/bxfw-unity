using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BXFW.SceneManagement
{
    /// <summary>
    /// Allows for scripts to reference a unity scene.
    /// <br>This class is useful to keep scene indexes in other script, without making them weak links. (as long as <see cref="SceneLoadable"/> is true, from the editor)</br>
    /// </summary>
    [Serializable]
    public sealed class UnitySceneReference
    {
        [SerializeField] private string sceneEditorPath; // Another setting meant to be editor written, but can be kept in runtime. (As it's a 'SceneManager' method)
        [SerializeField] private int sceneIndex = -1;    // Hmm, SceneManager.GetSceneByPath only works if the scene is open, so we really need the index.
        [SerializeField] private bool sceneLoadable;     // Editor writable only readonly setting

        private void CheckSceneIndexValidity()
        {
#if UNITY_EDITOR
            for (int i = 0; i < UnityEditor.EditorBuildSettings.scenes.Length; i++)
            {
                var editorScn = UnityEditor.EditorBuildSettings.scenes[i];

                // Check current index validity
                if (editorScn.path == sceneEditorPath)
                {
                    if (sceneIndex != i)
                    {
                        // Modify index if invalid
                        sceneIndex = i;
                    }
                    
                    return;
                }
            }

            Debug.LogWarning(string.Format("[UnitySceneReference::CheckSceneIndexValidity] Given scene path {0} does not exist. Most likely this scene has been moved or re-named. Please re-assign the scene reference.", sceneEditorPath));
#else
            // UnitySceneReferenceList exists on built applications
            for (int i = 0; i < UnitySceneReferenceList.Instance.entries.Length; i++)
            {
                var editorScn = UnitySceneReferenceList.Instance.entries[i];

                // Check current index validity
                if (editorScn.editorPath == sceneEditorPath)
                {
                    if (sceneIndex != i)
                    {
                        // Modify index if invalid
                        sceneIndex = i;
                    }

                    return;
                }
            }

            Debug.LogError(string.Format("[UnitySceneReference::CheckSceneIndexValidity] Given scene path {0} does not exist. Most likely this scene has been moved or re-named. The scene will fail!", sceneEditorPath));
#endif
        }

        /// <summary>
        /// The scene, loaded from the given editor path.
        /// <br>Only returns a valid scene if the scene was loaded.</br>
        /// <br>See <see cref="SceneLoadable"/> and <see cref="SceneIndex"/> for </br>
        /// </summary>
        public Scene CurrentOpenScene
        {
            get
            {
                // These methods only work if the scene is currently open
                // see : https://forum.unity.com/threads/scenemanager-getscenebyxxx-is-totally-broken.387958/
                // Basically, we can't access the scene array, we can only know if we load all scenes one by one.
                Scene loadedScene = SceneLoadable ? SceneManager.GetSceneByBuildIndex(sceneIndex) : SceneManager.GetSceneByPath(sceneEditorPath);

                //if (!loadedScene.IsValid())
                //    Debug.LogWarning("[UnitySceneReference::(get)Scene] Given scene is not valid because it is not loaded.");

                CheckSceneIndexValidity();

                return loadedScene;
            }
        }
        /// <summary>
        /// Returns the scene index, assigned in the editor.
        /// <br>Most likely throws an exception or returns -1 if this scene does not exist.</br>
        /// </summary>
        public int SceneIndex
        {
            get
            {
                CheckSceneIndexValidity();

                return sceneIndex;
            }
        }
        /// <summary>
        /// Returns the given scene name.
        /// </summary>
        public string SceneName
        {
            get
            {
                CheckSceneIndexValidity();

                return CurrentOpenScene.name;
            }
        }
        /// <summary>
        /// Returns the editor location of the scene reference. (Read-only)
        /// </summary>
        public string ScenePath
        {
            get
            {
                CheckSceneIndexValidity();

                return sceneEditorPath;
            }
        }
        /// <summary>
        /// Returns whether if the gathered scene is loadable.
        /// <br>If not, the <see cref="CurrentOpenScene"/> may not return null (or it can be null), but the given scene can't be loaded at all.</br>
        /// <br>This is because unity requires scenes to be registered on Player Settings.</br>
        /// </summary>
        public bool SceneLoadable
        {
            get
            {
                CheckSceneIndexValidity();

                return SceneIndex >= 0 && SceneIndex < SceneManager.sceneCountInBuildSettings && sceneLoadable;
            }
            set
            {
                sceneLoadable = value;
            }
        }
    }
}