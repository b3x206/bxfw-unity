using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using UnityEditorInternal;

namespace BXFW.Collections.ScriptEditor
{
    // ---------------------------
    // | > Array                 | // Array in a ReorderableList GUI view
    // |                  [ 0 ]  |
    // | > ChanceArray1          | // These are renameable (?), or just leave as is??
    // | Chance= ---o----------- | // When the chance slider is changed, other items in the array's chance should be also changed
    // | > List                  | 
    // |   Chance --o----------- |
    // |   Data [O sampleData  ] |
    // |   Chance -----------o-- |
    // |   Data [O sampleData2 ] |
    // | > ChanceArray2          |
    // | Chance= -----------o--- |
    // | > List                  |
    // |   Chance -------------o | // There's only 1 object, the chance slider is disabled and in 100%
    // |   Data [O sampleData3 ] |
    // ---------------------------

    /// <summary>
    /// Creates an property drawer editor for <see cref="ChanceValuesListBase"/>, 
    /// where changing the chance value of a variable will change other values as well. (which looks cool)
    /// </summary>
    [CustomPropertyDrawer(typeof(ChanceValuesListBase), true)]
    public class ChanceValuesListEditor : PropertyDrawer
    {
        /// <summary>
        /// The nameof the <see cref="ChanceValuesListBase"/>'s list field.
        /// </summary>
        protected virtual string ListFieldName => "m_list";
        /// <summary>
        /// The nameof the <see cref="ChanceValue{T}"/>'s value field.
        /// </summary>
        protected virtual string ValueFieldName => "Value";
        /// <summary>
        /// The nameof the <see cref="ChanceValue{T}"/>'s chance field.
        /// </summary>
        protected virtual string ChanceFieldName => $"m_{nameof(IChanceValue.Chance)}";

        private readonly PropertyRectContext mainCtx = new PropertyRectContext();
        private readonly PropertyRectContext listCtx = new PropertyRectContext();
        protected SerializedProperty listTargetProperty;
        protected const float MultiEditInfoBoxHeight = 36f;

        protected virtual void DrawListHeader(Rect r)
        {
            GUI.Label(r, "List Values");
        }
        protected virtual float GetListElementHeight(int index)
        {
            float height = 0f;

            using SerializedProperty chancesListProperty = listTargetProperty.FindPropertyRelative(ListFieldName);

            if (chancesListProperty.arraySize <= 0)
            {
                return EditorGUIUtility.singleLineHeight + listCtx.Padding;
            }

            using SerializedProperty chanceValueProperty = chancesListProperty.GetArrayElementAtIndex(index);
            using SerializedProperty valueProperty = chanceValueProperty.FindPropertyRelative(ValueFieldName);

            height += EditorGUIUtility.singleLineHeight + listCtx.Padding; // chance slider
            height += 6f + listCtx.Padding;
            height += EditorGUI.GetPropertyHeight(valueProperty) + listCtx.Padding; // value
            return height;
        }
        protected virtual void DrawListElements(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Reset this per every call, as the rect is local.
            listCtx.Reset();

            using SerializedProperty chanceValueListProperty = listTargetProperty.FindPropertyRelative(ListFieldName);
            using SerializedProperty chanceValueProperty = chanceValueListProperty.GetArrayElementAtIndex(index);

            using SerializedProperty chanceProperty = chanceValueProperty.FindPropertyRelative(ChanceFieldName);
            using SerializedProperty valueProperty = chanceValueProperty.FindPropertyRelative(ValueFieldName);

            // Draw chance
            EditorGUI.BeginChangeCheck();
            float currentChance = EditorGUI.Slider(
                listCtx.GetPropertyRect(rect, EditorGUIUtility.singleLineHeight),
                chanceProperty.displayName,
                chanceProperty.floatValue,
                0f,
                ChanceValuesListBase.ChanceUpperLimit
            );
            if (chanceValueListProperty.arraySize <= 1)
            {
                chanceProperty.floatValue = ChanceValuesListBase.ChanceUpperLimit;
            }
            else if (EditorGUI.EndChangeCheck())
            {
                // If the chance is changed, change the others by it's delta and register all to undo
                float chanceDelta = (currentChance - chanceProperty.floatValue) / (chanceValueListProperty.arraySize - 1);
                // woo i love iterating SerializedProperties, but this is WAY simpler than fiddling around with weird SerializedProperty interception
                for (int i = 0; i < chanceValueListProperty.arraySize; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }

                    using SerializedProperty chanceAtIndexProperty = chanceValueListProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ChanceFieldName);
                    chanceAtIndexProperty.floatValue = Mathf.Clamp(chanceAtIndexProperty.floatValue - chanceDelta, 0f, ChanceValuesListBase.ChanceUpperLimit);
                }

                // After all sets are done, set the property too
                chanceProperty.floatValue = currentChance;
            }

            // Draw value (with line seperator)
            GUIAdditionals.DrawUILine(
                listCtx.GetPropertyRect(rect, 6),
                EditorGUIUtility.isProSkin ? Color.gray : new Color(0.12f, 0.12f, 0.12f, 1f)
            );
            EditorGUI.PropertyField(listCtx.GetPropertyRect(rect, valueProperty), valueProperty);

