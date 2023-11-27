using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(DisallowCharsAttribute))]
    public class DisallowCharsDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 22f;
        private DisallowCharsAttribute Attribute => attribute as DisallowCharsAttribute;
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType == SerializedPropertyType.String)
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

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginChangeCheck();
                string editString = EditorGUI.TextField(position, label, property.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!string.IsNullOrEmpty(Attribute.disallowText))
                    {
                        if (Attribute.isRegex)
                        {
                            Regex r = new Regex(Attribute.disallowText, Attribute.regexOpts);
                            // Remove all matches from the string
                            property.stringValue = r.Replace(editString, string.Empty);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder(editString);
                            for (int i = sb.Length - 1; i >= 0; i--)
                            {
                                if (Attribute.disallowText.Any(c => c == sb[i]))
                                {
                                    sb.Remove(i, 1);
                                }
                            }

                            property.stringValue = sb.ToString();
                        }
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, $"Given type isn't valid for property {label.text}. Please pass string as type.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
