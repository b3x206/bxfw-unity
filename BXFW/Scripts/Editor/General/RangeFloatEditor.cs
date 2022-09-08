using BXFW.Tools.Editor;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(RangeFloat))]
    internal class RangeFloatEditor : PropertyDrawer
    {
        private bool rangedView = false;
        private Vector2 rangeMinMax;

        //private const float GUI_PADDING = 6;
        private const float GUI_SLIDER_LINE_PADDING = 2;
        private const float GUI_RANGE_VIEW_TEXT_HEIGHT = 12;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            rangedView = false;

            // Called before OnGUI

            // cbt
            var attb = property.GetTarget().Key.GetCustomAttribute<RFAttribute>();
            if (attb != null)
            {
                rangedView = true;
                rangeMinMax = new Vector2(attb.min, attb.max);
            }
           
            return (EditorGUIUtility.singleLineHeight * 2f) + 
                (rangedView ? EditorGUIUtility.singleLineHeight + GUI_RANGE_VIEW_TEXT_HEIGHT :
                    EditorGUIUtility.singleLineHeight);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.HelpBox(new Rect(position)
            {
                height = EditorGUIUtility.singleLineHeight * 2
            }, "This is still not done. Don't use.", MessageType.Warning);

            position.y += EditorGUIUtility.singleLineHeight * 2;

            GUI.Label(new Rect()
            {
                // Need to offset the Y by 4 to make it look normal
                // amazing
                x = position.x,
                y = position.y - 4,
                width = position.width * 0.4f,
                height = position.height
            }, label);

            // Position is always 40% offseted
            position.x += position.width * 0.4f;
            position.width -= position.width * 0.4f;

            // These properties are float
            var propMin = property.FindPropertyRelative("m_min");
            var propMax = property.FindPropertyRelative("m_max");

            // Has 'Range' attribute
            if (rangedView)
            {
                // Draw a 2 handle slider with a box in it
                float actualFieldHeight = position.height - GUI_RANGE_VIEW_TEXT_HEIGHT;
                float xClampMin = position.x + GUI_SLIDER_LINE_PADDING;
                float xClampMax = position.x + position.width - GUI_SLIDER_LINE_PADDING;
                float buttonsYPos = position.y; // Pos Y is already padded

                // Slider line
                GUI.Box(new Rect()
                {
                    x = xClampMin,
                    y = buttonsYPos,
                    height = actualFieldHeight / 2f,
                    width = xClampMax - xClampMin,
                }, GUIContent.none);

                // Rect position is solely dependant of the property's min / max value
                // Map method taken from https://forum.unity.com/threads/mapping-or-scaling-values-to-a-new-range.180090/
                float r_posMinX = Additionals.Map(xClampMin, xClampMax, rangeMinMax.x, rangeMinMax.y, propMin.floatValue);
                if (GUI.RepeatButton(new Rect(
                    r_posMinX, buttonsYPos,
                    actualFieldHeight, actualFieldHeight), GUIContent.none, EditorStyles.miniButton))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Change (using inverted map)
                        r_posMinX += Event.current.delta.x;

                        propMin.floatValue = Mathf.Clamp(
                            Additionals.Map(rangeMinMax.x, rangeMinMax.y, xClampMin, xClampMax, r_posMinX),
                            rangeMinMax.x, propMax.floatValue - float.Epsilon);
                    }
                }
                float r_posMaxX = Additionals.Map(xClampMin, xClampMax, rangeMinMax.x, rangeMinMax.y, propMax.floatValue);
                if (GUI.RepeatButton(new Rect(
                    r_posMaxX, buttonsYPos,
                    actualFieldHeight, actualFieldHeight), GUIContent.none, EditorStyles.miniButton))
                {
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Change (using inverted map)
                        r_posMaxX += Event.current.delta.x;

                        propMax.floatValue = Mathf.Clamp(
                            Additionals.Map(rangeMinMax.x, rangeMinMax.y, xClampMin, xClampMax, r_posMaxX),
                            propMin.floatValue + float.Epsilon, rangeMinMax.y);
                    }
                }

                // Draw the min number at bottom
                float yPosLabels = position.y + actualFieldHeight;
                GUI.Label(new Rect(position.x, yPosLabels, position.width / 2f, GUI_RANGE_VIEW_TEXT_HEIGHT),
                    new GUIContent(rangeMinMax.x.ToString(), "Minimum size."), new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                    });
                GUI.Label(new Rect(position.x + (position.width / 2f), yPosLabels, position.width / 2f, GUI_RANGE_VIEW_TEXT_HEIGHT),
                    new GUIContent(rangeMinMax.x.ToString(), "Maximum size."), new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleRight,
                    });
            }
            else
            {
                // Draw 2 property fields normally
                // Apparently unity doesn't know what 'normally' is
                //EditorGUI.PropertyField(
                //    new Rect(position.x, position.y, position.width / 2f, position.height),
                //    propMin);

                const float labelWidth = 50;
                GUI.Label(new Rect(position.x, position.y, labelWidth, position.height), "Min");
                propMin.floatValue = EditorGUI.FloatField(new Rect(position.x + labelWidth, position.y, (position.width / 2f) - labelWidth, position.height), propMin.floatValue);
                GUI.Label(new Rect(position.x + (position.width / 2f), position.y, labelWidth, position.height), "Max");
                propMax.floatValue = EditorGUI.FloatField(new Rect(position.x + labelWidth + (position.width / 2f) - labelWidth, position.y, position.width / 2f, position.height), propMin.floatValue);
                //EditorGUI.PropertyField(
                //    new Rect(position.x + (position.width / 2f), position.y, position.width / 2f, position.height),
                //    propMax);
            }

            EditorGUI.EndProperty();
        }
    }
}