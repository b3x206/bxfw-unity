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
    [AddComponentMenu("")]
    public class PoolObjectDestroyInterceptor : MonoBehaviour
    {
        /// <summary>
        /// This flag is set to true if the object is to be removed by either the <see cref="ObjectPooler"/> 
        /// or by the <see cref="OnApplicationQuit"/> callback.
        /// </summary>
        internal bool isDestroyedWithCleanupIntent = false;

#if UNITY_EDITOR || DEBUG
        private void OnApplicationQuit()
        {
            isDestroyedWithCleanupIntent = true;
        }
        private void OnDestroy()
        {
            if (!Application.isPlaying || isDestroyedWithCleanupIntent)
            {
                return;
            }

            throw new InvalidOperationException("[PoolObjectDestroyInterceptor::OnDestroy] Cannot destroy pooled object, as it is never meant to be disposed except for explicit removal of pools (or OnApplicationQuit).");
        }
#endif
    }
}
