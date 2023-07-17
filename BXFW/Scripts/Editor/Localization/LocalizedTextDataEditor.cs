using UnityEngine;
using UnityEditor;
using BXFW.Data;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace BXFW.ScriptEditor
{
    [CustomPropertyDrawer(typeof(LocalizedTextData))]
    public class LocalizedTextDataEditor : PropertyDrawer
    {
        /// <summary>
        /// Height padding applied in the editor view.
        /// </summary>
        private const float PADDING = 2f;
        /// <summary>
        /// Height of the property field.
        /// </summary>
        private const float HEIGHT = 72f;
        /// <summary>
        /// Indent applied (to child elements) when the property field is uncollapsed.
        /// </summary>
        private const float INDENT = 15f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight + PADDING;

            return HEIGHT + PADDING;
        }

        private float currentPropY = -1f;
        private Rect GetPropertyRect(Rect parentRect, float customHeight = -1f)
        {
            var propHeight = customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight;
            if (currentPropY == -1f)
            {
                // First call
                currentPropY = parentRect.y;
            }

            currentPropY += propHeight;
            return new Rect(parentRect.xMin, parentRect.yMin + (EditorGUIUtility.singleLineHeight * (currentPropY + 1)) + 8, parentRect.width, propHeight);
        }

        private string GetPropertyKey(SerializedProperty property)
        {
            return string.Format("{0}::{1}", property.serializedObject.targetObject.name, property.propertyPath);
        }
        /// <summary>
        /// The currently edited locale for that <see cref="SerializedProperty"/>.
        /// </summary>
        private readonly List<KeyValuePair<string, string>> editedLocales = new List<KeyValuePair<string, string>>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= PADDING;
            position.y += PADDING / 2f;
            currentPropY = -1f;

            label = EditorGUI.BeginProperty(position, label, property);
            property.isExpanded = EditorGUI.Foldout(GetPropertyRect(position), property.isExpanded, label);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            // i have sleep
            //// Indent
            //position.x += INDENT;
            //position.width -= INDENT;

            //string editedLocaleValue = "en"; // default
            //if (editedLocales.Any(r => r.Key == GetPropertyKey(property)))
            //{
            //    if (result.Key == )
            //    {
            //        // Value of edited locale is already drawn, use the gathered value and remove from array.
            //        _ = editedLocales.Remove();
            //    }
            //}

            //// Show the locale selector
            //if (EditorGUI.DropdownButton(GetPropertyRect(position), new GUIContent("Locale"), FocusType.Keyboard))
            //{
            //    List<CultureInfo> spoofables = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
            //    GenericMenu menu = new GenericMenu();

            //    menu.AddItem(new GUIContent("None"), string.IsNullOrWhiteSpace(target.spoofLocale), () =>
            //    {
            //        target.spoofLocale = string.Empty;
            //    });
            //    menu.AddSeparator(string.Empty);

            //    if (target.TextData != null)
            //    {
            //        // Add existing spoofables
            //        LocalizedTextData targetData = target.TextData.SingleOrDefault(x => x.TextID == target.textID);
            //        foreach (var idValuePair in targetData.Data)
            //        {
            //            if (spoofables.RemoveAll(x => x.TwoLetterISOLanguageName == idValuePair.Key) != 0)
            //            {
            //                menu.AddItem(new GUIContent(string.Format("{0} (exists)", idValuePair.Key)), target.spoofLocale == idValuePair.Key, () =>
            //                {
            //                    Undo.RecordObject(target, "Change spoof locale.");
            //                    target.spoofLocale = idValuePair.Key;
            //                });
            //            }
            //        }
            //        menu.AddSeparator(string.Empty);
            //    }

            //    for (int i = 0; i < spoofables.Count; i++)
            //    {
            //        CultureInfo info = spoofables[i];

            //        menu.AddItem(new GUIContent(info.TwoLetterISOLanguageName.ToString()), target.spoofLocale == info.TwoLetterISOLanguageName, () =>
            //        {
            //            Undo.RecordObject(property.serializedObject.targetObject, "add locale (dict)");
            //            target.spoofLocale = info.TwoLetterISOLanguageName;
            //        });
            //    }
            //}
            //// Interface will show an GenericMenu dropdown, text area and locale itself
            //EditorGUI.TextArea();

            //// Add into the drawn list + locales
            //editedLocales.Push(new KeyValuePair<string, string>(GetPropertyKey(property), ));
            //EditorGUI.EndProperty();
        }
    }
}