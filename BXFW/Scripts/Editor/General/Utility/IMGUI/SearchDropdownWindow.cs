using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Editor window that handles the dropdown.
    /// <br>This dropdown does async searching and recycled scroll rects.</br>
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
            public static Color SearchBarBackgroundColor = new Color(.4f, .4f, .4f);
            /// <summary>
            /// Background color used to draw the NameBar.
            /// </summary>
            public static Color NameBarBackgroundColor = new Color(.1f, .1f, .1f);
            /// <summary>
            /// String used for a checkmark.
            /// </summary>
            public static string CheckMarkString = "✔";
        }

        private const float SearchBarHeight = 18f;
        private const float SearchBarPadding = 30f;
        private const float ElementNameBarHeight = 22f;
        private const float ElementNameBarPadding = 15f;
        private const float CheckmarkIconWidth = 15f;

        // -- State
        /// <summary>
        /// Stack of the traversed elements trees/linked lists.
        /// </summary>
        private readonly Stack<SearchDropdownElement> m_ElementStack = new Stack<SearchDropdownElement>(16);
        /// <summary>
        /// A manager used for it's settings.
        /// </summary>
        private SearchDropdown parentManager;
        /// <summary>
        /// The current string query to apply for the searching filtering.
        /// </summary>
        public string searchString;
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
            //SearchDropdownWindow window = CreateWindow<SearchDropdownWindow>();
            SearchDropdownWindow window = CreateInstance<SearchDropdownWindow>();
            window.parentManager = parentManager;

            // Show with size constraints.
            // - Calculate the height (for the time being use this, can use 'GetElementsHeight' after this)
            float height = (window.parentManager.RootElement.Count * (EditorGUIUtility.singleLineHeight + 2f)) + SearchBarHeight + ElementNameBarHeight;

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
            if (minSize != Vector2.zero && minSize.MinAxis() > 0f)
            {
                dropdownSize.x = Mathf.Max(dropdownSize.x, minSize.x);
                dropdownSize.y = Mathf.Max(dropdownSize.y, minSize.y);
            }

            // - Actual show, this does what we want
            window.ShowAsDropDown(parentRect, dropdownSize);

            // Setup the window
            // m_ElementStack should always have 1 element
            window.m_ElementStack.Push(parentManager.RootElement);

            return window;
        }

        private Vector2 scrollRectPosition = Vector2.zero;
        // Child elements are matched to this list by index
        // A dictionary would have been more suitable but index matching for the time being will do.
        //private List<float> elementHeightsCache = new List<float>(64);
        //private float currentElementHeightsCache = 0f;

        /// <summary>
        /// Returns the current stack layers element.
        /// </summary>
        private float GetElementsHeight(SearchDropdownElement element)
        {
            float height = -1f;
            int i = 0;
            foreach (SearchDropdownElement child in element)
            {
                height += child.GetHeight(GetElementState(element, i));
                i++;
            }

            return height;
        }
        /// <summary>
        /// Returns the element's state from <paramref name="parent"/> on a child that has <paramref name="childIndex"/>.
        /// </summary>
        private ElementGUIDrawingState GetElementState(SearchDropdownElement parent, int childIndex)
        {
            // Get whether if the element is visible
            if (!IsElementVisible(childIndex))
            {
                return ElementGUIDrawingState.Default;
            }

            // Depending on the current GUI thing action, use the Event.current
            // .. TODO

            return ElementGUIDrawingState.Default;
        }
        /// <summary>
        /// Returns whether if the element of this search dropdown window is visible.
        /// </summary>
        private bool IsElementVisible(int elementIndex)
        {
            // TODO
            return false;
        }

        private void OnGUI()
        {
            if (parentManager == null)
            {
                Close();
            }

            if (parentManager.IsSearchable)
            {
                DrawSearchBar();
            }

            DrawElementNameBar();
            DrawElement(m_ElementStack.Peek());
        }
        private void OnDisable()
        {
            OnClosed?.Invoke();
        }

        private void DrawSearchBar()
        {
            Rect searchBarRect = GUILayoutUtility.GetRect(position.width, SearchBarHeight);
            // Draw a centered + padded search bar.
            EditorGUI.DrawRect(searchBarRect, StyleList.SearchBarBackgroundColor);
            searchString = EditorGUI.TextField(new Rect()
            {
                x = searchBarRect.x + SearchBarPadding,
                y = searchBarRect.y + 1f,
                height = SearchBarHeight - 2f,
                width = searchBarRect.width - (SearchBarPadding * 2f)
            }, searchString, StyleList.SearchBarStyle);
        }
        private void DrawElementNameBar()
        {
            // ElementNameBar will contain a clickable back arrow and the name of the selected
            // The name of the selected may or may not get truncated, so please don't make your names that long
            Rect elementBarRect = GUILayoutUtility.GetRect(position.width, ElementNameBarHeight);

            // Background
            EditorGUI.DrawRect(elementBarRect, StyleList.NameBarBackgroundColor);

            // Back button
            SearchDropdownElement last = GetLastSelected();
            if (last != parentManager.RootElement)
            {
                if (GUI.Button(new Rect()
                {
                    x = elementBarRect.x + ElementNameBarPadding,
                    y = elementBarRect.y + ((elementBarRect.height - StyleList.LeftArrowStyle.fixedHeight) / 2f),
                    width = StyleList.LeftArrowStyle.fixedWidth,
                    height = StyleList.LeftArrowStyle.fixedHeight,
                }, GUIContent.none, StyleList.LeftArrowStyle))
                {
                    m_ElementStack.Pop();
                }
            }

            // Name display
            EditorGUI.LabelField(new Rect(elementBarRect)
            {
                x = elementBarRect.x + ElementNameBarPadding + StyleList.LeftArrowStyle.fixedWidth + 5f,
                width = elementBarRect.width - ((ElementNameBarPadding * 2f) + StyleList.LeftArrowStyle.fixedWidth + 5f)
            }, last.content.text, StyleList.CenteredLabelStyle);
        }
        private PropertyRectContext elementCtx = new PropertyRectContext(0f);
        /// <summary>
        /// Draws the current element tree/node.
        /// <br>Also handles selection events for the elements.</br>
        /// </summary>
        /// <param name="element">Element to draw it's children.</param>
        private void DrawElement(SearchDropdownElement element)
        {
            Event e = Event.current;
            elementCtx.Reset();

            scrollRectPosition = GUILayout.BeginScrollView(scrollRectPosition, false, true);
            // Draw ONLY the visible elements
            // Get an index range for the drawing and only refresh that if needed
            // For the top and the start, draw boxes that are very large minus the sizes of drawn
            // --

            // Inflate the bounds / visibility checking size a bit
            Rect localWindowPosition = new Rect(scrollRectPosition.x, scrollRectPosition.y, position.width + 100f, position.height + 10f);

            // Begin a new area to not have horizontal scroll bars
            // Since the scroll view is handled by rect calculations
            GUILayout.BeginVertical(GUILayout.Width(position.width - GUI.skin.verticalScrollbar.fixedWidth));

            // FIXME : Unoptimized ver
            // Reason : pushed rect quantity is too large
            // all elements are checked, which makes this o(n)
            int i = 0;
            foreach (SearchDropdownElement child in element)
            {
                // -- Draw GUI
                ElementGUIDrawingState state = GetElementState(child, i);
                // GUILayoutUtility is bad and will always reserve the width as maximum
                // Which causes the scroll box to overflow a bit
                // So, what do? Begin an area? or something else?
                // Yes, but use 'BeginVertical'
                Rect reservedRect = GUILayoutUtility.GetRect(0f, child.GetHeight(state));

                // Rect.max : bottom-right corner
                if (!localWindowPosition.Contains(reservedRect.max))
                {
                    continue;
                }

                child.OnGUI(reservedRect, state);
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
                        GUI.Label(sideMarkRect, StyleList.CheckMarkString);
                    }
                }
                else
                {
                    if (e.type == EventType.Repaint)
                    {
                        StyleList.RightArrowStyle.Draw(sideMarkRect, GUIContent.none, false, false, false, false);
                    }
                }

                // -- Interact GUI
                // TODO : Send global state to the element object
                if (e.isMouse)
                {
                    if (e.type == EventType.MouseUp && e.button == 0 && reservedRect.Contains(e.mousePosition))
                    {
                        // Push into the top of stack and stop drawing the rest
                        // If the element has no more child, do a selected event + close
                        m_ElementStack.Push(child);

                        if (!child.HasChildren)
                        {
                            Close();
                        }

                        e.Use();
                        break;
                    }
                }

                i++;
            }
            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Returns the last selected element from the selection stack.
        /// <br>Ignores the root element and elements with children and only returns the final selection.</br>
        /// </summary>
        public SearchDropdownElement GetSelected()
        {
            // Try getting the last element on the stack.
            if (!m_ElementStack.TryPeek(out SearchDropdownElement elem))
            {
                return null;
            }
            // Root element + elements with children is ignored
            if (elem == parentManager.RootElement || elem.HasChildren)
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
