using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;

namespace BXFW.GUIForge.ScriptEditor
{
    /// <summary>
    /// A small drawer.
    /// <br>PropertyDrawer's are not supported by GUIForge (yet)</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(GUIRect))]
    internal class GUIRectEditor : PropertyDrawer
    {
        private const float hPadding = 2; 

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight * 2) + (hPadding * 3); // Fixed size struct
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var target = (GUIRect)property.GetTarget().Value;
            // target.Value is struct, which is non-nullable

            // Field size is relative sized rect. (40% relative)
            GUIRect rectRelative = new GUIRect(0f, 0f, .6f, 0f, position)
            {
                isRelative = true,
            };
            rectRelative.x = position.width - rectRelative.absWidth;

            GUIRect rectRelativeDown = new GUIRect(rectRelative);
            rectRelativeDown.y += EditorGUIUtility.singleLineHeight + hPadding;

            EditorGUI.BeginChangeCheck();

            target.position = EditorGUI.Vector2Field(position, "Position", target.position);
            target.size = EditorGUI.Vector2Field(new Rect(position)
            {
                y = position.y + EditorGUIUtility.singleLineHeight + hPadding
            }, "Size", target.size);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.exposedReferenceValue != null)
                    Undo.RecordObject(property.exposedReferenceValue, "Change rect");
            }

            EditorGUI.EndProperty();
        }
    }
}
