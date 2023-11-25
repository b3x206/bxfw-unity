using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A <see cref="GUILayoutGroup"/> utility used to easily manage <see cref="GUILayoutGroup"/> and <see cref="GUILayoutGroup"/> classes.
    /// <br>Since these classes are internal, this utility allows easy public access to it's variables.</br>
    /// </summary>
    public class InternalGUILayoutGroup : InternalGUILayoutEntry, ICollection<InternalGUILayoutEntry>
    {
        // Yes, this sucks. i am aware of it.
        // it is needed because we don't have lower level control of the GUILayout system
        // And i want lower level control of the GUILayout system.
        protected override Type GuiLayoutEntryType => GUIAdditionals.ImguiAssembly.GetType("UnityEngine.GUILayoutGroup", true);

        /// <summary>
        /// Returns whether if this group is vertical.
        /// </summary>
        public bool IsVertical
        {
            get
            {
                return (bool)Fields["isVertical"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["isVertical"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Returns whether if this group is resetting the coordinates of layouting.
        /// </summary>
        public bool ResetCoords
        {
            get
            {
                return (bool)Fields["isVertical"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["isVertical"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Spacing between entries.
        /// </summary>
        public float Spacing
        {
            get
            {
                return (float)Fields["spacing"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["isVertical"].SetValue(BoxedEntry, value);
            }
        }

        public bool SameSize
        {
            get
            {
                return (bool)Fields["sameSize"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["sameSize"].SetValue(BoxedEntry, value);
            }
        }

        public bool IsWindow
        {
            get
            {
                return (bool)Fields["isWindow"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["isWindow"].SetValue(BoxedEntry, value);
            }
        }

        public int WindowID
        {
            get
            {
                return (int)Fields["windowID"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["windowID"].SetValue(BoxedEntry, value);
            }
        }

        /// <summary>
        /// Position of the current drawing / rect getting cursor.
        /// </summary>
        public int CursorPosition
        {
            get
            {
                return (int)Fields["m_Cursor"].GetValue(BoxedEntry);
            }
            protected set
            {
                Fields["m_Cursor"].SetValue(BoxedEntry, value);
            }
        }

        protected int StretchableCountX
        {
            get
            {
                return (int)Fields["m_StretchableCountX"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_StretchableCountX"].SetValue(BoxedEntry, value);
            }
        }

        protected int StretchableCountY
        {
            get
            {
                return (int)Fields["m_StretchableCountY"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_StretchableCountY"].SetValue(BoxedEntry, value);
            }
        }

        protected bool UserSpecifiedWidth
        {
            get
            {
                return (bool)Fields["m_UserSpecifiedWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_UserSpecifiedWidth"].SetValue(BoxedEntry, value);
            }
        }

        protected bool UserSpecifiedHeight
        {
            get
            {
                return (bool)Fields["m_UserSpecifiedHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_UserSpecifiedHeight"].SetValue(BoxedEntry, value);
            }
        }

        protected float ChildMinWidth
        {
            get
            {
                return (float)Fields["m_ChildMinWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_ChildMinWidth"].SetValue(BoxedEntry, value);
            }
        }

        protected float ChildMaxWidth
        {
            get
            {
                return (float)Fields["m_ChildMaxWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_ChildMaxWidth"].SetValue(BoxedEntry, value);
            }
        }

        protected float ChildMinHeight
        {
            get
            {
                return (float)Fields["m_ChildMinHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_ChildMinHeight"].SetValue(BoxedEntry, value);
            }
        }

        protected float ChildMaxHeight
        {
            get
            {
                return (float)Fields["m_ChildMaxHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["m_ChildMaxHeight"].SetValue(BoxedEntry, value);
            }
        }

        /// <summary>
        /// Dummy rect, used to return a dummy value.
        /// </summary>
        public static readonly Rect DummyRect = new Rect(0f, 0f, 1f, 1f);

        protected void SetMarginLeft(int value)
        {
            Fields["m_MarginLeft"].SetValue(BoxedEntry, value);
        }
        protected void SetMarginRight(int value)
        {
            Fields["m_MarginRight"].SetValue(BoxedEntry, value);
        }
        protected void SetMarginTop(int value)
        {
            Fields["m_MarginTop"].SetValue(BoxedEntry, value);
        }
        protected void SetMarginBottom(int value)
        {
            Fields["m_MarginBottom"].SetValue(BoxedEntry, value);
        }
        
        /// <summary>
        /// Resets the current drawing cursor position.
        /// </summary>
        public void ResetCursor()
        {
            CursorPosition = 0;
        }

        /// <summary>
        /// Peeks the next <see cref="GUILayoutEntry"/>'s rect.
        /// <br/>
        /// <br>Throws an exception (on <see cref="EventType.Repaint"/>, on other events this returns a <see cref="DummyRect"/>) 
        /// if not enough entries were allocated beyond <see cref="CursorPosition"/>.</br>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        public Rect PeekNextRect()
        {
            if (CursorPosition < Count)
            {
                return this[CursorPosition].EntryRect;
            }

            if (Event.current.type == EventType.Repaint)
            {
                throw new IndexOutOfRangeException($"[InternalGUILayoutGroup::PeekNextRect] Getting control with index '{CursorPosition}'s position in a group with only {Count} controls while doing \"{Event.current.rawType}\".");
            }

            return DummyRect;
        }
        /// <summary>
        /// Tries to peek the next <see cref="GUILayoutEntry"/>'s rect.
        /// </summary>
        /// <returns>
        /// If successful, returns <see langword="true"/>. Otherwise <see langword="false"/> is returned.
        /// In a case of a dummy rect, this function will return <see langword="false"/>.
        /// <br>This method will set <paramref name="rect"/> a dummy rect until the next entry is created.</br>
        /// </returns>
        public bool TryPeekNextRect(out Rect rect)
        {
            rect = DummyRect;

            if (CursorPosition < Count)
            {
                rect = this[CursorPosition].EntryRect;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Peeks the next <see cref="GUILayoutEntry"/>.
        /// <br/>
        /// <br>Throws an exception (on <see cref="EventType.Repaint"/>, on other events this returns a <see langword="null"/>) 
        /// if not enough entries were allocated beyond <see cref="CursorPosition"/>.</br>
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"/>
        public InternalGUILayoutEntry PeekNextEntry()
        {
            if (CursorPosition < Count)
            {
                return this[CursorPosition];
            }

            if (Event.current.type == EventType.Repaint)
            {
                throw new IndexOutOfRangeException($"[InternalGUILayoutGroup::PeekNextEntry] Getting control with index '{CursorPosition}'s position in a group with only {Count} controls while doing \"{Event.current.rawType}\".");
            }

            return null;
        }
        /// <summary>
        /// Tries to peek the next <see cref="GUILayoutEntry"/>.
        /// </summary>
        /// <returns>
        /// If successful, returns <see langword="true"/>. Otherwise <see langword="false"/> is returned.
        /// In a case of a null entry, this function will return <see langword="false"/>.
        /// <br>This method will set <paramref name="entry"/> null until the next entry is created.</br>
        /// </returns>
        public bool TryPeekNextEntry(out InternalGUILayoutEntry entry)
        {
            entry = null;

            if (CursorPosition < Count)
            {
                entry = this[CursorPosition];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the next entry and increments cursor position by 1.
        /// <br>Throws an exception (on <see cref="EventType.Repaint"/>, on other events this returns a <see langword="null"/>) 
        /// if not enough entries were allocated beyond <see cref="CursorPosition"/>.</br>
        /// </summary>
        public InternalGUILayoutEntry GetNextEntry()
        {
            if (CursorPosition < Count)
            {
                InternalGUILayoutEntry entry = this[CursorPosition];
                CursorPosition++;
                return entry;
            }

            if (Event.current.type == EventType.Repaint)
            {
                throw new IndexOutOfRangeException($"[InternalGUILayoutGroup::GetNextEntry] Getting control with index '{CursorPosition}'s position in a group with only {Count} controls while doing \"{Event.current.rawType}\".");
            }

            return null;
        }
        /// <summary>
        /// Tries getting the next entry and if successful increments cursor position by 1.
        /// </summary>
        /// <returns>
        /// If successful, returns <see langword="true"/>. Otherwise <see langword="false"/> is returned.
        /// In a case of a null entry, this function will return <see langword="false"/>.
        /// <br>This method will set <paramref name="nextEntry"/> null until the next entry is created.</br>
        /// </returns>
        public bool TryGetNextEntry(out InternalGUILayoutEntry nextEntry)
        {
            nextEntry = null;

            if (CursorPosition < Count)
            {
                nextEntry = this[CursorPosition];
                CursorPosition++;
                return true;
            }

            return false;
        }

        #region List Entries
        /// <summary>
        /// The list type for the entries.
        /// <br>Since the <see cref="GUILayoutGroup.entries"/> contains an internal generic type, have to use reflection for this as well.</br>
        /// </summary>
        private Type EntriesType => Fields["entries"].FieldType;
        /// <summary>
        /// Field value of the given entries on the <see cref="InternalGUILayoutEntry.BoxedEntry"/>.
        /// </summary>
        private object EntriesFieldValue => Fields["entries"].GetValue(BoxedEntry);
        /// <summary>
        /// The first list accessor property inside this list type.
        /// </summary>
        private PropertyInfo ListIndexAccessorProperty => EntriesType.GetProperties().First(p => p.GetIndexParameters().Length == 1);

        /// <summary>
        /// Count of the total <see cref="GUILayoutEntry"/>ies inside this group.
        /// </summary>
        public int Count => (int)EntriesType.GetProperty(nameof(IList.Count), GetDictionaryFlags).GetValue(EntriesFieldValue);

        /// <summary>
        /// Is always <see langword="false"/>.
        /// </summary>
        public bool IsReadOnly => false;

        public InternalGUILayoutEntry this[int index]
        {
            get => new InternalGUILayoutEntry(ListIndexAccessorProperty.GetValue(EntriesFieldValue, new object[] { index }));
            set => ListIndexAccessorProperty.SetValue(EntriesFieldValue, value.BoxedEntry, new object[] { index });
        }
        /// <summary>
        /// Adds a new <see cref="GUILayoutEntry"/> into the entries.
        /// </summary>
        /// <param name="item">
        /// Item to add it's <see cref="InternalGUILayoutEntry.BoxedEntry"/>.
        /// <br>This value cannot be null.</br>
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public void Add(InternalGUILayoutEntry item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "[InternalGUILayoutGroup::Add] Given item was null. Cannot add item.");
            }

            EntriesType.GetMethod(nameof(IList.Add), GetDictionaryFlags).Invoke(EntriesFieldValue, new object[] { item.BoxedEntry });
        }
        /// <summary>
        /// Clears the list of given entries.
        /// </summary>
        public void Clear()
        {
            EntriesType.GetMethod(nameof(IList.Clear), GetDictionaryFlags).Invoke(EntriesFieldValue, null);
        }
        /// <summary>
        /// Returns whether if the given entry group contains the item.
        /// </summary>
        public bool Contains(InternalGUILayoutEntry item)
        {
            if (item == null)
            {
                return false;
            }

            return (bool)EntriesType.GetMethod(nameof(IList.Contains), GetDictionaryFlags).Invoke(EntriesFieldValue, new object[] { item.BoxedEntry });
        }
        /// <summary>
        /// Copies the array elements into given <paramref name="array"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="IndexOutOfRangeException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void CopyTo(InternalGUILayoutEntry[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "[InternalGUILayoutGroup::CopyTo] Given array parametert is null.");
            }
            if (array.Length < arrayIndex + Count)
            {
                throw new IndexOutOfRangeException("[InternalGUILayoutGroup::CopyTo] Given array parameter is too small.");
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "[InternalGUILayoutGroup::CopyTo] Given 'arrayIndex' is lower than 0 and negative.");
            }

            // Copy the values of 'refParams' into the array
            for (int i = arrayIndex; i < arrayIndex + Count; i++)
            {
                array[i] = this[i];
            }
        }
        /// <summary>
        /// Removes an entry.
        /// </summary>
        public bool Remove(InternalGUILayoutEntry item)
        {
            if (item == null)
            {
                return false;
            }

            return (bool)EntriesType.GetMethod(nameof(IList.Remove), GetDictionaryFlags).Invoke(EntriesFieldValue, new object[] { item.BoxedEntry });
        }
        /// <summary>
        /// Removes all matching entries from this group.
        /// </summary>
        public int RemoveAll(Predicate<InternalGUILayoutEntry> predicate)
        {
            int removeCount = 0;
            for (int i = Count - 1; i >= 0; i--)
            {
                if (predicate(this[i]))
                {
                    RemoveAt(i);
                    removeCount++;
                }
            }

            return removeCount;
        }

        /// <summary>
        /// Removes an entry at given <paramref name="index"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            EntriesType.GetMethod(nameof(IList.RemoveAt), GetDictionaryFlags).Invoke(EntriesFieldValue, new object[] { index });
        }

        public IEnumerator<InternalGUILayoutEntry> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        /// <summary>
        /// Creates an empty GUILayoutGroup.
        /// </summary>
        public InternalGUILayoutGroup() : base()
        { }

        /// <summary>
        /// Creates a <see cref="InternalGUILayoutGroup"/> from another <see cref="GUILayoutGroup"/>.
        /// <br>This is <b>NOT</b> a copy constructor. It does not copy the <paramref name="layoutGroup"/>.</br>
        /// </summary>
        /// <param name="entry">The <see cref="GUILayoutGroup"/> to create from. This cannot be null and any other type than the <see cref="GuiLayoutEntryType"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public InternalGUILayoutGroup(object layoutGroup) : base(layoutGroup)
        { }

        /// <summary>
        /// Creates a GUILayoutGroup with some settings, still empty.
        /// </summary>
        public InternalGUILayoutGroup(GUIStyle style, GUILayoutOption[] options)
        {
            if (options != null)
            {
                ApplyOptions(options);
            }

            // Why the original constructor does something this silly?
            // That is because the original 'ApplyStyleSettings' wasn't overriden and was just neglected?
            // What? Okay. We got POOP (proper OOP) from unity in the menu here.
            // --
            // I can only assume that some guy designed/made the IMGUI Layout system of unity to
            // just work enough and just left the company then nobody touched the source ever again.
            // --
            // or y'know, they didn't intend crazy people like me to poke around in the internal api's
            SetMarginLeft(style.margin.left);
            SetMarginRight(style.margin.right);
            SetMarginTop(style.margin.top);
            SetMarginBottom(style.margin.bottom);
        }
    }
}
