using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BXFW.SceneManagement
{
    /// <summary>
    /// Allows for scripts to reference a unity scene.
    /// <br>This class is useful to keep scene indexes in other script, without making them weak links.</br>
    /// </summary>
    [Serializable]
    public sealed class UnitySceneReference
    {
        [SerializeField] private string sceneEditorPath; // Another setting meant to be editor written, but can be kept in runtime. (As it's a 'SceneManager' method)
        [SerializeField] private bool sceneLoadable;     // Editor writable only readonly setting

        /// <summary>
        /// The scene, loaded from the given editor path.
        /// <br>See <see cref="SceneLoadable"/> and <see cref="SceneIndex"/> for </br>
        /// </summary>
        public Scene Scene
        {
            get
            {
                return SceneManager.GetSceneByPath(sceneEditorPath);
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

                return Scene.buildIndex;
            }
        }
        /// <summary>
        /// Returns the given scene name.
        /// </summary>
        public string SceneName
        {
            get
            {
                return Scene.name;
            }
        }
        /// <summary>
        /// Returns the editor location of the scene reference. (Read-only)
        /// </summary>
        public string ScenePath
        {
            get
            {
                return sceneEditorPath;
            }
        }
        /// <summary>
        /// Returns whether if the gathered scene is loadable.
        /// <br>If not, the <see cref="Scene"/> may not return null (or it can be null), but the given scene can't be loaded at all.</br>
        /// <br>This is because unity requires scenes to be registered on Player Settings.</br>
        /// </summary>
        public bool SceneLoadable
        {
            get
            {
                return Scene.buildIndex >= 0 && Scene.buildIndex < SceneManager.sceneCountInBuildSettings && sceneLoadable;
            }
        }
    }
}