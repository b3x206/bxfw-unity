using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;
using UnityEditorInternal;

namespace BXFW.Collections.ScriptEditor
{
    /// <summary>
    /// This class allows for editing a <see cref="SerializableDictionary{TKey, TValue}"/> in a kinda scuffed way.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        protected readonly PropertyRectContext mainGUIContext = new PropertyRectContext();
        protected readonly PropertyRectContext reorderableListContext = new PropertyRectContext();
        /// <summary>
        /// General GUI height of displayed warning <see cref="EditorGUI.HelpBox(Rect, string, MessageType)"/>.
        /// </summary>
        protected const float DictionaryWarningHeight = 36;
        /// <summary>
        /// GUI height for the 'Add Element' button.
        /// </summary>
        protected const float AddElementButtonHeight = 30;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                return DictionaryWarningHeight + mainGUIContext.Padding;
            }

            // Expanded height check
            float height = EditorGUIUtility.singleLineHeight + mainGUIContext.Padding;
            if (!property.isExpanded)
            {
                return height;
            }

            ReorderableList list = EditorAdditionals.GetListForProperty(property.serializedObject, property.FindPropertyRelative("m_Pairs"), true, true, false, true);

            // Sanity check thing
            SerializableDictionaryBase dict = ((SerializableDictionaryBase)property.GetTarget().value);
            if (!dict.KeysAreUnique())
            {
                height += DictionaryWarningHeight + mainGUIContext.Padding;
            }

            // ReorderableList height
            listTargetProperty = property;
            height += list.GetHeight();

            // Add Element button
            height += AddElementButtonHeight + mainGUIContext.Padding;

            return height;
        }

        protected SerializedProperty listTargetProperty;
        protected Rect addElementButtonRect;

        protected virtual void DrawListHeader(Rect r)
        {
            GUI.Label(r, "Keys & Values");
        }
        protected virtual float GetListElementHeight(int index)
        {
            float height = 0f;

            using SerializedProperty pairsProperty = listTargetProperty.FindPropertyRelative("m_Pairs");

            if (pairsProperty.arraySize <= 0)
            {
                return EditorGUIUtility.singleLineHeight + reorderableListContext.Padding;
            }

            using SerializedProperty pairElementProperty = pairsProperty.GetArrayElementAtIndex(index);
            // ah the javascript vibes
            using SerializedProperty keyProperty = pairElementProperty.FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.key));
            using SerializedProperty valueProperty = pairElementProperty.FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.value));

            height += EditorGUI.GetPropertyHeight(keyProperty) + reorderableListContext.Padding;
            height += 6f + reorderableListContext.Padding;
            height += EditorGUI.GetPropertyHeight(valueProperty) + reorderableListContext.Padding;
            return height;
        }
        protected virtual void DrawListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Reset this per every call, as the rect is local lol.
            reorderableListContext.Reset();

            // Draw pair
            using SerializedProperty pairElementProperty = listTargetProperty.FindPropertyRelative("m_Pairs").GetArrayElementAtIndex(index);
            SerializableDictionaryBase dict = (SerializableDictionaryBase)listTargetProperty.GetTarget().value;

            // (draw it uncollapsed, there will be no 'Pair' property drawer)
            using SerializedProperty keyProperty = pairElementProperty.FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.key));
            using SerializedProperty valueProperty = pairElementProperty.FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.value));

            // Draw key, line, value
            EditorGUI.PropertyField(reorderableListContext.GetPropertyRect(rect, keyProperty), keyProperty);
            GUIAdditionals.DrawUILine(
                reorderableListContext.GetPropertyRect(rect, 6),
                EditorGUIUtility.isProSkin ? Color.gray : new Color(0.12f, 0.12f, 0.12f, 1f)
            );
            EditorGUI.PropertyField(reorderableListContext.GetPropertyRect(rect, valueProperty), valueProperty);

            // Ensure that the keys are unique
            // Otherwise revert the 'PropertyField'
            if (pairElementProperty.serializedObject.ApplyModifiedProperties())
            {
                if (!dict.KeysAreUnique())
                {
                    // google UnityEditor.Undo
                    // holy hell! new response just dropped
                    // This also uses the GUI so just exit GUI here
                    // (otherwise we get OnGUI stack empty cannot pop)
                    EditorGUIUtility.editingTextField = false;
                    Undo.PerformUndo();
                    GUIUtility.ExitGUI();
                }
            }
        }

        protected void ShowAddDropdown()
        {
            // Add in a result of the 'BasicDropdown'.
            // --
            // This is the spicy part where we get to draw our own key
            // And create a SerializedProperty from scratch
            // --
            // The dummy object workaround
            // A : Use the dummy inside the 'dict'
            SerializableDictionaryBase dict = (SerializableDictionaryBase)listTargetProperty.GetTarget().value;

            // B : Draw the dummy
            SerializedProperty pairDummyKeyProperty = listTargetProperty.FindPropertyRelative("m_DummyPair").FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.key));

            float keyPropertyHeight = EditorGUI.GetPropertyHeight(pairDummyKeyProperty);

            string guiKeyPropertyControlName = "SerializableDictionaryDrawer::AddDropdown::Key";
            bool hasSelectedKeyPropertyControlOnce = false;
            BasicDropdown.ShowDropdown(GUIUtility.GUIToScreenRect(addElementButtonRect), new Vector2(addElementButtonRect.width, 70f + keyPropertyHeight), () =>
            {
                // Most likely the entire Inspector was hidden
                if (pairDummyKeyProperty.IsDisposed())
                {
                    BasicDropdown.HideDropdown();
                    return;
                }

                GUI.SetNextControlName(guiKeyPropertyControlName);
                EditorGUILayout.PropertyField(pairDummyKeyProperty);
                // select on the first dropdown view
                if (!hasSelectedKeyPropertyControlOnce)
                {
                    GUI.FocusControl(guiKeyPropertyControlName);
                    hasSelectedKeyPropertyControlOnce = true;
                }
                pairDummyKeyProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();

                // warning and add buttons
                bool keyInvalid = !dict.DummyPairIsValid();
                using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(keyInvalid))
                {
                    bool hasPressedEnter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
                    if ((GUILayout.Button("Add") || hasPressedEnter) && !keyInvalid)
                    {
                        // the thing is, we can't get a 'array cell' as a 'PropertyTargetInfo'
                        // so the best thing i could do for now
                        Undo.RecordObject(listTargetProperty.serializedObject.targetObject, "add pair");
                        dict.AddDummyPair();

                        //using SerializedProperty pairsProperty = listTargetProperty.FindPropertyRelative("m_Pairs");
                        //pairsProperty.arraySize++;
                        //PropertyTargetInfo info = pairsProperty.GetArrayElementAtIndex(pairsProperty.arraySize - 1).FindPropertyRelative(nameof(SerializableDictionary<object, object>.Pair.key)).GetTarget();

                        //pairsProperty.serializedObject.ApplyModifiedProperties();
                        BasicDropdown.HideDropdown();
                    }
                }

                if (keyInvalid)
                {
                    EditorGUILayout.HelpBox("Given key already exists or is invalid. Please use a different key.", MessageType.Info);
                }

                // on press esc, clear all tags and close window
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    BasicDropdown.HideDropdown();
                }
            });
            // and with that, our dictionary saga is concluded.
            // the only TODO remaining is that the ability to add to the struct owned dicts, but at that point this is just, stupidly hard with the constraints of the SerializedProperty
            // (like i have to typetest to literally all ""supported"" SerializedPropertyType's and only do this object set if the type is not ""supported"")
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainGUIContext.Reset();

            label = EditorGUI.BeginProperty(position, label, property);

            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(mainGUIContext.GetPropertyRect(position, DictionaryWarningHeight), "Cannot edit multiple 'SerializedDictionary'ies at the same time.", MessageType.Info);
                EditorGUI.EndProperty();
                return;
            }

            property.isExpanded = EditorGUI.Foldout(mainGUIContext.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            // Get identity of 'SerializedDictionary'
            ReorderableList list = EditorAdditionals.GetListForProperty(property.serializedObject, property.FindPropertyRelative("m_Pairs"), true, true, false, true);
            list.drawHeaderCallback = DrawListHeader;
            list.elementHeightCallback = GetListElementHeight;
            list.drawElementCallback = DrawListElements;

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);

            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            // This must only happen in a case of 'Debug' inspector edit.
            SerializableDictionaryBase dict = ((SerializableDictionaryBase)property.GetTarget().value);
            if (!dict.KeysAreUnique())
            {
                EditorGUI.HelpBox(mainGUIContext.GetPropertyRect(position, DictionaryWarningHeight), "Dictionary keys are not unique. This will cause problems.", MessageType.Warning);
            }

            // Set this property before calling the ReorderableList methods
            listTargetProperty = property;

            // Draw the 'ReorderableList'
            float height = list.GetHeight();
            list.DoList(mainGUIContext.GetPropertyRect(indentedPosition, height));
            property.serializedObject.ApplyModifiedProperties();

            // Draw the '+ Add Element' button
            addElementButtonRect = mainGUIContext.GetPropertyRect(indentedPosition, AddElementButtonHeight);
            addElementButtonRect.x += indentedPosition.width * 0.1f;
            addElementButtonRect.width = indentedPosition.width * 0.8f;
            if (GUI.Button(addElementButtonRect, "+ Add Element"))
            {
                ShowAddDropdown();
            }

            EditorGUI.indentLevel--;
        }
    }
}
