using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the '<see cref="Texture2D"/>' inspector for sprites.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorBigSpriteFieldAttribute))]
    internal class InspectorBigSpriteFieldDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private float targetBoxRectHeight
        {
            get
            {
                var targetAttribute = attribute as InspectorBigSpriteFieldAttribute;

                return targetAttribute.spriteBoxRectHeight;
            }
        }
        private KeyValuePair<FieldInfo, object> target;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
            {
                // Same story, calling 'GetPropertyHeight' before drawing gui or not allowing to dynamically change height while drawing is dumb
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += targetBoxRectHeight; // Hardcode the size as unity doesn't change it.
            }

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
            {
                EditorGUI.HelpBox(position,
                    string.Format("Warning : Usage of 'InspectorBigSpriteFieldDrawer' on field \"{0} {1}\" even though the field type isn't sprite.", property.type, property.name),
                    MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();

            // fixes position.height being incorrect on some cases
            position.height = EditorGUI.GetPropertyHeight(property, label, true) + targetBoxRectHeight;
            Sprite setValue = (Sprite)EditorGUI.ObjectField(position, new GUIContent(property.displayName, property.tooltip), property.objectReferenceValue, typeof(Sprite), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.objectReferenceValue != null)
                {
                    Undo.RecordObject(property.objectReferenceValue, "Inspector");
                }

                property.objectReferenceValue = setValue;
            }
        }
    }

    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    internal class InspectorLineDrawer : PropertyDrawer
    {
        private InspectorLineAttribute targetAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (targetAttribute != null)
                addHeight = targetAttribute.GetYPosHeightOffset();

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            targetAttribute = (InspectorLineAttribute)property.GetTarget().Key.GetCustomAttribute(typeof(InspectorLineAttribute));

            var posRect = GUIAdditionals.DrawUILine(position, targetAttribute.LineColor, targetAttribute.LineThickness, targetAttribute.LinePadding);
            EditorGUI.PropertyField(posRect, property, label, true);
        }
    }

    [CustomPropertyDrawer(typeof(InspectorReadOnlyViewAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var gEnabled = GUI.enabled;

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = gEnabled;
        }
    }
}