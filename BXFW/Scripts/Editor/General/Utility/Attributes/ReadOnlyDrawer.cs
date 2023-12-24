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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);

            return UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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
            }
        }
    }
}
