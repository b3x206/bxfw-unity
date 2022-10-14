using UnityEditor;
using BXFW.Tools.Editor;
using UnityEngine;
using System.Collections.Generic;
using BXFW.Data;
using System.Text;
using Codice.Client.BaseCommands;

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

                                menu.AddItem(new GUIContent(data.TextID), target.textID == data.TextID, () =>
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
                        var spoofables = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures);
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("None"), string.IsNullOrWhiteSpace(target.spoofLocale), () =>
                        {
                            target.spoofLocale = string.Empty;
                        });
                        menu.AddSeparator(string.Empty);

                        for (int i = 0; i < spoofables.Length; i++)
                        {
                            var info = spoofables[i];

                            menu.AddItem(new GUIContent(info.TwoLetterISOLanguageName.ToString()), target.spoofLocale == info.TwoLetterISOLanguageName, () =>
                            {
                                Undo.RecordObject(target, "Change spoof locale.");
                                target.spoofLocale = info.TwoLetterISOLanguageName;
                            });
                        }

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Spoof Locale", GUILayout.Width(150));
                        if (GUILayout.Button(string.IsNullOrWhiteSpace(target.spoofLocale) ?
                            $"None ({LocalizedTextData.ISOCurrentLocale})" : target.spoofLocale, EditorStyles.popup))
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