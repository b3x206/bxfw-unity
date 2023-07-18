using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using BXFW.Tools.Editor;
using System;

namespace BXFW.ScriptEditor
{
    // H moment : 
    // https://discussions.unity.com/t/how-to-edit-array-list-property-with-custom-propertydrawer/218416/2
    // You can’t make a PropertyDrawer for arrays or generic lists themselves. […] On the plus side, elements inside arrays and lists do work with PropertyDrawers.
    // This was meant to be an PropertyDrawer for an array, but i will just create a custom class.

    [CustomPropertyDrawer(typeof(RangeFloatArray))]
    internal class RangeFloatArrayDrawer : PropertyDrawer
    {
        private static RangeFloatArray GetTarget(SerializedProperty targetProperty)
        {
            return (RangeFloatArray)targetProperty.GetTarget().Value;
        }
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + DR_PADDING;
        }

        private static int NumberRepresentationLength(float number)
        {
            // Double with optional 3 precision numbers
            return number.ToString("G.###").Length;
        }

        /// <summary>
        /// Padding between GUI elements.
        /// </summary>
        private const float ElemPadding = 3f;
        /// <summary>
        /// Increment/Decrement element count button's width.
        /// </summary>
        private const float IncDecElemBtnWidth = 20f;

        /// <summary>
        /// Width of the drag area's boxes.
        /// </summary>
        private const float DraggableBoxWidth = 6f;
        /// <summary>
        /// Width of the 'modify value' box.
        /// </summary>
        private const float DraggableBoxModifyValueWidth = 50f;
        private const float DraggableBoxModifyValueHeight = 60f;
        /// <summary>
        /// Minimum value between the value slidables.
        /// <br>The slidables will clip if the mouse position and the slidable distance is greater than this value * 2f.</br>
        /// </summary>
        // Not used as RangeFloatArray.EXISTING_OFFSET works fine.
        // private const float MinDistBetweenValueSlidables = 3f;

        /// <summary>
        /// Initial width of the min/max value displays.
        /// </summary>
        private const float TextValueInitialWidth = 40f;
        /// <summary>
        /// Size of the shown text value(s), calculated per character.
        /// <br>Yes, the default unity editor font is not monospace, but idc, as i will use <see cref="TextAlignment.Center"/> for GUI purposes.</br>
        /// </summary>
        private const float TextValueCharWidth = 6f;

        /// <summary>
        /// Width (in percentage) for the property name.
        /// </summary>
        private const float PropNameWidthPercent = .3f;

        /// <summary>
        /// Contains an identification of a SerializedProperty.
        /// <br>Uses <see cref="SerializedProperty.serializedObject"/>.targetObject.name :: <see cref="SerializedProperty.propertyPath"/></br>
        /// </summary>
        private class PropertyID : IEquatable<PropertyID>, IEquatable<SerializedProperty>
        {
            /// <summary>
            /// Prefix for the '<see cref="propPath"/>'.
            /// </summary>
            private const string PROP_PARENT_PREFIX = "::";

            public readonly string propPath;  // Drawn property path (won't use the actual SerializedProperty as it gets disposed)

            public PropertyID(SerializedProperty prop)
            {
                propPath = string.Format("{0}{1}{2}", prop.serializedObject.targetObject.name, PROP_PARENT_PREFIX, prop.propertyPath);

            }
            //public PropertyID(SerializedProperty prop, RangeFloatArray target)
            //{
            //    propPath = string.Format("{0}{1}{2}", prop.serializedObject.targetObject.name, PROP_PARENT_PREFIX, prop.propertyPath);
            //    this.target = target;
            //}

            public bool Equals(PropertyID other)
            {
                return propPath == other.propPath;
            }

            public bool Equals(SerializedProperty other)
            {
                string[] splitPath = propPath.Split(PROP_PARENT_PREFIX, StringSplitOptions.None);
                // Should have the size of 2
                Assert.IsTrue(splitPath.Length == 2, string.Format("[RangeArrayDrawer::PropertyValues::Equals(SerializedProperty)] Length of 'splitPath' is not 2. propPath is '{0}'.", propPath));
                return splitPath[0] == other.serializedObject.targetObject.name && splitPath[1] == other.propertyPath;
            }
        }

        private PropertyID currentInteractedProperty; // Property that has it's events listened, if this is null all are listened else only this matching is listened.
        private Rect previousRepaintRect; // hack for getting the correct rect in repaint but not in layout
                                          // Setting the 'position' parameter during the EventType.Layout does not seem to break stuff
        private int dragIndex = -1;       // Index that is being dragged in the fake slider
        private int modifyIndex = -1;     // Index that is being modified by the SimpleDropdown
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeFloatArray target = GetTarget(property);

