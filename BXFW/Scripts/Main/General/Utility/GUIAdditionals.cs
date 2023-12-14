using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngineInternal;

namespace BXFW
{
    /// <summary>
    /// GUI additionals, provides GUI related drawing methods.
    /// </summary>
    public static class GUIAdditionals
    {
        /// <summary>
        /// Hashes contained in the <see cref="GUI"/> class, 
        /// for use with <see cref="GUIUtility.GetControlID(int, FocusType, Rect)"/>.
        /// <br>This is not the exact same hashlist unity internally uses,
        /// this is for custom ID usage for different interfaces that need a custom ID appended.</br>
        /// </summary>
        public static class HashList
        {
            public static readonly int BoxHash = "Box".GetHashCode();
            // Button is misspelled on the unity hashlist as 'Buton'
            public static readonly int ButtonHash = "Button".GetHashCode();
            public static readonly int RepeatButtonHash = "repeatButton".GetHashCode();
            public static readonly int ToggleHash = "Toggle".GetHashCode();
            public static readonly int ButtonGridHash = "ButtonGrid".GetHashCode();
            public static readonly int SliderHash = "Slider".GetHashCode();
            public static readonly int BeginGroupHash = "BeginGroup".GetHashCode();
            public static readonly int ScrollViewHash = "scrollView".GetHashCode();
        }

        /// <summary>
        /// Assembly that is the <c>UnityEngine.IMGUIModule</c>
        /// </summary>
        public static readonly Assembly ImguiAssembly = typeof(GUILayout).Assembly;

        /// <summary>
        /// Manages and allows access to the <see cref="GUILayoutUtility"/>'s current layout globals.
        /// <br>This allows for more control of the <see cref="GUILayout"/>.</br>
        /// </summary>
        public static class CurrentLayout
        {
            /// <summary>
            /// The internal <see cref="GUILayoutUtility.LayoutCache"/> type.
            /// </summary>
            public static readonly Type LayoutCacheType = ImguiAssembly.GetType("UnityEngine.GUILayoutUtility+LayoutCache", true);
            /// <summary>
            /// The field that contains the currently used cache value (in <see cref="GUILayoutUtility"/>).
            /// </summary>
            public static readonly FieldInfo CurrentLayoutCacheField = typeof(GUILayoutUtility).GetField("current", BindingFlags.NonPublic | BindingFlags.Static);

            private static InternalGUILayoutGroup m_RootWindows;
            /// <summary>
            /// List/Group of the root windows.
            /// <br>The current parent group that is being drawn on the cache.</br>
            /// <br>This group also handles <see cref="Event"/>'s input related stuff.
            /// Adding then removing at the same GUI call will cause the inputs to be ignored.
            /// Keep and then only remove when the added <see cref="InternalGUILayoutGroup"/> is about to be a duplicate.
            /// </br>
            /// </summary>
            public static InternalGUILayoutGroup RootWindows
            {
                get
                {
                    // Maybe make 'm_' values boxed WeakReferences?
                    // So that when the root GUILayoutGroup is gone the 'InternalGUILayoutGroup' is also gone?
                    if (m_RootWindows == null)
                    {
                        object current = CurrentLayoutCacheField.GetValue(null);
                        FieldInfo fiWindows = LayoutCacheType.GetField("windows", BindingFlags.NonPublic | BindingFlags.Instance);
                        m_RootWindows = new InternalGUILayoutGroup(fiWindows.GetValue(current));
                    }

                    return m_RootWindows;
                }
            }
            /// <summary>
            /// List of the layout groups in a stack.
            /// <br>Always push/pop the <see cref="InternalGUILayoutEntry.BoxedEntry"/>ies.</br>
            /// </summary>
            public static GenericStack LayoutGroups
            {
                get
                {
                    object current = CurrentLayoutCacheField.GetValue(null);
                    FieldInfo fiWindows = LayoutCacheType.GetField("layoutGroups", BindingFlags.NonPublic | BindingFlags.Instance);
                    return (GenericStack)fiWindows.GetValue(current);
                }
            }
            private static InternalGUILayoutGroup m_LastTopLevelGroup;
            /// <summary>
            /// The last group that is being proccessed.
            /// <br>This can be changed by using <see cref="PushLayoutGroup"/></br>
            /// </summary>
            public static InternalGUILayoutGroup LastTopLevelGroup
            {
                get
                {
                    if (m_LastTopLevelGroup == null)
                    {
                        object current = CurrentLayoutCacheField.GetValue(null);
                        FieldInfo fiTopLevel = LayoutCacheType.GetField("topLevel", BindingFlags.NonPublic | BindingFlags.Instance);
                        m_LastTopLevelGroup = new InternalGUILayoutGroup(fiTopLevel.GetValue(current));
                    }

                    return m_LastTopLevelGroup;
                }
                private set
                {
                    object current = CurrentLayoutCacheField.GetValue(null);
                    FieldInfo fiTopLevel = LayoutCacheType.GetField("topLevel", BindingFlags.NonPublic | BindingFlags.Instance);
                    fiTopLevel.SetValue(current, value.BoxedEntry);
                }
            }

