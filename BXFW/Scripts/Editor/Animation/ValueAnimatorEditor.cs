using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(ValueAnimatorBase.Sequence), true)]
    public class ValueAnimatorSequenceEditor : PropertyDrawer
    {
        private const float PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + PADDING;

            if (!property.isExpanded)
                return height;

            // ValueAnimatorBase.Sequence.Duration
            height += EditorGUIUtility.singleLineHeight + PADDING;

            // GUI.Button = ValueAnimatorBase.Sequence.Clear();
            height += EditorGUIUtility.singleLineHeight + PADDING;

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp) + PADDING;
            }
            
            return height;
        }

        // Maybe TODO : Create a 'AutoRectPropertyDrawer' class that you can inherit from and it contains this recting, other things, etc.)
        // Current class list that repeats this code is :
        // - ValueAnimatorEditor.cs
        // - BXSTweenSequenceEditor.cs
        // - BezierPathEditor.cs

        /// <summary>
        /// The current Y elapsed for this property field.
        /// </summary>
        private float m_currentY = 0f;
        private Rect GetPropertyRect(Rect baseRect, SerializedProperty property)
        {
            return GetPropertyRect(baseRect, EditorGUI.GetPropertyHeight(property));
        }
        private Rect GetPropertyRect(Rect baseRect, float height)
        {
            baseRect.height = height;                  // set to target height
            baseRect.y += m_currentY + (PADDING / 2f); // offset by Y
            m_currentY += height + PADDING;            // add Y offset

            return baseRect;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_currentY = 0f;
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            var targetValue = (ValueAnimatorBase.Sequence)property.GetTarget().value;

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
            {
                EditorGUI.FloatField(
                    GetPropertyRect(indentedPosition, EditorGUIUtility.singleLineHeight),
                    new GUIContent("Total Duration", "The length (in seconds) that this animation will take."),
                    targetValue.Duration
                );
            }

            if (GUI.Button(GetPropertyRect(indentedPosition, EditorGUIUtility.singleLineHeight), "Clear Frames"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Clear Frames");

                targetValue.Clear();
            }

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.PropertyField(GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
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
