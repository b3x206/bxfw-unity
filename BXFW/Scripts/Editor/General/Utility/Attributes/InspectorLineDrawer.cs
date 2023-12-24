using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// A <see cref="DecoratorDrawer"/> responsible for <see cref="InspectorLineAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    public class InspectorLineDrawer : DecoratorDrawer
    {
        private InspectorLineAttribute targetAttribute;

        public override float GetHeight()
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            return targetAttribute.GetYPosHeightOffset() * 2f;
        }

        public override void OnGUI(Rect position)
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            position.y += targetAttribute.GetYPosHeightOffset() / 2f;
            GUIAdditionals.DrawUILine(position, targetAttribute.Color, targetAttribute.LineThickness, targetAttribute.LinePadding);
        }
    }
}
