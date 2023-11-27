using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the '<see cref="Texture2D"/>' inspector for sprites with <see cref="BigSpriteFieldAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(BigSpriteFieldAttribute))]
    public class BigSpriteFieldDrawer : PropertyDrawer
    {
        private const float WarningHelpBoxHeight = 22f;
        private float TargetBoxHeight
        {
            get
            {
                return (attribute as BigSpriteFieldAttribute).spriteBoxRectHeight;
            }
        }
        private PropertyTargetInfo target;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.fieldInfo.FieldType != typeof(Sprite))
            {
                // Same story, calling 'GetPropertyHeight' before drawing gui or not allowing to dynamically change height while drawing is dumb
                addHeight += WarningHelpBoxHeight;
            }
            else
            {
                addHeight += TargetBoxHeight; // Hardcode the size as unity doesn't change it.
            }

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (target.fieldInfo == null)
            {
                target = property.GetTarget();
            }

            // Draw an object field for sprite property
            if (target.fieldInfo.FieldType != typeof(Sprite))
            {
                EditorGUI.HelpBox(
                    position,
                    string.Format("Warning : Usage of 'InspectorBigSpriteFieldDrawer' on field \"{0} {1}\" even though the field type isn't sprite.", property.type, property.name),
                    MessageType.Warning
                );
                return;
            }

            EditorGUI.BeginChangeCheck();

            // fixes position.height being incorrect on some cases
            position.height = EditorGUI.GetPropertyHeight(property, label, true) + TargetBoxHeight;
            Sprite setValue = (Sprite)EditorGUI.ObjectField(position, new GUIContent(property.displayName, property.tooltip), property.objectReferenceValue, typeof(Sprite), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.objectReferenceValue != null)
                {
                    Undo.RecordObject(property.objectReferenceValue, "set sprite");
                }

                property.objectReferenceValue = setValue;
            }
        }
    }
}