            // Apply changes
            chanceValueProperty.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                return MultiEditInfoBoxHeight + mainCtx.Padding;
            }

            float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding; // primary foldout

            if (!property.isExpanded)
            {
                return height;
            }

            ReorderableList list = EditorAdditionals.GetListForProperty(property.FindPropertyRelative(ListFieldName));
            // Set it's callbacks after receiving the 'list', this is required.
            list.drawHeaderCallback = DrawListHeader;
            list.elementHeightCallback = GetListElementHeight;
            list.drawElementCallback = DrawListElements;

            // Set this before calling anything in 'list'
            listTargetProperty = property;
            height += list.GetHeight();

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();

            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(mainCtx.GetPropertyRect(position, MultiEditInfoBoxHeight), "Cannot multi edit an ChanceValuesListEditor.", MessageType.Info);
                return;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            ChanceValuesListBase target = (ChanceValuesListBase)property.GetTarget().value;
            Rect foldoutPosition = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, label);

            // The array property used for this GUI call
            using SerializedProperty arrayProperty = property.FindPropertyRelative(ListFieldName);

            // Make drag drop area
            EditorGUIAdditionals.MakeDragDropArea(() =>
            {
                // Check type of dragged elements
                if (DragAndDrop.objectReferences.Length <= 0)
                {
                    return;
                }

                Type targetType = fieldInfo.FieldType.GetBaseTypes()
                    .First(t => t.GetGenericTypeDefinition() == typeof(ChanceValuesList<>))
                    .GetGenericArguments()
                    .SingleOrDefault();

                // Only supports by reference serialized stuff's drag/drop, the check for that is redundant.
                // Only equally distribute the chances if there's no elements in the array, otherwise set the chances to 0
                var objsFiltered = DragAndDrop.objectReferences.Where((obj) =>
                {
                    if (obj is GameObject g && targetType != typeof(GameObject))
                    {
                        if (!g.TryGetComponent(targetType, out Component _))
                        {
                            return false;
                        }
                    }
                    else if (obj.GetType() != targetType)
                    {
                        return false;
                    }

                    return true;
                }).ToArray();

                if (objsFiltered.Length <= 0)
                {
                    return;
                }

                // Chance value to set for added element
                float addedChance = arrayProperty.arraySize <= 0 ? (ChanceValuesListBase.ChanceUpperLimit / objsFiltered.Length) : 0f;
                // Type to create that is IChanceValue
                Type chanceDataType = typeof(ChanceValue<>).MakeGenericType(targetType);
                // Begin setting
                Undo.RecordObject(property.serializedObject.targetObject, "set values from array");

                arrayProperty.arraySize += objsFiltered.Length;
                for (int i = 0; i < objsFiltered.Length; i++)
                {
                    // Create IChanceValue with generic type
                    IChanceValue chanceData = (IChanceValue)Activator.CreateInstance(chanceDataType, new object[] { 0f });

                    // Call add from here because the object is already IList
                    // NOTE : DragAndDrop won't work on struct parent properties because the target is a copy in that instance
                    // but tbh as long as it works on normal classes it's fine. This is because serialized property moment
                    // no, this method doesn't copy the value
                    // We needed to access 'objectReferenceValue'
                    target.Add(chanceData);
                }
                // Evenly distribute chances + assign values
                for (int i = 0; i < arrayProperty.arraySize; i++)
                {
                    var assignObj = objsFiltered[i];
                    // Only try getting component if the target is not GameObject
                    if (assignObj is GameObject g && targetType != typeof(GameObject))
                    {
                        assignObj = g.GetComponent(targetType);
                    }

                    arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ValueFieldName).objectReferenceValue = assignObj;
                    arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ChanceFieldName).floatValue = addedChance;
                }
            }, () => GUI.enabled, new Rect(position) { height = EditorGUIUtility.singleLineHeight + 2f });

            // Handle right click events
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 1 && foldoutPosition.Contains(Event.current.mousePosition))
                {
                    GenericMenu rmbDropdown = new GenericMenu();

                    // Copy the 'arrayProperty' as it's disposed when it's made.
                    // Yes we need one memory leak here to not have 'SerializedProperty dispose!!1! You can't use it.'
                    SerializedProperty localCopyArray = arrayProperty.Copy();
                    rmbDropdown.AddItem(new GUIContent("Distribute Evenly", "Distributes all values evenly."), false, () =>
                    {
                        for (int i = 0; i < localCopyArray.arraySize; i++)
                        {
                            localCopyArray.GetArrayElementAtIndex(i).FindPropertyRelative(ChanceFieldName).floatValue = ChanceValuesListBase.ChanceUpperLimit / localCopyArray.arraySize;
                        }

                        // and this thing to call too, always needed because you can't determine
                        // whether if a property ever changes. It's impossible in c# (saying it satirically).
                        localCopyArray.serializedObject.ApplyModifiedProperties();
                        // Delegate SerializedProperty can be disposed, as it's used and when this event refires this value will be renewed
                        localCopyArray.Dispose();
                    });

                    rmbDropdown.ShowAsContext();
                    Event.current.Use();
                }
            }

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            ReorderableList list = EditorAdditionals.GetListForProperty(property.FindPropertyRelative(ListFieldName));
            // Set it's callbacks after receiving the 'list', this is required.
            list.drawHeaderCallback = DrawListHeader;
            list.elementHeightCallback = GetListElementHeight;
            list.drawElementCallback = DrawListElements;

            EditorGUI.indentLevel++;
            Rect indentedPosition = EditorGUI.IndentedRect(position);

            // Draw reorderable
            listTargetProperty = property;
            float height = list.GetHeight();
            list.DoList(mainCtx.GetPropertyRect(indentedPosition, height));
            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
}
