// The reason why this file that isn't in an 'Editor' folder
// 1 : Need to access some of the classes inside this file.
// No other reasons?
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;

/// Editor utils go on this namespace.
/// You can use these.
namespace BXFW.Tweening.Editor
{
    /// <summary>
    /// Override class for inspector of <see cref="BXTweenSettings"/>.
    /// <br>This does not affect <see cref="BXTweenSettingsEditor"/>.</br>
    /// </summary>
    [CustomEditor(typeof(BXTweenSettings))]
    public class BXTweenSettingsInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            // Only draw an inspector for the 'm_Script' field.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUILayout.Space();
            GUI.enabled = gEnabled;

            EditorGUILayout.HelpBox(string.Format("BXTweenSettings are editable in {0}.", BXTweenSettingsEditor.WindowMenuItemRegister), MessageType.Info);
            if (GUILayout.Button("Open BXTweenSettings"))
            {
                BXTweenSettingsEditor.OpenSettingsEditor();
            }
        }
    }

    /// <summary>
    /// Editor that allows for editing / generating a <see cref="BXTweenSettings"/> file.
    /// </summary>
    public class BXTweenSettingsEditor : EditorWindow
    {
        public const string WindowMenuItemRegister = "Window/BXTween/Settings";
        [MenuItem(WindowMenuItemRegister)]
        public static void OpenSettingsEditor()
        {
            var w = CreateWindow<BXTweenSettingsEditor>();

            // Set window title
            w.titleContent = new GUIContent("BXTween Settings");

            // Set size constraints
            w.minSize = new Vector2(250f, 400f);
        }

        // -- Variables
        private BXTweenSettings currentSettings;
        private BXTweenSettings CurrentSettings
        {
            get
            {
                if (currentSettings == null)
                    currentSettings = BXTweenSettings.GetBXTweenSettings();

                return currentSettings;
            }
        }
        private SerializedObject currentSettingsSO;
        private SerializedObject CurrentSettingsSO
        {
            get
            {
                if (currentSettingsSO == null)
                    currentSettingsSO = new SerializedObject(CurrentSettings);

                return currentSettingsSO;
            }
        }

        private void OnGUI()
        {
            // Draw the default property field for BXTweenSettings.
            CurrentSettingsSO.DrawCustomDefaultInspector(null);

            // Other GUI
            //if (Application.isPlaying)
            //{
            //    // Not necessary as it uses a scriptable object.
            //    EditorGUILayout.HelpBox("[Warning] : BXTween settings may NOT save after you modified it in runtime!", MessageType.Warning);
            //}
        }
    }

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
#endif