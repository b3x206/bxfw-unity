using UnityEngine;
using UnityEditor;

using BXFW.Data;
using BXFW.Tools.Editor;

using System.Text;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(LocalizedText))]
    internal class LocalizedTextEditor : Editor
    {
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
                        var btnRect = new Rect(EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight + 2)));
                        btnRect.x += btnRect.width * .4f; // Offset
                        btnRect.width *= .6f; // Resize (dynamic)
                        if (GUI.Button(btnRect, "Refresh keys"))
                        {
                            target.Refresh();
                        }

                        // Just draw an info box if file is not utf8 with bom
                        if (target.localeData != null)
                            if (target.localeData.bytes.Length > 3)
                                if (target.localeData.bytes[0] != 0xEF || target.localeData.bytes[1] != 0xBB || target.localeData.bytes[2] != 0xBF)
                                {
                                    // File doesn't contain utf8 bom
                                    EditorGUILayout.HelpBox("This file doesn't contain utf8 bom.\nMake sure your file is utf8.\nIf your file is utf8, you can ignore this message.", MessageType.Info);
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

                            GenericMenu menu = new GenericMenu();

                            if (string.IsNullOrEmpty(target.textID))
                            {
                                // Set to default id.
                                target.textID = dataList[0].TextID;
                            }

                            for (int i = 0; i < dataList.Count; i++)
                            {
                                var data = dataList[i];
                                
                                // foreaching
                                int j = 0, keyCount = data.LocaleDatas.Keys.Count();
                                StringBuilder previewStrings = new StringBuilder(data.TextID.Length);
                                foreach (string key in data.LocaleDatas.Keys)
                                {
                                    // Show 3 as maximum (4th element is + more)
                                    if (j == 3)
                                    {
                                        previewStrings.Append(string.Format(" + {0} more", keyCount - (j + 1)));
                                        break;
                                    }

                                    previewStrings.Append(j != keyCount - 1 ? string.Format("{0}, ", key) : key);
                                    j++;
                                }

                                menu.AddItem(new GUIContent(string.Format("{0} ({1})", data.TextID, previewStrings.ToString())), target.textID == data.TextID, () =>
                                {
                                    Undo.RecordObject(target, "Change text id.");
                                    target.textID = data.TextID;
                                });
                            }

                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Text ID", GUILayout.Width(150));
                            if (GUILayout.Button(target.textID, EditorStyles.popup))
                            {
                                menu.ShowAsContext();
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
                        List<CultureInfo> spoofables = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("None"), string.IsNullOrWhiteSpace(target.spoofLocale), () =>
                        {
                            target.spoofLocale = string.Empty;
                        });
                        menu.AddSeparator(string.Empty);

                        if (target.TextData != null)
                        {
                            // Add existing spoofables
                            LocalizedTextData targetData = target.TextData.SingleOrDefault(x => x.TextID == target.textID);
                            foreach (var idValuePair in targetData.LocaleDatas)
                            {
                                if (spoofables.RemoveAll(x => x.TwoLetterISOLanguageName == idValuePair.Key) != 0)
                                {
                                    menu.AddItem(new GUIContent(string.Format("{0} (exists)", idValuePair.Key)), target.spoofLocale == idValuePair.Key,() =>
                                    {
                                        Undo.RecordObject(target, "Change spoof locale.");
                                        target.spoofLocale = idValuePair.Key;
                                    });
                                }
                            }
                            menu.AddSeparator(string.Empty);
                        }

                        for (int i = 0; i < spoofables.Count; i++)
                        {
                            CultureInfo info = spoofables[i];

                            menu.AddItem(new GUIContent(info.TwoLetterISOLanguageName.ToString()), target.spoofLocale == info.TwoLetterISOLanguageName, () =>
                            {
                                Undo.RecordObject(target, "change spoof locale");
                                target.spoofLocale = info.TwoLetterISOLanguageName;
                            });
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Spoof Locale", GUILayout.Width(150));
                        if (GUILayout.Button(string.IsNullOrWhiteSpace(target.spoofLocale) ?
                            string.Format("None ({0})", LocalizedTextData.CurrentISOLocaleName) : target.spoofLocale, EditorStyles.popup))
                        {
                            menu.ShowAsContext();
                        }
                        GUILayout.EndHorizontal();
                    })
                }
            });
        }
    }
}