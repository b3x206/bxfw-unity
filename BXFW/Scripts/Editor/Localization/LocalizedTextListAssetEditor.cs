using BXFW.Data;
using BXFW.Tools.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(LocalizedTextListAsset))]
    public class LocalizedTextListAssetEditor : Editor
    {
        protected ReorderableList textAssetsList;
        private Rect lastRepaintAddElementRect;
        protected string addElementTextIDString;

        /// <summary>
        /// The property to uncollapse.
        /// <br>Set this once to make a property as preview.</br>
        /// <br>This uncollapasation occurs on the next preview for the property that you are
        /// <see cref="ProjectWindowUtil.ShowCreatedAsset(UnityEngine.Object)"/>ing into.</br>
        /// </summary>
        public static int UncollapsePropertyIndex = -1;

        protected PropertyRectContext mainCtx = new PropertyRectContext();
        protected virtual float GetLocalizedTextElementHeightCallback(int index)
        {
            // GetPropertyHeight
            using SerializedProperty textListProperty = serializedObject.FindProperty("m_textList");
            using SerializedProperty textElementProperty = textListProperty.GetArrayElementAtIndex(index);

            return EditorGUI.GetPropertyHeight(textElementProperty) + mainCtx.Padding;
        }
        protected virtual void DrawLocalizedTextElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            // OnGUI
            mainCtx.Reset();
            LocalizedTextListAsset target = base.target as LocalizedTextListAsset;
            using SerializedProperty textListProperty = serializedObject.FindProperty("m_textList");
            using SerializedProperty textElementProperty = textListProperty.GetArrayElementAtIndex(index);
            // Check if the applied text id is unique
            using SerializedProperty textElementIDProperty = textElementProperty.FindPropertyRelative(nameof(LocalizedTextData.TextID));
            string previousTextIDValue = textElementIDProperty.stringValue;

            if (UncollapsePropertyIndex == index)
            {
                textElementProperty.isExpanded = true;
                UncollapsePropertyIndex = -1;
            }

            EditorGUI.PropertyField(mainCtx.GetPropertyRect(rect, textElementProperty), textElementProperty);

            // Check if text id is unique
            if (string.IsNullOrWhiteSpace(textElementIDProperty.stringValue) || target.Indexed().Any(td => td.Key != index && td.Value.TextID == textElementIDProperty.stringValue))
            {
                // Remove one char or set to unrelated value
                textElementIDProperty.stringValue = $"{previousTextIDValue}_{UnityEngine.Random.Range(0, 65535)}";
            }
        }
        protected virtual void DrawListHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Text Datas");
        }

        protected virtual void GetCustomPropertyDrawerDictionary(in Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict)
        {
            LocalizedTextListAsset target = base.target as LocalizedTextListAsset;

            dict.Add("m_textList", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                if (textAssetsList == null)
                {
                    SerializedProperty textElementsProperty = serializedObject.FindProperty("m_textList");
                    textAssetsList = new ReorderableList(serializedObject, textElementsProperty, true, true, false, true)
                    {
                        elementHeightCallback = GetLocalizedTextElementHeightCallback,
                        drawElementCallback = DrawLocalizedTextElementCallback,
                        drawHeaderCallback = DrawListHeaderCallback
                    };
                }

                EditorGUI.BeginChangeCheck();
                textAssetsList.DoLayoutList();
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(40f);

                if (GUILayout.Button("+ Add Element", GUILayout.ExpandWidth(true)))
                {
                    // Spawn a dropdown with a copy 
                    // BasicDropdown expects a ScreenRect
                    Rect guiAreaRect = GUIUtility.GUIToScreenRect(lastRepaintAddElementRect);
                    float dropdownStartingHeight = 86f;

                    string guiTagEditorControlName = "LocalizedTextListAssetEditor::TagField";
                    bool selectedTagEditorControlOnce = false;

                    BasicDropdown.ShowDropdown(guiAreaRect, new Vector2(lastRepaintAddElementRect.width, dropdownStartingHeight), (dropdown) =>
                    {
                        using SerializedProperty elementsProperty = serializedObject.FindProperty("m_textList");

                        // Just to do this dumb already selected thing
                        GUI.SetNextControlName(guiTagEditorControlName);
                        addElementTextIDString = EditorGUILayout.TextField("Text ID", addElementTextIDString);
                        if (!selectedTagEditorControlOnce)
                        {
                            EditorGUI.FocusTextInControl(guiTagEditorControlName);
                            selectedTagEditorControlOnce = true;
                        }

                        // Check if tag is unique
                        bool tagInvalid = string.IsNullOrWhiteSpace(addElementTextIDString) || target.TextIDExists(addElementTextIDString);
                        using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(tagInvalid))
                        {
                            bool hasPressedEnter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
                            if ((GUILayout.Button("Add") || hasPressedEnter) && !tagInvalid)
                            {
                                elementsProperty.arraySize++;
                                elementsProperty.GetArrayElementAtIndex(elementsProperty.arraySize - 1).FindPropertyRelative(nameof(LocalizedTextData.TextID)).stringValue = addElementTextIDString;
                                addElementTextIDString = string.Empty;
                                dropdown.Close();

                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        if (tagInvalid)
                        {
                            EditorGUILayout.HelpBox("Given TextID already exists or is invalid. Please type a different TextID.", MessageType.Info);
                        }

                        // If we press esc, clear TextID and close window
                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                        {
                            addElementTextIDString = string.Empty;
                            dropdown.Close();
                        }
                    });
                }
                if (Event.current.type == EventType.Repaint)
                {
                    lastRepaintAddElementRect = GUILayoutUtility.GetLastRect();
                }

                GUILayout.Space(40f);
                GUILayout.EndHorizontal();
            }));
        }

        private readonly Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> drawDict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
        public override void OnInspectorGUI()
        {
            if (drawDict.Count <= 0)
            {
                GetCustomPropertyDrawerDictionary(drawDict);
            }

            if (serializedObject.DrawCustomDefaultInspector(drawDict))
            {
                drawDict.Clear();
                GetCustomPropertyDrawerDictionary(drawDict);
            }
        }
    }
}
