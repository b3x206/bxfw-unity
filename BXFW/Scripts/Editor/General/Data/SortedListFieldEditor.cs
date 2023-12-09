using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(SortedListBase), true)]
    public class SortedListFieldEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + 2f;
            
            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp);
            }
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            Rect labelRect = new Rect(position) { y = position.y + 1f, height = EditorGUIUtility.singleLineHeight };
            position.height -= EditorGUIUtility.singleLineHeight + 2f;
            position.y += EditorGUIUtility.singleLineHeight + 2f;
            EditorGUI.LabelField(labelRect, label);

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            position = EditorGUI.IndentedRect(position);

            float currentY = position.y;

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.BeginChangeCheck();

                float propHeight = EditorGUI.GetPropertyHeight(visibleProp);
                EditorGUI.PropertyField(new Rect(position)
                {
                    height = propHeight,
                    y = currentY
                }, visibleProp);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set element in sorted array");
                    property.serializedObject.ApplyModifiedProperties();

                    SortedListBase listBase = property.GetTarget().Value as SortedListBase;

                    // Eh, this is fine. It doesn't hinder the ability of 'ReorderableList' setting it's values, it's just not clamped the cool way.
                    if (!listBase.IsSorted())
                    {
                        listBase.Sort();
                    }
                }

                currentY += propHeight;
            }

            EditorGUI.indentLevel = prevIndent;
            EditorGUI.EndProperty();
        }
    }
}