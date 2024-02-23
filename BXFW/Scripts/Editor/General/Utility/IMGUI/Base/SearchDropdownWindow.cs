using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Editor window that handles the dropdown.
    /// <br>This dropdown does async searching and recycled scroll rects, and is more optimized than the <see cref="UnityEditor.IMGUI.Controls.AdvancedDropdownGUI"/>.</br>
    /// </summary>
    public sealed class SearchDropdownWindow : EditorWindow
    {
        // -- GUI
        /// <summary>
        /// Contains the drawing style list.
        /// </summary>
        public static class StyleList
        {
            /// <summary>
            /// A style that is an arrow pointing right.
            /// </summary>
            public static GUIStyle RightArrowStyle = "ArrowNavigationRight";
            /// <summary>
            /// A style that is an arrow pointing left.
            /// </summary>
            public static GUIStyle LeftArrowStyle = "ArrowNavigationLeft";
            /// <summary>
            /// The style used to draw a label.
            /// </summary>
            public static GUIStyle LabelStyle = GUI.skin.label;
            /// <summary>
            /// The style used to draw a centered label.
            /// </summary>
            public static GUIStyle CenteredLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            /// <summary>
            /// A GUIStyle that uses word wrapping.
            /// </summary>
            public static GUIStyle WrapLabelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
            /// <summary>
            /// A GUIStyle that uses word wrapping and centering.
            /// </summary>
            public static GUIStyle CenteredWrapLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };
            /// <summary>
            /// The style used to draw a searching field.
            /// </summary>
            public static GUIStyle SearchBarStyle = new GUIStyle(EditorStyles.toolbarSearchField)
            {
                // this padding is necessary to make it actually look good
                margin = new RectOffset(5, 4, 4, 5)
            };
            /// <summary>
            /// Background color used to draw the SearchBar.
            /// </summary>
            public static Color SearchBarBackgroundColor = new Color(0.27f, 0.27f, 0.27f);
            /// <summary>
            /// Background color used to draw the NameBar.
            /// </summary>
            public static Color NameBarBackgroundColor = new Color(0.16f, 0.16f, 0.16f);
            /// <summary>
            /// Default string used for a checkmark.
            /// <br>To change this, use <see cref="CheckMarkContent"/>.</br>
            /// </summary>
            public const string CheckMarkString = "✔";
            /// <summary>
            /// Content used to display the check mark string.
            /// </summary>
            public static GUIContent CheckMarkContent = new GUIContent(CheckMarkString);

            /// <summary>
            /// Manages a rich text scope for the text styles.
            /// </summary>
            public struct RichTextScope : IDisposable
            {
                private readonly Dictionary<GUIStyle, bool> m_PreviousRichTextStates;

                /// <summary>
                /// Creates a new rich text scope.
                /// </summary>
                /// <param name="isRichText">Set whether if this GUI drawing scope contains rich texts.</param>
                public RichTextScope(bool isRichText)
                {
                    m_PreviousRichTextStates = new Dictionary<GUIStyle, bool>()
                    {
                        { LabelStyle, LabelStyle.richText },
                        { CenteredLabelStyle, CenteredLabelStyle.richText },
                        { WrapLabelStyle, WrapLabelStyle.richText }
                    };

                    foreach (GUIStyle style in m_PreviousRichTextStates.Keys)
                    {
                        style.richText = isRichText;
                    }
                }

                public void Dispose()
                {
                    // States are not saved
                    if (m_PreviousRichTextStates == null)
                    {
                        return;
                    }

                    // Revert states
                    foreach (KeyValuePair<GUIStyle, bool> pair in m_PreviousRichTextStates)
                    {
                        pair.Key.richText = pair.Value;
                    }
                }
            }
        }

        private const float SearchBarHeight = 18f;
        private const float SearchBarPadding = 30f;
        private const float ElementNameBarHeight = 22f;
        private const float ElementNameBarPadding = 15f;
        private const float CheckmarkIconWidth = 15f;
        public const string SearchBarControlName = "SearchDropdownWindowBar";
        /// <summary>
        /// Width of the element lists view.
        /// </summary>
        private float ElementsViewWidth => position.width - (GUI.skin.verticalScrollbar.fixedWidth * 1.25f);

        // -- State
        /// <summary>
        /// Stack of the traversed elements trees/linked lists.
        /// </summary>
        private readonly Stack<SearchDropdownElement> m_ElementStack = new Stack<SearchDropdownElement>(16);
        /// <summary>
        /// The search filter elements.
        /// <br>This list is cleared once searching is done.</br>
        /// </summary>
        private readonly SearchDropdownElement searchFilteredElements = new SearchDropdownElement("Results", 128);
        /// <summary>
        /// Task dispatched for searching the 'searchString'.
        /// </summary>
        private Task searchingTask;
        /// <summary>
        /// Cancellation token used for the <see cref="searchingTask"/>.
        /// </summary>
        private CancellationTokenSource searchingTaskCancellationSource = new CancellationTokenSource();
        /// <inheritdoc cref="SearchString"/>
        private string m_SearchString;
        /// <summary>
        /// State of whether if the searching task reached limit.
        /// </summary>
        private bool reachedSearchLimit = false;
        /// <summary>
        /// Whether if this window is closing with a selection intent.
        /// </summary>
        public bool IsClosingWithSelectionIntent { get; private set; } = true;
        /// <summary>
        /// The used element's size on the last <see cref="EventType.Layout"/> on the <see cref="OnGUI"/> call.
        /// <br>This is used in the <see cref="EventType.Repaint"/> event.</br>
        /// <br/>
        /// <br>This is needed as asynchronous/seperate thread searching will add/remove values in different <see cref="OnGUI"/> calls.</br>
        /// </summary>
        private int lastLayoutElementsCount = -1;
        /// <summary>
        /// The current string query to apply for the searching filtering.
        /// </summary>
        public string SearchString
        {
            get
            {
                return m_SearchString;
            }
            set
            {
                if (m_SearchString == value)
                {
                    return;
                }

                // Reset state
                reachedSearchLimit = false;
                lastLayoutElementsCount = 0;

                // Set values
                m_SearchString = value;

                // Dispatch searching event
                searchingTaskCancellationSource.Cancel();
                searchingTaskCancellationSource.Dispose();
                searchingTaskCancellationSource = new CancellationTokenSource();

                searchFilteredElements.Clear();

                if (string.IsNullOrWhiteSpace(m_SearchString))
                {
                    return;
                }

                // Set filteredElements (async?)
                searchingTask = new Task(() =>
                {
                    SearchResultsRecursive(parentManager.RootElement);
                }, searchingTaskCancellationSource.Token);
                searchingTask.Start();
            }
        }
        /// <summary>
        /// Recursively searches and adds results to <see cref="searchFilteredElements"/>.
        /// <br>Added elements won't contain children.</br>
        /// </summary>
        private void SearchResultsRecursive(SearchDropdownElement element)
        {
            // TODO : Insert and sort according to the 'm_SearchString's IndexOf presence.
            foreach (SearchDropdownElement child in element)
            {
                // This will set the 'TaskStatus' to 'TaskStatus.RanToCompletion'
                if (searchingTaskCancellationSource.IsCancellationRequested)
                {
                    return;
                }
                // Place this here to be have it constantly checked
                if (parentManager.SearchElementsResultLimit > 0 && searchFilteredElements.Count >= parentManager.SearchElementsResultLimit)
                {
                    searchingTaskCancellationSource.Cancel();
                    reachedSearchLimit = true;
                    return;
                }

                if (child.HasChildren)
                {
                    SearchResultsRecursive(child);
                }
                else if (child.content.text.IndexOf(m_SearchString, parentManager.SearchComparison) != -1)
                {
                    searchFilteredElements.Add(child);
                }
            }
        }
        private Vector2 scrollRectPosition = Vector2.zero;
        /// <summary>
        /// Index of the currently highlighted element.
        /// <br>Used with the arrow key navigations / other things.</br>
        /// </summary>
        private int highlightedElementIndex = -1;
        /// <summary>
        /// Index of the first visible element.
        /// <br>Calculated inside <see cref="DrawElement(SearchDropdownElement)"/>.</br>
        /// </summary>
        private int firstVisibleElementIndex = -1;
        /// <summary>
        /// The event before any draw calls from the <see cref="OnGUI"/>.
        /// </summary>
        private Event beforeDrawEvent;

        // --
        /// <summary>
        /// A manager used for it's settings.
        /// </summary>
        private SearchDropdown parentManager;
        /// <summary>
        /// Called when the window is to be closed.
        /// </summary>
        public event Action OnClosed;

        /// <summary>
        /// Creates a new SearchDropdownWindow.
        /// </summary>
        /// <param name="parentRect">The rect of the button that triggered the dropdown.</param>
        /// <param name="parentManager">The SearchDropdown settigns manager itself.</param>
        /// <returns>The created window.</returns>
        /// <exception cref="ArgumentNullException"/>
        public static SearchDropdownWindow Create(Rect parentRect, SearchDropdown parentManager, Vector2 minSize, float maxHeight)
        {
            if (parentManager == null)
            {
                throw new ArgumentNullException(nameof(parentManager), "[OptimizedSearchDropdownWindow::Create] Given argument was null");
            }

            // Convert parent rect to screen space
            parentRect = GUIUtility.GUIToScreenRect(parentRect);

            // Create window without 'CreateWindow' as that adds / injects titlebar GUI
            // Which i don't want. It is only useful when IMGUI debugger is needed
            // SearchDropdownWindow window = CreateWindow<SearchDropdownWindow>();
            SearchDropdownWindow window = CreateInstance<SearchDropdownWindow>();
            window.parentManager = parentManager;
            window.wantsMouseMove = true;
            window.IsClosingWithSelectionIntent = !window.parentManager.AllowSelectionOfElementsWithChild;

            // Show with size constraints.
            // - Calculate the height (for the time being use this, can use 'GetElementsHeight'
            // after idk when, the most permanent solution is a tempoary one)
            float elementsHeight = Mathf.Max(window.parentManager.RootElement.Count * (EditorGUIUtility.singleLineHeight + 2f), 64f);
            float height = elementsHeight + SearchBarHeight + ElementNameBarHeight;

            // - Get the size constraints here as 'ShowAsDropDown' needs those.
            Vector2 dropdownSize = new Vector2(parentRect.width, height);

            if (maxHeight > 0f)
            {
                dropdownSize.y = Mathf.Min(dropdownSize.y, maxHeight);

                // Print a warning message if the height constraint is too dumb
                if (maxHeight <= 1f)
                {
                    Debug.LogWarning("[SearchDropdownWindow::Create] Given 'MaximumHeight' constraint is less (or equal) to 1. This could have been done unintentionally.");
                }
            }
            if (minSize.x > 0f)
            {
                dropdownSize.x = Mathf.Max(dropdownSize.x, minSize.x);
            }
            if (minSize.y > 0f)
            {
                dropdownSize.y = Mathf.Max(dropdownSize.y, minSize.y);
            }

            // - Actual show, this does what we want
            window.ShowAsDropDown(parentRect, dropdownSize);

            // Setup the window
            // m_ElementStack should always have 1 element
            window.m_ElementStack.Push(parentManager.RootElement);

            return window;
        }

        // The optimized version is still TODO.
        /// <summary>
        /// An AABB collision check.
        /// <br>If the rects are colliding this will return true.</br>
        /// </summary>
        /// <param name="lhs">First rect to check</param>
        /// <param name="rhs">Second rect to check</param>
        private bool AreRectsColliding(Rect lhs, Rect rhs)
        {
            return lhs.x < (rhs.x + rhs.width) &&
                (lhs.x + lhs.width) > rhs.x &&
                lhs.y < (rhs.y + rhs.height) &&
                (lhs.y + lhs.height) > rhs.y;
        }

        private void OnGUI()
        {
            if (parentManager == null)
            {
                Close();
                return;
            }

            // This idiot (EditorGUI.TextField) calls 'Use()' the global event
            // So copy it for the 'HandleGlobalGUIEvents'
            beforeDrawEvent = new Event(Event.current);
            if (parentManager.IsSearchable)
            {
                DrawSearchBar();
            }

            // Don't let the exception stop the execution
            // So only log the exception and reset the drawing settings
            SearchDropdownElement lastElement = string.IsNullOrEmpty(SearchString) ? GetLastSelected() : searchFilteredElements;
            using (StyleList.RichTextScope scope = new StyleList.RichTextScope(parentManager.AllowRichText))
            {
                try
                {
                    DrawElementNameBar(lastElement);
                    // Draw the search filter if there's a query
                    DrawElement(lastElement);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            HandleGlobalGUIEvents(lastElement);
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndoRedo;
        }
        private void OnDisable()
        {
            OnClosed?.Invoke();
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        private void DrawSearchBar()
        {
            Rect searchBarRect = GUILayoutUtility.GetRect(position.width, SearchBarHeight);
            // Draw a centered + padded search bar.
            EditorGUI.DrawRect(searchBarRect, StyleList.SearchBarBackgroundColor);

            Rect paddedSearchBarRect = new Rect()
            {
                x = searchBarRect.x + SearchBarPadding,
                y = searchBarRect.y + 1f,
                height = SearchBarHeight - 2f,
                width = searchBarRect.width - (SearchBarPadding * 2f)
            };

            GUI.SetNextControlName(SearchBarControlName);
            SearchString = EditorGUI.TextField(paddedSearchBarRect, SearchString, StyleList.SearchBarStyle);
        }
        private void DrawElementNameBar(SearchDropdownElement lastElement)
        {
            // ElementNameBar will contain a clickable back arrow and the name of the selected
            // The name of the selected may or may not get truncated, so please don't make your names that long
            Rect elementBarRect = GUILayoutUtility.GetRect(position.width, ElementNameBarHeight);

            // Background
            EditorGUI.DrawRect(elementBarRect, StyleList.NameBarBackgroundColor);

            // Back button
            // Only show 'lastElement' back button if 'lastElement' is not root element
            // if we are on the filtered elements, show only if it's a searchable window
            if (lastElement != parentManager.RootElement && (lastElement != searchFilteredElements || parentManager.IsSearchable))
            {
                if (GUI.Button(new Rect()
                {
                    x = elementBarRect.x + ElementNameBarPadding,
                    y = elementBarRect.y + ((elementBarRect.height - StyleList.LeftArrowStyle.fixedHeight) / 2f),
                    width = StyleList.LeftArrowStyle.fixedWidth,
                    height = StyleList.LeftArrowStyle.fixedHeight,
                }, GUIContent.none, StyleList.LeftArrowStyle))
                {
                    // depending on which element is the 'lastElement', either pop the stack or clear the search query
                    if (lastElement == searchFilteredElements)
                    {
                        SearchString = string.Empty;
                        EditorGUIUtility.editingTextField = false;
                    }
                    else
                    {
                        m_ElementStack.Pop();
                    }
                }
            }

            // Name display + Count
            EditorGUI.LabelField(new Rect(elementBarRect)
            {
                x = elementBarRect.x + ElementNameBarPadding + StyleList.LeftArrowStyle.fixedWidth + 5f,
                width = elementBarRect.width - ((ElementNameBarPadding * 2f) + StyleList.LeftArrowStyle.fixedWidth + 5f)
            }, parentManager.DisplayCurrentElementsCount ? $"{lastElement.content.text} | {lastElement.Count}" : lastElement.content.text, StyleList.CenteredLabelStyle);
        }

        private readonly PropertyRectContext elementCtx = new PropertyRectContext(0f);
        /// <summary>
        /// Draws the current element tree/node.
        /// <br>Also handles selection events for the elements.</br>
        /// </summary>
        /// <param name="element">Element to draw it's children.</param>
        private void DrawElement(SearchDropdownElement element)
        {
            // TODO (features) : 
            // * Add keyboard nav                         [ X ]
            // * Add visual interactions                  [ X ]
            // TODO (optimizations) : 
            // * General optimization to be done (such as accumulating up the rect heights)                                          [   ]
            // * Search results can contain elements with children (requires a seperate elements stack/slight restructuring of code) [   ]

            Event e = Event.current;
            elementCtx.Reset();

            if (e.type == EventType.Layout)
            {
                lastLayoutElementsCount = element.Count;
            }

            int elementSize = Mathf.Min(element.Count, lastLayoutElementsCount);

            // Draw no elements thing if no elements
            // We can return here as no elements to draw.
            if (elementSize <= 0)
            {
                // Depending on what are we drawing, show a different text
                if (element == searchFilteredElements)
                {
                    EditorGUILayout.LabelField(string.Format(parentManager.NoSearchResultsText, SearchString), StyleList.CenteredWrapLabelStyle);
                    return;
                }

                EditorGUILayout.LabelField(parentManager.NoElementPlaceholderText, StyleList.CenteredWrapLabelStyle);
                return;
            }

            scrollRectPosition = GUILayout.BeginScrollView(scrollRectPosition, false, true);
            // Draw ONLY the visible elements
            // Get an index range for the drawing and only refresh that if needed
            // For the top and the start, draw boxes that are very large minus the sizes of drawn
            // --

            // Inflate the bounds / visibility checking size a bit
            Rect localWindowAreaPosition = new Rect(scrollRectPosition.x, scrollRectPosition.y, position.width + 100f, position.height + 10f);

            // Begin a new area to not have horizontal scroll bars
            // Since the scroll view is handled by rect calculations
            GUILayout.BeginVertical(GUILayout.Width(ElementsViewWidth));

            // Set this visible element index dirty
            firstVisibleElementIndex = -1;

            // FIXME : Unoptimized ver
            // Reason : pushed rect quantity is too large
            // all element's heights are checked, which makes this o(n)
            // --
            // Wait, what? This is actually faster than the AdvancedDropdown
            // Okay, unity does cull non-rendered GUI stuff, but i think the AdvancedDropdown always calls GUI.Draw on all elements
            // Which causes immense lagging.
            // --
            // And the 'AdvancedDropdown' allocation was also very wasteful, we couldn't define an array size.
            for (int i = 0; i < elementSize; i++)
            {
                SearchDropdownElement child = element[i];

                // This can happen on some cases 
                // Such as while searching on a seperate thread
                if (child == null)
                {
                    continue;
                }

                // -- Draw GUI
                // The 'Getting control X's position in a group with only X controls'
                // error occurs due to the inconsistent amount of rects pushed on different events
                // To fix this, get the results in a thread and event safe way?
                // --
                // GUILayoutUtility is bad and will always reserve the width as maximum
                // Which causes the scroll box to overflow a bit
                // So, what do? Begin an area? or something else?
                // Yes, but use 'BeginVertical'
                Rect reservedRect = GUILayoutUtility.GetRect(0f, child.GetHeight(ElementsViewWidth));

                // Check if the rect is visible in any way
                if (!AreRectsColliding(localWindowAreaPosition, reservedRect))
                {
                    continue;
                }
                // Get this value to help with performance for other methods
                if (firstVisibleElementIndex < 0)
                {
                    firstVisibleElementIndex = i;
                }

                ElementGUIDrawingState state = ElementGUIDrawingState.Default;
                bool currentElementIsHighlighted = highlightedElementIndex == i;

                if (child.Selected)
                {
                    state = ElementGUIDrawingState.Selected;
                }
                if (currentElementIsHighlighted)
                {
                    state = ElementGUIDrawingState.Hover;
                }

                // It is safe to use the 'MouseMove' event in this window
                if (child.Interactable && e.type == EventType.MouseMove)
                {
                    if (reservedRect.Contains(e.mousePosition))
                    {
                        // Mouse highlighting
                        highlightedElementIndex = i;

                        if (e.isMouse && e.button == 0 && e.type == EventType.MouseDown)
                        {
                            state = ElementGUIDrawingState.Clicked;
                        }
                        else
                        {
                            state = ElementGUIDrawingState.Hover;
                        }
                    }
                    else if (highlightedElementIndex == i)
                    {
                        // Assume that the mouse exited the button
                        highlightedElementIndex = -1;
                    }
                }

                child.OnGUI(reservedRect, state);
                if (child.RequestsRepaint)
                {
                    Repaint();
                    child.RequestsRepaint = false;
                }

                // Checkmark / More elements icon
                Rect sideMarkRect = new Rect(reservedRect)
                {
                    x = reservedRect.x + reservedRect.width - CheckmarkIconWidth,
                    width = CheckmarkIconWidth
                };
                if (!child.HasChildren)
                {
                    if (child.Selected)
                    {
                        // Instead of just simply creating a 'GUIStyle(GUI.skin.label)' with centered text alignment i did this
                        // Eh it's faster if we are going for optimization i guess
                        float checkmarkHeight = GUI.skin.label.CalcHeight(StyleList.CheckMarkContent, sideMarkRect.width);
                        // Center inside rect
                        sideMarkRect.y += (sideMarkRect.height - checkmarkHeight) / 2f;
                        sideMarkRect.height = checkmarkHeight;
                        GUI.Label(sideMarkRect, StyleList.CheckMarkContent);
                    }
                }
                else
                {
                    if (e.type == EventType.Repaint)
                    {
                        // Center this style as well, it's not centered
                        float rightArrowHeight = StyleList.RightArrowStyle.CalcHeight(GUIContent.none, sideMarkRect.width);
                        sideMarkRect.y += (sideMarkRect.height - rightArrowHeight) / 2f;
                        sideMarkRect.height = rightArrowHeight;
                        StyleList.RightArrowStyle.Draw(sideMarkRect, GUIContent.none, false, false, false, false);
                    }
                }

                // -- Interact / Process GUI
                if (child.Interactable)
                {
                    if (reservedRect.Contains(e.mousePosition) && e.type == EventType.MouseUp && e.button == 0)
                    {
                        // Push into the top of stack and stop drawing the rest
                        // If the element has no more child, do a selected event + close
                        m_ElementStack.Push(child);
                        // Select the first element
                        // (note that this index is safe to set into any valid/invalid values)
                        // (but any invalid values are ignored completely)
                        highlightedElementIndex = 0;

                        if (!child.HasChildren)
                        {
                            IsClosingWithSelectionIntent = true;
                            Close();
                        }

                        e.Use();
                        break;
                    }
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // Only allow selection if we aren't displaying the 'filtered elements'
            if (parentManager.AllowSelectionOfElementsWithChild && element != parentManager.RootElement && element != searchFilteredElements)
            {
                if (GUILayout.Button("Select Element"))
                {
                    // Select the last element in the stack.
                    IsClosingWithSelectionIntent = true;
                    Close();
                }
            }

            // If there's a searching process, show an HelpBox
            if (searchingTask != null && !searchingTask.IsCompleted)
            {
                EditorGUILayout.HelpBox($"Searching \"{SearchString}\"...", MessageType.Info);
            }
            if (reachedSearchLimit)
            {
                EditorGUILayout.HelpBox($"Reached the search limit of {parentManager.SearchElementsResultLimit}.\nMake your search query more accurate.", MessageType.Info);
            }
        }

        /// <summary>
        /// Get how much to move <see cref="scrollRectPosition"/>.y to make the <see cref="highlightedElementIndex"/> element visible.
        /// </summary>
        /// <param name="lastElement">The parent element that the children of it that is visible.</param>
        /// <param name="deltaSign">Sign of the delta to check for. This makes the delta behaviour different depending on the given sign.</param>
        private float GetHighlightedElementHeightDelta(SearchDropdownElement lastElement, int deltaSign)
        {
            // Yes, there could be a better way of doing this.
            // But this works
            if (deltaSign > 0)
            {
                // Calculate heights before the currently highlighted element
                // Add the highlighted elements height as well.
                float visibleWindowAreaHeight = position.height - ElementNameBarHeight;
                if (parentManager.IsSearchable)
                {
                    visibleWindowAreaHeight -= SearchBarHeight;
                }

                float visibleUntilHighlightedHeight = 0f;
                for (int i = firstVisibleElementIndex; i <= highlightedElementIndex; i++)
                {
                    // Check the current scroll rect position, if it cannot accomodate the 'nextElement' scroll as much as 'targetElementHeight'.
                    SearchDropdownElement visibleNextElement = lastElement[Mathf.Clamp(i, 0, lastElement.Count - 1)];
                    visibleUntilHighlightedHeight += visibleNextElement.GetHeight(ElementsViewWidth);
                }

                // This returns a negative value without if (visibleHeightUntilHighlighted > visibleWindowAreaHeight)
                return Mathf.Max(0, visibleUntilHighlightedHeight - visibleWindowAreaHeight);
            }
            if (deltaSign < 0)
            {
                // Calculate the height difference until the 'highlightedElementIndex' from to the 'firstVisibleElementIndex' in here
                float negativeUntilVisibleDeltaHeight = 0f;

                for (int i = highlightedElementIndex; i < firstVisibleElementIndex; i++)
                {
                    SearchDropdownElement visibleNextElement = lastElement[i];
                    negativeUntilVisibleDeltaHeight -= visibleNextElement.GetHeight(ElementsViewWidth);
                }

                return negativeUntilVisibleDeltaHeight;
            }

            return 0;
        }
        /// <summary>
        /// Handles global dropdown events. (such as keyboard events)
        /// <br>Has to be called from <see cref="OnGUI"/>.</br>
        /// </summary>
        private void HandleGlobalGUIEvents(SearchDropdownElement lastElement)
        {
            GUIAdditionals.CheckOnGUI();

            // Get current unused event
            Event e = beforeDrawEvent;

            // EventType.MouseMove doesn't repaint by itself
            if (e.type == EventType.MouseMove)
            {
                Repaint();
            }

            // Handle keyboard events
            int previousHighlightedIndex = highlightedElementIndex;

            switch (e.type)
            {
                case EventType.KeyDown:
                    // Handle function keys
                    switch (e.keyCode)
                    {
                        case KeyCode.Escape:
                            // Pressing Escape while searching something should just clear the search query
                            // Do this double clear only in 'IsSearchable' parent managers,
                            // escape will close the window (on not 'IsSearchable') regardless of whether having a SearchString query or not.
                            if (!string.IsNullOrEmpty(SearchString) && parentManager.IsSearchable)
                            {
                                EditorGUIUtility.editingTextField = false;
                                SearchString = string.Empty;
                                Repaint();
                            }
                            else
                            {
                                IsClosingWithSelectionIntent = false;
                                Close();
                            }
                            return;

                        // Decrement/Increment highlighted element index
                        case KeyCode.UpArrow:
                            EditorGUIUtility.editingTextField = false;

                            // Iteratively select until the last valid interactable element
                            do
                            {
                                highlightedElementIndex = Mathf.Max(highlightedElementIndex - 1, 0);

                                // Cannot select last element
                                if (highlightedElementIndex == 0 && !lastElement[highlightedElementIndex].Interactable)
                                {
                                    highlightedElementIndex = previousHighlightedIndex;
                                    break;
                                }
                            }
                            while (!lastElement[highlightedElementIndex].Interactable);

                            scrollRectPosition.y += GetHighlightedElementHeightDelta(lastElement, -1);
                            Repaint();
                            break;
                        case KeyCode.DownArrow:
                            EditorGUIUtility.editingTextField = false;

                            // Iteratively select until the last valid interactable element
                            do
                            {
                                highlightedElementIndex = Mathf.Min(highlightedElementIndex + 1, lastElement.Count - 1);

                                // Cannot select last element
                                if (highlightedElementIndex == lastElement.Count - 1 && !lastElement[highlightedElementIndex].Interactable)
                                {
                                    highlightedElementIndex = previousHighlightedIndex;
                                    break;
                                }
                            }
                            while (!lastElement[highlightedElementIndex].Interactable);

                            // Check if the next element is visible, otherwise scroll it's height amount
                            // Now it's o(n) but less n's thanks to the firstVisibleElementIndex, very wholesome
                            scrollRectPosition.y += GetHighlightedElementHeightDelta(lastElement, 1);
                            Repaint();
                            break;

                        // Go back
                        case KeyCode.LeftArrow:
                            // Only go back while not editing text field.
                            if (!EditorGUIUtility.editingTextField && lastElement != parentManager.RootElement)
                            {
                                m_ElementStack.Pop();
                            }
                            break;
                        // Go forward, select if it doesn't have elements
                        case KeyCode.RightArrow:
                            if (EditorGUIUtility.editingTextField)
                            {
                                break;
                            }
                            goto case KeyCode.Return;
                        // Proceed with selection / search query
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            // Stop editing text
                            EditorGUIUtility.editingTextField = false;
                            SearchDropdownElement nextElement =
                                highlightedElementIndex >= 0 && highlightedElementIndex < lastElement.Count
                                ? lastElement[highlightedElementIndex] : lastElement.FirstOrDefault();

                            // Check if next element actually exists (or stop if we pressed the right arrow on a child without children [selection])
                            if (nextElement == null || (!nextElement.HasChildren && e.keyCode == KeyCode.RightArrow))
                            {
                                return;
                            }

                            // Select the next child into stack
                            m_ElementStack.Push(nextElement);
                            // Click on the next element (and only do this if it's not a right arrow click)
                            if (!nextElement.HasChildren)
                            {
                                IsClosingWithSelectionIntent = true;
                                Close();
                                return;
                            }
                            break;

                        default:
                            break;
                    }

                    // If the key is not a special key and is just a letter/number start searching
                    if (parentManager.IsSearchable && !EditorGUIUtility.editingTextField)
                    {
                        // TODO : FocusTextInControl does selection of the previous characters
                        // If no characters are appended then an input is missed, either way both behaviours are annoying
                        // This is still not fixed even if we ignore the used event
                        char character = e.character;
                        if (!e.functionKey && !char.IsControl(character))
                        {
                            // Get the last key as nice key
                            // EditorGUI.FocusTextInControl(SearchBarControlName);
                            GUI.FocusControl(SearchBarControlName);
                            SearchString += character;
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// Closes the window in a case of an undo or redo.
        /// </summary>
        private void HandleUndoRedo()
        {
            if (parentManager.CloseOnUndoRedoAction)
            {
                IsClosingWithSelectionIntent = false;
                Close();
            }
        }

        /// <summary>
        /// Returns the last selected element from the selection stack.
        /// <br>Ignores the root element. (If the last selected is the root element this will return null)</br>
        /// <br>To get the closing intent of this window use the <see cref="IsClosingWithSelectionIntent"/>.</br>
        /// </summary>
        public SearchDropdownElement GetSelected()
        {
            // Try getting the last element on the stack.
            if (!m_ElementStack.TryPeek(out SearchDropdownElement elem))
            {
                return null;
            }
            // Root element is ignored
            if (elem == parentManager.RootElement)
            {
                return null;
            }

            return elem;
        }
        /// <summary>
        /// Returns the last element on the selection stack.
        /// </summary>
        public SearchDropdownElement GetLastSelected()
        {
            if (!m_ElementStack.TryPeek(out SearchDropdownElement elem))
            {
                return null;
            }

            return elem;
        }
    }
}
