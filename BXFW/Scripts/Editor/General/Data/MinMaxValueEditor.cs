using System.Reflection;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(MinMaxValue))]
    public class MinMaxValueEditor : PropertyDrawer
    {
        private readonly List<PropertyTargetInfo> targetPropertyFields = new List<PropertyTargetInfo>();
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + Padding;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.y += Padding / 2f;
            position.height -= Padding;

            bool showMixed = EditorGUI.showMixedValue;

            // Do this becuase this is still technically not a property field GUI drawer
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            using SerializedProperty minProperty = property.FindPropertyRelative("m_Min");
            using SerializedProperty maxProperty = property.FindPropertyRelative("m_Max");

            Vector2 setValue = EditorGUI.Vector2Field(position, label, new Vector2(minProperty.floatValue, maxProperty.floatValue));

            if (EditorGUI.EndChangeCheck())
            {
                // note to self : Multi editing only happens with the same types, which in that case would have the same attribute on the same object.
                FieldInfo targetPropertyFieldInfo = property.GetTarget().fieldInfo;

                // Check supported attributes (for the first object)
                ClampAttribute clamp = targetPropertyFieldInfo.GetCustomAttribute<ClampAttribute>();
                if (clamp != null)
                {
                    setValue = new Vector2(
                        Mathf.Clamp(setValue.x, (float)clamp.min, (float)clamp.max),
                        Mathf.Clamp(setValue.y, (float)clamp.min, (float)clamp.max)
                    );
                }
                // Set limiter
                if (setValue.x > setValue.y)
                {
                    setValue.y = setValue.x;
                }

                // Set limited values
                minProperty.floatValue = setValue.x;
                maxProperty.floatValue = setValue.y;
            }
            EditorGUI.showMixedValue = showMixed;
            EditorGUI.EndProperty();
        }
    }
    /// <summary>
    /// Same as the <see cref="MinMaxValueEditor"/>, but integers.
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxValueInt))]
    public class MinMaxValueIntEditor : PropertyDrawer
    {
        private readonly List<PropertyTargetInfo> targetPropertyFields = new List<PropertyTargetInfo>();
        private const float Padding = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + Padding;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.y += Padding / 2f;
            position.height -= Padding;

            bool showMixed = EditorGUI.showMixedValue;

            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            using SerializedProperty minProperty = property.FindPropertyRelative("m_Min");
            using SerializedProperty maxProperty = property.FindPropertyRelative("m_Max");

            Vector2Int setValue = EditorGUI.Vector2IntField(position, label, new Vector2Int(minProperty.intValue, maxProperty.intValue));

            if (EditorGUI.EndChangeCheck())
            {
                property.GetTargetsNoAlloc(targetPropertyFields);

                // Check supported attributes (for the first object)
                ClampAttribute clamp = targetPropertyFields[0].fieldInfo.GetCustomAttribute<ClampAttribute>();
                if (clamp != null)
                {
                    setValue = new Vector2Int(
                        Mathf.Clamp(setValue.x, (int)clamp.min, (int)clamp.max),
                        Mathf.Clamp(setValue.y, (int)clamp.min, (int)clamp.max)
                    );
                }
                // Set limiter
                if (setValue.x > setValue.y)
                {
                    setValue.y = setValue.x;
                }

                minProperty.intValue = setValue.x;
                maxProperty.intValue = setValue.y;
            }
            EditorGUI.showMixedValue = showMixed;
            EditorGUI.EndProperty();
        }
    }
}
