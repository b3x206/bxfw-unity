using System;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A color list containing colors for the inspector line. Default color is <see cref="White"/>.
    /// <br/>
    /// <br><see cref="White"/> = r:1 g:1 b:1</br>
    /// <br><see cref="Red"/>   = r:1 g:0 b:0</br>
    /// <br><see cref="Green"/> = r:0 g:1 b:0</br>
    /// <br><see cref="Blue"/>  = r:0 g:0 b:1</br>
    /// <br/>
    /// <br><see cref="Gray"/>    = r:0.5 g:0.5 b:0.5</br>
    /// <br><see cref="Magenta"/> = r:1 g:0 b:1</br>
    /// <br><see cref="Yellow"/>  = r:1 g:0.92 b:0.016</br>
    /// <br><see cref="Cyan"/>    = r:0 g:1 b:1</br>
    /// <br/>
    /// <br>These colors are based on the <see cref="Color"/> struct's defaults.</br>
    /// </summary>
    public enum LineColor
    {
        White, Red, Green, Blue,
        Gray, Magenta, Yellow, Cyan,
    }

    // Maybe rename this to 'SeperatorLineAttribute' or 'PropertyHeadingLineAttribute'?
    /// <summary>
    /// Attribute to draw a line to a property using a property decorator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class InspectorLineAttribute : PropertyAttribute
    {
#if UNITY_EDITOR
        public Color Color { get; private set; }
        public int LineThickness { get; private set; }
        public int LinePadding { get; private set; }
#endif
        /// <summary>
        /// Creates a gray line with default padding and thickness.
        /// </summary>
        public InspectorLineAttribute()
        {
#if UNITY_EDITOR
            // unity is stupid so we cannot access any function from the constructors of a monobehaviour
            // TODO : either read the EditorPrefs key of the pro skin or set it to a static boolean
            // Color = UnityEditor.EditorGUIUtility.isProSkin ? Color.gray : new Color(0.3f, 0.3f, 0.3f);
            Color = Color.gray;
            LineThickness = 2;
            LinePadding = 3;
#endif
        }

        /// <summary>
        /// Creates a gray line with given thickness.
        /// <br>The <see cref="LinePadding"/> is set to 3.</br>
        /// </summary>
        public InspectorLineAttribute(int thickness)
        {
#if UNITY_EDITOR
            Color = Color.gray;
            LineThickness = thickness;
            LinePadding = 3;
#endif
        }

        /// <summary>
        /// Creates a gray line with given padding and thickness.
        /// </summary>
        public InspectorLineAttribute(int thickness, int padding)
        {
#if UNITY_EDITOR
            Color = Color.gray;
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

        public InspectorLineAttribute(LineColor color, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            switch (color)
            {
                case LineColor.White:
                    Color = Color.white;
                    break;
                case LineColor.Red:
                    Color = Color.red;
                    break;
                case LineColor.Green:
                    Color = Color.green;
                    break;
                case LineColor.Blue:
                    Color = Color.blue;
                    break;
                case LineColor.Gray:
                    Color = Color.gray;
                    break;
                case LineColor.Magenta:
                    Color = Color.magenta;
                    break;
                case LineColor.Yellow:
                    Color = Color.yellow;
                    break;
                case LineColor.Cyan:
                    Color = Color.cyan;
                    break;
            }
            LineThickness = thickness;
            LinePadding = padding;
#endif
        }

        public InspectorLineAttribute(float r, float g, float b, int thickness = 2, int padding = 3)
        {
#if UNITY_EDITOR
            Color = new Color(r, g, b);
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
}
