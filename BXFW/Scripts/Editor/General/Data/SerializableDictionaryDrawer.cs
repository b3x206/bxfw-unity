using UnityEngine;
using UnityEditor;
using System.Linq;
using BXFW.Tools.Editor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// This cursed class edits a <see cref="SerializableDictionary{TKey, TValue}"/> in a scuffed way.
    /// </summary>
    /// TODO : Add a <see cref="BasicDropdown"/> as the adding context menu and make adding key+value experience more solid.
    /// TODO 2 : Fix this ReorderableList bullcrap
    [CustomPropertyDrawer(typeof(SerializableDictionaryBase), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        // hooray, the editor works inside nested children
        // too bad it's fragile thanks to the 'ReorderableList' and it's joys
        // This isn't a good way of architecturing a dictionary, but if it works no problem.
        private readonly PropertyRectContext mainGUIContext = new PropertyRectContext();
        private const float NonUniqueValuesWarningHeight = 32;
        private readonly PropertyRectContext reorderableListContext = new PropertyRectContext();
        /// <summary>
        /// Current reorderable list drawing list.
        /// <br>This is done to be able to make the 'ReorderableList' be draggable otherwise
        /// it doesn't work if you create the same ReorderableList constantly. Basically unique persistent ReorderableList storage</br>
        /// </summary>
        private static readonly Dictionary<string, ReorderableList> idDrawList = new Dictionary<string, ReorderableList>();
        private const int IdDrawListDictSizeLimit = 64;
        /// <summary>
        /// Current property used for drawing.
        /// </summary>
        private SerializedProperty m_baseProperty;
        /// <summary>
        /// The backup used for an hack in <see cref="OnGUI(Rect, SerializedProperty, GUIContent)"/>
        /// </summary>
        private SerializedProperty m_basePropertyClone;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Yes, this is not a very nice way of doing this, but it will do for now.
            // Because the 'ReorderableList' is not quite draggable if this is not done.
            string sPropId = property.GetIDString();
            if (!idDrawList.TryGetValue(sPropId, out ReorderableList list))
            {
                list = new ReorderableList(property.serializedObject, property.FindPropertyRelative("m_Keys"), true, true, true, true)
                {
                    drawHeaderCallback = DrawListHeader,
                    drawElementCallback = DrawListElements,
                    elementHeightCallback = GetElementHeight,
                    onReorderCallbackWithDetails = OnListSwitchPairs,
                };

                idDrawList.Add(sPropId, list);
                // 'Dictionary.Add's ordering is undefined behaviour (which is amazing)
                if (idDrawList.Count > IdDrawListDictSizeLimit)
                {
                    idDrawList.Remove(idDrawList.Keys.First(k => k != sPropId));
                }
            }

            float height = EditorGUIUtility.singleLineHeight + mainGUIContext.Padding;
            if (!property.isExpanded)
            {
                return height;
            }

            SerializableDictionaryBase dict = ((SerializableDictionaryBase)property.GetTarget().value);
            if (!dict.KeysAreUnique())
            {
                height += NonUniqueValuesWarningHeight + mainGUIContext.Padding;
            }

            // Set the current undisposed SerializedProperties
            m_baseProperty = property;
            list.serializedProperty = property.FindPropertyRelative("m_Keys");
            height += list.GetHeight();

            return height;
        }

        private void DrawListHeader(Rect r)
        {
            // the pointer is set to zero (something calls dispose on list?) when the list starts drawing
            // i mean why?
            // this hack of "backing up the 'SerializedProperty' with a new 'SerializedObject'" works
            // this occurs when the inspector mode is switched from-to inspector to debug to inspector back
            // why? is this my crappy code or a unity bug? i don't even really know.
            if (m_baseProperty == null || m_baseProperty.IsDisposed() || m_baseProperty.serializedObject.IsDisposed())
            {
                // this also makes the reorderable list non-reorderable
                // which is amazing, but there's a simple solution for that.
                m_baseProperty = m_basePropertyClone;
            }

            EditorGUI.LabelField(r, "Keys & Values");
        }
        private float GetElementHeight(int index)
        {
            float height = 0f;
            // Do this for one iteration as the OnGUI hack fixes this
            if (m_baseProperty == null || m_baseProperty.IsDisposed() || m_baseProperty.serializedObject.IsDisposed())
            {
                return EditorGUIUtility.singleLineHeight + reorderableListContext.Padding;
            }

            SerializedProperty keysProperty = m_baseProperty.FindPropertyRelative("m_Keys");
            SerializedProperty valuesProperty = m_baseProperty.FindPropertyRelative("m_Values");

            if (valuesProperty.arraySize != keysProperty.arraySize)
            {
                valuesProperty.arraySize = keysProperty.arraySize;
            }

            if (keysProperty.arraySize <= 0)
            {
                return EditorGUIUtility.singleLineHeight + reorderableListContext.Padding;
            }

            height += EditorGUI.GetPropertyHeight(keysProperty.GetArrayElementAtIndex(index)) + reorderableListContext.Padding;
            height += 6 + reorderableListContext.Padding; // GUIAdditionals.DrawLine();
            height += EditorGUI.GetPropertyHeight(valuesProperty.GetArrayElementAtIndex(index)) + reorderableListContext.Padding;
            return height;
        }
        private void DrawListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Reset this per every call, as the rect is local lol.
            reorderableListContext.Reset();

            SerializedProperty keysProperty = m_baseProperty.FindPropertyRelative("m_Keys");
            SerializedProperty valuesProperty = m_baseProperty.FindPropertyRelative("m_Values");

            // yes, please, give me another hack babyyyy
            if (keysProperty.arraySize <= index)
            {
                keysProperty.arraySize = index + 1;
            }
            if (valuesProperty.arraySize != keysProperty.arraySize)
            {
                valuesProperty.arraySize = keysProperty.arraySize;
            }

            // Draw key
            SerializableDictionaryBase dict = (SerializableDictionaryBase)m_baseProperty.GetTarget().value;
            object prevValue = dict.GetKey(index);
            EditorGUI.PropertyField(reorderableListContext.GetPropertyRect(rect, keysProperty.GetArrayElementAtIndex(index)), keysProperty.GetArrayElementAtIndex(index), new GUIContent($"Key {index}"));

            // Draw line
            GUIAdditionals.DrawUILine(
                reorderableListContext.GetPropertyRect(rect, 6),
                EditorGUIUtility.isProSkin ? Color.gray : new Color(0.12f, 0.12f, 0.12f, 1f)
            );
            // Draw value
            EditorGUI.PropertyField(reorderableListContext.GetPropertyRect(rect, valuesProperty.GetArrayElementAtIndex(index)), valuesProperty.GetArrayElementAtIndex(index), new GUIContent("Value"));

            // Ensure that the keys are unique
            // Otherwise revert the 'PropertyField'
            if (keysProperty.serializedObject.ApplyModifiedProperties())
            {
                if (!dict.KeysAreUnique())
                {
                    // Q : How do we support struct parents?
                    // A : no.
                    // Society if SerializedProperty value was assignable with any c# type
                    // --

                    // Now we gotta do stupid boxed array stuff
                    dict.SetKey(index, prevValue);
                }
            }
            valuesProperty.serializedObject.ApplyModifiedProperties();
        }

        private void OnListSwitchPairs(ReorderableList list, int oldIndex, int newIndex)
        {
            // Switch the switched value when the list values are switched
            // ReorderableList completely replaces the behaviour with this, which is okay.
            // (but it's still an undocumented mess of a class, or i suck at programming, probably the latter)
            m_baseProperty.FindPropertyRelative("m_Keys").MoveArrayElement(oldIndex, newIndex);
            m_baseProperty.FindPropertyRelative("m_Values").MoveArrayElement(oldIndex, newIndex);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainGUIContext.Reset();
            string sPropId = property.GetIDString();
            ReorderableList list = idDrawList[sPropId];
            label = EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(mainGUIContext.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), property.isExpanded, label);

            if (!property.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            // EditorGUI.IndentedRect indents too much
            float indentDiffScale = (position.width - indentedPosition.width) / 1.33f;
            indentedPosition.x -= indentDiffScale;
            indentedPosition.width += indentDiffScale;

            SerializableDictionaryBase dict = ((SerializableDictionaryBase)property.GetTarget().value);
            if (!dict.KeysAreUnique())
            {
                EditorGUI.HelpBox(mainGUIContext.GetPropertyRect(position, NonUniqueValuesWarningHeight), "Dictionary keys are not unique. This will cause problems.", MessageType.Warning);
            }
            
            m_baseProperty = property;

            float height = list.GetHeight();
            // Clone the 'SerializedObject'
            SerializedObject so = new SerializedObject(m_baseProperty.serializedObject.targetObjects);
            m_basePropertyClone = so.FindProperty(m_baseProperty.propertyPath);

            // go ahead, dispose my [object Object] here.
            // This is a terrible hack done to fix the stupid automatic disposal of the 'm_baseProperty'
            // Instead this method will now dispose 'm_basePropertyClone' which is, like, why?
            // This occured solely because i used 'ReorderableList', any other array viewing method and it would have worked fine.
            list.serializedProperty = m_basePropertyClone.FindPropertyRelative("m_Keys");
            list.DoList(mainGUIContext.GetPropertyRect(indentedPosition, height));
            so.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
    }
}
