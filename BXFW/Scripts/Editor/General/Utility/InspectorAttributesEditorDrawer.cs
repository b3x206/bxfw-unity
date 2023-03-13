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
    [CustomPropertyDrawer(typeof(BigSpriteFieldAttribute))]
    internal class BigSpriteFieldDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private float targetBoxRectHeight
        {
            get
            {
                var targetAttribute = attribute as BigSpriteFieldAttribute;

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
    internal class InspectorLineDrawer : DecoratorDrawer
    {
        private InspectorLineAttribute targetAttribute;

        public override float GetHeight()
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            return targetAttribute.GetYPosHeightOffset() * 2f;
        }

        public override void OnGUI(Rect position)
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            position.y += targetAttribute.GetYPosHeightOffset() / 2f;
            GUIAdditionals.DrawUILine(position, targetAttribute.LineColor, targetAttribute.LineThickness, targetAttribute.LinePadding);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyViewAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
    {
        private PropertyDrawer targetTypeCustomDrawer;
        private bool UseCustomDrawer => targetTypeCustomDrawer != null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);

            return UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var gEnabled = GUI.enabled;

            GUI.enabled = false;
            if (UseCustomDrawer)
            {
                // yeah, it will display 'No GUI implemented'. definitely.
                targetTypeCustomDrawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            GUI.enabled = gEnabled;
        }
    }

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    internal class ClampDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private ClampAttribute CAttribute => attribute as ClampAttribute;
        private const float DR_PADDING = 2f; 

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.Float)
            {
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                float v = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), (float)CAttribute.min, (float)CAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped float");
                    property.floatValue = v;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();
                int v = Mathf.Clamp(EditorGUI.IntField(position, label, property.intValue), (int)CAttribute.min, (int)CAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped int");
                    property.intValue = v;
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either int or float.", MessageType.Warning);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ClampVectorAttribute))]
    internal class ClampVectorDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private ClampVectorAttribute CAttribute => attribute as ClampVectorAttribute;
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Integer && property.propertyType != SerializedPropertyType.Float)
            {
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            #region Vector2
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2Field(position, label, property.vector2Value); 
                var vClamped = new Vector2(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX), 
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                var vClamped = new Vector2Int(
                    Mathf.Clamp(v.x, (int)CAttribute.minX, (int)CAttribute.maxX),
                    Mathf.Clamp(v.y, (int)CAttribute.minY, (int)CAttribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2IntValue = vClamped;
                }
            }
            #endregion
            #region Vector3
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3Field(position, label, property.vector3Value);
                var vClamped = new Vector3(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX),
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY),
                    Mathf.Clamp(v.z, (float)CAttribute.minZ, (float)CAttribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector3Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                var vClamped = new Vector3Int(
                    Mathf.Clamp(v.x, (int)CAttribute.minX, (int)CAttribute.maxX),
                    Mathf.Clamp(v.y, (int)CAttribute.minY, (int)CAttribute.maxY),
                    Mathf.Clamp(v.z, (int)CAttribute.minZ, (int)CAttribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3IntValue = vClamped;
                }
            }
            #endregion
            #region Vector4
            else if (property.propertyType == SerializedPropertyType.Vector4)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector4Field(position, label, property.vector4Value);
                var vClamped = new Vector4(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX),
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY),
                    Mathf.Clamp(v.z, (float)CAttribute.minZ, (float)CAttribute.maxZ),
                    Mathf.Clamp(v.w, (float)CAttribute.minZ, (float)CAttribute.maxW)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector4Value = vClamped;
                }
            }
            #endregion
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either Vector or VectorInt.", MessageType.Warning);
            }
        }
    }
}