            /// <summary>
            /// Pushes the <paramref name="group"/> and sets it to the last group.
            /// <br>Calling this is only recommended when the <see cref="Event.current"/>.type is 
            /// <see cref="EventType.Layout"/> (or <see cref="EventType.Used"/>), but this method doesn't prohibit/act differently on any event.</br>
            /// <br/>
            /// <br>To pop the pushed element, use <see cref="PopLastLayoutGroup"/> or 
            /// <see cref="GUILayout.EndArea"/> (has <see cref="GUI.EndGroup"/> side effect) depending on which type of group you have pushed in.</br>
            /// </summary>
            /// <param name="layoutEventPush">
            /// Whether to push the <paramref name="group"/> into the <see cref="RootWindows"/> stack,
            /// acting like the <see cref="EventType.Layout"/> behaviour of <see cref="GUILayoutUtility.BeginLayoutArea"/>.
            /// </param>
            public static void PushLayoutGroup(InternalGUILayoutGroup group, bool layoutEventPush = true)
            {
                // Does the same as GUILayoutUtility.BeginLayoutArea's EventType.Layout behaviour
                CheckOnGUI();

                if (layoutEventPush)
                {
                    RootWindows.Add(group);
                }

                LayoutGroups.Push(group.BoxedEntry);
                LastTopLevelGroup = group;
            }

            /// <summary>
            /// Pops the last used <see cref="InternalGUILayoutGroup"/>.
            /// <br>This doesn't remove the element from the <see cref="RootWindows"/>.</br>
            /// </summary>
            public static void PopLastLayoutGroup()
            {
                CheckOnGUI();

                LayoutGroups.Pop();
                LastTopLevelGroup = new InternalGUILayoutGroup(LayoutGroups.Peek());
            }
        }

        /// <summary>
        /// Checks whether if there's a valid OnGUI context.
        /// <br>Throws <see cref="ArgumentException"/> if the context is not valid.</br>
        /// </summary>
        public static void CheckOnGUI()
        {
            typeof(GUIUtility).GetMethod("CheckOnGUI", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }
        /// <summary>
        /// Returns whether if there's a valid OnGUI context.
        /// </summary>
        public static bool IsOnGUI()
        {
            // 'CheckOnGUI' checks whether if the guiDepth is less or equal to zero
            // Then it throws an exception.
            return (int)typeof(GUIUtility).GetProperty("guiDepth", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) > 0;
        }

        private static readonly Type GUILayoutGroupType = ImguiAssembly.GetType("UnityEngine.GUILayoutGroup", true);
        /// <summary>
        /// Begins a positioned layout GUI.
        /// <br>Can accept custom functions that create in a different way.</br>
        /// </summary>
        /// <param name="position">Position to start the layout group from.</param>
        /// <param name="width">Width of the layout to start.</param>
        /// <param name="style">Style of the layout group. This defines styling (such as a background, etc.)</param>
        /// <param name="content">Content of the layout group to accommodate for.</param>
        /// <param name="createLayoutGroupFunc">Function to create the <see cref="GUILayoutGroup"/>.</param>
        /// <returns>The boxed <see cref="GUILayoutGroup"/> value. Use reflection to access the values for now.</returns>
        private static InternalGUILayoutGroup BeginLayoutPositionGroupInternal(Vector2 position, float width, GUIStyle style, GUIContent content, Func<GUIStyle, InternalGUILayoutGroup> createLayoutGroupFunc)
        {
            InternalGUILayoutGroup layoutGroupObject = createLayoutGroupFunc(style);

            if (Event.current.type == EventType.Layout)
            {
                layoutGroupObject.ResetCoords = true;
                layoutGroupObject.MinWidth = layoutGroupObject.MaxWidth = width;
                layoutGroupObject.MaxHeight = 0f; // this can be 0, CalculateHeight will set this
            }

            Rect groupTypeRectValue = layoutGroupObject.EntryRect;
            layoutGroupObject.EntryRect = Rect.MinMaxRect(position.x, position.y, groupTypeRectValue.xMax, groupTypeRectValue.yMax);

            GUI.BeginGroup(layoutGroupObject.EntryRect, content, style);
            return layoutGroupObject;
        }

        /// <inheritdoc cref="BeginLayoutPosition(Vector2, float, GUIStyle, GUIContent)"/>
        public static InternalGUILayoutGroup BeginLayoutPosition(Vector2 position, float width)
        {
            return BeginLayoutPosition(position, width, GUIStyle.none, GUIContent.none);
        }
        /// <inheritdoc cref="BeginLayoutPosition(Vector2, float, GUIStyle, GUIContent)"/>
        public static InternalGUILayoutGroup BeginLayoutPosition(Vector2 position, float width, GUIStyle style)
        {
            return BeginLayoutPosition(position, width, style, GUIContent.none);
        }
        /// <inheritdoc cref="BeginLayoutPosition(Vector2, float, GUIStyle, GUIContent)"/>
        public static InternalGUILayoutGroup BeginLayoutPosition(Vector2 position, float width, GUIContent content)
        {
            return BeginLayoutPosition(position, width, GUIStyle.none, content);
        }
        /// <summary>
        /// Begins a positioned layouted GUI.
        /// <br>Basically creates a new GUILayout context with position from the given parameters.</br>
        /// <br>Useful for <see cref="UnityEditor.PropertyDrawer"/>'s and other things.</br>
        /// </summary>
        /// <param name="position">Position to start the layout group from.</param>
        /// <param name="width">Width of the layout to start.</param>
        /// <param name="style">Style of the layout group. This defines styling (such as a background, etc.)</param>
        /// <param name="content">Content of the layout group to accommodate for.</param>
        /// <returns>The created <see cref="GUILayoutGroup"/> on a wrapper.</returns>
        public static InternalGUILayoutGroup BeginLayoutPosition(Vector2 position, float width, GUIStyle style, GUIContent content)
        {
            CheckOnGUI();

            MethodInfo miBeginLayoutArea = typeof(GUILayoutUtility).GetMethod("BeginLayoutArea", BindingFlags.NonPublic | BindingFlags.Static);
            return BeginLayoutPositionGroupInternal(position, width, style, content, (style) => new InternalGUILayoutGroup(miBeginLayoutArea.Invoke(null, new object[] { style, GUILayoutGroupType })));
        }

        /// <summary>
        /// Ends the positioned area.
        /// <br>Both injected layout groups and normal layout groups can use this.</br>
        /// </summary>
        public static void EndLayoutPosition()
        {
            GUILayout.EndArea();
        }

        /// <summary>
        /// Returns whether if the previous OnGUI <see cref="Event"/> with 
        /// <see cref="EventType.Layout"/> type allocated enough controls to create <paramref name="nextEntryCount"/> amount of GUI elements.
        /// <br>Basically whether if we can allocate a <see cref="GUILayoutEntry"/> or not.</br>
        /// <br>Always returns <see langword="false"/> <b>while NOT on a GUI method.</b></br>
        /// </summary>
        /// <param name="nextEntryCount">Amount of groups to create after this method. This value will never be lower than 1.</param>
        public static bool CanCreateGUIEntry(int nextEntryCount = 1)
        {
            if (!IsOnGUI())
            {
                return false;
            }

            nextEntryCount = Mathf.Max(nextEntryCount, 1);

            return CanGetNextInGroup(CurrentLayout.RootWindows, nextEntryCount);
        }
        /// <summary>
        /// Returns whether if the <paramref name="group"/> has more entries beyond it's current cursor position.
        /// </summary>
        /// <param name="groupCount">Amount of groups to nest after this method. This value will never be lower than 1.</param>
        public static bool CanGetNextInGroup(InternalGUILayoutGroup group, int nextCount)
        {
            if (!IsOnGUI())
            {
                return false;
            }

            nextCount = Mathf.Max(nextCount, 1);

            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "[GUIAdditionals::CanGetNextInGroup] Given parameter was null.");
            }

