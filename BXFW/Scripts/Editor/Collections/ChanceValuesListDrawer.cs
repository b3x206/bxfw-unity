using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

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

    // TODO : ReorderableList implementation, this way of just cheating with the 'SerializedProperty' is usually bad
    // Yes, i have lazily made the UI prettier, but still. If it only wasn't absolute pain to work with ReorderableList...

    /// <summary>
    /// Creates an property drawer editor for <see cref="ChanceValuesListBase"/>, 
    /// where changing the chance value of a variable will change other values as well. (which looks cool)
    /// </summary>
    [CustomPropertyDrawer(typeof(ChanceValuesListBase), true)]
    public class ChanceValuesListDrawer : PropertyDrawer
    {
        /// <summary>
        /// The nameof the <see cref="ChanceValuesListBase"/>'s list field.
        /// </summary>
        protected virtual string ListFieldName => "m_list";
        /// <summary>
        /// The nameof the <see cref="ChanceValue{T}"/>'s value field.
        /// </summary>
        protected virtual string ListChanceValueFieldName => "Value";
        /// <summary>
        /// The nameof the <see cref="ChanceValue{T}"/>'s chance field.
        /// </summary>
        protected virtual string ListValueChanceName => $"m_{nameof(IChanceValue.Chance)}";

        private readonly PropertyRectContext mainCtx = new PropertyRectContext();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0f;

            // Get the list properties heights
            foreach (SerializedProperty p in property.GetVisibleChildren())
            {
                height += EditorGUI.GetPropertyHeight(p, label, true);
            }

            return height;
        }

        /// <summary>
        /// Previous list of chances to keep, before changing anything on the array.
        /// <br>Key holds a reference to the chance data, while the float keeps the copy of the previous chance.</br>
        /// <br>
        /// (ah well, just don't use the interface as it's the previous value,
        /// it is only contained to check if the array was reordered)
        /// </br>
        /// <br>The previous 'Chance' is the 'Value' of the KeyValuePair.</br>
        /// </summary>
        private readonly List<KeyValuePair<IChanceValue, float>> prevChanceList = new List<KeyValuePair<IChanceValue, float>>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            mainCtx.Reset();

            label = EditorGUI.BeginProperty(position, label, property);
            ChanceValuesListBase target = (ChanceValuesListBase)property.GetTarget().value;
            Rect foldoutFakeLabelPosition = mainCtx.PeekPropertyRect(new Rect(
                position.x + (EditorStyles.foldoutHeader.border.right - 2),
                position.y,
                position.width - (EditorStyles.foldoutHeader.border.right + 52),
                position.height
            ), EditorGUIUtility.singleLineHeight);

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

                    arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ListChanceValueFieldName).objectReferenceValue = assignObj;
                    arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ListValueChanceName).floatValue = addedChance;
                }
            }, () => GUI.enabled, new Rect(position) { height = EditorGUIUtility.singleLineHeight + 2f });

            // Handle right click events
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 1 && foldoutFakeLabelPosition.Contains(Event.current.mousePosition))
                {
                    GenericMenu rmbDropdown = new GenericMenu();

                    // Copy the 'arrayProperty' as it's disposed when it's made.
                    // Yes we need one memory leak here to not have 'SerializedProperty dispose!!1! You can't use it.'
                    SerializedProperty localCopyArray = arrayProperty.Copy();
                    rmbDropdown.AddItem(new GUIContent("Distribute Evenly", "Distributes all values evenly."), false, () =>
                    {
                        for (int i = 0; i < localCopyArray.arraySize; i++)
                        {
                            localCopyArray.GetArrayElementAtIndex(i).FindPropertyRelative(ListValueChanceName).floatValue = ChanceValuesListBase.ChanceUpperLimit / localCopyArray.arraySize;
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

            // Set a copy of the chance values in here from target
            prevChanceList.Clear();
            prevChanceList.Capacity = arrayProperty.arraySize;
            prevChanceList.AddRange(target.ChanceValues.Cast(x => new KeyValuePair<IChanceValue, float>(x, x.Chance)));

            // Draw the rest of the properties with the cool fading
            foreach (SerializedProperty p in property.GetVisibleChildren())
            {
                EditorGUI.BeginChangeCheck();
                // Draw property itself
                EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, EditorGUI.GetPropertyHeight(p, GUIContent.none, true)), p);
                // Apply property early to check for array chances
                p.serializedObject.ApplyModifiedProperties();

                // Array check
                if (EditorGUI.EndChangeCheck() || prevChanceList.Any(p => p.Key.Chance != p.Value))
                {
                    // Less than 2 element
                    if (target.ChanceValues.Count < 2)
                    {
                        // Set only single element chance.
                        if (target.ChanceValues.Count == 1)
                        {
                            arrayProperty.GetArrayElementAtIndex(0).FindPropertyRelative(ListValueChanceName).floatValue = ChanceValuesListBase.ChanceUpperLimit;
                        }

                        continue;
                    }

                    float delta = 0f;
                    // Figure out the modified value's index.
                    int modifiedValueIndex = -1;
                    for (int i = 0; i < Mathf.Min(target.ChanceValues.Count, prevChanceList.Count); i++)
                    {
                        if (target.ChanceValues[i].Chance != prevChanceList[i].Value)
                        {
                            modifiedValueIndex = i;
                            // Calculate the chance delta as well
                            delta = Mathf.Clamp(prevChanceList[i].Value - target.ChanceValues[i].Chance, -ChanceValuesListBase.ChanceUpperLimit, ChanceValuesListBase.ChanceUpperLimit);
                            break;
                        }
                    }

                    bool shouldModifyValues = modifiedValueIndex > -1;
                    // Check if we added / removed value from array (should be done after the reorder check)
                    if (!shouldModifyValues)
                    {
                        if (target.ChanceValues.Count > prevChanceList.Count)
                        {
                            // New item added but can just set the chance to zero
                            arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1)
                                .FindPropertyRelative(ListValueChanceName).floatValue = 0f;
                        }
                        if (target.ChanceValues.Count < prevChanceList.Count)
                        {
                            // Items removed, get the removal delta and evenly distribute
                            // (since 'delta' is subtracted this has to be negative)
                            delta = prevChanceList.GetRange(target.ChanceValues.Count, prevChanceList.Count - target.ChanceValues.Count).Sum(c => c.Value);
                            shouldModifyValues = true;
                        }
                    }

                    int validChanceDataCount = prevChanceList.Where(f => !Mathf.Approximately(f.Value, 0f)).Count(); // valid for calculation, non-zero
                    for (int i = 0; i < target.ChanceValues.Count && shouldModifyValues; i++)
                    {
                        if (i == modifiedValueIndex)
                        {
                            continue;
                        }

                        // -> delta => (change between the current value)
                        // -> delta division percentage => (target.ChanceDatas[i].Chance / avgValue)
                        // -> data count excl. modified => (target.ChanceDatas.Count - 1)

                        // Only use the current valid values. (try not to divide by 0 to avoid NaN)
                        float deltaValue = delta / (validChanceDataCount <= 1 ? target.ChanceValues.Count - 1 : validChanceDataCount - 1);
                        // Since target is read-only do the assigning using the SerializedProperty
                        using SerializedProperty valueChanceField = arrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(ListValueChanceName);
                        valueChanceField.floatValue = Mathf.Clamp(valueChanceField.floatValue + deltaValue, 0f, ChanceValuesListBase.ChanceUpperLimit);
                    }

                    arrayProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            // Draw a fake box for the property background.
            EditorGUI.DrawRect(foldoutFakeLabelPosition, EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f));
            EditorGUI.LabelField(foldoutFakeLabelPosition, new GUIContent(property.displayName, property.tooltip));
            EditorGUI.EndProperty();
        }
    }
}
