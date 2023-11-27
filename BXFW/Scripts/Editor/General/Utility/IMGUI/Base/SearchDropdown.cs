using System;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A list dropdown displayer. Similar to the <see cref="UnityEditor.IMGUI.Controls.AdvancedDropdown"/>, it offers async searching and slightly better optimization.
    /// <br>This class is still in work-in-progress, but it is faster than the AdvancedDropdown.</br>
    /// </summary>
    /// <remarks>
    /// The usage of this class can be found in the source.
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// // Assembly-CSharp-Editor
    /// // Example dropdown.
    /// using UnityEngine;
    /// using UnityEditor;
    /// using BXFW.Tools.Editor;
    /// 
    /// public class ExampleDropdown : SearchDropdown
    /// {
    ///     protected override SearchDropdownElement BuildRoot()
    ///     {
    ///         // Create a root element to return
    ///         // Other elements are to be attached to this root
    ///         SearchDropdownElement root = new SearchDropdownElement("Root Element")
    ///         {
    ///             // Any element can be started as a c# collection
    ///             new SearchDropdownElement("Child 1"),
    ///             new SearchDropdownElement("Child 2"),
    ///             new SearchDropdownElement("Child 3")
    ///             {
    ///                 // Every child can have it's own values and so on...
    ///                 new SearchDropdownElement("Child Of Child 1"),
    ///                 new SearchDropdownElement("Child Of Child 2")
    ///             }
    ///         };
    ///         // The children can be also systematically be added using SearchDropdownElement.Add().        
    /// 
    ///         return root;
    ///     }
    ///     protected override void OnElementSelected(SearchDropdownElement element)
    ///     {
    ///         // .. Do anything with the element, there is also a global event on a 'SearchDropdown' called OnElementSelectedEvent
    ///         // .. Which gets called with this ..
    ///         // However this event gets called first before the property 'Event'.
    ///     }
    /// }
    /// // ... Instantiation
    /// // Assembly-CSharp
    /// using UnityEngine;
    /// 
    /// public class SampleClass : MonoBehaviour
    /// {
    ///     public string dropdownSettingString;
    /// }
    /// // ...
    /// // Assembly-CSharp-Editor
    /// using UnityEngine;
    /// using UnityEditor;
    /// using BXFW.Tools.Editor;
    /// 
    /// [CustomEditor(typeof(SampleClass))]
    /// public class SampleClassEditor : Editor
    /// {
    ///     // Unity gives a bogus/dummy rect on the Event.current.type == EventType.Layout
    ///     private Rect lastRepaintDropdownParentRect;
    /// 
    ///     public override void OnInspectorGUI()
    ///     {
    ///         var target = base.target as SampleClass;    
    /// 
    ///         // Draw the private 'm_Script' field (optional)
    ///         using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
    ///         {
    ///             EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
    ///         }
    ///         
    ///         // Draw the dropdown button
    ///         EditorGUILayout.BeginHorizontal();
    ///         EditorGUILayout.LabelField($"Dropdown Value : {target.dropdownSettingString}", GUILayout.Width(EditorGUIUtility.labelWidth));
    ///         if (GUILayout.Button("Set Value From Dropdown"))
    ///         {
    ///             ExampleDropdown dropdown = new ExampleDropdown();
    ///             dropdown.Show(lastRepaintDropdownParentRect);
    ///             dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
    ///             {
    ///                 // Will not take a 'SerializedProperty' inside a delegate
    ///                 // Because SerializedObject and SerializedProperty disposes automatically after the OnGUI call
    ///                 // But you can clone the entire SerializedObject and SerializedProperty just for this delegate, 
    ///                 // then apply changed values and dispose of it inside this delegate
    ///                 // -- 
    ///                 // For this example, a basic undo with direct access to the object is used
    ///                 Undo.RecordObject(target, "set value from dropdown");
    ///                 target.dropdownSettingString = element.content.text;    
    ///             };
    ///         }
    ///         EditorGUILayout.EndHorizontal();
    /// 
    ///         // Get the last rect for getting the proper value
    ///         // This is only needed on automatically layouted GUI's, with the GUI's
    ///         // that you know the rect to you can use that rect instead.
    ///         if (Event.current.type == EventType.Repaint)
    ///         {
    ///             lastRepaintDropdownParentRect = GUILayoutUtility.GetLastRect();
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
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
        // TODO : Make these fields public virtual?
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
        /// Whether to allow selection event to be fired for elements with children.
        /// <br>This will show an extra button and will allow selection of elements with children.</br>
        /// </summary>
        protected internal virtual bool AllowSelectionOfElementsWithChild => false;
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
        /// <summary>
        /// Placeholder string displayed for dropdowns without any elements.
        /// </summary>
        public virtual string NoElementPlaceholderText { get; set; }
        /// <summary>
        /// String used to show that there's no results.
        /// <br>Can have a format argument as {0}, where it will be replaced with the search query.</br>
        /// </summary>
        public virtual string NoSearchResultsText { get; set; } = "No results found on search query \"{0}\"\nTry searching for other elements or check if the search string matches.";

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
        /// <br>This element is null during <see cref="BuildRoot"/>.</br>
        /// </summary>
        public SearchDropdownElement RootElement => m_RootElement;
        /// <summary>
        /// Show the dropdown at the <paramref name="rect"/> position.
        /// <br>This also creates a new root.</br>
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
                if (!m_Window.IsClosingWithSelectionIntent)
                {
                    OnDiscardEvent?.Invoke();
                    return;
                }

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
        /// <summary>
        /// Called when the dropdown is discarded.
        /// (No element selection intent was specified and the dropdown closed)
        /// </summary>
        public event Action OnDiscardEvent;
    }
}
