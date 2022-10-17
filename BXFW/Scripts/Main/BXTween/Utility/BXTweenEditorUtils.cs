// The reason why this file that isn't in an 'Editor' folder
// 1 : Need to access some of the classes inside this file.
// No other reasons?
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// Editor utils go on this namespace.
/// You can use these.
namespace BXFW.Tweening.Editor
{
    #region BXFW Settings
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
            var w = GetWindow<BXTweenSettingsEditor>(utility: true, "BXTween Settings", focus: true);
            // Set size constraints
            w.minSize = new Vector2(250f, 400f);
            // Show already hidden window.
            w.Show();
        }

        // -- Variables
        private BXTweenSettings currentSettings;
        private BXTweenSettings CurrentSettings
        {
            get
            {
                if (currentSettings == null)
                    currentSettings = BXTweenSettings.Instance;

                if (currentSettings == null)
                {
                    // We are still null, create instance at given const resources directory.
                    // Maybe we can add a EditorPref for creation directory?

                    Debug.Log($"[BXTweenSettingsEditor::(get)CurrentSettings] Current settings in directory {System.IO.Path.Combine(BXTweenStrings.SettingsResourceCreatePath, BXTweenStrings.SettingsResourceCreateName)} is null. Creating new");
                    currentSettings = BXTweenSettings.CreateEditorInstance(BXTweenStrings.SettingsResourceCreatePath, BXTweenStrings.SettingsResourceCreateName);
                }

                return currentSettings;
            }
        }

        private void OnGUI()
        {
            // Draw custom GUI for 'CurrentSettings'
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(new GUIContent(":: General"), EditorStyles.boldLabel);
            var enableTw = EditorGUILayout.Toggle(new GUIContent("Enable BXTween", "Enables BXTween. If this option is false, BXTween won't run on start."), CurrentSettings.enableBXTween);
            var ignoreTS = EditorGUILayout.Toggle(new GUIContent("Ignore Time.timeScale", "Ignores Time.timeScale. Basically slowing down game won't affect the tweens."), CurrentSettings.ignoreTimeScale);

            EditorGUILayout.LabelField(new GUIContent(":: BXTweenStrings"), EditorStyles.boldLabel);
            var lColor = EditorGUILayout.ColorField("Log Color", CurrentSettings.LogColor);
            var ldColor = EditorGUILayout.ColorField("Log Diagnostic Color", CurrentSettings.LogDiagColor);
            var wColor = EditorGUILayout.ColorField("Warning Color", CurrentSettings.WarnColor);
            var errColor = EditorGUILayout.ColorField("Error Color", CurrentSettings.ErrColor);

            EditorGUILayout.LabelField(new GUIContent(":: Default Settings (For BXTweenCTX<T>)"), EditorStyles.boldLabel);
            var dEaseType = (EaseType)EditorGUILayout.EnumPopup("Default Ease Type", CurrentSettings.DefaultEaseType);
            var dRepeatType = (RepeatType)EditorGUILayout.EnumPopup("Default Repeat Type", CurrentSettings.DefaultRepeatType);

            EditorGUILayout.LabelField(new GUIContent(":: Debug"), EditorStyles.boldLabel);
            var dbgMode = EditorGUILayout.Toggle(new GUIContent("Diagnostic Mode", "Enables extensive 'Debug.Log()'."), CurrentSettings.diagnosticMode);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(CurrentSettings, "Change BXTween Settings");
                // We can't know what setting we changed, so we just use variables
                // This is bad
                CurrentSettings.enableBXTween = enableTw;
                CurrentSettings.ignoreTimeScale = ignoreTS;

                CurrentSettings.LogColor = lColor;
                CurrentSettings.LogDiagColor = ldColor;
                CurrentSettings.WarnColor = wColor;
                CurrentSettings.ErrColor = errColor;

                CurrentSettings.DefaultEaseType = dEaseType;
                CurrentSettings.DefaultRepeatType = dRepeatType;

                CurrentSettings.diagnosticMode = dbgMode;
            }

            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                Undo.RecordObject(CurrentSettings, "Reset BXTween Settings");
                CurrentSettings.FromSettings(CreateInstance<BXTweenSettings>());
            }
        }
    }
    #endregion

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