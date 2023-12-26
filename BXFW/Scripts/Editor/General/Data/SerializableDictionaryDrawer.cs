using UnityEngine;
using UnityEditor;
using System.Linq;
using BXFW.Tools.Editor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// This class allows for editing a <see cref="SerializableDictionary{TKey, TValue}"/> in a kinda scuffed way.
    /// </summary>
    /// TODO : Add a <see cref="BasicDropdown"/> as the adding context menu and make adding key+value experience more solid.
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private readonly PropertyRectContext mainGUIContext = new PropertyRectContext();
        private readonly PropertyRectContext reorderableListContext = new PropertyRectContext();
        /// <summary>
        /// General GUI height of displayed warning <see cref="EditorGUI.HelpBox(Rect, string, MessageType)"/>.
        /// </summary>
        private const float DictionaryWarningHeight = 32;

        /// <summary>
        /// Current reorderable list drawing list.
        /// <br>This is done to be able to make the 'ReorderableList' be draggable otherwise
        /// it doesn't work if you create the same ReorderableList constantly. Basically unique persistent ReorderableList storage</br>
        /// </summary>
        private static readonly Dictionary<string, ReorderableList> idDrawList = new Dictionary<string, ReorderableList>();
        private const int IdDrawListDictSizeLimit = 64;

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

            // Add a ReorderableList to this SerializeableDictionaryDrawer
            // Yes, this is not a very nice way of doing this, but it will do for now.
            // Because the 'ReorderableList' is not quite draggable if this is not done.
            string sPropId = property.GetIDString();
            if (!idDrawList.TryGetValue(sPropId, out ReorderableList list))
            {
                list = new ReorderableList(property.serializedObject, property.FindPropertyRelative("m_Pairs").Copy(), true, true, true, true)
                {
                    drawHeaderCallback = DrawListHeader,
                    drawElementCallback = DrawListElements,
                    elementHeightCallback = GetElementHeight,
                    onCanAddCallback = OnListCanAddCallback,
                };

                idDrawList.Add(sPropId, list);
                // 'Dictionary.Add's ordering is undefined behaviour (i love hashmaps)
                if (idDrawList.Count > IdDrawListDictSizeLimit)
                {
                    idDrawList.Remove(idDrawList.Keys.First(k => k != sPropId));
                }
            }

            // Sanity check thing
            SerializableDictionaryBase dict = ((SerializableDictionaryBase)property.GetTarget().value);
            if (!dict.KeysAreUnique())
            {
                height += DictionaryWarningHeight + mainGUIContext.Padding;
            }

            // ReorderableList height
            listTargetProperty = property;
            height += list.GetHeight();

            return height;
        }

        private SerializedProperty listTargetProperty;
        private Rect lastRepaintRect;

        private void DrawListHeader(Rect r)
        {
            EditorGUI.LabelField(r, "Keys & Values");
        }

        private float GetElementHeight(int index)
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
        private void DrawListElements(Rect rect, int index, bool isActive, bool isFocused)
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
                    Undo.PerformUndo();
                    EditorGUIUtility.editingTextField = false;
                    throw new ExitGUIException();
                }
            }
        }

        //private object m_CurrentAddDropdownBoxedValue;
        private bool OnListCanAddCallback(ReorderableList list)
        {
            // TODO : EditorGUIAdditionals.AnyObjectField()?

            // Add in a result of the 'BasicDropdown'.
            // --
            // This is the spicy part where we get to draw our own pair
            // And create a SerializedProperty from scratch
            // And add it to the dict in some way
            // From the very limited user api of unity serializer
            //SerializableDictionaryBase dict = (SerializableDictionaryBase)onGUIProperty.GetTarget().value;

            //PropertyDrawer drawer = EditorAdditionals.GetPropertyDrawerFromType(dict.KeyType);
            //FieldInfo currentBoxedDropdownValueField = typeof(SerializableDictionaryDrawer).GetField(nameof(m_CurrentAddDropdownBoxedValue), BindingFlags.NonPublic | BindingFlags.Instance);
            //typeof(PropertyDrawer).GetField("m_FieldInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drawer, currentBoxedDropdownValueField);

            // aAAAAAAAh
            // yes, we cannot 'ObjectField' c# types
            // fun
            //float drawerHeight = drawer.GetPropertyHeight()
            //BasicDropdown.ShowDropdown(GUIUtility.GUIToScreenRect(lastRepaintRect), new Vector2(lastRepaintRect.width * 0.8f, 60f), (BasicDropdown d) =>
            //{
            //    Rect reservedDrawerRect = 
            //    drawer.OnGUI();
            //});

            //return false;

            list.ClearSelection();
            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainGUIContext.Reset();

            if (Event.current.type == EventType.Repaint)
            {
                lastRepaintRect = position;
            }

            label = EditorGUI.BeginProperty(position, label, property);

            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(mainGUIContext.GetPropertyRect(position, DictionaryWarningHeight), "Cannot edit multiple 'SerializedDictionary'ies at the same time.", MessageType.Warning);
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
            string sPropId = property.GetIDString();
            ReorderableList list = idDrawList[sPropId];

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

            EditorGUI.indentLevel--;
        }
    }
}
