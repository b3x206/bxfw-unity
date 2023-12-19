using UnityEngine;
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

        private const string RelativeDirectoryName = "SceneList";
        private const string FileName = "ListReference";

        public void OnPreprocessBuild(BuildReport report)
        {
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
                entries[i] = new SceneEntry(editorScene.path, new SerializableGUID(editorScene.guid));
            }

            // Add the entry.
            UnitySceneReferenceList.Instance.entries = entries;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (DeleteSceneListAfterBuild)
            {
                string assetDirRelative = Path.Combine("Assets/Resources/", RelativeDirectoryName);

                AssetDatabase.DeleteAsset($"{Path.Combine(assetDirRelative, FileName)}.asset");
                Directory.Delete(Path.Combine(Directory.GetCurrentDirectory(), assetDirRelative));
                AssetDatabase.Refresh();
            }
        }
    }

    /// <summary>
    /// Draws the property inspector for the <see cref="UnitySceneReference"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(UnitySceneReference))]
    public class UnitySceneReferenceDrawer : PropertyDrawer
    {
        private SceneAsset m_targetSceneAsset;
        private SerializedProperty m_sceneGUIDProperty;
        private SerializedProperty m_sceneIndexProperty;
        private SerializableGUID SceneGUIDValue => (SerializableGUID)m_sceneGUIDProperty.GetTarget().value;
        private bool ShowOtherGUI => SceneGUIDValue != default;

        // -- Utility
        private void GatherRelativeProperties(SerializedProperty property)
        {
            m_sceneIndexProperty = property.FindPropertyRelative("sceneIndex");
            m_sceneGUIDProperty = property.FindPropertyRelative("sceneGUID");
        }
        private void SetSceneGUIDValue(SerializedProperty property, SerializableGUID value)
        {
            // Passed 'property' param is the parent. (UnitySceneReference)
            var guidFieldInfo = m_sceneGUIDProperty.GetTarget(); // This is the field info we need to set

            // Serialize the stuff we do (otherwise it doesn't serialize, as we use a EditorGUI.ObjectField)
            // Can record entire parent object, as it's probably just the script.
            Undo.RecordObject(property.serializedObject.targetObject, "set scene");
            guidFieldInfo.fieldInfo.SetValue(property.GetTarget().value, value);
        }
        private bool ShowWarningGUI(SerializedProperty property)
        {
            return ShowWarningGUI((UnitySceneReference)property.GetTarget().value);
        }
        private bool ShowWarningGUI(UnitySceneReference target)
        {
            return ShowOtherGUI && !target.SceneLoadable;
        }

        // -- GUI
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            GatherRelativeProperties(property);
            m_targetSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(SceneGUIDValue));

            return (EditorGUIUtility.singleLineHeight + 2) * (1 + (ShowWarningGUI(property) ? 3.8f : 0));
        }

        private float m_currentY;
        public Rect GetFieldRect(Rect pos, SerializedProperty property, float padding = 2f)
        {
            return GetFieldRect(pos, EditorGUI.GetPropertyHeight(property), padding);
        }
        public Rect GetFieldRect(Rect pos, float height = -1f, float padding = 2f)
        {
            if (height <= 0f)
            {
                height = EditorGUIUtility.singleLineHeight;
            }

            Rect r = new Rect
            {
                x = pos.x,
                y = m_currentY + (padding / 2f),
                width = pos.width,
                height = height - (padding / 2f)
            };

            m_currentY += height;
            return r;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            GatherRelativeProperties(property);
            UnitySceneReference target = (UnitySceneReference)property.GetTarget().value;
            m_currentY = position.y;

            Rect scAssetGUIAreaRect = GetFieldRect(position);
            float scAssetGUIareaWidth = scAssetGUIAreaRect.width; // prev width
            const float scAssetGUISmallerWidth = .9f;

            if (ShowOtherGUI)
            {
                scAssetGUIAreaRect.width = scAssetGUIareaWidth * scAssetGUISmallerWidth;
            }
            m_targetSceneAsset = (SceneAsset)EditorGUI.ObjectField(scAssetGUIAreaRect, "Scene Asset", m_targetSceneAsset, typeof(SceneAsset), false);
            if (ShowOtherGUI)
            {
                scAssetGUIAreaRect.x += scAssetGUIareaWidth * scAssetGUISmallerWidth;
                scAssetGUIAreaRect.width = scAssetGUIareaWidth * (1f - scAssetGUISmallerWidth);

                using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
                {
                    GUI.Label(scAssetGUIAreaRect, $"I:{target.SceneIndex}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
                }
            }

            string scenePath = AssetDatabase.GetAssetPath(m_targetSceneAsset);
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
                        m_sceneIndexProperty.intValue = EditorBuildSettings.scenes.Length - 1;
                        Debug.Log($"[UnitySceneReference] Assigned scene to index {EditorBuildSettings.scenes.Length - 1}.");
                    }
                }
            }

            EditorGUI.EndProperty();
        }
    }
}