#if UNITY_EDITOR
using System;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    /// This is in the BXFW asmdef because BXTweenCTX uses it in #if UNITY_EDITOR context to start tweens
    /// Since that functionality wasn't really used, it could be removed and this class could be put into BXFW.Editor.
    /// <summary>
    /// Execute coroutines in edit mode.
    /// </summary>
    public static class EditModeCoroutineRunner
    {
        #region Execution
        private struct EditorCoroutine : IEnumerable
        {
            public IEnumerator coroutine;  // Coroutine itself
            public WeakReference owner;    // Weak reference : reference that isn't counted as a reference.
                                           // With the weak reference, we can check if object was gc collected

            // I mean, unity already nulls any destroyed object, and coroutines are called with an alive c#/unity object.
            // But this is what the unity editor coroutine package do (not exactly, but probably same; just uses different class??) so

            public IEnumerator GetEnumerator()
            {
                return coroutine;
            }
        }
        /// <summary>
        /// Coroutines to execute. Managed by the EditModeCoroutineExec.
        /// </summary>
        private static readonly List<EditorCoroutine> CoroutineInProgress = new List<EditorCoroutine>();
        /// <summary>
        /// Default static constructor assigning execution to update.
        /// </summary>
        static EditModeCoroutineRunner()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (CoroutineInProgress.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < CoroutineInProgress.Count; i++)
            {
                // Null coroutine
                if (CoroutineInProgress[i].coroutine == null ||
                   (CoroutineInProgress[i].owner != null && !CoroutineInProgress[i].owner.IsAlive) ||
                // Normal
                   !CoroutineInProgress[i].coroutine.MoveNext())
                {
                    CoroutineInProgress.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Add coroutine to execute.
        /// <br>This doesn't have a object parameter, </br>
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Added IEnumerator value.</returns>
        public static IEnumerator StartCoroutine(IEnumerator c)
        {
            CoroutineInProgress.Add(new EditorCoroutine { coroutine = c, owner = null });
            return c;
        }
        /// <summary>
        /// Add a coroutine to execute.
        /// </summary>
        public static IEnumerator StartCoroutine(IEnumerator c, object owner)
        {
            CoroutineInProgress.Add(new EditorCoroutine
            {
                coroutine = c,
                owner = owner == null ? null : new WeakReference(owner)
            });
            return c;
        }
        /// <summary>
        /// Remove coroutine to execute. Also stops execution.
        /// </summary>
        /// <param name="c">IEnumerator value.</param>
        /// <returns>Whether if the coroutine exists in the list (already stopped coroutines will return false).</returns>
        public static bool StopCoroutine(IEnumerator c)
        {
            int index = CoroutineInProgress.FindIndex(ec => EqualityComparer<IEnumerator>.Default.Equals(ec.coroutine, c));
            bool indFound = index > -1;

            if (indFound)
            {
                CoroutineInProgress.RemoveAt(index);
            }

            return indFound;
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
#endif