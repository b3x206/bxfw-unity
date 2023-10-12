using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using BXFW.Data;
using BXFW.Tools.Editor;

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
            Rect r = new Rect(parentRect.x, parentRect.y + currentPropY, parentRect.width, propHeight);
            // Add height later
            currentPropY += propHeight;

            return r;
        }

        private static readonly string KEY_EDIT_LOCALE = $"{nameof(LocalizedTextDataEditor)}::EditedLocale";
        private static GUIStyle placeholderStyle;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (placeholderStyle == null)
            {
                placeholderStyle = new GUIStyle(GUI.skin.label);
                placeholderStyle.normal.textColor = Color.gray;
            }

            position.height -= PADDING;
            position.y += PADDING / 2f;
            currentPropY = 0f;

            // TODO + FIXME : This style of getting property target will cause inability to change values of a LocalizedTextData that is on a struct.
            // Use the 'property.FindPropertyRelative' instead and only use 'GetTarget' as a means of getting the property values if needed.
            var targetPair = property.GetTarget();
            var target = targetPair.value as LocalizedTextData;
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
            string editedLocaleValue = property.GetString(KEY_EDIT_LOCALE, LocalizedTextData.DefaultLocale); // default
            // Add to target if it does not exist
            if (!target.LocaleDatas.ContainsKey(editedLocaleValue))
            {
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                target.LocaleDatas.Add(editedLocaleValue, string.Empty);
            }

            // LocalizedTextData.TextID (could be useful for classifying in an array with linq commands)
            Rect txtIDAreaRect = GetPropertyRect(position);
            string tIDValue = EditorGUI.TextField(txtIDAreaRect, "Text ID", target.TextID);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set TextID value");
                target.TextID = tIDValue;
            }

            // Get a empty property rect for nice spacing (yes this is a solution, i am expert at solving)
            GetPropertyRect(position, PADDING * 2f);

            // Show the locale selector
            Rect dropdownRect = GetPropertyRect(position);
            if (EditorGUI.DropdownButton(new Rect(dropdownRect) { width = dropdownRect.width - 35 }, new GUIContent(string.Format("Locale ({0})", editedLocaleValue)), FocusType.Keyboard))
            {
                var addableLanguageList = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.NeutralCultures));
                addableLanguageList.Sort((CultureInfo x, CultureInfo y) => x.TwoLetterISOLanguageName.CompareTo(y.TwoLetterISOLanguageName));
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
                            property.SetString(KEY_EDIT_LOCALE, idValuePair.Key);
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
                        property.SetString(KEY_EDIT_LOCALE, info.TwoLetterISOLanguageName);
                        target.LocaleDatas.Add(info.TwoLetterISOLanguageName, string.Empty);
                        EditorAdditionals.RepaintAll();
                        EditorGUIUtility.editingTextField = false;
                    });
                }

                menu.ShowAsContext();
            }

            // Remove locale menu button
            GUI.enabled = target.LocaleDatas.Keys.Count > 1;
            Rect removeLocaleBtnRect = new Rect(dropdownRect) { x = dropdownRect.x + (dropdownRect.width - 30), width = 30 };
            if (GUI.Button(removeLocaleBtnRect, new GUIContent("X")))
            {
                // Remove from object
                Undo.RecordObject(property.serializedObject.targetObject, "remove locale");
                target.LocaleDatas.Remove(editedLocaleValue);
                // Set edited locale value
                editedLocaleValue = target.LocaleDatas.Keys.First();
                property.SetString(KEY_EDIT_LOCALE, editedLocaleValue);
            }
            GUI.enabled = gEnabled;

            // Interface will show an GenericMenu dropdown, text area and locale itself
            EditorGUI.BeginChangeCheck();
            Rect txtEditAreaRect = GetPropertyRect(position, HEIGHT);
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

            // End prop
            EditorGUI.EndProperty();
        }
    }
}
