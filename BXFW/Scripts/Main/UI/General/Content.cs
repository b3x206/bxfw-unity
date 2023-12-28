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
        [Tooltip("Text content that this button stores."), TextArea]
        public string text;
        [Tooltip("Sprite content."), SpriteArea]
        public Sprite sprite;

        /// <summary>
        /// The none content, used to specify an empty content.
        /// </summary>
        public static readonly Content None = new Content();

        /// <summary>
        /// Creates an empty content.
        /// </summary>
        public Content()
        { }

        /// <summary>
        /// Creates content with an image.
        /// </summary>
        public Content(Sprite image)
        {
            sprite = image;
        }

        /// <summary>
        /// Creates content with a text.
        /// </summary>
        public Content(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Creates content with a text &amp; image.
        /// </summary>
        public Content(string text, Sprite image)
        {
            this.text = text;
            sprite = image;
        }
    }
}
