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

        private const string RELATIVE_DIR_NAME = "SceneList";
        private const string FILE_NAME = "ListReference";

        public void OnPreprocessBuild(BuildReport report)
        {
            // Pack the references with a serialized list of scene references
            // This class is stored in 'UnitySceneReference'?
            if (UnitySceneReferenceList.Instance == null)
                UnitySceneReferenceList.CreateEditorInstance(RELATIVE_DIR_NAME, FILE_NAME);

            // Gather entries from 'EditorBuildSettings.scenes' and convert them.
            SceneEntry[] entries = new SceneEntry[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var editorScene = EditorBuildSettings.scenes[i];
                entries[i] = new SceneEntry(editorScene.path, new SerializableGUID(editorScene.guid));
            }

            // Add the entry.
            UnitySceneReferenceList.Instance.entries = entries;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (DeleteSceneListAfterBuild)
            {
                string assetDirRelative = string.Format("Assets/Resources/{0}", RELATIVE_DIR_NAME);

                AssetDatabase.DeleteAsset(string.Format("{0}.asset", Path.Combine(assetDirRelative, FILE_NAME)));
                AssetDatabase.Refresh();
                FileUtil.DeleteFileOrDirectory(assetDirRelative);
            }
        }
    }

    /// <summary>
    /// Draws the property inspector for the <see cref="UnitySceneReference"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnitySceneReference))]
    internal class UnitySceneReferenceDrawer : PropertyDrawer
    {
        private SceneAsset sceneAsset;
        private SerializedProperty sceneGUID;
        private SerializableGUID SceneGUIDValue 
        { 
            get { return (SerializableGUID)sceneGUID.GetTarget().Value; }
        }
        private void SetSceneGUIDValue(SerializedProperty property, SerializableGUID value)
        {
            // Passed 'property' param is the parent. (UnitySceneReference)
            var guidFieldInfo = sceneGUID.GetTarget(); // This is the field info we need to set

            // Serialize the stuff we do (otherwise it doesn't serialize, as we use a EditorGUI.ObjectField)
            // Can record entire parent object, as it's probably just the script.
            Undo.RecordObject(property.serializedObject.targetObject, "set scene");
            guidFieldInfo.Key.SetValue(property.GetTarget().Value, value);
        }
        private SerializedProperty sceneIndex;

        private bool ShowOtherGUI => SceneGUIDValue != default;
        private bool ShowWarningGUI(SerializedProperty property)
        {
            return ShowWarningGUI((UnitySceneReference)property.GetTarget().Value);
        }
        private bool ShowWarningGUI(UnitySceneReference target)
        {
            return ShowOtherGUI && !target.SceneLoadable;
        }

        private void GatherRelativeProperties(SerializedProperty property)
        {
            sceneIndex = property.FindPropertyRelative("sceneIndex");
            sceneGUID = property.FindPropertyRelative("sceneGUID");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GatherRelativeProperties(property);
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(SceneGUIDValue));

            return (EditorGUIUtility.singleLineHeight + 2) * (1 + (ShowWarningGUI(property) ? 3.8f : 0));
        }

        private float currentY;
        public Rect GetFieldRect(Rect pos, SerializedProperty property, float padding = 2f)
        {
            return GetFieldRect(pos, EditorGUI.GetPropertyHeight(property), padding);
        }
        public Rect GetFieldRect(Rect pos, float height = -1f, float padding = 2f)
        {
            if (height <= 0f)
                height = EditorGUIUtility.singleLineHeight;

            Rect r = new Rect
            {
                x = pos.x,
                y = currentY + (padding / 2f),
                width = pos.width,
                height = height - (padding / 2f)
            };

            currentY += height;
            return r;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GatherRelativeProperties(property);
            UnitySceneReference target = (UnitySceneReference)property.GetTarget().Value;
            currentY = position.y;

            Rect scAssetGUIAreaRect = GetFieldRect(position);
            float scAssetGUIareaWidth = scAssetGUIAreaRect.width; // prev width
            const float scAssetGUISmallerWidth = .9f;

            if (ShowOtherGUI)
            {
                scAssetGUIAreaRect.width = scAssetGUIareaWidth * scAssetGUISmallerWidth;
            }
            sceneAsset = (SceneAsset)EditorGUI.ObjectField(scAssetGUIAreaRect, "Scene Asset", sceneAsset, typeof(SceneAsset), false);
            if (ShowOtherGUI)
            {
                scAssetGUIAreaRect.x += scAssetGUIareaWidth * scAssetGUISmallerWidth;
                scAssetGUIAreaRect.width = scAssetGUIareaWidth * (1f - scAssetGUISmallerWidth);

                var gEnabled = GUI.enabled;
                GUI.enabled = false;
                GUI.Label(scAssetGUIAreaRect, $"I:{target.SceneIndex}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                GUI.enabled = gEnabled;
            }

            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            GUID sceneGUID = AssetDatabase.GUIDFromAssetPath(scenePath);
            // Register the 'SceneAsset'
            if (SceneGUIDValue != sceneGUID)
            {
                // First plan was to modify runtime scenes and add the given reference
                // But unity says it can load scenes from the editor soo
                // But it can't be loaded as well, so yeah.

                // Set parent record undos so that we can undo what we did
                // (check whether if the given GUID is empty, otherwise don't call the intensive conversion constructor which does reflections)
                SetSceneGUIDValue(property, sceneGUID.Empty() ? default : new SerializableGUID(sceneGUID));
            }

            if (ShowOtherGUI)
            {
                if (ShowWarningGUI(property))
                {
                    EditorGUI.HelpBox(GetFieldRect(position, EditorGUIUtility.singleLineHeight * 2.3f), "[UnitySceneReference] Given scene does not exist on the Player Settings Scene build index.\nIt will be impossible to load this asset.", MessageType.Warning);

                    if (GUI.Button(GetFieldRect(position, EditorGUIUtility.singleLineHeight * 1.5f), "Fix Issue"))
                    {
                        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes.Length + 1);
                        foreach (var sceneReg in EditorBuildSettings.scenes)
                        {
                            scenes.Add(sceneReg);
                        }
                        scenes.Add(new EditorBuildSettingsScene(sceneGUID, true));

                        // Assign and log the index
                        EditorBuildSettings.scenes = scenes.ToArray();
                        sceneIndex.intValue = EditorBuildSettings.scenes.Length - 1;
                        Debug.Log($"[UnitySceneReference] Assigned scene to index {EditorBuildSettings.scenes.Length - 1}.");
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}