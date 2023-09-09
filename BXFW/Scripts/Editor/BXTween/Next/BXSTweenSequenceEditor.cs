using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System.Linq;

namespace BXFW.Tweening.Next.Editor
{
    [CustomPropertyDrawer(typeof(BXSTweenSequence))]
    public class BXSTweenSequenceEditor : PropertyDrawer
    {
        private const float PADDING = 2f;
        /// <summary>
        /// Name list of fields to be omitted.
        /// </summary>
        private static readonly string[] OMIT_NAMES =
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
            float height = EditorGUIUtility.singleLineHeight + PADDING;

            if (!property.isExpanded)
                return height;

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                if (OMIT_NAMES.Any(name => visibleProp.name == name))
                {
                    continue;
                }

                if (visibleProp.name == $"m_{nameof(BXSTweenable.Duration)}")
                {
                    height += EditorGUIUtility.singleLineHeight + PADDING;

                    continue;
                }

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

            foreach (var visibleProp in property.GetVisibleChildren())
            {
                if (OMIT_NAMES.Any(name => visibleProp.name == name))
                {
                    continue;
                }

                if (visibleProp.name == $"m_{nameof(BXSTweenable.Duration)}")
                {
                    using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
                    {
                        // Draw a read-only property
                        EditorGUI.FloatField(GetPropertyRect(indentedPosition, EditorGUIUtility.singleLineHeight), "Total Duration", ((BXSTweenable)property.GetTarget().Value).Duration);
                    }

                    continue;
                }

                EditorGUI.PropertyField(GetPropertyRect(indentedPosition, visibleProp), visibleProp, true);
            }
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    }
}
