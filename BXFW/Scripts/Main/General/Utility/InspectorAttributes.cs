﻿using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Attribute to draw <see cref="Sprite"/> fields as a big preview.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    public class BigSpriteFieldAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public readonly float spriteBoxRectHeight = 44f;
#endif
        public BigSpriteFieldAttribute(float spriteHeight = 44f)
        {
#if UNITY_EDITOR
            spriteBoxRectHeight = spriteHeight;
#endif
        }
    }

    /// <summary>
    /// Attribute to draw a line using <see cref="EditorAdditionals.DrawUILine(Color, int, int)"/>.
    /// </summary>
    public class InspectorLineAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Color LineColor { get; private set; }
        public int LineThickness { get; private set; }
        public int LinePadding { get; private set; }
#endif
        public InspectorLineAttribute(float r, float g, float b, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            LineColor = new Color(r, g, b);
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// Return the draw height.
        /// </summary>
        internal float GetYPosHeightOffset()
        {
            return LineThickness + LinePadding;
        }
#endif
    }

    /// <summary>
    /// Attribute to disable gui on fields.
    /// </summary>
    public class ReadOnlyViewAttribute : PropertyAttribute { }

    /// <summary>
    /// Attribute to draw clamped integers and floats in fields.
    /// </summary>
    public class ClampAttribute : PropertyAttribute
    {
        public readonly double min;
        public readonly double max;

        public ClampAttribute(double min, double max) { this.min = min; this.max = max; }
    }

    public class ClampVectorAttribute : PropertyAttribute
    {
        public readonly double minX, minY, minZ, minW;
        public readonly double maxX, maxY, maxZ, maxW;

        public ClampVectorAttribute(
            double minX, double minY, double minZ, double minW,
            double maxX, double maxY, double maxZ, double maxW
        )
        {
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;
            this.minW = minW;

            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
            this.maxW = maxW;
        }
        public ClampVectorAttribute(
            double minX, double minY, double minZ,
            double maxX, double maxY, double maxZ
        ) : this(minX, minY, minZ, 0f, maxX, maxY, maxZ, 0f)
        { }
        public ClampVectorAttribute(
            double minX, double minY,
            double maxX, double maxY
        ) : this(minX, minY, 0f, 0f, maxX, maxY, 0f, 0f)
        { }
        public ClampVectorAttribute(double min, double max) :
            this(min, min, min, min, max, max, max, max)
        { }
    }
}
