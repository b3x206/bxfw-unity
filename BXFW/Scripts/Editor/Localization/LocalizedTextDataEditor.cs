using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using BXFW.Data;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// A <see cref="CultureInfo.TwoLetterISOLanguageName"/> names selector <see cref="SearchDropdown"/>.
    /// </summary>
    public class LocalizationKeySelectorDropdown : SearchDropdown
    {
        public class Item : SearchDropdownElement
        {
            /// <summary>
            /// The two letter or whatever format localization key that this item has.
            /// </summary>
            public readonly string localeKey;
            /// <summary>
            /// Whether if this is an existing key on the localization data.
            /// </summary>
            public readonly bool exists = false;

            public Item(string prettyName, string key, bool existing) : base(prettyName)
            {
                localeKey = key;
                exists = existing;
            }
        }

        /// <summary>
        /// Data to generate the dropdown accordingly to.
        /// </summary>
        private readonly LocalizedTextData m_referenceData;
        private readonly string m_editedLocale;
        /// <summary>
        /// Whether to add a none element, which selects an item with null <see cref="Item.localeKey"/>.
        /// </summary>
        public bool addNoneElement;
        protected internal override StringComparison SearchComparison => StringComparison.OrdinalIgnoreCase;

        private class CultureInfoTwoLetterComparer : IEqualityComparer<CultureInfo>
        {
            private static CultureInfoTwoLetterComparer m_Default;
            public static CultureInfoTwoLetterComparer Default
            {
                get
                {
                    m_Default ??= new CultureInfoTwoLetterComparer();

                    return m_Default;
                }
            }

            public bool Equals(CultureInfo x, CultureInfo y)
            {
                if (x is null)
                {
                    return y is null;
                }

                return x.TwoLetterISOLanguageName == y.TwoLetterISOLanguageName;
            }

            public int GetHashCode(CultureInfo obj)
            {
                return obj.TwoLetterISOLanguageName.GetHashCode();
            }
        }

        protected override SearchDropdownElement BuildRoot()
        {
            SearchDropdownElement rootItem = new SearchDropdownElement("Languages");
            List<CultureInfo> addableLanguageList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures).Distinct(CultureInfoTwoLetterComparer.Default));
            addableLanguageList.Sort((x, y) => x.TwoLetterISOLanguageName.CompareTo(y.TwoLetterISOLanguageName));

            if (addNoneElement)
            {
                rootItem.Add(new Item("None", string.Empty, false));
                rootItem.Add(new SearchDropdownSeperatorElement());
            }

            // Show selected ones if 'm_referenceData' does exist
            if (m_referenceData != null)
            {
                foreach (KeyValuePair<string, string> idValuePair in m_referenceData)
                {
                    bool IsMatchingTwoLetterISO(CultureInfo ci)
                    {
                        return ci.TwoLetterISOLanguageName == idValuePair.Key;
                    }

                    // Remove + check if it was removed.
                    int removeInfoIndex = addableLanguageList.IndexOf(IsMatchingTwoLetterISO);
                    if (removeInfoIndex >= 0)
                    {
                        CultureInfo removeInfo = addableLanguageList[removeInfoIndex];
                        string optionName = $"{removeInfo.EnglishName} ({idValuePair.Key}) [Exists]";
                        Item keyOption = new Item(optionName, idValuePair.Key, true)
                        {
                            Selected = idValuePair.Key == m_editedLocale
                        };
                        rootItem.Add(keyOption);

                        // Remove everything for duplicate two letters
                        addableLanguageList.RemoveAll(IsMatchingTwoLetterISO);
                    }
                }

                rootItem.Add(new SearchDropdownSeperatorElement());
            }

            // Show the rest of localizations
            for (int i = 0; i < addableLanguageList.Count; i++)
            {
                CultureInfo info = addableLanguageList[i];
                Item keyOption = new Item($"{info.EnglishName} ({info.TwoLetterISOLanguageName})", info.TwoLetterISOLanguageName, false)
                {
                    Selected = info.TwoLetterISOLanguageName == m_editedLocale
                };
                rootItem.Add(keyOption);
            }

            // Cannot constraint window size as that is not very possible.
            return rootItem;
        }

        public LocalizationKeySelectorDropdown()
        { }
        /// <summary>
        /// Adds the currently selected locale as <paramref name="editedLocale"/>.
        /// </summary>
        public LocalizationKeySelectorDropdown(string editedLocale)
        {
            m_editedLocale = editedLocale;
        }
        /// <summary>
        /// Adds the currently selected locale as <paramref name="editedLocale"/> and the contained locale data keys as <paramref name="data"/>.
        /// </summary>
        public LocalizationKeySelectorDropdown(LocalizedTextData data, string editedLocale)
        {
            m_referenceData = data;
            m_editedLocale = editedLocale;
        }
    }

    [CustomPropertyDrawer(typeof(LocalizedTextData))]
    public class LocalizedTextDataEditor : PropertyDrawer
    {
        /// <summary>
        /// Height of the text area.
        /// </summary>
        private const float TextAreaHeight = 72f;
        /// <summary>
        /// Indent applied (to child elements) when the property field is uncollapsed.
        /// </summary>
        private const float InnerElementIndent = 15f;

        private readonly PropertyRectContext mainCtx = new PropertyRectContext(2f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Foldout
            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            if (!property.isExpanded)
            {
                return height;
            }

            // TextID
            height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            // Selected Locale
            height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
            // TextArea
            height += TextAreaHeight + mainCtx.Padding;

            return height;
        }

        private static readonly string KeyEditLocale = $"{nameof(LocalizedTextDataEditor)}::EditedLocale";
        private static GUIStyle placeholderStyle;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (placeholderStyle == null)
            {
                placeholderStyle = new GUIStyle(GUI.skin.label);
                placeholderStyle.normal.textColor = Color.gray;
            }
            mainCtx.Reset();

            // TODO + FIXME : This style of getting property target will cause inability to change values of a LocalizedTextData that is on a struct.
            // Use the 'property.FindPropertyRelative' instead and only use 'GetTarget' as a means of getting the property values if needed.
            PropertyTargetInfo targetPair = property.GetTarget();
            LocalizedTextData target = targetPair.value as LocalizedTextData;

            position = EditorGUI.IndentedRect(position);

            Rect initialFoldoutRect = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            label = EditorGUI.BeginProperty(initialFoldoutRect, label, property);
            property.isExpanded = EditorGUI.Foldout(initialFoldoutRect, property.isExpanded, label);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            // Indent
            position.x += InnerElementIndent;
            position.width -= InnerElementIndent;
            // For some reason this gets used the other EditorGUI properties (and acts as if it was a bug)
            // But it doesn't get used with EditorGUI.DropdownButton
            int previousIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Gather currently edited locale value
            string editedLocaleValue = property.GetString(KeyEditLocale, LocalizedTextData.DefaultLocale); // default
            // Add to target if it does not exist
            if (!target.LocaleDatas.ContainsKey(editedLocaleValue))
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                target.LocaleDatas.Add(editedLocaleValue, string.Empty);
            }

            // LocalizedTextData.TextID (could be useful for classifying in an array with linq commands)
            Rect textIDAreaRect = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            string textIDValue = EditorGUI.TextField(textIDAreaRect, "Text ID", target.TextID);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set TextID value");
                target.TextID = textIDValue;
            }

            // Show the locale selector
            Rect baseDropdownRect = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            Rect dropdownRect = new Rect(baseDropdownRect) { width = baseDropdownRect.width - 35f };
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(string.Format("Locale ({0})", editedLocaleValue)), FocusType.Keyboard))
            {
                LocalizationKeySelectorDropdown localeSelectorDropdown = new LocalizationKeySelectorDropdown(target, editedLocaleValue);
                localeSelectorDropdown.OnElementSelectedEvent += (SearchDropdownElement item) =>
                {
                    if (!(item is LocalizationKeySelectorDropdown.Item key))
                    {
                        return;
                    }

                    // Switch the currently edited locale, if the key doesn't exist create one.
                    if (!key.exists)
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "add locale (dict)");
                        target.LocaleDatas.Add(key.localeKey, string.Empty);
                    }

                    property.SetString(KeyEditLocale, key.localeKey);
                    editedLocaleValue = key.localeKey;
                    EditorAdditionals.RepaintAll();
                    EditorGUIUtility.editingTextField = false;
                };

                localeSelectorDropdown.Show(dropdownRect);
            }

            // Remove locale menu button
            using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(target.LocaleDatas.Keys.Count <= 1))
            {
                Rect removeLocaleBtnRect = new Rect(baseDropdownRect) { x = baseDropdownRect.x + (baseDropdownRect.width - 30), width = 30 };
                if (GUI.Button(removeLocaleBtnRect, new GUIContent("X", "Removes the currently selected locale key and value.")))
                {
                    // Remove from object
                    Undo.RecordObject(property.serializedObject.targetObject, "remove locale");
                    target.LocaleDatas.Remove(editedLocaleValue);
                    // Set edited locale value
                    editedLocaleValue = target.LocaleDatas.Keys.First();
                    property.SetString(KeyEditLocale, editedLocaleValue);
                }
            }

            // Interface will show an GenericMenu dropdown, text area and locale itself
            EditorGUI.BeginChangeCheck();
            Rect txtEditAreaRect = mainCtx.GetPropertyRect(position, TextAreaHeight);
            string lValue = EditorGUI.TextArea(txtEditAreaRect, target.LocaleDatas[editedLocaleValue], new GUIStyle(EditorStyles.textArea) { wordWrap = true });
            // placeholder (if locale string value is empty)
            if (string.IsNullOrEmpty(lValue))
            {
                EditorGUI.LabelField(new Rect(txtEditAreaRect)
                {
                    x = txtEditAreaRect.x + 2f,
                    height = EditorGUIUtility.singleLineHeight
                }, "<empty>", placeholderStyle);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set locale string");
                target.LocaleDatas[editedLocaleValue] = lValue;
            }

            // End prop + reset stuff
            EditorGUI.indentLevel = previousIndentLevel;
            EditorGUI.EndProperty();
        }
    }
}
