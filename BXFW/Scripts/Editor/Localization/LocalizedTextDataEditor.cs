using UnityEngine;
using UnityEditor;
using BXFW.Data;
using BXFW.Tools.Editor;
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
        /// Height of the text area.
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

            return currentPropY + PADDING;
        }

        private float currentPropY = -1f;
        private Rect GetPropertyRect(Rect parentRect, float customHeight = -1f)
        {
            var propHeight = customHeight > 0f ? customHeight : EditorGUIUtility.singleLineHeight;
            //if (currentPropY == -1f)
            //{
            //    // First call
            //    currentPropY = parentRect.y;
            //}

            Rect r = new Rect(parentRect.x, parentRect.y + currentPropY, parentRect.width, propHeight);
            // Add height later
            currentPropY += propHeight;

            return r;
        }

        private string GetPropertyKey(SerializedProperty property)
        {
            return string.Format("{0}::{1}",
                property.serializedObject.targetObject.name ?? property.serializedObject.targetObject.GetInstanceID().ToString(),
                property.propertyPath);
        }
        /// <summary>
        /// The currently edited locale for that <see cref="SerializedProperty"/>.
        /// </summary>
        private readonly Dictionary<string, string> editedLocales = new Dictionary<string, string>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= PADDING;
            position.y += PADDING / 2f;
            currentPropY = -1f;

            var targetPair = property.GetTarget();
            var target = targetPair.Value as LocalizedTextData;
            var gEnabled = GUI.enabled;

            Rect initialFoldoutRect = GetPropertyRect(position);
            label = EditorGUI.BeginProperty(initialFoldoutRect, label, property);
            property.isExpanded = EditorGUI.Foldout(initialFoldoutRect, property.isExpanded, label);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            // Indent
            position.x += INDENT;
            position.width -= INDENT;

            // Gather currently edited locale value
            string editedLocaleValue = LocalizedTextData.DefaultLocale; // default
            if (!editedLocales.TryGetValue(GetPropertyKey(property), out string savedEditedLocaleValue))
            {
                // Set saved value.
                editedLocales.Add(GetPropertyKey(property), editedLocaleValue);
                savedEditedLocaleValue = editedLocaleValue;
            }
            // Get saved Value
            editedLocaleValue = savedEditedLocaleValue;
            // Add to target if it does not exist
            if (!target.Data.ContainsKey(editedLocaleValue))
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                target.Data.Add(editedLocaleValue, string.Empty);
            }

            // Show the locale selector
            Rect dropdownRect = GetPropertyRect(position);
            if (EditorGUI.DropdownButton(new Rect(dropdownRect) { width = dropdownRect.width - 35 }, new GUIContent(string.Format("Locale ({0})", editedLocaleValue)), FocusType.Keyboard))
            {
                var addableLanguageList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
                addableLanguageList.Sort((CultureInfo x, CultureInfo y) => { return x.TwoLetterISOLanguageName.CompareTo(y.TwoLetterISOLanguageName); });
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Cancel"), false, () => { });

                // Add existing (to switch into locale previews)
                menu.AddSeparator(string.Empty);
                foreach (var idValuePair in target)
                {
                    // Remove + check if it was removed.
                    if (addableLanguageList.RemoveAll(ci => ci.TwoLetterISOLanguageName == idValuePair.Key) != 0)
                    {
                        menu.AddItem(new GUIContent(string.Format("{0} (exists)", idValuePair.Key)), idValuePair.Key == editedLocaleValue, () =>
                        {
                            // Switch the currently edited locale.
                            editedLocales[GetPropertyKey(property)] = idValuePair.Key;
                            editedLocaleValue = idValuePair.Key;
                            EditorAdditionals.RepaintAll();
                            EditorGUIUtility.editingTextField = false;
                        });
                    }
                }

                // Add non-existing
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < addableLanguageList.Count; i++)
                {
                    CultureInfo info = addableLanguageList[i];

                    menu.AddItem(new GUIContent(info.TwoLetterISOLanguageName.ToString()), false, () =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "add locale (dict)");
                        editedLocales[GetPropertyKey(property)] = info.TwoLetterISOLanguageName;
                        target.Data.Add(info.TwoLetterISOLanguageName, string.Empty);
                        EditorAdditionals.RepaintAll();
                        EditorGUIUtility.editingTextField = false;
                    });
                }

                menu.ShowAsContext();
            }

            // Remove locale menu button
            GUI.enabled = target.Data.Keys.Count > 1;
            Rect removeLocaleBtnRect = new Rect(dropdownRect) { x = dropdownRect.x + (dropdownRect.width - 30), width = 30 };
            if (GUI.Button(removeLocaleBtnRect, new GUIContent("X")))
            {
                // Remove from object
                Undo.RecordObject(property.serializedObject.targetObject, "remove locale");
                target.Data.Remove(editedLocaleValue);
                // Set edited locale value
                editedLocaleValue = target.Data.Keys.First();
                editedLocales[GetPropertyKey(property)] = editedLocaleValue;
            }
            GUI.enabled = gEnabled;

            // Interface will show an GenericMenu dropdown, text area and locale itself
            EditorGUI.BeginChangeCheck();
            Rect txtEditAreaRect = GetPropertyRect(position, HEIGHT);
            string lValue = EditorGUI.TextArea(txtEditAreaRect, target.Data[editedLocaleValue], new GUIStyle(EditorStyles.textArea) { wordWrap = true });
            // placeholder (if locale string value is empty)
            if (string.IsNullOrEmpty(lValue))
            {
                GUIStyle placeholderStyle = new GUIStyle(GUI.skin.label);
                placeholderStyle.normal.textColor = Color.gray;

                EditorGUI.LabelField(new Rect(txtEditAreaRect) 
                {
                    x = txtEditAreaRect.x + 2f, 
                    height = EditorGUIUtility.singleLineHeight
                }, "<empty>", placeholderStyle);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set locale string");
                target.Data[editedLocaleValue] = lValue;
            }

            // End prop
            EditorGUI.EndProperty();
        }
    }
}