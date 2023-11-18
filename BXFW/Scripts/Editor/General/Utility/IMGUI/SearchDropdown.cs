using System;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A list dropdown displayer. Similar to the <see cref="UnityEditor.IMGUI.Controls.AdvancedDropdown"/>, it offers async searching and slightly better optimization.
    /// <br>This class is still in work-in-progress, but it is faster than the AdvancedDropdown.</br>
    /// </summary>
    public abstract class SearchDropdown
    {
        /// <summary>
        /// The given spawned window instance.
        /// <br>Calling close on this will close the window.</br>
        /// </summary>
        private SearchDropdownWindow m_Window;

        private Vector2 m_MinimumSize = Vector2.zero;
        /// <summary>
        /// Minimum size possible for this dropdown.
        /// </summary>
        protected internal virtual Vector2 MinimumSize
        {
            get
            {
                return m_MinimumSize;
            }
            set
            {
                m_MinimumSize = value;
            }
        }
        private float m_MaximumHeight = 300f;
        /// <summary>
        /// The maximum height possible for this dropdown.
        /// <br>If this value clashes with the <see cref="MinimumSize"/> the <see cref="MinimumSize"/>.y is preferred instead.</br>
        /// <br/>
        /// <br>This value both can be overriden or be set as an usual variable on <see cref="BuildRoot"/>.</br>
        /// </summary>
        protected internal virtual float MaximumHeight
        {
            get
            {
                return Mathf.Max(m_MaximumHeight, MinimumSize.y);
            }
            set
            {
                m_MaximumHeight = value;
            }
        }
        /// <summary>
        /// Whether to allow rich text reprensentation on the OptimizedSearchDropdown elements. 
        /// <br>Note : Any <see cref="SearchDropdownElement"/> can override or ignore this. This only applies
        /// to the global label (<see cref="SearchDropdownWindow.StyleList"/>) styling.</br>
        /// </summary>
        protected internal virtual bool AllowRichText => false;
        /// <summary>
        /// Whether if this 'OptimizedSearchDropdown' will have a search bar.
        /// <br>This will affect the height.</br>
        /// </summary>
        protected internal virtual bool IsSearchable => true;
        /// <summary>
        /// Amount of maximum children count of search result.
        /// <br>After this many elements the searching will stop.</br>
        /// <br/>
        /// <br>Setting this zero or lower than zero will set this limit to infinite which is not really recommended.</br>
        /// </summary>
        protected internal virtual int SearchElementsResultLimit => 10000;
        /// <summary>
        /// The string comparison mode used to search for text.
        /// </summary>
        protected internal virtual StringComparison SearchComparison => StringComparison.Ordinal;
        /// <summary>
        /// Whether to display the current elements count inside the header text.
        /// </summary>
        protected internal virtual bool DisplayCurrentElementsCount => true;

        // ?? TODO : Search children using some haystack algorithm
        // Sorting can be done by the caller who builds the root.

        /// <summary>
        /// Creates a search dropdown, nothing fancy is done here.
        /// </summary>
        public SearchDropdown()
        {
            // Q : What is the purpose of making the state visible and public if we can't change anything on it?
            // A : State can be serialized, spawning windows at the current given state
            // But, like, editor windows are SerializableObject's anyway so, i may or may not make the state visible
        }

        private SearchDropdownElement m_RootElement;
        /// <summary>
        /// The root element.
        /// </summary>
        public SearchDropdownElement RootElement => m_RootElement;
        /// <summary>
        /// Show the dropdown at the <paramref name="rect"/> position.
        /// </summary>
        /// <param name="rect">Position of the button that opened the dropdown.</param>
        public void Show(Rect rect)
        {
            if (m_Window != null)
            {
                m_Window.Close();
                m_Window = null;
            }

            // The 'AdvancedDropdown' handles all of it's data using a 'DataSource'
            // I will do it the good ole way of suffering
            m_RootElement = BuildRoot();
            m_Window = SearchDropdownWindow.Create(rect, this, MinimumSize, MaximumHeight);
            m_Window.OnClosed += () =>
            {
                SearchDropdownElement selected = m_Window.GetSelected();

                if (selected != null)
                {
                    OnElementSelected(selected);
                    OnElementSelectedEvent?.Invoke(selected);
                }
                m_RootElement = null;
            };
        }

        /// <summary>
        /// Sets the searching filter of the window.
        /// <br>Only applies to <see cref="IsSearchable"/> dropdowns.</br>
        /// </summary>
        protected void SetFilter(string searchString)
        {
            m_Window.SearchString = searchString;
        }

        /// <summary>
        /// Build the root of this searching dropdown.
        /// </summary>
        /// <returns>The root element containing the elements. If the root element has no children nothing will happen.</returns>
        protected abstract SearchDropdownElement BuildRoot();

        /// <summary>
        /// Callback when an element is selected.
        /// <br>The element mostly shouldn't have children. This method by default does nothing.</br>
        /// </summary>
        protected virtual void OnElementSelected(SearchDropdownElement element)
        { }
        /// <summary>
        /// Called when an element is selected.
        /// </summary>
        public event Action<SearchDropdownElement> OnElementSelectedEvent;
    }
}
