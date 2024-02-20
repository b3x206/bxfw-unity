using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the property inheriting from by the <see cref="DrawIfAttribute"/>.
    /// <br>Any attribute overriding the <see cref="DrawIfAttribute"/> can implement 
    /// it's own behaviour without having to write a <see cref="PropertyDrawer"/>.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(ConditionalDrawAttribute), true)]
    public class ConditionalDrawAttributeDrawer : PropertyDrawer
    {
        private PropertyDrawer targetTypeCustomDrawer;
        private bool UseCustomDrawer => targetTypeCustomDrawer != null;
        /// <summary>
        /// The target condition value to act on.
        /// </summary>
        private ConditionalDrawAttribute.DrawCondition currentCondition = ConditionalDrawAttribute.DrawCondition.True;
        private ConditionalDrawAttribute Attribute => (ConditionalDrawAttribute)attribute;

        /// <summary>
        /// Error details string resulting from the given attribute's <see cref="ConditionalDrawAttribute.DrawCondition.Error"/>.
        /// </summary>
        private string errorString;
        private const float MinWarningBoxHeight = 36f;
        private float WarningBoxHeight = MinWarningBoxHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // First called method before OnGUI
            // This also resets 'drawField'
            var parentPair = property.GetParentOfTargetField();

            // Always reset this for the next 'WarningBox'
            WarningBoxHeight = MinWarningBoxHeight;

            // Note : If the parent object is an list, override everything and cause an error
            // This is because Array's have the PropertyDrawer attribute applied to their children instead of the array itself
            if (parentPair.TargetIsSerializedList)
            {
                errorString = "Cannot use 'ConditionalDrawAttribute' on 'Array' targets : This is caused by unity applying the PropertyDrawer attribute to the children, which causes issues.";
                currentCondition = ConditionalDrawAttribute.DrawCondition.Error;
            }
            else
            {
                try
                {
                    // This throws exceptions if the target field that the 'DrawIf' is applied to is IEnumerable.
                    currentCondition = Attribute.GetDrawCondition(fieldInfo, parentPair.value, out errorString);
                }
                catch (Exception e)
                {
                    currentCondition = ConditionalDrawAttribute.DrawCondition.Error;
                    errorString = $"An exception occured | {e.Message}\n{e.StackTrace}";
                }
            }

            // Edit : This doesn't calculate the height incorrectly, it's just that ReorderableList incorrectly caches the height of the element, this is worse, thanks.
            WarningBoxHeight = Mathf.Max(MinWarningBoxHeight, EditorStyles.helpBox.CalcHeight(new GUIContent(errorString), EditorGUIUtility.currentViewWidth));

            switch (currentCondition)
            {
                case ConditionalDrawAttribute.DrawCondition.False:
                    return 0f;
                default:
                case ConditionalDrawAttribute.DrawCondition.True:
                    targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);
                    return UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);

                case ConditionalDrawAttribute.DrawCondition.Error:
                    return WarningBoxHeight;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            switch (currentCondition)
            {
                case ConditionalDrawAttribute.DrawCondition.False:
                    // No draw
                    return;
                default:
                case ConditionalDrawAttribute.DrawCondition.True:
                    // Draw (with CustomDrawer)
                    if (UseCustomDrawer)
                    {
                        targetTypeCustomDrawer.OnGUI(position, property, label);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                    }
                    return;

                case ConditionalDrawAttribute.DrawCondition.Error:
                    label = EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.HelpBox(position, string.Format("[ConditionalDrawAttribute] '{0}' on field '{1}'.", errorString, label.text), MessageType.Warning);
                    EditorGUI.EndProperty();
                    return;
            }
        }
    }
}
