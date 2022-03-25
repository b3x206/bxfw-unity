using System.Collections;
using System.Collections.Generic;
using UnityEditor;

/// Editor utils go on this namespace.
/// You can use these.
namespace BXFW.Tweening.Editor
{
    /// NOTE : Part of <see cref="BXTween"/>.
    /// Same stuff applies here too. (This is just some simple editor scripts)
    /////////////////////////////////////////////////////////////////////////////
    /// <summary> <c>EXPERIMENTAL</c>, editor playback. </summary>            /// 
    /// Maybe TODO : Add generic IEnumerator support for custom return types. ///
    /////////////////////////////////////////////////////////////////////////////
    public static class EditModeCoroutineExec
    {
        #region Execution
        /// <summary>
        /// Coroutines to execute. Managed by the EditModeCoroutineExec.
        /// </summary>
        private static List<IEnumerator> CoroutineInProgress = new List<IEnumerator>();
        /// <summary>
        /// Default static constructor assigning execution to update.
        /// </summary>
        static EditModeCoroutineExec()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (CoroutineInProgress.Count <= 0)
            { return; }

            for (int i = 0; i < CoroutineInProgress.Count; i++)
            {
                // Null coroutine
                if (CoroutineInProgress[i] == null)
                { continue; }

                // Normal
                if (!CoroutineInProgress[i].MoveNext())
                { CoroutineInProgress.Remove(CoroutineInProgress[i]); }
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Add coroutine to execute.
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Added IEnumerator value.</returns>
        public static IEnumerator StartCoroutine(IEnumerator c)
        {
            CoroutineInProgress.Add(c);
            return c;
        }
        /// <summary>
        /// Remove coroutine to execute. Also stops execution.
        /// </summary>
        /// <param name="c">IEnumerator value.</param>
        public static void StopCoroutine(IEnumerator c)
        {
            CoroutineInProgress.Remove(c);
        }
        /// <summary>
        /// Stops all coroutines.
        /// </summary>
        public static void StopAllCoroutines()
        {
            CoroutineInProgress.Clear();
        }
        #endregion
    }
}
