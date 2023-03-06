using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;
using BXFW.SceneManagement;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the property inspector for the <see cref="UnitySceneReference"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnitySceneReference))]
    public class UnitySceneReferenceDrawer : PropertyDrawer
    {
        private SceneAsset sceneAsset;
        private SerializedProperty sceneEditorPath;
        private SerializedProperty sceneLoadable;
        private bool ShowOtherGUI => !string.IsNullOrEmpty(sceneEditorPath.stringValue);
        private bool ShowWarningGUI(SerializedProperty property)
        {
            UnitySceneReference target = (UnitySceneReference)property.GetTarget().Value;

            return ShowOtherGUI && !target.SceneLoadable;
        }

        private void GatherRelativeProperties(SerializedProperty property)
        {
            sceneEditorPath = property.FindPropertyRelative("sceneEditorPath");
            sceneLoadable = property.FindPropertyRelative("sceneLoadable");
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GatherRelativeProperties(property);
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneEditorPath.stringValue);

            return (EditorGUIUtility.singleLineHeight + 2) * ((ShowOtherGUI ? 2 : 1) + (sceneLoadable.boolValue && ShowWarningGUI(property) ? 3.8f : 0));
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
                GUI.Label(scAssetGUIAreaRect, $"I:{target.SceneIndex}");
                GUI.enabled = gEnabled;
            }

            string sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
            // Register the 'SceneAsset'
            if (sceneAsset != null && sceneEditorPath.stringValue != sceneAssetPath)
            {
                // First plan was to modify runtime scenes and add the given reference
                // But unity says it can load scenes from the editor soo
                // But it can't be loaded as well, so yeah.
                sceneEditorPath.stringValue = sceneAssetPath;
            }

            if (ShowOtherGUI)
            {
                sceneLoadable.boolValue = EditorGUI.Toggle(GetFieldRect(position), "Scene Loadable", sceneLoadable.boolValue);

                if (sceneLoadable.boolValue)
                {
                    // Draw a warning field if sceneLoadable is true but SceneLoadable, the property is false (meaning the scene does not exist on indexes)
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
                            scenes.Add(new EditorBuildSettingsScene(sceneAssetPath, true));

                            // Assign and log the index
                            EditorBuildSettings.scenes = scenes.ToArray();
                            Debug.Log($"[UnitySceneReference] Assigned scene to index {EditorBuildSettings.scenes.Length - 1}.");
                        }
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}