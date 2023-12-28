using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the target <see cref="PropertyDrawer"/> for this drawer's target 
    /// field's type on a disabled <see cref="EditorGUI.DisabledScope"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyViewAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        private PropertyDrawer targetTypeCustomDrawer;
        private bool UseCustomDrawer => targetTypeCustomDrawer != null;

        private bool propertyHeightIsDirty = true;
        private float propertyHeight = 0f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);

            if (propertyHeightIsDirty)
            {
                propertyHeight = UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
                propertyHeightIsDirty = false;
            }

            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Use 'GUI.changed' after the drawers as it checks for any change
            // Unlike 'EditorGUI.BeginChangeCheck' which only checks for specific controls
            using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
            {
                if (UseCustomDrawer)
                {
                    // yeah, it will display 'No GUI implemented'. definitely.
                    // Use this to not default into the defualt property drawer.
                    targetTypeCustomDrawer.OnGUI(position, property, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }

                propertyHeightIsDirty = GUI.changed;
            }
        }
    }
}
