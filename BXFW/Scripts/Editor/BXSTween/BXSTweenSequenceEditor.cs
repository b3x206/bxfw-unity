using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System.Linq;

namespace BXFW.Tweening.Editor
{
    [CustomPropertyDrawer(typeof(BXSTweenSequence))]
    public class BXSTweenSequenceEditor : PropertyDrawer
    {
        private readonly PropertyRectContext mainCtx = new PropertyRectContext();
        /// <summary>
        /// Name list of fields to be omitted.
        /// </summary>
        private static readonly string[] FieldOmitNameList =
        {
            $"m_{nameof(BXSTweenable.TickType)}",
            $"m_{nameof(BXSTweenable.UseEaseCurve)}",
            $"m_{nameof(BXSTweenable.Ease)}",
            $"m_{nameof(BXSTweenable.EaseCurve)}",
            $"m_{nameof(BXSTweenable.Clamp01EasingSetter)}",
            $"m_{nameof(BXSTweenable.Speed)}",
            $"m_{nameof(BXSTweenable.LoopType)}",
        };

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;

            if (!property.isExpanded)
            {
                return height;
            }

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                if (FieldOmitNameList.Any(name => visibleProp.name == name))
                {
                    continue;
                }

                if (visibleProp.name == $"m_{nameof(BXSTweenable.Duration)}")
                {
                    height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;

                    continue;
                }

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

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                if (FieldOmitNameList.Any(name => visibleProp.name == name))
                {
                    continue;
                }

                if (visibleProp.name == $"m_{nameof(BXSTweenable.Duration)}")
                {
                    using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
                    {
                        // Draw a read-only property
                        EditorGUI.FloatField(mainCtx.GetPropertyRect(indentedPosition, EditorGUIUtility.singleLineHeight), "Total Duration", ((BXSTweenable)property.GetTarget().value).Duration);
                    }

                    continue;
                }

                EditorGUI.PropertyField(mainCtx.GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
