using System.Linq;
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
        private readonly List<KeyValuePair<FieldInfo, object>> targetPairs = new List<KeyValuePair<FieldInfo, object>>();
        private const float PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + PADDING;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.y += PADDING / 2f;
            position.height -= PADDING;

            bool showMixed = EditorGUI.showMixedValue;

            //Rect labelRect = new Rect(position) { width = EditorGUIUtility.labelWidth };
            //EditorGUI.LabelField(labelRect, label);
            //position.width -= labelRect.width;
            //position.x += labelRect.width;

            // Do this becuase this is still technically not a property field GUI drawer
            property.GetTargetsNoAlloc(targetPairs);
            var firstValuePair = targetPairs.First();
            EditorGUI.showMixedValue = targetPairs.Any(p => ((MinMaxValue)p.Value) != ((MinMaxValue)firstValuePair.Value));

            //Rect vectorsRect = new Rect(position) { x = position.x + labelRect.width, width = position.width - labelRect.width };
            //Rect minRect = new Rect(vectorsRect) { width = vectorsRect.width / 2f };
            //Rect maxRect = new Rect(vectorsRect) { x = vectorsRect.x + minRect.width, width = vectorsRect.width / 2f };
            //EditorGUI.BeginChangeCheck();
            //EditorGUI.PropertyField(minRect, property.FindPropertyRelative("m_Min"), new GUIContent("Min"));
            //EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("m_Max"), new GUIContent("Max"));

            Vector2 v = EditorGUI.Vector2Field(position, label, (MinMaxValue)firstValuePair.Value);

            if (EditorGUI.EndChangeCheck())
            {
                Vector2 setValue = v;

                using SerializedProperty minProperty = property.FindPropertyRelative("m_Min");
                using SerializedProperty maxProperty = property.FindPropertyRelative("m_Max");

                // Check supported attributes (for the first object)
                ClampAttribute clamp = firstValuePair.Key.GetCustomAttribute<ClampAttribute>();
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
        private readonly List<KeyValuePair<FieldInfo, object>> targetPairs = new List<KeyValuePair<FieldInfo, object>>();
        private const float PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + PADDING;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.y += PADDING / 2f;
            position.height -= PADDING;

            bool showMixed = EditorGUI.showMixedValue;

            property.GetTargetsNoAlloc(targetPairs);
            var firstValuePair = targetPairs.First();
            EditorGUI.showMixedValue = targetPairs.Any(p => ((MinMaxValueInt)p.Value) != ((MinMaxValueInt)firstValuePair.Value));

            EditorGUI.BeginChangeCheck();
            Vector2Int v = EditorGUI.Vector2IntField(position, label, (MinMaxValueInt)firstValuePair.Value);
            if (EditorGUI.EndChangeCheck())
            {
                Vector2Int setValue = v;
                using SerializedProperty minProperty = property.FindPropertyRelative("m_Min");
                using SerializedProperty maxProperty = property.FindPropertyRelative("m_Max");

                // Check supported attributes (for the first object)
                ClampAttribute clamp = firstValuePair.Key.GetCustomAttribute<ClampAttribute>();
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