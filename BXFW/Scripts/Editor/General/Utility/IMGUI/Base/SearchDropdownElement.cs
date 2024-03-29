using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// GUI drawing state for the element.
    /// <br><see cref="Default"/> : The default drawing behaviour.</br>
    /// <br><see cref="Selected"/> : Selected element drawing behaviour.</br>
    /// <br><see cref="Hover"/> : Hovered element drawing behaviour.</br>
    /// <br><see cref="Clicked"/> : Clicked/Pressed drawing behaviour.</br>
    /// </summary>
    public enum ElementGUIDrawingState
    {
        Default,
        Selected,
        Hover,
        Clicked,
    }

    /// <summary>
    /// A search dropdown element.
    /// <br>Can define it's own GUI, etc.</br>
    /// </summary>
    public class SearchDropdownElement : ICollection<SearchDropdownElement>, IComparable<SearchDropdownElement>, IEquatable<SearchDropdownElement>
    {
        /// <summary>
        /// The content that this element contains.
        /// </summary>
        public GUIContent content = GUIContent.none;
        /// <summary>
        /// Whether if this element is selected.
        /// <br/>
        /// <br>Setting this will effect the GUI on the default <see cref="OnGUI(Rect, ElementGUIDrawingState)"/>.</br>
        /// </summary>
        public bool Selected { get; set; } = false;
        /// <summary>
        /// Whether if this element can be interacted with.
        /// <br>Setting this <see langword="false"/> will set the state constantly to Default and make this element unselectable.</br>
        /// </summary>
        public virtual bool Interactable { get; set; } = true;
        /// <summary>
        /// Whether if this element requests a repaint.
        /// <br>Useful for constantly updating elements.</br>
        /// </summary>
        public virtual bool RequestsRepaint { get; set; } = false;

        /// <summary>
        /// The rectangle reserving context.
        /// <br>The values can be overriden/changed.</br>
        /// </summary>
        protected readonly PropertyRectContext drawingContext = new PropertyRectContext(2f);
        
        /// <summary>
        /// Internal contained children.
        /// </summary>
        private readonly List<SearchDropdownElement> m_Children;
        /// <summary>
        /// Whether if this element has children.
        /// </summary>
        public bool HasChildren => Count > 0;
        /// <summary>
        /// Size of the children contained.
        /// </summary>
        public int Count => m_Children.Count;
        /// <summary>
        /// Capacity reserved for the children.
        /// <br>Changing this allocates more memory for the internal children array.</br>
        /// </summary>
        public int ChildCapacity
        {
            get => m_Children.Capacity;
            set => m_Children.Capacity = Mathf.Clamp(value, 0, int.MaxValue);
        }
        public bool IsReadOnly => false;
        /// <summary>
        /// An indiced access operator for this element.
        /// </summary>
        public SearchDropdownElement this[int index] => m_Children[index];

        /// <inheritdoc cref="SearchDropdownElement(GUIContent)"/>
        public SearchDropdownElement(string label)
            : this(new GUIContent(label))
        { }
        /// <inheritdoc cref="SearchDropdownElement(GUIContent)"/>
        public SearchDropdownElement(string label, string tooltip)
            : this(new GUIContent(label, tooltip))
        { }

        /// <inheritdoc cref="SearchDropdownElement(GUIContent, int)"/>
        public SearchDropdownElement(string label, int childrenCapacity)
            : this(new GUIContent(label), childrenCapacity)
        { }
        /// <inheritdoc cref="SearchDropdownElement(GUIContent, int)"/>
        public SearchDropdownElement(string label, string tooltip, int childrenCapacity)
            : this(new GUIContent(label, tooltip), childrenCapacity)
        { }

        /// <summary>
        /// Creates an <see cref="SearchDropdownElement"/> with content assigned.
        /// </summary>
        public SearchDropdownElement(GUIContent content)
        {
            this.content = content;
            m_Children = new List<SearchDropdownElement>(64);
        }
        /// <summary>
        /// Creates an <see cref="SearchDropdownElement"/> with content assigned.
        /// <br>The child capacity can be defined for chunking/memory optimization.</br>
        /// </summary>
        public SearchDropdownElement(GUIContent content, int childrenCapacity)
        {
            this.content = content;
            m_Children = new List<SearchDropdownElement>(childrenCapacity);
        }

        // -- This approach is fine as we will only draw few elements at once, not all of them
        // Only the 'GetHeight' may get called too many times sometimes
        /// <summary>
        /// Returns the height of the default element.
        /// <br>The default height is <see cref="EditorGUIUtility.singleLineHeight"/> + <see cref="drawingContext"/>.Padding</br>
        /// <br/>
        /// <br>
        /// <b>Warning</b> : This method affects performance vastly (for the time being). Override it carefully, ensure that this is not intensive. 
        /// (this method will be slow until the heights of non-visible is cached).
        /// </br>
        /// </summary>
        /// <param name="viewWidth">Width of the draw area allocated for this element.</param>
        public virtual float GetHeight(float viewWidth)
        {
            // Calling 'GUI.skin.label.CalcHeight' like 10000 times is most likely not good
            return EditorGUIUtility.singleLineHeight + drawingContext.Padding;
        }
        /// <summary>
        /// Draws the GUI of the default element.
        /// <br>The default element has the following : An icon on the left and the description on the right.</br>
        /// </summary>
        /// <param name="position">Position to draw the GUI on.</param>
        /// <param name="drawingState">
        /// The element state, depending on the cursor interaction.
        /// Elements can ignore this all together and use the <see cref="Event.current"/> but the default behaviour doesn't.
        /// <br>This is just a more convenient way of receiveing events.</br>
        /// <br>
        /// <b>Warning : </b> Check the <see cref="Event.current"/> for this state's correctness.
        /// This event is usually only correct in <see cref="EventType.Repaint"/>.
        /// </br>
        /// </param>
        public virtual void OnGUI(Rect position, ElementGUIDrawingState drawingState)
        {
            // * Left           : Reserve a Icon drawing rect
            // * Left <-> Right : The text drawing rect
            // * Right          : Arrow to display if the menu has children.
            // -- Icon rect width is EditorGUIUtility.singleLineHeight, same as the right arrow
            // -- Horizontal padding is 5f
            drawingContext.Reset();
            Rect contextRect = drawingContext.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);

            Rect iconRect = new Rect(contextRect)
            {
                width = EditorGUIUtility.singleLineHeight
            };
            Rect textRect = new Rect(contextRect)
            {
                x = contextRect.x + EditorGUIUtility.singleLineHeight + 5f,
                width = contextRect.width - (iconRect.width + EditorGUIUtility.singleLineHeight)
            };

            // -- Background box tint
            Color stateColor = new Color(0.2f, 0.2f, 0.2f);
            switch (drawingState)
            {
                case ElementGUIDrawingState.Selected:
                    stateColor = new Color(0.15f, 0.35f, 0.39f);
                    break;
                case ElementGUIDrawingState.Hover:
                    stateColor = new Color(0.15f, 0.15f, 0.15f);
                    break;
                case ElementGUIDrawingState.Clicked:
                    stateColor = new Color(0.1f, 0.1f, 0.1f);
                    break;

                default:
                    break;
            }
            EditorGUI.DrawRect(position, stateColor);
            // -- Elements
            if (content.image != null)
            {
                GUI.DrawTexture(iconRect, content.image, ScaleMode.ScaleToFit);
            }
            // This also sets tooltips, etc.
            GUI.Label(textRect, content, SearchDropdownWindow.StyleList.LabelStyle);
        }

        /// <summary>
        /// Adds a child element to this element.
        /// </summary>
        /// <param name="item">The item to add. This mustn't be null. The overriding method should assert this unless null is suitable.</param>
        public virtual void Add(SearchDropdownElement item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "[SearchDropdownElement::Add] Given argument was null.");
            }

            m_Children.Add(item);
        }
        /// <summary>
        /// Clears all children inside this element.
        /// </summary>
        public virtual void Clear()
        {
            m_Children.Clear();
        }
        /// <summary>
        /// Returns whether if this element contains child <paramref name="item"/>.
        /// </summary>
        public bool Contains(SearchDropdownElement item)
        {
            return m_Children.Contains(item);
        }
        /// <summary>
        /// Copies self into the <paramref name="array"/>.
        /// </summary>
        public void CopyTo(SearchDropdownElement[] array, int arrayIndex)
        {
            m_Children.CopyTo(array, arrayIndex);
        }
        /// <summary>
        /// Removes a child element if it exists.
        /// </summary>
        /// <param name="item">The item to remove. This value can be anything.</param>
        public virtual bool Remove(SearchDropdownElement item)
        {
            return m_Children.Remove(item);
        }
        /// <summary>
        /// Sorts the current layer children.
        /// </summary>
        public void Sort()
        {
            m_Children.Sort();
        }
        /// <inheritdoc cref="Sort"/>
        public void Sort(IComparer<SearchDropdownElement> comparer)
        {
            m_Children.Sort(comparer);
        }
        /// <inheritdoc cref="Sort"/>
        public void Sort(Comparison<SearchDropdownElement> comparison)
        {
            m_Children.Sort(comparison);
        }

        /// <summary>
        /// Sorts all children recursively.
        /// </summary>
        public void SortAll()
        {
            Sort();
            // This will iterate until the last child without the child
            foreach (SearchDropdownElement child in m_Children)
            {
                child.SortAll();
            }
        }
        /// <inheritdoc cref="SortAll"/>
        public void SortAll(IComparer<SearchDropdownElement> comparer)
        {
            Sort(comparer);
            foreach (SearchDropdownElement child in m_Children)
            {
                child.SortAll(comparer);
            }
        }
        /// <inheritdoc cref="SortAll"/>
        public void SortAll(Comparison<SearchDropdownElement> comparison)
        {
            Sort(comparison);
            foreach (SearchDropdownElement child in m_Children)
            {
                child.SortAll(comparison);
            }
        }

        public IEnumerator<SearchDropdownElement> GetEnumerator()
        {
            foreach (SearchDropdownElement elem in m_Children)
            {
                yield return elem;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Used for sortability.
        /// <br>This can be overridden to change the default item value.</br>
        /// </summary>
        public virtual int CompareTo(SearchDropdownElement other)
        {
            // Always larger than null element
            if (other == null)
            {
                return 1;
            }
            if (other.content == null)
            {
                return content == null ? 0 : 1;
            }

            // Strip 'content.text' from rich text if parent allows rich text?
            // TODO : Either determine that using more references (yay, more code and bloat) or allow the user to manually override this
            // For the time being just do normal comparisons as if the rich texted stuff is colored similarly the comparison shouldn't be hurt much.
            return content.text.CompareTo(other.content.text);
        }

        /// <summary>
        /// Returns a string that is the <see cref="content"/>.ToString + <see cref="Count"/>.
        /// </summary>
        public override string ToString()
        {
            return $"Content : \"{content}\", Count : {Count}";
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(content, Selected, drawingContext, m_Children);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SearchDropdownElement);
        }
        public bool Equals(SearchDropdownElement other)
        {
            return !(other is null) &&
                   EqualityComparer<GUIContent>.Default.Equals(content, other.content) &&
                   Selected == other.Selected &&
                   EqualityComparer<PropertyRectContext>.Default.Equals(drawingContext, other.drawingContext) &&
                   EqualityComparer<List<SearchDropdownElement>>.Default.Equals(m_Children, other.m_Children);
        }

        public static bool operator ==(SearchDropdownElement left, SearchDropdownElement right)
        {
            return EqualityComparer<SearchDropdownElement>.Default.Equals(left, right);
        }
        public static bool operator !=(SearchDropdownElement left, SearchDropdownElement right)
        {
            return !(left == right);
        }
    }
}
