using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    // * TODO : Add a 'BezierPathDrawerEditWindow' thing for allowing easier drawing to the scene and random places.
    // This is a long term plan.
    /// <summary>
    /// Updates the path whenever a change happens.
    /// </summary>
    [CustomPropertyDrawer(typeof(BezierPath))]
    public class BezierPathEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (!property.isExpanded)
            {
                return height;
            }

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(visibleProp) + mainCtx.Padding;
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            EditorGUI.BeginChangeCheck();
            foreach (var visibleProp in property.GetVisibleChildren())
            {
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // Re-generate path because something was changed
                // FIXME : This won't work on struct parents, the path will be generated in runtime instead
                Undo.RecordObject(property.serializedObject.targetObject, "set value");
                ((BezierPath)property.GetTarget().value).UpdatePath();
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
