using UnityEditor;
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

            // Get the current given area and subtract from it
            float labelWidth = EditorGUIUtility.labelWidth;
            // Reserved rect for the label
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            // Reserved rect for the vector2 field
            Rect vecfieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            // Draw
            EditorGUI.LabelField(labelRect, label);
            Vector2 matValue = EditorGUI.Vector2Field(vecfieldRect, GUIContent.none, value);
            // Apply
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
                EditorGUI.HelpBox(position, $"ShowAsVector3 on invalid field {prop.displayName}. Please use vector fields only.", MessageType.Warning);
                return;
            }

            Vector3 value = (Vector3)prop.vectorValue;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // act as this is begin horizontal (because unity is acting dumb)
            // Get the current given area and subtract from it
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
            Rect vecfieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            EditorGUI.LabelField(labelRect, label);
            Vector3 matValue = EditorGUI.Vector3Field(vecfieldRect, GUIContent.none, value);

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = matValue;
            }
        }
    }
}
