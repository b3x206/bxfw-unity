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
    [CustomEditor(typeof(LocalizedText)), CanEditMultipleObjects]
    public class LocalizedTextEditor : Editor
    {
        /// <summary>
        /// Selects a <see cref="LocalizedText.textID"/> from the list of available id's on the <see cref="LocalizedText"/>.
        /// </summary>
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
            LocalizedText[] targets = base.targets.Cast<LocalizedText>().ToArray();
            LocalizedText firstTarget = targets.First();

            serializedObject.DrawCustomDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(LocalizedText.textID), new KeyValuePair<MatchGUIActionOrder, Action>(
                    MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        // Draw locale project show selector
                        bool allLocaleDataValid = targets.All(t => t.LocaleData != null && t.LocaleData == firstTarget.LocaleData);
                        if (!allLocaleDataValid)
                        {
                            return;
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUIUtility.labelWidth);
                        if (GUILayout.Button("Show Locale Data on project"))
                        {
                            ProjectWindowUtil.ShowCreatedAsset(firstTarget.LocaleData);
                            if (targets.Length <= 1)
                            {
                                // Also uncollapse the selected text ID
                                LocalizedTextListAssetEditor.UncollapsePropertyIndex = firstTarget.LocaleData.IndexOf((td) => td.TextID == firstTarget.textID);
                            }
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.Space(EditorGUIUtility.singleLineHeight);

                        // Draw dropdown if we have a textID.
                        bool hasNullLocaleData = targets.Any(t => t.LocaleData == null);
                        if (!hasNullLocaleData)
                        {
                            // Get text id's parsed
                            bool hasNoDataKeyList = targets.Any(t => t.LocaleData.Count <= 0);
                            if (hasNoDataKeyList)
                            {
                                EditorGUILayout.HelpBox($"There's no text data in attached localized text list file.", MessageType.Warning);
                                return;
                            }

                            foreach (LocalizedText setTarget in targets)
                            {
                                if (string.IsNullOrWhiteSpace(setTarget.textID))
                                {
                                    setTarget.textID = setTarget.LocaleData[0].TextID;
                                    EditorUtility.SetDirty(setTarget);
                                }
                            }

                            // Show this button ONLY if the localeData's are the same
                            if (targets.All(t => t.LocaleData == firstTarget.LocaleData))
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Text ID", GUILayout.Width(150));
                                bool valuesAreMixed = targets.Any(t => t.textID != firstTarget.textID);
                                if (GUILayout.Button(valuesAreMixed ? "-" : firstTarget.textID, EditorStyles.popup))
                                {
                                    TextIDSelector dropdown = new TextIDSelector(firstTarget);
                                    dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
                                    {
                                        if (!(element is TextIDSelector.Item item))
                                        {
                                            return;
                                        }

                                        Undo.IncrementCurrentGroup();
                                        Undo.SetCurrentGroupName("Change text id");
                                        int group = Undo.GetCurrentGroup();

                                        foreach (LocalizedText setTarget in targets)
                                        {
                                            Undo.RecordObject(setTarget, string.Empty);
                                            setTarget.textID = item.textID;
                                        }

                                        Undo.CollapseUndoOperations(group);
                                    };

                                    dropdown.Show(textIDSelectButtonRect);
                                }
                                if (Event.current.type == EventType.Repaint)
                                {
                                    textIDSelectButtonRect = GUILayoutUtility.GetLastRect();
                                }

                                GUILayout.EndHorizontal();
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox(firstTarget.useSingletonLocaleData ? "Add a LocalizedTextListAsset to any resources folder to get started." : "Add text data to get started.", MessageType.Info);
                        }
                    })
                },
                { nameof(LocalizedText.spoofLocale), new KeyValuePair<MatchGUIActionOrder, Action>(
                    MatchGUIActionOrder.OmitAndInvoke, () =>
                    {
                        // Create a dropdowned spoof locale button
                        GUILayout.BeginHorizontal();

                        string spoofLocale = string.IsNullOrWhiteSpace(firstTarget.spoofLocale) ? string.Format("None ({0})", LocalizedTextData.CurrentISOLocaleName) : firstTarget.spoofLocale;

                        bool valuesAreMixed = targets.Any(t => t.spoofLocale != firstTarget.spoofLocale);
                        GUILayout.Label("Spoof Locale", GUILayout.Width(150));
                        if (GUILayout.Button(valuesAreMixed ? "-" : spoofLocale, EditorStyles.popup))
                        {
                            LocalizationKeySelectorDropdown dropdown = new LocalizationKeySelectorDropdown(firstTarget.CurrentSelectedData, firstTarget.spoofLocale) { addNoneElement = true };
                            dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
                            {
                                if (!(element is LocalizationKeySelectorDropdown.Item item))
                                {
                                    return;
                                }

                                Undo.IncrementCurrentGroup();
                                Undo.SetCurrentGroupName("Change spoof locale");
                                int group = Undo.GetCurrentGroup();

                                foreach (LocalizedText setTarget in targets)
                                {
                                    Undo.RecordObject(setTarget, string.Empty);
                                    setTarget.spoofLocale = item.localeKey;
                                }

                                Undo.CollapseUndoOperations(group);
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