            return (group.CursorPosition + nextCount) <= group.Count;
        }

        private const string JBMONO_FONT_NAME = "Jetbrains Mono";
        private const string CONSOLAS_FONT_NAME = "Consolas";
        private const string COURIER_FONT_NAME = "Courier";
        /// <summary>
        /// Name of the selected monospace font.
        /// </summary>
        private static string MonospaceFontName
        {
            get
            {
                string[] fonts = Font.GetOSInstalledFontNames();
                string selected;
                if (fonts.Any(name => name == JBMONO_FONT_NAME))
                {
                    selected = JBMONO_FONT_NAME;
                }
                else if (fonts.Any(name => name == CONSOLAS_FONT_NAME))
                {
                    selected = CONSOLAS_FONT_NAME;
                }
                else
                {
                    selected = COURIER_FONT_NAME;
                }

                return selected;
            }
        }
        private static Font m_MonospaceFont;
        /// <summary>
        /// A monospace font loaded (and cached) from the system fonts.
        /// <br>This will go in this preference order =&gt; Jetbrains Mono, Consolas and will use Courier if all fails.</br>
        /// </summary>
        public static Font MonospaceFont
        {
            get
            {
                if (m_MonospaceFont == null)
                {
                    m_MonospaceFont = Font.CreateDynamicFontFromOSFont(MonospaceFontName, 12);
                }

                return m_MonospaceFont;
            }
        }
        private static int m_MonospaceFontSize = 12;
        /// <summary>
        /// Generation size of the monospace font.
        /// </summary>
        public static int MonospaceFontSize
        {
            get
            {
                return m_MonospaceFontSize;
            }
            set
            {
                m_MonospaceFontSize = Math.Clamp(value, 1, int.MaxValue);
                m_MonospaceFont = Font.CreateDynamicFontFromOSFont(MonospaceFontName, m_MonospaceFontSize);
            }
        }

        /// <summary>
        /// A drag box variable to check if a drag box is being dragged.
        /// </summary>
        private static bool isDragged = false;
        /// <summary>
        /// <br>Usage: Create a draggable box. The box can intercept drag events and invoke a callback.</br>
        /// <br>The <paramref name="onDrag"/> is invoked when the box is being dragged.</br>
        /// </summary>
        /// <param name="rect">
        /// The rectangle that this box is contained in.
        /// The <paramref name="onDrag"/> callback can modify this, this method 
        /// does not modify anything, only intercepts the dragging event.
        /// </param>
        /// <param name="onDrawButton">
        /// Called when the button GUI is to be drawn.
        /// The bool parameter is whether if the button is being dragged or not.
        /// </param>
        /// <returns>The control id of this gui.</returns>
        public static int DragBox(Rect rect, Action<bool> onDrawButton, Action<Vector2> onDrag)
        {
            int controlID = GUIUtility.GetControlID(HashList.RepeatButtonHash, FocusType.Passive, rect);

            if (rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    isDragged = true;
                    GUIUtility.hotControl = controlID;
                    Event.current.Use();
                }
            }
            
