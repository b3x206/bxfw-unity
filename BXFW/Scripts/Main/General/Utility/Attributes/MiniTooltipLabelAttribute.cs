using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to show a tooltip label on the top of the field.
    /// </summary>
    public sealed class MiniTooltipLabelAttribute : PropertyAttribute
    {
        /// <summary>
        /// Set alignment of the label in the GUI.
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        /// <summary>
        /// Content of this tooltip label.
        /// </summary>
        public readonly string text;

        /// <summary>
        /// Padding to apply inbetween the field and the label.
        /// </summary>
        public readonly float padding = 2f;

        /// <summary>
        /// Whether to draw the label as bold.
        /// </summary>
        public readonly bool bold;

        public MiniTooltipLabelAttribute(string text)
        {
            this.text = text;
        }
        public MiniTooltipLabelAttribute(string text, bool bold)
        {
            this.text = text;
            this.bold = bold;
        }
        public MiniTooltipLabelAttribute(string text, float padding)
        {
            this.text = text;
            this.padding = Mathf.Clamp(padding, 0f, 65536f);
        }
        public MiniTooltipLabelAttribute(string text, float padding, bool bold)
        {
            this.text = text;
            this.padding = Mathf.Clamp(padding, 0f, 65536f);
            this.bold = bold;
        }
    }
}
