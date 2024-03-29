﻿using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using System.IO;
using System.Collections.Generic;

using BXFW.Tools.Editor;
using BXFW.SceneManagement;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Creates a new file with all of the scenes listed.
    /// <br>These scenes can be referenced strongly with dragging and dropping a Unity <see cref="SceneAsset"/>, with the index changing correctly on <see cref="UnitySceneReference"/>.</br>
    /// </summary>
    internal class UnitySceneReferenceBuildCallback : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public static bool DeleteSceneListAfterBuild = true;

        private static string RelativeDirectoryName = string.Empty;
        private const string FileName = "MainSceneList";

        public void OnPreprocessBuild(BuildReport report)
        {
            // Gather new name
            if (string.IsNullOrEmpty(RelativeDirectoryName))
            {
                RelativeDirectoryName = $"{Random.Range(0, int.MaxValue)}.SceneList";
            }

            // Pack the references with a serialized list of scene references
            // This class is stored in 'UnitySceneReference'?
            if (UnitySceneReferenceList.Instance == null)
            {
                UnitySceneReferenceList.CreateEditorInstance(RelativeDirectoryName, FileName);
            }

            // Gather entries from 'EditorBuildSettings.scenes' and convert them.
            SceneEntry[] entries = new SceneEntry[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var editorScene = EditorBuildSettings.scenes[i];
                entries[i] = new SceneEntry(editorScene.path, editorScene.guid.ToString());
            }

            // Add the entry.
            UnitySceneReferenceList.Instance.entries = entries;
            AssetDatabase.Refresh();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (DeleteSceneListAfterBuild)
            {
                // delete the asset and it's meta
                string assetName = $"{Path.Combine(Directory.GetCurrentDirectory(), "Assets/Resources/", RelativeDirectoryName, FileName)}.asset";
                File.Delete(assetName);
                File.Delete($"{assetName}.meta");

                // delete the directory and it's meta
                string assetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets/Resources/", RelativeDirectoryName);
                Directory.Delete(assetDirectory);
                File.Delete($"{assetDirectory}.meta");
                AssetDatabase.Refresh();

                // screw you meta
                RelativeDirectoryName = string.Empty;
            }
        }
    }

    /// <summary>
    /// Draws the property inspector for the <see cref="UnitySceneReference"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnitySceneReference))]
    public class UnitySceneReferenceDrawer : PropertyDrawer
    {
        private string m_previousTargetSceneGUID;
        private SceneAsset m_targetSceneAsset;
        private GUID GetTargetSceneGUID()
        {
            string scenePath = AssetDatabase.GetAssetPath(m_targetSceneAsset);
            return AssetDatabase.GUIDFromAssetPath(scenePath);
        } 

        private SerializedProperty m_sceneGUIDProperty;
        private SerializedProperty m_sceneIndexProperty;
        private string SceneGUIDValue => (string)m_sceneGUIDProperty.GetTarget().value;
        private bool ShowDetailsGUI => !string.IsNullOrEmpty(SceneGUIDValue);

        // -- Utility
        private void GatherRelativeProperties(SerializedProperty property)
        {
            m_sceneIndexProperty = property.FindPropertyRelative("sceneIndex");
            m_sceneGUIDProperty = property.FindPropertyRelative("sceneGUID");
        }
        private void SetSceneGUIDValue(SerializedProperty property, string value)
        {
            if (m_sceneGUIDProperty == null || m_sceneGUIDProperty.IsDisposed())
            {
                GatherRelativeProperties(property);
            }

            // Serialize the stuff we do (otherwise it doesn't serialize, as we use a EditorGUI.ObjectField)
            // Can record entire parent object, as it's probably just the script.
            m_sceneGUIDProperty.stringValue = value;
        }

        private bool shouldShowWarningDirty = true;
        private bool shouldShowWarningGUI = false;
        private bool ShowWarningGUI(SerializedProperty property)
        {
            return ShowWarningGUI((UnitySceneReference)property.GetTarget().value);
        }
        private bool ShowWarningGUI(UnitySceneReference target)
        {
            if (shouldShowWarningDirty)
            {
                shouldShowWarningGUI = ShowDetailsGUI && !target.SceneLoadable;
                shouldShowWarningDirty = false;
            }

            return shouldShowWarningGUI;
        }

        // -- GUI
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();
        private const float WarningHelpBoxHeight = 38f;
        private const float FixIssueButtonHeight = 24f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GatherRelativeProperties(property);
            // Load if the scene was modified
            if (m_previousTargetSceneGUID != SceneGUIDValue)
            {
                m_targetSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(SceneGUIDValue));
                m_previousTargetSceneGUID = SceneGUIDValue;
            }

            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (ShowWarningGUI(property))
            {
                height += WarningHelpBoxHeight + mainCtx.Padding;
                height += FixIssueButtonHeight + mainCtx.Padding;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GatherRelativeProperties(property);
            UnitySceneReference target = (UnitySceneReference)property.GetTarget().value;
            mainCtx.Reset();

            Rect sceneAssetFieldRect = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            float sceneAssetFieldWidth = sceneAssetFieldRect.width; // prev width
            const float sceneAssetFieldSmallerWidth = 0.9f;

            EditorGUI.BeginChangeCheck();
            if (ShowDetailsGUI)
            {
                sceneAssetFieldRect.width = sceneAssetFieldWidth * sceneAssetFieldSmallerWidth;
            }
            m_targetSceneAsset = (SceneAsset)EditorGUI.ObjectField(sceneAssetFieldRect, "Scene Asset", m_targetSceneAsset, typeof(SceneAsset), false);
            if (ShowDetailsGUI)
            {
                sceneAssetFieldRect.x += sceneAssetFieldWidth * sceneAssetFieldSmallerWidth;
                sceneAssetFieldRect.width = sceneAssetFieldWidth * (1f - sceneAssetFieldSmallerWidth);

                using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(sceneAssetFieldRect, $"I:{target.SceneIndex}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                GUID sceneGUID = GetTargetSceneGUID();
                // 'sceneGUIDString' is many zeroes if 'sceneGUID' is empty.
                string sceneGUIDString = sceneGUID.ToString();

                // Register the 'SceneAsset'
                if (SceneGUIDValue != sceneGUIDString)
                {
                    // Mark current inspector dirty
                    shouldShowWarningDirty = true;
                    // Set parent record undos so that we can undo what we did
                    // (check whether if the given GUID is empty, otherwise don't call the intensive conversion constructor which does reflections)
                    SetSceneGUIDValue(property, sceneGUID.Empty() ? string.Empty : sceneGUIDString);
                }
            }

            // This accomodates for the 'ShowDetailsGUI'
            if (ShowWarningGUI(target))
            {
                EditorGUI.HelpBox(mainCtx.GetPropertyRect(position, WarningHelpBoxHeight), "[UnitySceneReference] Given scene does not exist on the Player Settings Scene build index.\nIt will be impossible to load this asset.", MessageType.Warning);

                if (GUI.Button(mainCtx.GetPropertyRect(position, FixIssueButtonHeight), "Fix Issue"))
                {
                    List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes.Length + 1);
                    foreach (EditorBuildSettingsScene sceneReg in EditorBuildSettings.scenes)
                    {
                        scenes.Add(sceneReg);
                    }

                    scenes.Add(new EditorBuildSettingsScene(GetTargetSceneGUID(), true));

                    // Assign and log the index
                    EditorBuildSettings.scenes = scenes.ToArray();
                    m_sceneIndexProperty.intValue = EditorBuildSettings.scenes.Length - 1;
                    Debug.Log($"[UnitySceneReference] Assigned scene to index {EditorBuildSettings.scenes.Length - 1}.");
                }
            }

            EditorGUI.EndProperty();
        }
    }
}