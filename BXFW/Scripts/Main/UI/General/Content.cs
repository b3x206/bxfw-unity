﻿using System;
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
        [Tooltip("Sprite content."), BigSpriteField]
        public Sprite sprite;
        
        public Content()
        { }

        /// <summary>
        /// Creates a tab button content with an image.
        /// </summary>
        public Content(Sprite image)
        {
            sprite = image;
        }

        /// <summary>
        /// Creates a tab button content with a text.
        /// </summary>
        public Content(string text)
        {
            this.text = text;
        }

        /// <summary>
        /// Creates a tab button content with a text &amp; image.
        /// </summary>
        public Content(string text, Sprite image)
        {
            this.text = text;
            sprite = image;
        }
    }
}
