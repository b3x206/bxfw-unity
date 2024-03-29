﻿using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ValueAnimatorBase.Sequence), true)]
    public class ValueAnimatorSequenceEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();
        private const float ClearFramesButtonHeight = 20f;
        private const float ReverseFramesButtonHeight = 20f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (!property.isExpanded)
            {
                return height;
            }

            // ValueAnimatorBase.Sequence.Duration
            height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            // GUI.Button = ValueAnimatorBase.Sequence.Clear();
            height += ClearFramesButtonHeight + mainCtx.Padding;
            // GUI.Button = ValueAnimatorBase.Sequence.Reverse();
            height += ReverseFramesButtonHeight + mainCtx.Padding;
            // Line
            height += 6f;

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp) + mainCtx.Padding;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            ValueAnimatorBase.Sequence targetValue = (ValueAnimatorBase.Sequence)property.GetTarget().value;

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
            {
                EditorGUI.FloatField(
                    mainCtx.GetPropertyRect(indentedPosition, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Total Duration", "The length (in seconds) that this animation will take."),
                    targetValue.Duration
                );
            }

            if (GUI.Button(mainCtx.GetPropertyRect(indentedPosition, ClearFramesButtonHeight), "Clear Frames"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Clear Frames");

                targetValue.Clear();
            }
            if (GUI.Button(mainCtx.GetPropertyRect(indentedPosition, ReverseFramesButtonHeight), "Reverse Frames"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Reverse Frames");

                targetValue.Reverse();
            }
            GUIAdditionals.DrawUILine(mainCtx.GetPropertyRect(position, 6), EditorGUIUtility.isProSkin ? Color.gray : new Color(0.4f, 0.4f, 0.4f));

            foreach (SerializedProperty visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }

    [CustomEditor(typeof(ValueAnimatorBase), true)]
    public class ValueAnimatorEditor : Editor
    {
        protected virtual void GetCustomPropertyDrawerDictionary(Dictionary<string, KeyValuePair<MatchGUIActionOrder, System.Action>> dict)
        {
            var target = base.target as ValueAnimatorBase;
            dict.Add("m_CurrentAnimIndex", new KeyValuePair<MatchGUIActionOrder, System.Action>(
                MatchGUIActionOrder.OmitAndInvoke,
                () =>
                {
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.BeginHorizontal();
                    var animIndexSet = EditorGUILayout.IntField(
                        new GUIContent("Current Animation Index", "Sets the index from 'animation' array."),
                        target.CurrentAnimIndex
                    );
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        animIndexSet++;
                    }
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        animIndexSet--;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "set CurrentAnimIndex");
                        target.CurrentAnimIndex = animIndexSet;
                    }
                })
            );
        }

        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, System.Action>>();
            GetCustomPropertyDrawerDictionary(dict);

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