            if (isDragged && GUIUtility.hotControl == controlID)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    onDrag(Event.current.delta);
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    isDragged = false;
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }

            // Can intercept events manually so do that
            // GUI's change state is also not set when we use
            // the event and do something so set it manually
            GUI.changed = Event.current.type == EventType.Used;
            onDrawButton(isDragged && GUIUtility.hotControl == controlID);

            return controlID;
        }
        /// <inheritdoc cref="DragBox(Rect, Action{bool}, Action{Vector2})"/>
        public static int DragBox(Rect rect, GUIContent content, GUIStyle style, Action<Vector2> onDrag)
        {
            return DragBox(rect, (bool _) =>
            {
                GUI.Box(rect, content, style);
            }, onDrag);
        }
        /// <inheritdoc cref="DragBox(Rect, Action{bool}, Action{Vector2})"/>
        public static int DragBox(Rect rect, GUIContent content, Action<Vector2> onDrag)
        {
            return DragBox(rect, content, GUI.skin.box, onDrag);
        }

        /// <summary>
        /// Returns a optionally overridable by <paramref name="options"/> Rect.
        /// <br>Unfortunately, the <see cref="GUILayoutUtility.GetRect(float, float, float, float, GUILayoutOption[])"/> 
        /// doesn't care about the <paramref name="options"/> parameter being overrides.</br>
        /// <br>This method uses the <paramref name="options"/> as if it was an override if the option types match for the width+height.</br>
        /// <br>Basically, passing an array with some elements that are unrelated to this layout rect + fixed height will not enforce itself.</br>
        /// </summary>
        /// <returns>Rect from the <see cref="GUILayoutUtility.GetRect(float, float, float, float, GUILayoutOption[])"/>.</returns>
        public static Rect GetOptionalGUILayoutRect(float minWidth, float maxWidth, float minHeight, float maxHeight, params GUILayoutOption[] options)
        {
            if (options.Length > 0)
            {
                FieldInfo guiOptionTypeField = typeof(GUILayoutOption).GetField("type", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo guiOptionValueField = typeof(GUILayoutOption).GetField("value", BindingFlags.NonPublic | BindingFlags.Instance);

                // -- Dynamic size options
                GUILayoutOption minWidthOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.minWidth == 2
                    ((int)guiOptionTypeField.GetValue(o)) == 2
                );
                GUILayoutOption maxWidthOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.maxWidth == 3
                    ((int)guiOptionTypeField.GetValue(o)) == 3
                );
                if (minWidthOption != null)
                {
                    minWidth = (float)guiOptionValueField.GetValue(minWidthOption);
                }

                if (maxWidthOption != null)
                {
                    maxWidth = (float)guiOptionValueField.GetValue(maxWidthOption);
                }

                GUILayoutOption minHeightOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.minHeight == 4
                    ((int)guiOptionTypeField.GetValue(o)) == 4
                );
                GUILayoutOption maxHeightOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.maxHeight == 5
                    ((int)guiOptionTypeField.GetValue(o)) == 5
                );
                if (minHeightOption != null)
                {
                    minHeight = (float)guiOptionValueField.GetValue(minHeightOption);
                }

                if (maxHeightOption != null)
                {
                    maxHeight = (float)guiOptionValueField.GetValue(maxHeightOption);
                }

                // -- Fixed size options (override)
                GUILayoutOption fixedWidthOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.fixedWidth == 0
                    ((int)guiOptionTypeField.GetValue(o)) == 0
                );
                GUILayoutOption fixedHeightOption = options.SingleOrDefault(o =>
                    // GUILayoutOption.Type.fixedHeight == 1
                    ((int)guiOptionTypeField.GetValue(o)) == 1
                );
                if (fixedWidthOption != null)
                {
                    minWidth = maxWidth = ((float)guiOptionValueField.GetValue(fixedWidthOption));
                }
                if (fixedHeightOption != null)
                {
                    minHeight = maxHeight = ((float)guiOptionValueField.GetValue(fixedHeightOption));
                }
            }

            return GUILayoutUtility.GetRect(minWidth, maxWidth, minHeight, maxHeight, options);
        }

        /// <summary>
        /// Draws line.
        /// <br>Color defaults to <see cref="Color.white"/>.</br>
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, float width)
        {
            DrawLine(start, end, width, Color.white);
        }

        /// <summary>
        /// Draws line with color.
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, float width, Color col)
        {
            var gc = GUI.color;
            GUI.color = col;
            DrawLine(start, end, width, Texture2D.whiteTexture);
            GUI.color = gc;
        }

        /// <summary>
        /// Draws line with texture.
        /// <br>The texture is not used for texture stuff, only for color if your line is not thick enough.</br>
        /// </summary>
        public static void DrawLine(Vector2 start, Vector2 end, float width, Texture2D tex)
        {
            var guiMat = GUI.matrix;

            if (start == end)
            {
                return;
            }

            if (width <= 0)
            {
                return;
            }

            Vector2 d = end - start;
            float a = Mathf.Rad2Deg * Mathf.Atan(d.y / d.x);
            if (d.x < 0)
            {
                a += 180;
            }

            int width2 = (int)Mathf.Ceil(width / 2);

            GUIUtility.RotateAroundPivot(a, start);
            GUI.DrawTexture(new Rect(start.x, start.y - width2, d.magnitude, width), tex);

            GUI.matrix = guiMat;
        }

        /// <summary>
        /// Tiny value for <see cref="PlotLine(Rect, Func{float, float}, float, float, float, int)"/>.
        /// </summary>
        private const float PlotDrawEpsilon = .01f;
        /// <summary>
        /// Size of drawn label padding.
        /// </summary>
        private const float PlotTextPaddingX = 24f;
        private const float PlotTextPaddingY = 12f;
        private const int PlotTextFontSize = 9;
        private static GUIStyle PlotSmallerFontStyle;
        /// <summary>
        /// Plots the <paramref name="plotFunction"/> to the <see cref="GUI"/>.
        /// <br>The plotting is not accurate and does ignore some of the characteristics of certain functions
        /// (i.e <see cref="Mathf.Tan(float)"/>), but it looks good enough for a rough approximation.</br>
        /// <br/>
        /// <br>Note : This calls <see cref="DrawLine(Vector2, Vector2, float)"/> lots of times instead of doing something optimized.</br>
        /// <br>It is also very aliased. For drawing (only) bezier curves that look good, use the <see cref="UnityEditor.Handles"/> class. (editor only)</br>
        /// </summary>
        /// <param name="position">Rect positioning to draw the line.</param>
        /// <param name="plotFunction">The plot function that returns rational numbers and is linear. (no self intersections, double values in one value or anything)</param>
        /// <param name="vFrom">The first value to feed the plot function while linearly interpolating.</param>
        /// <param name="vTo">The last value to feed the plot function while linearly interpolating.</param>
        /// <param name="segments">Amount of times that the <see cref="DrawLine(Vector2, Vector2, float)"/> will be called. This should be a value larger than 1</param>
        public static void PlotLine(Rect position, Func<float, float> plotFunction, bool showFromToLabels, bool showMinMaxLabels, float vFrom = 0f, float vTo = 1f, float lineWidth = 2.5f, int segments = 20)
        {
            // Only do this plotting if we are actually drawing and not layouting
            // As this plotter has no interactions and will only paint
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            PlotSmallerFontStyle ??= new GUIStyle(GUI.skin.label) { fontSize = PlotTextFontSize, wordWrap = true };

            if (segments < 1)
            {
                segments = 2;
            }

            if ((vFrom + PlotDrawEpsilon) >= vTo)
            {
                vFrom = vTo - PlotDrawEpsilon;
            }

            // Draw dark box behind
            var guiPrevColor = GUI.color;
            GUI.color = new Color(.4f, .4f, .4f, .2f);
            GUI.DrawTexture(
                position,
                Texture2D.whiteTexture, ScaleMode.StretchToFill
            );
            GUI.color = guiPrevColor;

            // very naive plotting for GUI, using approximation + stepping (sigma)
            // If someone that is good at math saw this they would have a seizure
            // Here's how to make it less naive
            // A : Make it more efficient
            // B : A better drawing algorithm (perhaps use meshes? stepping is more different? idk.)

            // ----
            // Get local maximum value in the given range (because Y is calculated by min/max)
            float localMinimum = float.MaxValue; // Minimum text to draw
            float localMaximum = float.MinValue; // Maximum text to draw
            bool allValuesZero = true; // Avoid NaN's (because a NaN explosion happens in that case)
            for (int i = 0; i < segments; i++)
            {
                // i is always 1 less then segments
                float currentSegmentElapsed = (float)i / (segments - 1);
                float lerpValue = Mathf.Lerp(vFrom, vTo, currentSegmentElapsed);

                float plotValue = plotFunction(lerpValue);

                if (!Mathf.Approximately(plotValue, 0f))
                {
                    allValuesZero = false;
                }

                if (plotValue > localMaximum)
                {
                    localMaximum = plotValue;
                }

                if (plotValue < localMinimum)
                {
                    localMinimum = plotValue;
                }
            }

            // Labels have a reserved 'PLOT_TEXT_PADDING' width
            Rect plotPosition = position;

            // Draw from/to text (x, positioned bottom)
            if (showFromToLabels)
            {
                plotPosition.height -= PlotTextPaddingY;

                PlotSmallerFontStyle.alignment = TextAnchor.UpperLeft;
                Rect leftLabelRect = new Rect
                {
                    x = position.x,
                    y = position.yMax - PlotTextPaddingY,
                    width = 32f,
                    height = PlotTextPaddingY
                };
                if (showMinMaxLabels)
                {
                    leftLabelRect.x += PlotTextPaddingX;
                }
                GUI.Label(
                    leftLabelRect,
                    vFrom.ToString("0.0#"), PlotSmallerFontStyle
                ); // left

                PlotSmallerFontStyle.alignment = TextAnchor.UpperRight;
                Rect rightLabelRect = new Rect
                {
                    x = position.xMax - 32f,
                    y = position.yMax - PlotTextPaddingY,
                    width = 32f,
                    height = PlotTextPaddingY
                };
                GUI.Label(
                    rightLabelRect,
                    vTo.ToString("0.0#"), PlotSmallerFontStyle
                ); // right
            }
            if (showMinMaxLabels)
            {
                plotPosition.x += PlotTextPaddingX;
                plotPosition.width -= PlotTextPaddingX;

                PlotSmallerFontStyle.alignment = TextAnchor.UpperLeft;
                // Draw local min/max text (y, positioned left)
                Rect topLabelRect = new Rect
                {
                    x = position.x,
                    y = position.yMin,
                    width = 32f,
                    height = PlotTextPaddingY
                };
                GUI.Label(
                    topLabelRect,
                    localMaximum.ToString("0.0#"), PlotSmallerFontStyle
                ); // up
                Rect bottomLabelRect = new Rect
                {
                    x = position.x,
                    y = position.yMax - PlotTextPaddingY,
                    width = 32f,
                    height = PlotTextPaddingY
                };
                if (showFromToLabels)
                {
                    // Offset again for the 'from-to' labels
                    bottomLabelRect.y -= PlotTextPaddingY;
                }
                GUI.Label(
                    bottomLabelRect,
                    localMinimum.ToString("0.0#"), PlotSmallerFontStyle
                ); // down
            }

            // This will throw a lot of errors, especially if the values are 0.
            if (allValuesZero)
            {
                // As a fallback, draw a line that goes through lowest part of 'plotPosition'
                DrawLine(new Vector2(plotPosition.x, plotPosition.yMax), new Vector2(plotPosition.xMax, plotPosition.yMax), lineWidth);
                return;
            }

            // Draw the area divider (if suitable)
            //     |
            //     |
            // ---------]-> i call this divider lmao
            //     |
            //     |
            // Y divider
            if (vFrom < 0f && vTo > 0f)
            {
                // xMin is on the left
                float yDividerXpos = plotPosition.xMin + (plotPosition.width * Mathf.InverseLerp(vFrom, vTo, 0f));
                DrawLine(new Vector2(yDividerXpos, plotPosition.yMin), new Vector2(yDividerXpos, plotPosition.yMax), 2, new Color(0.6f, 0.6f, 0.6f, 0.2f));
            }
            // X divider
            if (localMinimum < 0f && localMaximum > 0f)
            {
                // yMax is on the bottom
                float xDividerYpos = plotPosition.yMax - (plotPosition.height * Mathf.InverseLerp(localMinimum, localMaximum, 0f));
                DrawLine(new Vector2(plotPosition.xMin, xDividerYpos), new Vector2(plotPosition.xMax, xDividerYpos), 2, new Color(0.6f, 0.6f, 0.6f, 0.2f));
            }

            Vector2 previousPosition = new Vector2(
                plotPosition.xMin,
                // Initial plot position
                plotPosition.y + (plotPosition.height * Mathf.InverseLerp(localMaximum, localMinimum, plotFunction(vFrom)))
            );
            for (int i = 1; i < segments + 1; i++)
            {
                float currentSegmentElapsed = (float)i / segments;
                float lerpValue = Mathf.Lerp(vFrom, vTo, currentSegmentElapsed);
                float plotValue = plotFunction(lerpValue);

                float currentX = plotPosition.x + (currentSegmentElapsed * plotPosition.width);
                // 'y' is inverted in GUI
                // Closer to maximum, the less should be the added height
                float currentY = plotPosition.y + (plotPosition.height * Mathf.InverseLerp(localMaximum, localMinimum, plotValue));

                DrawLine(previousPosition, new Vector2(currentX, currentY), lineWidth, GUI.color);

                previousPosition = new Vector2(currentX, currentY);
            }
        }
        /// <inheritdoc cref="PlotLine(Rect, Func{float, float}, bool, bool, float, float, float, int)"/>
        public static void PlotLine(Rect position, Func<float, float> plotFunction, float vFrom = 0f, float vTo = 1f, float lineWidth = 2.5f, int segments = 20)
        {
            PlotLine(position, plotFunction, true, true, vFrom, vTo, lineWidth, segments);
        }

        private const float PlotLineLayoutedHeight = 48;
        private const float PlotLineLayoutedMinWidth = 60;
        /// <summary>
        /// A layouted version of <see cref="PlotLine(Rect, Func{float, float}, bool, bool, float, float, float, int)"/>.
        /// <br>Reserves a rectangle on the <see cref="GUILayout"/> with a height of <see cref="PlotLineLayoutedHeight"/>, can be overriden.</br>
        /// <br/>
        /// <br>Documentation for original 'PlotLine' : </br>
        /// <inheritdoc cref="PlotLine(Rect, Func{float, float}, bool, bool, float, float, float, int)"/>
        /// </summary>
        /// <param name="plotFunction">The plot function that returns rational numbers and is linear. (no self intersections, double values in one value or anything)</param>
        /// <param name="vFrom">The first value to feed the plot function while linearly interpolating.</param>
        /// <param name="vTo">The last value to feed the plot function while linearly interpolating.</param>
        /// <param name="segments">Amount of times that the <see cref="DrawLine(Vector2, Vector2, int)"/> will be called. This should be a value larger than 1</param>
        public static void PlotLineLayout(Func<float, float> plotFunction, bool showFromToLabels, bool showMinMaxLabels, float vFrom = 0f, float vTo = 1f, float lineWidth = 2.5f, int segments = 20, params GUILayoutOption[] options)
        {
            // get reserved rect
            Rect reservedRect = GetOptionalGUILayoutRect(PlotLineLayoutedMinWidth, float.MaxValue, PlotLineLayoutedHeight, PlotLineLayoutedHeight, options);
            // some padding
            reservedRect.x += 2f;
            reservedRect.width -= 4f;
            reservedRect.y += 2f;
            reservedRect.height -= 4f;

            PlotLine(reservedRect, plotFunction, showFromToLabels, showMinMaxLabels, vFrom, vTo, lineWidth, segments);
        }

        /// <summary>
        /// <br>This version always shows the from-to labels and the min-max labels.</br>
        /// <br/>
        /// <inheritdoc cref="PlotLineLayout(Func{float, float}, bool, bool, float, float, float, int, GUILayoutOption[])"/>
        /// </summary>
        /// <inheritdoc cref="PlotLineLayout(Func{float, float}, bool, bool, float, float, float, int, GUILayoutOption[])"/>
        public static void PlotLineLayout(Func<float, float> plotFunction, float vFrom = 0f, float vTo = 1f, float lineWidth = 2.5f, int segments = 20, params GUILayoutOption[] options)
        {
            PlotLineLayout(plotFunction, true, true, vFrom, vTo, lineWidth, segments, options);
        }

        /// <summary>
        /// Draws a ui line and returns the padded position rect.
        /// <br>For angled / rotated lines, use the <see cref="DrawLine(Vector2, Vector2, int)"/> method. (uses GUI position)</br>
        /// </summary>
        /// <param name="parentRect">Parent rect to draw relative to.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thiccness of the line.</param>
        /// <param name="padding">Padding of the line. (Space left for the line, between properties)</param>
        /// <returns>The new position target rect, offseted.</returns>
        public static Rect DrawUILine(Rect parentRect, Color color, int thickness = 2, int padding = 3)
        {
            // Rect that is passed as an parameter.
            Rect drawRect = new Rect(parentRect.position, new Vector2(parentRect.width, thickness));

            drawRect.y += padding / 2;
            drawRect.x -= 2;
            drawRect.width += 6;

            // Rect with proper height.
            Rect returnRect = new Rect(new Vector2(parentRect.position.x, drawRect.position.y + (thickness + padding)), parentRect.size);
            if (Event.current.type == EventType.Repaint)
            {
                var gColor = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(drawRect, Texture2D.whiteTexture);
                GUI.color = gColor;
            }

            return returnRect;
        }

        /// <summary>
        /// Draws a straight line in the gui system. (<see cref="GUILayout"/> method)
        /// <br>For angled / rotated lines, use the <see cref="DrawLine"/> method.</br>
        /// </summary>
        /// <param name="color">Color of the line.</param>
        /// <param name="thickness">Thiccness of the line.</param>
        /// <param name="padding">Padding of the line. (Space left for the line, between properties)</param>
        public static void DrawUILineLayout(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = GUILayoutUtility.GetRect(1f, float.MaxValue, padding + thickness, padding + thickness);

            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;

            if (Event.current.type == EventType.Repaint)
            {
                var gColor = GUI.color;
                GUI.color *= color;
                GUI.DrawTexture(r, Texture2D.whiteTexture);
                GUI.color = gColor;
            }
        }

        #region RenderTexture Based
        private static readonly RenderTexture tempRT = new RenderTexture(1, 1, 1, RenderTextureFormat.ARGB32);
        /// <summary>
        /// An unlit material with color.
        /// </summary>
        private static readonly Material tempUnlitMat = new Material(Shader.Find("Custom/Unlit/UnlitTransparentColorShader"));
        /// <summary>
        /// Circle material, with customizable parameters.
        /// </summary>
        private static readonly Material tempCircleMat = new Material(Shader.Find("Custom/Vector/Circle"));

        /// <summary>
        /// Get a texture of a circle.
        /// </summary>
        public static Texture GetCircleTexture(Vector2 size, Color circleColor, Color strokeColor = default, float strokeThickness = 0f)
        {
            tempCircleMat.color = circleColor;
            tempCircleMat.SetColor("_StrokeColor", strokeColor);
            tempCircleMat.SetFloat("_StrokeThickness", strokeThickness);

            return BlitQuad(size, tempCircleMat);
        }

        /// <summary>
        /// Get a texture of a rendered mesh.
        /// </summary>
        public static Texture GetMeshTexture(Vector2 size, Mesh meshTarget, Material meshMat,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            // No need for a 'BlitMesh' method for this, as it's already complicated

            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(size.x);
            tempRT.height = Mathf.CeilToInt(size.y);
            Matrix4x4 matrixMesh = Matrix4x4.TRS(meshPos, meshRot, meshScale);

            // This is indeed redundant code, BUT: we also apply transform data to camera also so this is required
            if (camProj == Matrix4x4.identity)
            {
                camProj = Matrix4x4.Ortho(-1, 1, -1, 1, 0.01f, 1024f);
            }

            Matrix4x4 matrixCamPos = Matrix4x4.TRS(camPos, camRot, new Vector3(1, 1, -1));
            Matrix4x4 matrixCam = (camProj * matrixCamPos.inverse);

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, meshTarget, meshMat);
            return tempRT;
        }

        /// <summary>
        /// Get a texture of a quad, rendered using that material.
        /// <br>It is not recommended to use this method as shaders could be moving.
        /// Use the <see cref="DrawMaterialTexture(Rect, Texture2D, Material)"/> instead.</br>
        /// </summary>
        public static Texture GetMaterialTexture(Vector2 size, Texture2D texTarget, Material matTarget)
        {
            matTarget.mainTexture = texTarget;

            return BlitQuad(size, matTarget);
        }

        /// <summary>
        /// Utility method to bilt a quad (to variable <see cref="tempRT"/>)
        /// </summary>
        internal static RenderTexture BlitQuad(Vector2 size, Material matTarget)
        {
            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(size.x);
            tempRT.height = Mathf.CeilToInt(size.y);

            // Stretch quad (to fit into texture)
            Vector3 scale = new Vector3(1f, 1f * tempRT.Aspect(), 1f);
            // the quad that we get using GetQuad is offsetted.
            Matrix4x4 matrixMesh = Matrix4x4.TRS(new Vector3(0.5f, -0.5f, -1f), Quaternion.AngleAxis(-180, Vector3.up), scale);
            //Matrix4x4 matrixCam = Matrix4x4.Ortho(-1, 1, -1, 1, .01f, 1024f);
            Matrix4x4 matrixCam = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, .01f, 1024f);

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, RenderTextureUtility.GetQuad(), matTarget);

            return tempRT;
        }

        /// <summary>
        /// Internal utility method to draw quads (with materials).
        /// </summary>
        private static void DrawQuad(Rect guiRect, Material matTarget)
        {
            GUI.DrawTexture(guiRect, BlitQuad(guiRect.size, matTarget));
        }

        /// <summary>
        /// Draws a circle at given rect.
        /// </summary>
        public static void DrawCircle(Rect guiRect, Color circleColor, Color strokeColor = default, float strokeThickness = 0f)
        {
            tempCircleMat.color = circleColor;
            tempCircleMat.SetColor("_StrokeColor", strokeColor);
            tempCircleMat.SetFloat("_StrokeThickness", strokeThickness);

            DrawQuad(guiRect, tempCircleMat);
        }

        /// <summary>
        /// Draws a texture with material.
        /// </summary>
        /// <param name="guiRect"></param>
        /// <param name="texTarget"></param>
        /// <param name="matTarget"></param>
        public static void DrawMaterialTexture(Rect guiRect, Texture2D texTarget, Material matTarget)
        {
            matTarget.mainTexture = texTarget;

            DrawQuad(guiRect, matTarget);
        }

        /// <summary>
        /// Draws a textured white mesh.
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Texture2D meshTexture,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            DrawMesh(guiRect, meshTarget, meshTexture, Color.white, meshPos, meshRot, meshScale, camPos, camRot, camProj);
        }

        /// <summary>
        /// Draws a mesh on the given rect.
        /// <br>Uses a default unlit material for the material field.</br>
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Color meshColor,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            DrawMesh(guiRect, meshTarget, null, meshColor, meshPos, meshRot, meshScale, camPos, camRot, camProj);
        }

        /// <summary>
        /// Draws a textured mesh with a color of your choice.
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Texture2D meshTexture, Color meshColor,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            var mcPrev = tempUnlitMat.color;
            tempUnlitMat.mainTexture = meshTexture;
            tempUnlitMat.color = meshColor;

            DrawMesh(guiRect, meshTarget, tempUnlitMat, meshPos, meshRot, meshScale, camPos, camRot, camProj);

            tempUnlitMat.color = mcPrev;
        }

        /// <summary>
        /// Draws a mesh on the given rect.
        /// <br>The mesh is centered to camera, however the position, rotation and the scale of the mesh can be changed.</br>
        /// <br>The target resolution is the size of the <paramref name="guiRect"/>.</br>
        /// </summary>
        public static void DrawMesh(Rect guiRect, Mesh meshTarget, Material meshMat,
            Vector3 meshPos, Quaternion meshRot, Vector3 meshScale,
            Vector3 camPos = default, Quaternion camRot = default, Matrix4x4 camProj = default)
        {
            tempRT.Release();
            tempRT.width = Mathf.CeilToInt(guiRect.size.x);
            tempRT.height = Mathf.CeilToInt(guiRect.size.y);
            Matrix4x4 matrixMesh = Matrix4x4.TRS(meshPos, meshRot, meshScale);

            // This is indeed redundant code, BUT: we also apply transform data to camera also so this is required
            if (camProj == Matrix4x4.identity)
            {
                camProj = Matrix4x4.Ortho(-0.5f, 0.5f, -0.5f, 0.5f, .01f, 1024f);
            }

            Matrix4x4 matrixCamPos = Matrix4x4.TRS(camPos, camRot, new Vector3(1, 1, -1));
            Matrix4x4 matrixCam = camProj * matrixCamPos.inverse;

            // Draw mesh manually
            tempRT.BlitMesh(matrixMesh, matrixCam, meshTarget, meshMat);

            GUI.DrawTexture(guiRect, tempRT);
        }
        #endregion
    }
}