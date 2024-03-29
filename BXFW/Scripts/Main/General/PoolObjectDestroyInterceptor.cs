using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// <c>Debug Only</c> : Intercepts the 'OnDestroy' event for pooled object and listens for destruction events.
    /// <br>
    /// This is not meant to be added to usual objects.
    /// It will prevent destruction of it unless the <see cref="isDestroyedWithCleanupIntent"/> flag is set.
    /// </br>
    /// </summary>
    [AddComponentMenu(""), DisallowMultipleComponent]
    public sealed class PoolObjectDestroyInterceptor : MonoBehaviour
    {
        /// <summary>
        /// This flag is set to true if the object is to be removed by either the <see cref="ObjectPooler"/> 
        /// or by the <see cref="OnApplicationQuit"/> callback.
        /// </summary>
        internal bool isDestroyedWithCleanupIntent = false;

#if UNITY_EDITOR || DEBUG
        // Need this type of workaround to fix the gazillion billion errors
        // The unity function execution order flowchart is a LIE,
        // it was inconsistent solely with 'OnApplicationQuit'
#if UNITY_EDITOR
        private void QuitStateChanged()
        {
            isDestroyedWithCleanupIntent = true;
        }
#endif
        public void Initialize()
        {
#if UNITY_EDITOR
            Application.quitting += QuitStateChanged;
#endif
        }
        // Sometimes the 'OnDestroy' gets called before 'OnApplicationQuit'
        private void OnApplicationQuit()
        {
#if UNITY_EDITOR
            Application.quitting -= QuitStateChanged;
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
