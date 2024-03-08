using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.Collections.ScriptEditor
{
    /// <summary>
    /// An inspector for the <see cref="SortedListBase"/>.
    /// <br>Only draws the given internal list value 'm_list'.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(SortedListBase), true)]
    public class SortedListFieldEditor : PropertyDrawer
    {
        private const float WarningBoxHeight = 40f;
        private const float FoldoutArrowWidth = 3f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProperty = property.FindPropertyRelative("m_list");

            // Only draw the warning box if the given child type is not serializable
            float height = listProperty == null ?
                EditorGUIUtility.singleLineHeight + 2f + WarningBoxHeight :
                EditorGUI.GetPropertyHeight(listProperty);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Using the given label gives a random value of the drawn ReorderableList
            label = new GUIContent(ObjectNames.NicifyVariableName(property.name), property.tooltip);
            
            Rect foldoutLabelRect = new Rect(position) { y = position.y + 1f, height = EditorGUIUtility.singleLineHeight };
            SerializedProperty listProperty = property.FindPropertyRelative("m_list");
            // List type cannot be serialized.
            if (listProperty == null)
            {
                EditorGUI.LabelField(new Rect(foldoutLabelRect)
                {
                    x = foldoutLabelRect.x + FoldoutArrowWidth,
                    width = foldoutLabelRect.width - FoldoutArrowWidth
                }, label, EditorStyles.boldLabel);

                EditorGUI.HelpBox(new Rect()
                {
                    x = foldoutLabelRect.x + 12f,
                    y = position.y + EditorGUIUtility.singleLineHeight + 2f,
                    height = WarningBoxHeight,
                    width = foldoutLabelRect.width - 12f
                }, $"[SortedList] Type could not be serialized for field '{label.text}'! Ensure that the type is serialized.\nIf you don't want the type serializable use [HideInInspector]", MessageType.Warning);

                EditorGUI.EndProperty();
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, listProperty, GUIContent.none, true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set element in sorted array");
                property.serializedObject.ApplyModifiedProperties();

                SortedListBase listBase = property.GetTarget().value as SortedListBase;

                // Eh, this is fine. It doesn't hinder the ability of 'ReorderableList'
                // setting it's values, it's just not clamped the cool way.
                if (!listBase.IsSorted())
                {
                    listBase.Sort();
                }
            }

            // Draw the label last to draw it over the given ReorderableList
            EditorGUI.LabelField(new Rect(foldoutLabelRect)
            {
                x = foldoutLabelRect.x + FoldoutArrowWidth,
                width = foldoutLabelRect.width - FoldoutArrowWidth
            }, label, EditorStyles.boldLabel);

            EditorGUI.EndProperty();
        }
    }
}