            Event e = Event.current;
            // e.type == Layout gives incorrect positioning
            // This makes the popup window jitter.
            if (e.type == EventType.Repaint)
            {
                previousRepaintRect = position;
            }
            if (e.type == EventType.Layout || e.type == EventType.Used)
            {
                position = previousRepaintRect;
            }

            // top/bottom paddings
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            EditorGUI.BeginChangeCheck();
            EditorGUI.LabelField(new Rect(position) { width = position.width * PropNameWidthPercent }, label);
            float propertyLabelWidth = position.width * PropNameWidthPercent;
            float setRemainingWidth = position.width * (1f - PropNameWidthPercent);
            // modify main rect
            position.x += position.width * PropNameWidthPercent;
            position.width = setRemainingWidth;

            // | -> content => tooltip = [value]
            // {val1} --o--o--o-- {val2} [+][-]

            // Draw the sized box
            float minTextWidth = TextValueInitialWidth + (NumberRepresentationLength(target.Min) * TextValueCharWidth);
            float maxTextWidth = TextValueInitialWidth + (NumberRepresentationLength(target.Max) * TextValueCharWidth);
            // Compensate for padding?
            Rect dragAreaBoxRect = new Rect(
                position.x + minTextWidth + ElemPadding,
                position.y,
                position.width - (minTextWidth + ElemPadding + maxTextWidth + ElemPadding + ((IncDecElemBtnWidth + ElemPadding) * 2f)),
                position.height
            );
            GUI.Box(dragAreaBoxRect, GUIContent.none, GUI.skin.horizontalSlider);
            // Draw the values (as editable float fields) on other ends
            Rect minValueFieldRect = new Rect(
                position.x,
                position.y,
                minTextWidth,
                position.height
            );
            float minValue = EditorGUI.FloatField(minValueFieldRect, target.Min);
            Rect maxValueFieldRect = new Rect(
                position.x + minTextWidth + dragAreaBoxRect.width + ElemPadding,
                position.y,
                maxTextWidth,
                position.height
            );
            float maxValue = EditorGUI.FloatField(maxValueFieldRect, target.Max);

            // Draw the 'increment/decrement array elements'
            int arrayLength = target.Count;
            Rect incArrayCntRect = new Rect(
                maxValueFieldRect.x + maxValueFieldRect.width + ElemPadding,
                position.y,
                IncDecElemBtnWidth,
                position.height
            );
            if (GUI.Button(incArrayCntRect, new GUIContent("+", "Increment element count in array.")))
            {
                arrayLength++;
            }
            Rect decArrayCntRect = new Rect(
                incArrayCntRect.x + incArrayCntRect.width + ElemPadding,
                position.y,
                IncDecElemBtnWidth,
                position.height
            );
            if (GUI.Button(decArrayCntRect, new GUIContent("-", "Decrement element count in array.")))
            {
                arrayLength--;
            }

            // Check if the xIndex thing's indices actually exist
            // Otherwise set them to -1 to avoid errors
            if ((currentInteractedProperty?.Equals(property) ?? false))
            {
                if (dragIndex >= target.Count)
                {
                    dragIndex = -1;
                }
                if (modifyIndex >= target.Count)
                {
                    modifyIndex = -1;
                }
            }

            // Modify Dropdown UI
            float dragAreaWidth = dragAreaBoxRect.width - (DraggableBoxWidth + (ElemPadding * 2f)); // The corrected drag area width. !! correction needed !!
            // Use the last interacted's states.
            Rect modifyValueRect = modifyIndex >= 0 && (currentInteractedProperty?.Equals(property) ?? false) ? new Rect(
                Mathf.Lerp(
                    dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth,
                    Additionals.Map(0f, 1f, target.Min, target.Max, target[modifyIndex])) - (DraggableBoxModifyValueWidth / 2f),
                position.y + (DraggableBoxModifyValueHeight / 2f),
                DraggableBoxModifyValueWidth,
                DraggableBoxModifyValueHeight
            ) : Rect.zero;
            if (modifyIndex >= 0)
            {
                // works slightly better
                if (!BasicDropdown.IsBeingShown())
                {
                    BasicDropdown.ShowDropdown(GUIUtility.GUIToScreenRect(modifyValueRect), modifyValueRect.size, (BasicDropdown dropdown) =>
                    {
                        if (modifyIndex < 0)
                        {
                            dropdown.Close();
                            currentInteractedProperty = null;
                            return;
                        }

                        GUIStyle smallTextStyle = new GUIStyle(GUI.skin.box) { fontSize = 8, alignment = TextAnchor.UpperLeft };
                        smallTextStyle.normal.textColor = Color.white;
                        EditorGUI.BeginChangeCheck();
                        GUILayout.Label(new GUIContent($"Modify Index={modifyIndex}"), smallTextStyle);
                        float modified = EditorGUILayout.FloatField(target[modifyIndex]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(property.serializedObject.targetObject, "set RangeFloatArray value");
                            target[modifyIndex] = modified;
                        }
                    });
                }
                else if (e.type == EventType.Repaint)
                {
                    BasicDropdown.SetPosition(GUIUtility.GUIToScreenRect(modifyValueRect));
                }
            }
            else if ((currentInteractedProperty?.Equals(property) ?? false))
            {
                BasicDropdown.HideDropdown();
            }

