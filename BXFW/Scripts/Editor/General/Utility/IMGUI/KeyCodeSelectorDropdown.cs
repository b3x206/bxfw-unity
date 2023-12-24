using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Allows for a selection dropdown for <see cref="KeyCode"/>.
    /// </summary>
    public class KeyCodeSelectorDropdown : SearchDropdown
    {
        /// <summary>
        /// The currently selected keycode.
        /// <br>The selected element is renewed when the root is built again (<see cref="BuildRoot"/>).</br>
        /// </summary>
        public KeyCode selectedKeyCode;
        /// <summary>
        /// Whether to sort the resulting elements array.
        /// </summary>
        public bool sortKeysAlphabetically = false;

        protected internal override bool AllowRichText => true;

        /// <summary>
        /// Item that contains extra data for selection.
        /// </summary>
        public class Item : SearchDropdownElement
        {
            /// <summary>
            /// The item's keycode value.
            /// </summary>
            public readonly KeyCode keyValue;

            public Item(KeyCode keyCodeValue) : base($"{keyCodeValue} <color=#a8d799> | {(long)keyCodeValue}</color>")
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, string label) : base(label)
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, GUIContent content) : base(content)
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, string label, string tooltip) : base(label, tooltip)
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, string label, int childrenCapacity) : base(label, childrenCapacity)
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, GUIContent content, int childrenCapacity) : base(content, childrenCapacity)
            {
                keyValue = keyCodeValue;
            }

            public Item(KeyCode keyCodeValue, string label, string tooltip, int childrenCapacity) : base(label, tooltip, childrenCapacity)
            {
                keyValue = keyCodeValue;
            }
        }

        protected override SearchDropdownElement BuildRoot()
        {
            IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();
            string[] keyEnumNames = Enum.GetNames(typeof(KeyCode));
            SearchDropdownElement rootElement = new SearchDropdownElement("Select KeyCode", keyCodes.Count());

            foreach (KeyValuePair<int, KeyCode> pair in keyCodes.Indexed())
            {
                // Append the name like this as 'GetEnumName' or 'Enum.ToString' always only gets the first alias for 'ToString'
                rootElement.Add(new Item(pair.Value, $"{keyEnumNames[pair.Key]} <color=#a8d799> | {(long)pair.Value}</color>")
                {
                    Selected = pair.Value != KeyCode.None && pair.Value == selectedKeyCode
                });
            }

            if (sortKeysAlphabetically)
            {
                rootElement.Sort();
            }

            return rootElement;
        }

        /// <summary>
        /// Creates a KeyCodeSelectorDropdown without any selected.
        /// <br>To show the dropdown, use <see cref="SearchDropdown.Show(Rect)"/>.</br>
        /// </summary>
        public KeyCodeSelectorDropdown()
        {
            selectedKeyCode = KeyCode.None;
        }
        /// <summary>
        /// Sets a predefined selected keycode.
        /// <br>Note : <see cref="KeyCode.None"/> is never selectable.</br>
        /// </summary>
        public KeyCodeSelectorDropdown(KeyCode selected)
        {
            selectedKeyCode = selected;
        }
        /// <summary>
        /// Sets whether to sort the added keycodes.
        /// </summary>
        public KeyCodeSelectorDropdown(bool sortKeysStrings)
        {
            sortKeysAlphabetically = sortKeysStrings;
        }
        /// <summary>
        /// Sets a predefined KeyCode and whether to sort keys alphabetically.
        /// </summary>
        public KeyCodeSelectorDropdown(KeyCode selected, bool sortKeysStrings)
        {
            selectedKeyCode = selected;
            sortKeysAlphabetically = sortKeysStrings;
        }
    }
}
