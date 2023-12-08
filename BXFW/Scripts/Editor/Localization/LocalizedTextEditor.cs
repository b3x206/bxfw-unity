using UnityEngine;
using UnityEditor;

using BXFW.Data;
using BXFW.Tools.Editor;

using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : Editor
    {
        public class TextIDSelector : SearchDropdown
        {
            protected internal override StringComparison SearchComparison => StringComparison.OrdinalIgnoreCase;

            public class Item : SearchDropdownElement
            {
                /// <summary>
                /// The TextID value of this item.
                /// </summary>
                public readonly string textID;

                public Item(string textID, string label) : base(label)
                {
                    this.textID = textID;
                }
                public Item(string textID, GUIContent content) : base(content)
                {
                    this.textID = textID;
                }
            }

            private readonly LocalizedText textComponent;

            protected override SearchDropdownElement BuildRoot()
            {
                SearchDropdownElement rootElement = new SearchDropdownElement("Select Text ID");

                foreach (LocalizedTextData data in textComponent)
                {
                    // foreaching
                    int j = 0, keyCount = data.LocaleDatas.Keys.Count();
                    StringBuilder availableLocalesSb = new StringBuilder(data.TextID.Length);
                    foreach (string key in data.LocaleDatas.Keys)
                    {
                        // Show 3 as maximum (4th element is + more)
                        if (j == 3)
                        {
                            availableLocalesSb.Append(string.Format(" + {0} more", keyCount - (j + 1)));
                            break;
                        }

                        availableLocalesSb.Append(j != keyCount - 1 ? string.Format("{0}, ", key) : key);
                        j++;
                    }

                    rootElement.Add(new Item(data.TextID, string.Format("{0} ({1})", data.TextID, availableLocalesSb.ToString()))
                    {
                        Selected = data.TextID == textComponent.textID
                    });
                }

                return rootElement;
            }

            public TextIDSelector(LocalizedText text)
            {
                if (text == null)
                {
                    throw new ArgumentNullException(nameof(text), "[LocalizedTextEditor::TextIDSelector::ctor] Given parameter was null.");
                }

                textComponent = text;
            }
        }

        /// <summary>
        /// Last repaint rect for the spoof locale button selector.
        /// </summary>
        private Rect spoofLocaleButtonRect;
        /// <summary>
        /// Last repaint rect for the text şd button selector.
        /// </summary>
        private Rect textIDSelectButtonRect;

        public override void OnInspectorGUI()
        {
            var target = base.target as LocalizedText;

            serializedObject.DrawCustomDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, System.Action>>
            {
                // Apparently text files are dark magic ._.
                // And you have to guess it's encoding by luck
                // why save a localization file as ansi? lol
                
                { nameof(LocalizedText.localeData), new KeyValuePair<MatchGUIActionOrder, System.Action>(
                    MatchGUIActionOrder.After, () =>
                    {
                        Rect btnRect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight + 2)));
                        btnRect.x += EditorGUIUtility.labelWidth;     // Offset
                        btnRect.width -= EditorGUIUtility.labelWidth; // Resize (dynamic)
                        if (GUI.Button(btnRect, "Refresh keys"))
                        {
                            target.Refresh();
                        }

                        // Just draw an info box if file is not utf8 with bom
                        if (target.localeData != null)
                        {
                            if (target.localeData.bytes.Length > 3)
                            {
                                if (target.localeData.bytes[0] != 0xEF || target.localeData.bytes[1] != 0xBB || target.localeData.bytes[2] != 0xBF)
                                {
                                    // File doesn't contain utf8 bom
                                    EditorGUILayout.HelpBox("This file doesn't contain utf8 bom.\nMake sure your file is utf8.\nIf your file is utf8, you can ignore this message.", MessageType.Info);
                                }
                            }
                        }
                        GUILayout.Space(EditorGUIUtility.singleLineHeight);
                    }
                )},
                { nameof(LocalizedText.textID), new KeyValuePair<MatchGUIActionOrder, System.Action>(
                    MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        // Draw dropdown if we have a textID.
                        if (target.localeData != null)
                        {
                            var dataList = target.TextData;
                            
                            // Get text id's parsed
                            if (dataList.Count <= 0)
                            {
                                EditorGUILayout.HelpBox($"There's no text data in file '{target.name}'.", MessageType.Warning);
                                return;
                            }

                            if (string.IsNullOrEmpty(target.textID))
                            {
                                // Set to default id.
                                target.textID = dataList[0].TextID;
                            }

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Text ID", GUILayout.Width(150));
                            if (GUILayout.Button(target.textID, EditorStyles.popup))
                            {
                                TextIDSelector dropdown = new TextIDSelector(target);
                                dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
                                {
                                    if (!(element is TextIDSelector.Item item))
                                    {
                                        return;
                                    }

                                    Undo.RecordObject(target, "Change text id.");
                                    target.textID = item.textID;
                                };

                                dropdown.Show(textIDSelectButtonRect);
                            }
                            if (Event.current.type == EventType.Repaint)
                            {
                                textIDSelectButtonRect = GUILayoutUtility.GetLastRect();
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("Add text data to get started.", MessageType.Info);
                        }
                    })
                },
                { nameof(LocalizedText.spoofLocale), new KeyValuePair<MatchGUIActionOrder, System.Action>(
                    MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        // Bro got a 'ToString' format provider :skull:
                        // Teacher is now grading papers of coding yay. OOP is easier than DSA lul.
                        // DSA is just leetcode, the class. OOP is what i am doing now yay
                        // --
                        // tbh dsa would have been enjoyable if the teacher wasn't such an
                        // egoistical maniac, acting like he's the interviever for FAANG (idk the new acronym) companies
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Spoof Locale", GUILayout.Width(150));
                        if (GUILayout.Button(string.IsNullOrWhiteSpace(target.spoofLocale) ?
                            string.Format("None ({0})", LocalizedTextData.CurrentISOLocaleName) : target.spoofLocale, EditorStyles.popup))
                        {
                            LocalizationKeySelectorDropdown dropdown = new LocalizationKeySelectorDropdown(target.CurrentSelectedData, target.spoofLocale) { addNoneElement = true };
                            dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
                            {
                                if (!(element is LocalizationKeySelectorDropdown.Item item))
                                {
                                    return;
                                }

                                Undo.RecordObject(target, "Change spoof locale.");
                                target.spoofLocale = item.localeKey;
                            };

                            dropdown.Show(spoofLocaleButtonRect);
                        }
                        if (Event.current.type == EventType.Repaint)
                        {
                            spoofLocaleButtonRect = GUILayoutUtility.GetLastRect();
                        }

                        GUILayout.EndHorizontal();
                    })
                }
            });
        }
    }
}
