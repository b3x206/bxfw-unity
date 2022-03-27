// The reason why this file that isn't in an 'Editor' folder
// 1 : Need to access some of the classes inside this file.
// No other reasons?
#if UNITY_EDITOR
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using BXFW.Tools.Editor;
using System;
using System.IO;

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

        private BXTweenSettings.HashHelper.OnHashMatchFailAction currentToSetFailAction = BXTweenPersistentSettings.OnHashMatchFailAction;
        private bool currentCheckBXTweenSettingsHash = BXTweenPersistentSettings.CheckBXTweenSettingsHash;
        private void OnGUI()
        {
            // Draw custom GUI for 'CurrentSettings'
            EditorGUILayout.LabelField(new GUIContent(":: BXTweenStrings"), EditorStyles.boldLabel);
            CurrentSettings.LogColor = EditorGUILayout.ColorField("Log Color", CurrentSettings.LogColor);
            CurrentSettings.LogDiagColor = EditorGUILayout.ColorField("Log Diagnostic Color", CurrentSettings.LogDiagColor);
            CurrentSettings.WarnColor = EditorGUILayout.ColorField("Warning Color", CurrentSettings.WarnColor);
            CurrentSettings.ErrColor = EditorGUILayout.ColorField("Error Color", CurrentSettings.ErrColor);

            EditorGUILayout.LabelField(new GUIContent(":: Default Settings"), EditorStyles.boldLabel);
            CurrentSettings.DefaultEaseType = (EaseType)EditorGUILayout.EnumPopup("Default Ease Type", CurrentSettings.DefaultEaseType);
            CurrentSettings.DefaultRepeatType = (RepeatType)EditorGUILayout.EnumPopup("Default Repeat Type", CurrentSettings.DefaultRepeatType);

            EditorGUILayout.LabelField(new GUIContent(":: Debug"), EditorStyles.boldLabel);
            CurrentSettings.diagnosticMode = EditorGUILayout.Toggle(new GUIContent("Diagnostic Mode", "Enables extensive 'Debug.Log()'."), CurrentSettings.diagnosticMode);
            
            EditorGUILayout.LabelField(new GUIContent(":: Persistent Settings"), EditorStyles.boldLabel);
            currentCheckBXTweenSettingsHash = EditorGUILayout.Toggle(new GUIContent("Verify Hash", "Should you be a bad guy and disable fun?"), currentCheckBXTweenSettingsHash);
            if (currentCheckBXTweenSettingsHash)
            {
                currentToSetFailAction = (BXTweenSettings.HashHelper.OnHashMatchFailAction)EditorGUILayout.EnumPopup(new GUIContent("Enable Hash Check : ", "If this is disabled, changes to BXTweenSettings are ignored completely."), currentToSetFailAction);
            }

            // If the settings doesn't match, use an apply button.
            if (currentCheckBXTweenSettingsHash != BXTweenPersistentSettings.CheckBXTweenSettingsHash || currentToSetFailAction != BXTweenPersistentSettings.OnHashMatchFailAction)
            {
                if (GUILayout.Button("Apply"))
                {
                    if (EditorUtility.DisplayDialog("Warning", "Do you 'really' wanna apply these hash settings?", "Yes", "No"))
                    {
                        // Generate & find file
                        // Note that this is a 'REALLY' dumb method of finding this file's directory.
                        var fileDir = string.Empty;
                        foreach (var file in Directory.GetFiles(string.Format("{0}/Assets", Directory.GetCurrentDirectory()), "*.cs"))
                        {
                            // Sole reason of using 'typeof' is older c# support.
                            if (file.Contains(typeof(BXTweenPersistentSettings).Name))
                            {
                                // This is the file we are looking for
                                fileDir = file;
                                break;
                            }
                        }

                        File.WriteAllText(fileDir, string.Format(@"
using BXFW.Tweening;
/// <summary>
/// Automatically generated persistent settings file.
/// <br>Modified in <see cref={0}BXFW.Tweening.Editor.BXTweenSettingsEditor{0}/>.</br>
/// <br>NOTE : For this class to be safe, COMPILE PROJECT USING IL2CPP!</br>
/// </summary>
public static class BXTweenPersistentSettings
{
    public const bool CheckBXTweenSettingsHash = {1};
    public const BXTweenSettings.HashHelper.OnHashMatchFailAction OnHashMatchFailAction = {2};
}
", '"', currentToSetFailAction, currentCheckBXTweenSettingsHash));

                        Debug.Log(string.Format("[BXTweenEditorUtils::SetPersistentSettings] Write all text to '{0}'.", fileDir));
                    }
                }
            }
        }
    }

    // -- These classes do hash related stuff for protecting the settings.
    // If it's modified then the game will check a compiled 'ProtectedBXTweenSettings' for other stuff. 
    internal class BXTweenSettingsBuildPreProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.result == BuildResult.Failed) return;

            // Save hash data of BXTween to 'Resources'
            AssetDatabase.CreateAsset(BXTweenSettings.HashHelper.GetTweenSettingsHashObject(), 
                string.Format("{0}.asset", BXTweenSettings.HashResourceName));
            AssetDatabase.Refresh();
        }
    }
    internal class BXTweenSettingsBuildPostProcessor : IPostprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Delete the hash data from resources.
            AssetDatabase.DeleteAsset(string.Format("{0}.asset", BXTweenSettings.HashResourceName));
            AssetDatabase.Refresh();
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