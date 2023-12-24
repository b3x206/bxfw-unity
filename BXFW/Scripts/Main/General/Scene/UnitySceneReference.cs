using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BXFW.SceneManagement
{
    /// <summary>
    /// Allows for scripts to reference a unity scene, but only in editor. (has no support for 'StreamingAssets' or 'AssetBundles')
    /// <br>The scene should be registered in the build settings to be loadable, otherwise only the GUID of the scene is contained.</br>
    /// </summary>
    [Serializable]
    public sealed class UnitySceneReference
    {
        // Serialize the GUID as it never changes (unless the scene was deleted, etc.)
        [SerializeField] private string sceneGUID;
        [SerializeField] private int sceneIndex = -1;

        private bool CheckSceneIndexValidity()
        {
#if UNITY_EDITOR
            for (int i = 0; i < UnityEditor.EditorBuildSettings.scenes.Length; i++)
            {
                var editorScn = UnityEditor.EditorBuildSettings.scenes[i];

                // Check current index validity
                if (editorScn.guid.ToString() == sceneGUID)
                {
                    if (sceneIndex != i)
                    {
                        // Modify index if invalid
                        sceneIndex = i;
                    }
                    
                    return true;
                }
            }
#else
            // UnitySceneReferenceList exists on built applications
            for (int i = 0; i < UnitySceneReferenceList.Instance.entries.Length; i++)
            {
                var editorScn = UnitySceneReferenceList.Instance.entries[i];

                // Check current index validity
                if (editorScn.editorGUID == sceneGUID)
                {
                    if (sceneIndex != i)
                    {
                        // Modify index if invalid
                        sceneIndex = i;
                    }

                    return true;
                }
            }
#endif
            // does not exist
            sceneIndex = -1;
            return false;
        }

        /// <summary>
        /// The scene entry. Contains the appopriate GUID and editor path.
        /// </summary>
        public SceneEntry Entry
        {
            get
            {
                if (!SceneLoadable)
                {
                    return null;
                }

#if UNITY_EDITOR
                var eScn = UnityEditor.EditorBuildSettings.scenes[sceneIndex];
                return new SceneEntry(eScn.path, eScn.guid.ToString());
#else
                return UnitySceneReferenceList.Instance.entries[sceneIndex];
#endif
            }
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
                // Basically, we can't access the scene array, we can only know if the scene was loaded.
                if (!CheckSceneIndexValidity())
                {
                    return default;
                }
                
                Scene loadedScene = SceneManager.GetSceneByBuildIndex(sceneIndex);

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

                if (Entry == null)
                {
                    return string.Empty;
                }

                string extRemoved = Entry.editorPath.Remove(Entry.editorPath.IndexOf(".unity"));
                return extRemoved.Substring(extRemoved.LastIndexOf('/') + 1);
            }
        }
        /// <summary>
        /// Returns whether if the gathered scene is loadable and valid.
        /// <br>If not, the <see cref="CurrentOpenScene"/> may not return null (or it can be null), but the given scene can't be loaded at all.</br>
        /// <br>This is because unity requires scenes to be registered on Player Settings.</br>
        /// </summary>
        public bool SceneLoadable
        {
            get
            {
                CheckSceneIndexValidity();
                return SceneIndex >= 0 && SceneIndex < SceneManager.sceneCountInBuildSettings;
            }
        }
    }
}
