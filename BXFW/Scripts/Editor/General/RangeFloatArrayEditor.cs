using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;
using System;
using System.Reflection;
using System.Linq;

namespace BXFW.ScriptEditor
{
    internal class BasicDropdown : EditorWindow
    {
        private static readonly Type popupLocationType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PopupLocation");
        private static readonly Type popupLocationArrayType = popupLocationType.MakeArrayType();
        // Enum type should be 'PopupLocation'.
        private static Array GetPopupLocations()
        {
            Array retValue = (Array)Activator.CreateInstance(popupLocationArrayType, args: 2);
            retValue.SetValue(Enum.ToObject(popupLocationType, 0), 0); /* PopupLocation.Below,  */
            retValue.SetValue(Enum.ToObject(popupLocationType, 4), 1); /* PopupLocation.Overlay */
            
            return retValue;
        }

        private static BasicDropdown Instance;
        private Action<BasicDropdown> onGUICall;
        public static bool IsBeingShown()
        {
            return Instance != null;
        }
        public static void ShowDropdown(Rect parentRect, Vector2 size, Action<BasicDropdown> onGUICall)
        {
            if (Instance == null)
            {
                Instance = CreateInstance<BasicDropdown>();
            }

            Instance.position = new Rect(Instance.position) { x = parentRect.xMin, y = parentRect.yMax, size = size };
            Instance.onGUICall = onGUICall;
            // void ShowAsDropDown(Rect buttonRect, Vector2 windowSize, PopupLocation[] priorities)
            MethodInfo showDropdown = typeof(EditorWindow).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Single(f => f.Name == "ShowAsDropDown" && f.GetParameters().Length == 3);
            //MethodInfo showDropdown = typeof(EditorWindow).GetMethod("ShowAsDropDown", BindingFlags.Instance | BindingFlags.NonPublic);
            showDropdown.Invoke(Instance, new object[] { parentRect, size, GetPopupLocations() });
        }
        public static void HideDropdown()
        {
            if (Instance != null)
            {
                Instance.Close();
            }
        }
        public static void SetPosition(Rect screenPosition)
        {
            Instance.position = screenPosition;
        }
        private void OnGUI()
        {
            if (onGUICall == null || Instance == null)
            {
                Close();
            }

            onGUICall(this);
        }
    }

    // H moment : 
    // https://discussions.unity.com/t/how-to-edit-array-list-property-with-custom-propertydrawer/218416/2
    // You can’t make a PropertyDrawer for arrays or generic lists themselves. […] On the plus side, elements inside arrays and lists do work with PropertyDrawers.
    // This was meant to be an PropertyDrawer for an array, but i will just create a custom class.

    [CustomPropertyDrawer(typeof(RangeFloatArray))]
    internal class RangeArrayDrawer : PropertyDrawer
    {
        private RangeFloatArray GetTarget(SerializedProperty targetProperty)
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
        private const float TextValueInitialWidth = 12f;
        /// <summary>
        /// Size of the shown text value(s), calculated per character.
        /// <br>Yes, the default unity editor font is not monospace, but idc, as i will use <see cref="TextAlignment.Center"/> for GUI purposes.</br>
        /// </summary>
        private const float TextValueCharWidth = 6f;

        /// <summary>
        /// Width (in percentage) for the property name.
        /// </summary>
        private const float PropNameWidthPercent = .3f;

        private Rect previousRepaintRect; // hack for getting the correct rect in repaint but not in layout
                                          // Setting the 'position' parameter during the EventType.Layout does not seem to break stuff
        private int dragIndex = -1;       // Index that is being dragged in the fake slider
        private int modifyIndex = -1;     // Index that is being modified by the SimpleDropdown
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {            
            var target = GetTarget(property);
            Event e = Event.current;
            // e.type == Layout gives incorrect positioning
            // This makes the popup window jitter.
            if (e.type == EventType.Repaint)
            {
                previousRepaintRect = position;
            }
            if (e.type == EventType.Layout)
            {
                position = previousRepaintRect;
            }

            // Check if the xIndex thing's indices actually exist
            // Otherwise set them to -1 to avoid errors
            if (dragIndex >= target.Count)
            {
                dragIndex = -1;
            }
            if (modifyIndex >= target.Count)
            {
                modifyIndex = -1;
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
            // val1 --|--|--|-- val2 [+][-]

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
            Debug.Log($"min: {minTextWidth}, max: {maxTextWidth} | minR : {minValueFieldRect}, maxR : {maxValueFieldRect} | e: {e.type}");

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

            // Modify UI
            float dragAreaWidth = dragAreaBoxRect.width - (DraggableBoxWidth + (ElemPadding * 2f)); // The corrected drag area width. !! correction needed !!
            Rect modifyValueRect = modifyIndex >= 0 ? new Rect(
                Mathf.Lerp(dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, Additionals.Map(0f, 1f, target.Min, target.Max, target[modifyIndex])) - (DraggableBoxModifyValueWidth / 2f),
                position.y + (DraggableBoxModifyValueHeight / 2f),
                DraggableBoxModifyValueWidth,
                DraggableBoxModifyValueHeight
            ) : Rect.zero;
            if (modifyIndex >= 0)
            {
                // works slightly better
                if (!BasicDropdown.IsBeingShown())
                {
                    BasicDropdown.ShowDropdown(GUIUtility.GUIToScreenRect(modifyValueRect), modifyValueRect.size, (BasicDropdown _) =>
                    {
                        if (modifyIndex < 0)
                        {
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
                else if (e.type != EventType.Layout)
                {
                    BasicDropdown.SetPosition(GUIUtility.GUIToScreenRect(modifyValueRect));
                }
            }
            else
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
            for (int i = 0; i < target.Count; i++)
            {
                float xPosition = Mathf.Lerp(dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth, Additionals.Map(0f, 1f, target.Min, target.Max, target[i]));
                Rect draggableRect = new Rect(xPosition, position.y + position.height / 5f, DraggableBoxWidth, position.height);
                // Draw a GUI seperately
                GUI.Box(draggableRect, new GUIContent(string.Empty, $"value={target[i]}\nindex={i}"), GUI.skin.horizontalSliderThumb);
                // Intercept events manually
                switch (e.type)
                {
                    case EventType.MouseDown:
                        // Dragging
                        if (e.button == 0 && dragIndex < 0)
                        {
                            dragIndex = draggableRect.Contains(e.mousePosition) ? i : -1;
                            Undo.RecordObject(property.serializedObject.targetObject, "drag set range float");
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
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragIndex >= 0)
                        {
                            e.Use();
                        }
                        dragIndex = -1;
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
                            }

                            // Get position + center
                            xPosition = Mathf.Clamp(e.mousePosition.x, dragAreaBoxRect.x, dragAreaBoxRect.x + dragAreaWidth) - (draggableRect.width);
                            GUI.Box(new Rect(dragAreaBoxRect.x, position.y, 50f, 20f), "from");
                            GUI.Box(new Rect(dragAreaBoxRect.x + dragAreaWidth, position.y, 50f, 20f), "to");

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