            // Change checks for the 'Min/Max' values
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set RangeFloatArray value");
                target.Min = minValue;
                target.Max = maxValue;

                arrayLength = Mathf.Clamp(arrayLength, 0, int.MaxValue);
                target.Resize(arrayLength);
            }

            // Draw buttons with the draggable 'circle' buttons
            // (like the RangeAttribute thing, clamped between 2 values)
            // Ensure the button values can't be the same
            bool usedEvent = false;
            for (int i = 0; i < target.Count; i++)
            {
                // Draw actual target's values
                float xPosition = Mathf.Lerp(dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, Additionals.Map(0f, 1f, target.Min, target.Max, target[i]));
                Rect draggableRect = new Rect(xPosition, position.y + (position.height / 5f), DraggableBoxWidth, position.height);
                // Draw a GUI seperately
                GUI.Box(draggableRect, new GUIContent(string.Empty, $"value={target[i]}\nindex={i}"), GUI.skin.horizontalSliderThumb);

                // Only call/check event(s) for single used GUI knob element
                // This fixes the jankiness partially
                if (!usedEvent && (currentInteractedProperty?.Equals(property) ?? true))
                {
                    // Intercept events manually
                    switch (e.type)
                    {
                        // This is called when the mouse button goes up, which is not what we want
                        // So just use the ContextClick event only for showing the box
                        // MouseDown is used to hide instead
                        case EventType.ContextClick:
                            // Right context click, show the modify UI
                            if (e.button == 1 && modifyIndex < 0)
                            {
                                modifyIndex = draggableRect.Contains(e.mousePosition) ? i : -1;
                            }
                            // Click anywhere else that isn't right click + modifyValue does not contains position, hide the 'modifyIndex'
                            //else if (!modifyValueRect.Contains(e.mousePosition))
                            //{
                            //    modifyIndex = -1;
                            //}

                            if (position.Contains(e.mousePosition))
                            {
                                // Hide context menu
                                e.Use();
                                usedEvent = true;
                            }
                            if (modifyIndex >= 0)
                            {
                                // Set handle here also
                                currentInteractedProperty = new PropertyID(property);
                            }
                            break;
                        case EventType.MouseDown:
                            // Dragging
                            if (e.button == 0 && dragIndex < 0)
                            {
                                dragIndex = draggableRect.Contains(e.mousePosition) ? i : -1;
                                Undo.RecordObject(property.serializedObject.targetObject, "drag set range float");
                            }
                            else
                            {
                                dragIndex = -1;
                            }

                            // Right click, show the modify UI
                            if (e.button == 1 && modifyIndex < 0)
                            {
                                modifyIndex = draggableRect.Contains(e.mousePosition) ? i : -1;
                            }
                            // Click anywhere else that isn't right click + modifyValue does not contains position, hide the 'modifyIndex'
                            else if (!modifyValueRect.Contains(e.mousePosition))
                            {
                                modifyIndex = -1;
                            }

                            if (dragIndex >= 0 || modifyIndex >= 0)
                            {
                                e.Use();
                                // Set loop state
                                usedEvent = true;
                                // Set handle (to ignore other properties)
                                currentInteractedProperty = new PropertyID(property);
                            }
                            break;
                        case EventType.MouseUp:
                            if (dragIndex >= 0)
                            {
                                e.Use();
                                // clear handle (interaction is done, stop ignoring)
                                currentInteractedProperty = null;
                                dragIndex = -1;
                                usedEvent = true;
                            }
                            break;

                        case EventType.Repaint:
                        case EventType.MouseMove:
                        case EventType.MouseDrag:
                            if (dragIndex >= 0)
                            {
                                if (e.type != EventType.Repaint)
                                {
                                    // Use the event to make dragging smooth
                                    // Otherwise it's a jittery mess
                                    e.Use();
                                    usedEvent = true;
                                }

                                // Set an handle for other properties to not invoke this.
                                currentInteractedProperty ??= new PropertyID(property);

                                // Get position + center
                                xPosition = Mathf.Clamp(e.mousePosition.x - draggableRect.width, dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth);

                                // Map the dragging position correctly
                                target[dragIndex] = Additionals.Map(target.Min, target.Max, dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, xPosition);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}