﻿using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Use this class inside your shader's property declaration as <c>[ShowAsVector2]</c> for displaying only 2 fields in a shader field.
    /// </summary>
    public class ShowAsVector2Drawer : MaterialPropertyDrawer
    {
        private bool isDrawingOnInvalidType = false;

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            isDrawingOnInvalidType = prop.type != MaterialProperty.PropType.Vector;

            return isDrawingOnInvalidType ? EditorGUIUtility.singleLineHeight * 2 : base.GetPropertyHeight(prop, label, editor);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (isDrawingOnInvalidType)
            {
                EditorGUI.HelpBox(position, $"ShowAsVector2 on invalid field {prop.displayName}. Please use vector fields only.", MessageType.Warning);
                return;
            }

            Vector2 value = (Vector2)prop.vectorValue;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // act as this is begin horizontal (because unity is acting dumb)
            // Get the current given area and subtract from it
            float labelWidth = position.width * .40f; // 40% of the given area, unity does it like this
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect vec2fieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);
            Vector2 matValue = EditorGUI.Vector2Field(vec2fieldRect, GUIContent.none, value);

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = matValue;
            }
        }
    }
    /// <summary>
    /// Use this class inside your shader's property declaration as <c>[ShowAsVector3]</c> for displaying only 3 fields in a shader field.
    /// </summary>
    public class ShowAsVector3Drawer : MaterialPropertyDrawer
    {
        private bool isDrawingOnInvalidType = false;

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            isDrawingOnInvalidType = prop.type != MaterialProperty.PropType.Vector;

            return isDrawingOnInvalidType ? EditorGUIUtility.singleLineHeight * 2 : base.GetPropertyHeight(prop, label, editor);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (isDrawingOnInvalidType)
            {
                EditorGUI.HelpBox(position, $"ShowAsVector2 on invalid field {prop.displayName}. Please use vector fields only.", MessageType.Warning);
                return;
            }

            Vector3 value = (Vector3)prop.vectorValue;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // act as this is begin horizontal (because unity is acting dumb)
            // Get the current given area and subtract from it
            float labelWidth = position.width * .40f; // 40% of the given area, unity does it like this
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect vec2fieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);
            Vector3 matValue = EditorGUI.Vector3Field(vec2fieldRect, GUIContent.none, value);

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = matValue;
            }
        }
    }
}