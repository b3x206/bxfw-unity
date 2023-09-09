using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    // TODO : Add a 'BezierPathDrawerEditWindow' thing for allowing easier drawing to the scene and random places.
    // This is a long term TODO

    /// <summary>
    /// Updates the path whenever a change happens.
    /// </summary>
    [CustomPropertyDrawer(typeof(BezierPath))]
    public class BezierPathEditor : PropertyDrawer
    {
        private const float PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + PADDING;

            if (!property.isExpanded)
                return height;

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp) + PADDING;
            }

            return height;
        }

        private float m_currentY = 0f;
        private Rect GetPropertyRect(Rect baseRect, SerializedProperty property)
        {
            return GetPropertyRect(baseRect, EditorGUI.GetPropertyHeight(property));
        }
        private Rect GetPropertyRect(Rect baseRect, float height)
        {
            baseRect.height = height;                // set to target height
            baseRect.y += m_currentY + PADDING / 2f; // offset by Y
            m_currentY += height + PADDING;          // add Y offset

            return baseRect;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            m_currentY = 0f;
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            EditorGUI.BeginChangeCheck();
            foreach (var visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.PropertyField(GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Re-generate path because something was changed
                Undo.RecordObject(property.serializedObject.targetObject, "set value");
                ((BezierPath)property.GetTarget().Value).UpdatePath();
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
