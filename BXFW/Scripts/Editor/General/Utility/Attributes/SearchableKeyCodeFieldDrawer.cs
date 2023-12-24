using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(SearchableKeyCodeFieldAttribute))]
    public class SearchableKeyCodeFieldDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private const float Padding = 2f;
        private bool propertyTypeIsValid = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            propertyTypeIsValid = property.GetPropertyType() == typeof(KeyCode) ;
            if (propertyTypeIsValid)
            {
                addHeight += EditorGUIUtility.singleLineHeight + Padding;
            }
            else
            {
                addHeight += WarningBoxHeight;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= Padding;
            position.y += Padding / 2f;

            if (propertyTypeIsValid)
            {
                KeyCode selectedValue = (KeyCode)property.longValue;

                // Draw a dropdown selector with label
                Rect labelRect = new Rect(position.x, position.y + (Padding / 2f), EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, label);

                bool previousShowMixed = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
                // For some reason adding padding to the 'dropdownRect's y coordinate causes it to be off center
                // Thanks unity for making amazing GUIStyle's that i enjoy with it's amazing quirks (lol)
                Rect dropdownRect = new Rect(position.x + labelRect.width, position.y, Mathf.Max(position.width - labelRect.width, EditorGUIUtility.fieldWidth), EditorGUIUtility.singleLineHeight);
                if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(ObjectNames.NicifyVariableName(selectedValue.ToString()), label.tooltip), FocusType.Keyboard))
                {
                    // Display a 'KeyCode' selector
                    KeyCodeSelectorDropdown keyCodeSelector = new KeyCodeSelectorDropdown((KeyCode)property.intValue);
                    keyCodeSelector.Show(dropdownRect);

                    SerializedObject copySo = new SerializedObject(property.serializedObject.targetObjects);
                    SerializedProperty copySetProperty = copySo.FindProperty(property.propertyPath);
                    keyCodeSelector.OnElementSelectedEvent += (SearchDropdownElement element) =>
                    {
                        if (!(element is KeyCodeSelectorDropdown.Item item))
                        {
                            return;
                        }

                        copySetProperty.longValue = (long)item.keyValue;
                        copySo.ApplyModifiedProperties();

                        copySetProperty.Dispose();
                        copySo.Dispose();
                    };
                    keyCodeSelector.OnDiscardEvent += () =>
                    {
                        copySetProperty.Dispose();
                        copySo.Dispose();
                    };
                }
                EditorGUI.showMixedValue = previousShowMixed;
            }
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass KeyCode.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
