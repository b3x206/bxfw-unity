using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// <c>Debug Only</c> : Intercepts the 'OnDestroy' event for pooled object and tries to prevent it.
    /// <br>
    /// This is not meant to be added to usual objects.
    /// It will prevent destruction of it unless the <see cref="isDestroyedWithCleanupIntent"/> flag is set.
    /// </br>
    /// </summary>
    [AddComponentMenu(""), DisallowMultipleComponent]
    public class PoolObjectDestroyInterceptor : MonoBehaviour
    {
        /// <summary>
        /// This flag is set to true if the object is to be removed by either the <see cref="ObjectPooler"/> 
        /// or by the <see cref="OnApplicationQuit"/> callback.
        /// </summary>
        internal bool isDestroyedWithCleanupIntent = false;

#if UNITY_EDITOR || DEBUG
        // Need this type of workaround to fix the gazillion billion errors
#if UNITY_EDITOR
        private void PlayStateChanged(UnityEditor.PlayModeStateChange stateChange)
        {
            isDestroyedWithCleanupIntent = stateChange == UnityEditor.PlayModeStateChange.ExitingPlayMode || stateChange == UnityEditor.PlayModeStateChange.EnteredEditMode;
        }
#endif
        public void Initialize()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += PlayStateChanged;
#endif
        }
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= PlayStateChanged;
#endif
            isDestroyedWithCleanupIntent = true;
        }
        private void OnDestroy()
        {
            if (!Application.isPlaying || !ObjectPooler.CanUsePooler || isDestroyedWithCleanupIntent)
            {
                return;
            }

            throw new InvalidOperationException($"[PoolObjectDestroyInterceptor::OnDestroy] Must not destroy pooled object (on path {gameObject.GetPath()}), as it is never meant to be disposed except for explicit removal of pools (or OnApplicationQuit).");
        }
#endif
    }
}
