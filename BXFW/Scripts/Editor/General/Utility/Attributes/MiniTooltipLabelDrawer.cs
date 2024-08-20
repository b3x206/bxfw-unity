using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(MiniTooltipLabelAttribute))]
    public class MiniTooltipLabelDrawer : DecoratorDrawer
    {
        public MiniTooltipLabelAttribute Attribute => attribute as MiniTooltipLabelAttribute;
        private GUIContent m_attributeContent;
        private GUIStyle m_attributeStyle;

        public override float GetHeight()
        {
            m_attributeContent ??= new GUIContent(Attribute.text);
            m_attributeStyle ??= new GUIStyle(Attribute.bold ? EditorStyles.miniBoldLabel : EditorStyles.miniLabel);

            return m_attributeStyle.CalcHeight(m_attributeContent, EditorGUIUtility.currentViewWidth) + Attribute.padding;
        }
        public override void OnGUI(Rect position)
        {
            // Use the scope to get the disabled color for this label
            using EditorGUI.DisabledScope _ = new EditorGUI.DisabledScope(true);

            m_attributeStyle.alignment = Attribute.Alignment switch
            {
                TextAlignment.Left => TextAnchor.UpperLeft,
                TextAlignment.Center => TextAnchor.UpperCenter,
                TextAlignment.Right => TextAnchor.UpperRight,
                _ => TextAnchor.MiddleLeft,
            };

            EditorGUI.LabelField(position, m_attributeContent, m_attributeStyle);
        }
    }
}
