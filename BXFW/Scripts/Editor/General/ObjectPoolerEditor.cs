using System;
using BXFW.Tools.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws a ReorderableList + Dropdown based adding of pool tags.
    /// </summary>
    [CustomEditor(typeof(ObjectPooler), true)]
    public class ObjectPoolerEditor : Editor
    {
        protected ReorderableList poolerTagsList;
        private Rect lastRepaintAddElementRect;
        protected string addElementTagString;

        protected PropertyRectContext mainCtx = new PropertyRectContext();
        protected virtual float GetPoolElementHeightCallback(int index)
        {
            // GetPropertyHeight
            using SerializedProperty elementsProperty = serializedObject.FindProperty("m_pools");
            using SerializedProperty poolProperty = elementsProperty.GetArrayElementAtIndex(index);

            float height = 0f;
            foreach (SerializedProperty visibleChild in poolProperty.GetVisibleChildren())
            {
                // If the visibleChild is the 'Pool.tag', ensure that the tag is not duplicate.
                if (visibleChild.name == nameof(ObjectPooler.Pool.tag))
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + mainCtx.Padding;
                    continue;
                }

                height += EditorGUI.GetPropertyHeight(visibleChild) + mainCtx.Padding;
            }

            return height;
        }
        protected virtual void DrawPoolElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Very fun property drawing.
            ObjectPooler target = base.target as ObjectPooler;
            // OnGUI
            mainCtx.Reset();
            using SerializedProperty elementsProperty = serializedObject.FindProperty("m_pools");
            using SerializedProperty poolProperty = elementsProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginChangeCheck();
            foreach (SerializedProperty visibleChild in poolProperty.GetVisibleChildren())
            {
                // If the visibleChild is the 'Pool.tag', ensure that the tag is not duplicate.
                if (visibleChild.name == nameof(ObjectPooler.Pool.tag))
                {
                    EditorGUI.BeginChangeCheck();
                    string previousTagValue = visibleChild.stringValue;
                    visibleChild.stringValue = EditorGUI.TextField(mainCtx.GetPropertyRect(rect, EditorGUIUtility.singleLineHeight), visibleChild.displayName, visibleChild.stringValue);
                    if (EditorGUI.EndChangeCheck() && (string.IsNullOrWhiteSpace(visibleChild.stringValue) || target.TagExists(visibleChild.stringValue)))
                    {
                        visibleChild.stringValue = previousTagValue;
                    }

                    continue;
                }

                EditorGUI.PropertyField(mainCtx.GetPropertyRect(rect, visibleChild), visibleChild);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        protected virtual void DrawListHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Pools");
        }

        protected virtual void GetCustomPropertyDrawerDictionary(in Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>> dict)
        {
            ObjectPooler target = base.target as ObjectPooler;

            dict.Add("m_pools", new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
            {
                // Ah, the joys of 'ReorderableList'
                // It is about to commence.
                if (poolerTagsList == null)
                {
                    SerializedProperty elementsProperty = serializedObject.FindProperty("m_pools");
                    poolerTagsList = new ReorderableList(serializedObject, elementsProperty, true, true, false, true)
                    {
                        elementHeightCallback = GetPoolElementHeightCallback,
                        drawElementCallback = DrawPoolElementCallback,
                        drawHeaderCallback = DrawListHeaderCallback
                    };
                }

                EditorGUI.BeginChangeCheck();
                poolerTagsList.DoLayoutList();
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

                    // more local variables, YEEEEEEEs! YYYEEEEEEESSS!
                    string guiTagEditorControlName = "PoolerEditorDropdown::TagField";
                    bool selectedTagEditorControlOnce = false;
                    BasicDropdown.ShowDropdown(guiAreaRect, new Vector2(lastRepaintAddElementRect.width, dropdownStartingHeight), () =>
                    {
                        using SerializedProperty elementsProperty = serializedObject.FindProperty("m_pools");

                        // Just to do this dumb already selected thing
                        GUI.SetNextControlName(guiTagEditorControlName);
                        addElementTagString = EditorGUILayout.TextField("Tag", addElementTagString);
                        if (!selectedTagEditorControlOnce)
                        {
                            EditorGUI.FocusTextInControl(guiTagEditorControlName);
                            selectedTagEditorControlOnce = true;
                        }

                        // Check if tag is unique
                        bool tagInvalid = string.IsNullOrWhiteSpace(addElementTagString) || target.TagExists(addElementTagString);
                        using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(tagInvalid))
                        {
                            bool hasPressedEnter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
                            if ((GUILayout.Button("Add") || hasPressedEnter) && !tagInvalid)
                            {
                                elementsProperty.arraySize++;
                                elementsProperty.GetArrayElementAtIndex(elementsProperty.arraySize - 1).FindPropertyRelative(nameof(ObjectPooler.Pool.tag)).stringValue = addElementTagString;
                                addElementTagString = string.Empty;
                                BasicDropdown.HideDropdown();

                                serializedObject.ApplyModifiedProperties();
                            }
                        }

                        if (tagInvalid)
                        {
                            EditorGUILayout.HelpBox("Given tag already exists or is invalid. Please type a different tag.", MessageType.Info);
                        }

                        // If we press esc, clear all tags and close window
                        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                        {
                            addElementTagString = string.Empty;
                            BasicDropdown.HideDropdown();
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
