using BXFW;
using System;
using UnityEngine;

namespace BXFW.UI
{
    /// <summary>
    /// Defined UI content. Similar to <see cref="GUIContent"/>.
    /// </summary>
    [Serializable]
    public class Content
    {
        [Tooltip("Text content that this button stores.")][TextArea] public string text;
        // [Tooltip("Tooltip content that this button stores.")] public string tooltip; (TODO : Unity UI tooltips system.)
        [InspectorBigSpriteField, Tooltip("Sprite content.")] public Sprite image;
        [Tooltip("Whether if we should receive content from already existing components. This is an editor parameter.")]
        [SerializeField] internal bool receiveContentFromComponents = false;

        public Content()
        { }

        /// <summary>
        /// Creates a tab button content with an image.
        /// </summary>
        public Content(Sprite image)
        {
            this.image = image;
        }

        /// <summary>
        /// Creates a tab button content with a text.
        /// </summary>
        public Content(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Creates a tab button content with a text & image.
        /// </summary>
        public Content(string text, Sprite image)
        {
            this.text = text;
            this.image = image;
        }
    }
}