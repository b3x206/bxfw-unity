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

            property.GetTargetsNoAlloc(targetPairs);
            var firstValuePair = targetPairs.First();
            EditorGUI.showMixedValue = targetPairs.Any(p => ((MinMaxValue)p.Value) != ((MinMaxValue)firstValuePair.Value));

            //Rect vectorsRect = new Rect(position) { x = position.x + labelRect.width, width = position.width - labelRect.width };
            EditorGUI.BeginChangeCheck();
            Vector2 v = EditorGUI.Vector2Field(position, label, (MinMaxValue)firstValuePair.Value);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("set MinMaxValue");
                int undoID = Undo.GetCurrentGroup();
                for (int i = 0; i < targetPairs.Count; i++)
                {
                    var pair = targetPairs[i];
                    var parent = property.GetParentOfTargetField().Value;
                    Vector2 setValue = v;

                    // Check supported attributes
                    ClampAttribute clamp = pair.Key.GetCustomAttribute<ClampAttribute>();
                    if (clamp != null)
                    {
                        setValue = new Vector2(
                            Mathf.Clamp(setValue.x, (float)clamp.min, (float)clamp.max),
                            Mathf.Clamp(setValue.y, (float)clamp.min, (float)clamp.max)
                        );
                    }

                    Undo.RecordObject(property.serializedObject.targetObject, string.Empty);
                    pair.Key.SetValue(parent, (MinMaxValue)setValue);
                }

                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUI.showMixedValue = showMixed;
            EditorGUI.EndProperty();
        }
    }
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
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("set MinMaxValue");
                int undoID = Undo.GetCurrentGroup();
                for (int i = 0; i < targetPairs.Count; i++)
                {
                    var pair = targetPairs[i];
                    var parent = property.GetParentOfTargetField().Value;
                    Vector2Int setValue = v;

                    // Check supported attributes
                    ClampAttribute clamp = pair.Key.GetCustomAttribute<ClampAttribute>();
                    if (clamp != null)
                    {
                        setValue = new Vector2Int(
                            Mathf.Clamp(setValue.x, (int)clamp.min, (int)clamp.max),
                            Mathf.Clamp(setValue.y, (int)clamp.min, (int)clamp.max)
                        );
                    }

                    Undo.RecordObject(property.serializedObject.targetObject, string.Empty);
                    pair.Key.SetValue(parent, (MinMaxValueInt)setValue);
                }

                Undo.CollapseUndoOperations(undoID);
            }
            EditorGUI.showMixedValue = showMixed;
            EditorGUI.EndProperty();
        }
    }
}