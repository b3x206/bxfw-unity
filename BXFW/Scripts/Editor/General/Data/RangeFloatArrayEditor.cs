using UnityEngine;
using UnityEditor;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws an inspector for <see cref="RangeFloatArray"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(RangeFloatArray))]
    public class RangeFloatArrayDrawer : PropertyDrawer
    {
        // -- Settings
        /// <summary>
        /// Padding applied to the GUI.
        /// <br>The padded area will be subtracted from the <see cref="OnGUI(Rect, SerializedProperty, GUIContent)"/>'s position parameter.</br>
        /// </summary>
        private const float Padding = 2f;
        /// <summary>
        /// Padding between horizontal GUI elements.
        /// </summary>
        private const float HorizontalPadding = 3f;

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
        private const float DraggableBoxModifyValueWidth = 60f;
        private const float DraggableBoxModifyValueHeight = 52.5f;
        /// <summary>
        /// Initial width of the min/max value displays.
        /// </summary>
        private const float TextValueInitialWidth = 40f;
        /// <summary>
        /// Width (in percentage) for the property name.
        /// </summary>
        private const float PropNameWidthPercent = .3f;

        /// <summary>
        /// Size of the shown text value(s), calculated per character.
        /// <br>Yes, the default unity editor font is not monospace, but idc, as i will use <see cref="TextAlignment.Center"/> for GUI purposes.</br>
        /// </summary>
        private const float TextValueCharWidth = 6f;
        /// <summary>
        /// Returns the length of the <paramref name="number"/> when it's converted to an integer.
        /// </summary>
        private static int NumberStringLength(float number)
        {
            // Could get the number length depending on how many times it gets divided by 10
            // but that only works for the integral part + it probably could be slower than ToString
            // non-scientific-notated double/float with optional 3 precision numbers
            return number.ToString("G.###").Length;
        }

        // -- State
        /// <summary>
        /// String identification used for the previously interacted property.
        /// <br>Gathered using <see cref="SerializedPropertyCustomData.GetIDString(SerializedProperty)"/>.</br>
        /// </summary>
        private string interactedPropertyID;
        /// <summary>
        /// Hack for getting the correct rect in repaint but not in layout
        /// Setting the 'position' parameter during the EventType.Layout does not seem to break stuff
        /// </summary>
        private Rect previousRepaintRect;
        /// <summary>
        /// Index that is being dragged in the fake slider handle.
        /// </summary>
        private int dragIndex = -1;
        /// <summary>
        /// Index that is being modified by the BasicDropdown.
        /// </summary>
        private int modifyIndex = -1;

        private GUIStyle tinyFontBoxStyle;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + Padding;
        }

        /// <summary>
        /// Draws the rest of the interface.
        /// </summary>
        /// <param name="mainPosition">Total size of the OnGUI rect.</param>
        /// <param name="position">The position to draw this main gui on.</param>
        private void DrawMainGUI(Rect mainPosition, Rect position, SerializedProperty property)
        {
            RangeFloatArray target = property.GetTarget().value as RangeFloatArray;
            Event e = Event.current;
            string currentPropertyID = SerializedPropertyCustomData.GetIDString(property);

            // This code is awful IMGUI code, yes i know.
            // But it works. While this may go as a 'Complex Method' it is complex
            // just because i wanted to create a slider with multiple knobs
            // --
            // FIXME : Create a 'Slider' and 'MultiSlider' to GUIAdditionals
            // -- 
            // | -> content => tooltip = [value]
            // {val1} --o--o--o-- {val2} [+][-]

            // Draw the sized box
            float minTextWidth = TextValueInitialWidth + (NumberStringLength(target.Min) * TextValueCharWidth);
            float maxTextWidth = TextValueInitialWidth + (NumberStringLength(target.Max) * TextValueCharWidth);
            // Compensate for padding?
            Rect dragAreaBoxRect = new Rect(
                position.x + minTextWidth + HorizontalPadding,
                position.y,
                position.width - (minTextWidth + HorizontalPadding + maxTextWidth + HorizontalPadding + ((IncDecElemBtnWidth + HorizontalPadding) * 2f)),
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
                position.x + minTextWidth + dragAreaBoxRect.width + HorizontalPadding,
                position.y,
                maxTextWidth,
                position.height
            );
            float maxValue = EditorGUI.FloatField(maxValueFieldRect, target.Max);

            // Draw the 'increment/decrement array elements'
            int arrayLength = target.Count;
            Rect incArrayCntRect = new Rect(
                maxValueFieldRect.x + maxValueFieldRect.width + HorizontalPadding,
                position.y,
                IncDecElemBtnWidth,
                position.height
            );
            if (GUI.Button(incArrayCntRect, new GUIContent("+", "Increment element count in array.")))
            {
                arrayLength++;
            }
            Rect decArrayCntRect = new Rect(
                incArrayCntRect.x + incArrayCntRect.width + HorizontalPadding,
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
            if (interactedPropertyID == currentPropertyID)
            {
                // Bound check the indices
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
            float dragAreaWidth = dragAreaBoxRect.width - (DraggableBoxWidth + (HorizontalPadding * 2f)); // The corrected drag area width. !! correction needed !!
            Rect modifyValueRect = modifyIndex >= 0 && (interactedPropertyID == currentPropertyID) ?
            new Rect(
                x: MathUtility.Map(dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, target.Min, target.Max, target[modifyIndex]) - (DraggableBoxModifyValueWidth / 2f),
                y: position.y + (DraggableBoxModifyValueHeight / 2f),
                width: DraggableBoxModifyValueWidth,
                height: DraggableBoxModifyValueHeight
            ) : default;

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
                            interactedPropertyID = string.Empty;
                            return;
                        }

                        EditorGUI.BeginChangeCheck();
                        GUILayout.Label(new GUIContent($"Modify Index={modifyIndex}"), tinyFontBoxStyle);
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
            else if (interactedPropertyID == currentPropertyID)
            {
                BasicDropdown.HideDropdown();
            }

            // -- Change checks for the 'Min/Max' values
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(property.serializedObject.targetObject, "set RangeFloatArray value");
                target.Min = minValue;
                target.Max = maxValue;

                arrayLength = Mathf.Clamp(arrayLength, 0, int.MaxValue);
                target.Resize(arrayLength);
            }

            // -- Draw draggable 'circle' buttons
            // (like the RangeAttribute thing, clamped between 2 values)
            // Ensure the button values can't be the same
            bool usedEvent = false; // This variable checks if any of the knobs used event, only process event once per every knob
            for (int i = 0; i < target.Count; i++)
            {
                // Draw actual target's values
                float xPosition = Mathf.Lerp(dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, MathUtility.Map(0f, 1f, target.Min, target.Max, target[i]));
                Rect draggableRect = new Rect(xPosition, position.y + (position.height / 5f), DraggableBoxWidth, position.height);
                Rect draggableTextRect = new Rect()
                {
                    x = xPosition - 5f,
                    y = draggableRect.y - 15f,
                    height = 15f,
                    width = 20f
                };
                // Draw a GUI seperately + Draw the index in a smaller font towards bottom
                GUI.Box(draggableRect, new GUIContent(string.Empty, $"value={target[i]}\nindex={i}"), GUI.skin.horizontalSliderThumb);
                GUI.Box(draggableTextRect, i.ToString(), tinyFontBoxStyle);

                // Only call/check event(s) for single used GUI knob element
                // Note : Using the event and the 'modifyIndex or dragIndex' being different also prevents that.
                if (!usedEvent && (string.IsNullOrEmpty(interactedPropertyID) || interactedPropertyID == currentPropertyID))
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

                            if (modifyIndex >= 0)
                            {
                                // Set handle here also
                                interactedPropertyID = SerializedPropertyCustomData.GetIDString(property);
                            }

                            if (mainPosition.Contains(e.mousePosition))
                            {
                                // Hide context menu
                                e.Use();
                                usedEvent = true;
                                // Show custom basic menu if no 'modifyIndex'
                                if (modifyIndex < 0)
                                {
                                    GenericMenu menu = new GenericMenu();
                                    menu.AddItem(new GUIContent("Scatter Values"), false, () =>
                                    {
                                        for (int i = 0; i < target.Count; i++)
                                        {
                                            // Set all zero as the target[i] setter sorts
                                            target[i] = target.Min;
                                        }

                                        for (int i = target.Count - 1; i >= 0; i--)
                                        {
                                            target[i] = Mathf.Lerp(target.Min, target.Max, (float)i / Mathf.Max(1, target.Count - 1));
                                        }
                                    });
                                    menu.AddItem(new GUIContent("Copy Property Path"), false, () =>
                                    {
                                        GUIUtility.systemCopyBuffer = property.propertyPath;
                                    });
                                    menu.ShowAsContext();
                                }
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
                                usedEvent = true;

                                // Set handle (to ignore other properties)
                                currentPropertyID = property.GetIDString();
                            }
                            break;
                        case EventType.MouseUp:
                            if (dragIndex >= 0)
                            {
                                e.Use();
                                usedEvent = true;

                                // Clear handle (interaction is done)
                                currentPropertyID = string.Empty;
                                dragIndex = -1;
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
                                    // --
                                    // Now it only lags when we are outside the inspector/component area
                                    // Idk how can i force the property drawer's window to keep receiving events
                                    e.Use();
                                    usedEvent = true;
                                }

                                // Set an handle for other properties to not invoke this.
                                currentPropertyID ??= property.GetIDString();

                                // Get position + center
                                xPosition = Mathf.Clamp(e.mousePosition.x - draggableRect.width, dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth);
                                // Map the dragging position correctly
                                target[dragIndex] = MathUtility.Map(target.Min, target.Max, dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, xPosition);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // -- If the property is editing multiple objects don't allow editing
            if (property.serializedObject.isEditingMultipleObjects)
            {
                EditorGUI.HelpBox(position, "Multi editing of 'RangeFloatArray' is not supported.", MessageType.Warning);
                return;
            }

            if (tinyFontBoxStyle == null)
            {
                tinyFontBoxStyle = new GUIStyle(GUI.skin.box) { fontSize = 8, alignment = TextAnchor.UpperCenter };
                tinyFontBoxStyle.normal.textColor = Color.white;
            }

            // -- Local Globals
            // Current event
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
            position.height -= Padding;
            position.y += Padding / 2f;

            // -- Label Field
            EditorGUI.BeginChangeCheck();
            float propertyLabelWidth = position.width * PropNameWidthPercent;
            float setRemainingWidth = position.width * (1f - PropNameWidthPercent);
            EditorGUI.LabelField(new Rect(position) { width = propertyLabelWidth }, label);
            // modify main rect (TODO : Don't, instead create 'DrawMainUI' function that takes rect)

            Rect mainGUIPosition = position;
            mainGUIPosition.x += position.width * PropNameWidthPercent;
            mainGUIPosition.width = setRemainingWidth;

            DrawMainGUI(position, mainGUIPosition, property);
        }
    }
}
