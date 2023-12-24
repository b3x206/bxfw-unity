using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 32f;
        private const string DefaultSelectedTag = "Untagged";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return WarningBoxHeight;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position,
                    string.Format("Warning : Usage of 'TagSelectorAttribute' on field \"{0} {1}\" even though the field type isn't 'System.String'.", property.type, property.name),
                    MessageType.Warning);

                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.BeginChangeCheck();

            // If the field's string value isn't untagged (default), set the value
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                property.stringValue = DefaultSelectedTag;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        }
    }
}
