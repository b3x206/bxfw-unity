using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Execute coroutines in edit mode using <see cref="EditorApplication.update"/>.
    /// </summary>
    public static class EditorCoroutineRunner
    {
        #region Execution
        private struct EditorCoroutine : IEnumerable
        {
            public IEnumerator coroutine;  // Coroutine itself
            public WeakReference owner;    // Weak reference : reference that isn't counted as a reference.
                                           // With the weak reference, we can check if object was deleted

            public IEnumerator GetEnumerator()
            {
                return coroutine;
            }
        }
        /// <summary>
        /// Coroutines to execute. Managed by the EditModeCoroutineExec.
        /// </summary>
        private static readonly List<EditorCoroutine> m_Routines = new List<EditorCoroutine>();
        /// <summary>
        /// Default static constructor assigning execution to update.
        /// </summary>
        static EditorCoroutineRunner()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (m_Routines.Count <= 0)
                return;

            for (int i = 0; i < m_Routines.Count; i++)
            {
                // Null coroutine
                if (m_Routines[i].coroutine == null ||
                   (m_Routines[i].owner != null && !m_Routines[i].owner.IsAlive) ||
                // Normal
                   !m_Routines[i].coroutine.MoveNext())
                {
                    m_Routines.RemoveAt(i);
                    i--;
                    continue;
                }
            }
        }
        #endregion

        #region Dispatch
        /// <summary>
        /// Add coroutine to execute.
        /// <br>This doesn't have a object parameter, </br>
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Added IEnumerator value.</returns>
        public static IEnumerator StartCoroutine(IEnumerator c)
        {
            m_Routines.Add(new EditorCoroutine { coroutine = c, owner = null });
            return c;
        }
        /// <summary>
        /// Add a coroutine to execute.
        /// </summary>
        public static IEnumerator StartCoroutine(IEnumerator c, object owner)
        {
            m_Routines.Add(new EditorCoroutine
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
            int index = m_Routines.FindIndex(ec => EqualityComparer<IEnumerator>.Default.Equals(ec.coroutine, c));
            bool isFound = index > -1;

            if (isFound)
                m_Routines.RemoveAt(index);

            return isFound;
        }
        /// <summary>
        /// Stops all coroutines.
        /// </summary>
        public static void StopAllCoroutines()
        {
            m_Routines.Clear();
        }
        #endregion
    }
}
