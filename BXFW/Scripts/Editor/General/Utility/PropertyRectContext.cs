using UnityEngine;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A simpler way to get <see cref="PropertyDrawer"/>'s rects seperated with given height and padding.
    /// </summary>
    /// <remarks>
    /// * This class requires you to add the <see cref="Padding"/> value to the actual height of the elements
    /// on the <see cref="PropertyDrawer.GetPropertyHeight(SerializedProperty, GUIContent)"/> preparation phase.
    /// <br>If this is abstracted away by something like <see cref="UnityEditorInternal.ReorderableList"/>, do the same thing on 
    /// it's <see cref="UnityEditorInternal.ReorderableList.elementHeightCallback"/>.</br>
    /// </remarks>
    public class PropertyRectContext
    {
        /// <summary>
        /// The current Y elapsed for this rect context.
        /// <br>Can be reset to zero using <see cref="Reset"/> or be used for tallying the height (not recommended).</br>
        /// </summary>
        public float CurrentY => m_CurrentY;
        /// <inheritdoc cref="CurrentY"/>
        private float m_CurrentY = 0f;
        /// <summary>
        /// Padding of this rect context.
        /// </summary>
        public virtual float Padding { get; set; } = 2f;

        /// <summary>
        /// Returns the <paramref name="property"/>'s rect.
        /// (by getting the height with <see cref="EditorGUI.GetPropertyHeight(SerializedProperty)"/>)
        /// </summary>
        public Rect GetPropertyRect(Rect baseRect, SerializedProperty property)
        {
            return GetPropertyRect(baseRect, EditorGUI.GetPropertyHeight(property));
        }
        /// <summary>
        /// Returns the base target rect.
        /// </summary>
        public Rect GetPropertyRect(Rect baseRect, float height)
        {
            baseRect.height = height;                  // set to target height
            baseRect.y += m_CurrentY + (Padding / 2f); // offset by Y
            m_CurrentY += height + Padding;            // add Y offset

            return baseRect;
        }

        /// <summary>
        /// Resets the context's current Y positioning.
        /// <br>Can be used when the context is to be used for reserving new rects.</br>
        /// <br>Always call this before starting new contexts to not have the positions shift forever.</br>
        /// </summary>
        public void Reset()
        {
            m_CurrentY = 0f;
        }

        /// <summary>
        /// Creates a PropertyRectContext where the <see cref="Padding"/> is 2f.
        /// </summary>
        public PropertyRectContext()
        { }
        /// <summary>
        /// Creates a PropertyRectContext where the <see cref="Padding"/> is the given parameter <paramref name="padding"/>.
        /// </summary>
        /// <param name="padding">Padding to set for this context.</param>
        public PropertyRectContext(float padding)
        {
            Padding = padding;
        }

        /// <summary>
        /// Converts the '<see cref="PropertyRectContext"/>' into information string.
        /// <br>May throw exceptions if <see cref="Padding"/> was overriden and could throw an exception on it's getter.</br>
        /// </summary>
        public override string ToString()
        {
            return $"PropertyRectContext | CurrentY={m_CurrentY}, Padding={Padding}";
        }
    }
}
