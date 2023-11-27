using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// A <see cref="GUILayoutEntry"/> utility used to easily manage <see cref="GUILayoutEntry"/> and <see cref="GUILayoutGroup"/> classes.
    /// <br>Since these classes are internal, this utility allows easy public access to it's variables.</br>
    /// </summary>
    /// This class is named weirdly like this because the UnityEngine assembly already would have the same name, which would cause clashes
    /// But the thing is, the class cannot be accessed in any normal means so maybe rename it? Or call it something prefixed with 'Unity'?
    public class InternalGUILayoutEntry : IEquatable<InternalGUILayoutEntry>
    {
        /// <inheritdoc cref="BoxedEntry"/>
        private object m_BoxedEntry;

        /// <summary>
        /// The internal <see cref="GUILayoutEntry"/>
        /// <br>Can be types deriving from <see cref="GUILayoutEntry"/> or just can be a <see cref="GUILayoutEntry"/>.</br>
        /// <br>Passing the <see cref="InternalGUILayoutEntry"/> into reflection methods will throw an exception due to a type mismatch.</br>
        /// </summary>
        public object BoxedEntry
        {
            get
            {
                m_BoxedEntry ??= Activator.CreateInstance(GuiLayoutEntryType);
                return m_BoxedEntry;
            }
        }

        // TODO : These dictionaries probably allocate a lot of garbage
        // Making them static readonly could help, but it will require to contain these dictionaries inside another datatype or something weird
        // Time to replace the MonoBleedingEdge.dll and the csc.exe with my own hacked compilers/libraries to make 'protected virtual static' a thing heheheh
        private Dictionary<string, FieldInfo> m_Fields;
        /// <summary>
        /// List of the fields of <see cref="GuiLayoutEntryType"/>.
        /// </summary>
        public Dictionary<string, FieldInfo> Fields
        {
            get
            {
                m_Fields ??= new Dictionary<string, FieldInfo>(GuiLayoutEntryType.GetFields(GetDictionaryFlags).Cast((FieldInfo f) => new KeyValuePair<string, FieldInfo>(f.Name, f)));
                return m_Fields;
            }
        }
        private Dictionary<string, PropertyInfo> m_Properties;
        /// <summary>
        /// List of the property fields of <see cref="GuiLayoutEntryType"/>.
        /// </summary>
        protected Dictionary<string, PropertyInfo> Properties
        {
            get
            {
                m_Properties ??= new Dictionary<string, PropertyInfo>(GuiLayoutEntryType.GetProperties(GetDictionaryFlags).Cast((PropertyInfo p) => new KeyValuePair<string, PropertyInfo>(p.Name, p)));
                return m_Properties;
            }
        }
        private Dictionary<string, MethodInfo> m_Methods;
        /// <summary>
        /// List of the functions/methods of <see cref="GuiLayoutEntryType"/>.
        /// </summary>
        protected Dictionary<string, MethodInfo> Methods
        {
            get
            {
                m_Methods ??= new Dictionary<string, MethodInfo>(GuiLayoutEntryType.GetMethods(GetDictionaryFlags).Cast((MethodInfo m) => new KeyValuePair<string, MethodInfo>(m.Name, m)));
                return m_Methods;
            }
        }
        /// <summary>
        /// Base type for the <see cref="GUILayoutEntry"/>.
        /// <br>The given <see cref="BoxedEntry"/> is created from this type.</br>
        /// </summary>
        protected virtual Type GuiLayoutEntryType => GUIAdditionals.ImguiAssembly.GetType("UnityEngine.GUILayoutEntry", true);
        /// <summary>
        /// Flags of the dictionary getting.
        /// </summary>
        protected virtual BindingFlags GetDictionaryFlags => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Minimum width allowed for the GUI.
        /// <br>This has to be explicitly defined or use <see cref="CalcWidth"/> method (return value is 'MinWidth').</br>
        /// </summary>
        public float MinWidth
        {
            get
            {
                return (float)Fields["minWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["minWidth"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Maximum width allowed for the GUI.
        /// </summary>
        public float MaxWidth
        {
            get
            {
                return (float)Fields["maxWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["maxWidth"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Minimum height allowed for the GUI.
        /// <br>This has to be explicitly defined or use <see cref="CalcHeight"/> method (return value is 'MinHeight').</br>
        /// </summary>
        public float MinHeight
        {
            get
            {
                return (float)Fields["minHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["minHeight"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Maximum height allowed for the GUI.
        /// </summary>
        public float MaxHeight
        {
            get
            {
                return (float)Fields["maxHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["maxHeight"].SetValue(BoxedEntry, value);
            }
        }

        /// <summary>
        /// Rectangle of this <see cref="GUILayoutEntry"/>.
        /// <br>
        /// Changing this only affects this <see cref="GUILayoutEntry"/>, it is not a property.
        /// In fact most properties inside this <see cref="InternalGUILayoutEntry"/> wrappers are actually not properties, they are just fields wrapped.
        /// </br>
        /// </summary>
        public Rect EntryRect
        {
            get
            {
                return (Rect)Fields["rect"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["rect"].SetValue(BoxedEntry, value);
            }
        }

        /// <summary>
        /// <see cref="GUIStyle"/> stretching toggle. Value can be 0 or 1 in the base implementation of <see cref="GUILayoutEntry"/>.
        /// </summary>
        public int StretchWidth
        {
            get
            {
                return (int)Fields["stretchWidth"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["stretchWidth"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// <see cref="GUIStyle"/> stretching toggle. Value can be 0 or 1 in the base implementation of <see cref="GUILayoutEntry"/>.
        /// </summary>
        public int StretchHeight
        {
            get
            {
                return (int)Fields["stretchHeight"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["stretchHeight"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// &lt;Undocumented&gt;
        /// <br>Has to do something with how the <see cref="GUIStyle"/> of this entry gets interpreted?</br>
        /// </summary>
        public bool IsConsideredForMargin
        {
            get
            {
                return (bool)Fields["consideredForMargin"].GetValue(BoxedEntry);
            }
            set
            {
                Fields["consideredForMargin"].SetValue(BoxedEntry, value);
            }
        }

        /// <summary>
        /// Style of this <see cref="GUILayoutEntry"/>.
        /// </summary>
        public GUIStyle Style
        {
            get
            {
                return (GUIStyle)Properties["style"].GetValue(BoxedEntry);
            }
            set
            {
                Properties["style"].SetValue(BoxedEntry, value);
            }
        }
        /// <summary>
        /// Left margin of the <see cref="Style"/> or the overriding <see cref="BoxedEntry"/>.
        /// </summary>
        public int MarginLeft => (int)Properties["marginLeft"].GetValue(BoxedEntry);
        /// <summary>
        /// Right margin of the <see cref="Style"/> or the overriding <see cref="BoxedEntry"/>.
        /// </summary>
        public int MarginRight => (int)Properties["marginRight"].GetValue(BoxedEntry);
        /// <summary>
        /// Top margin of the <see cref="Style"/> or the overriding <see cref="BoxedEntry"/>.
        /// </summary>
        public int MarginTop => (int)Properties["marginTop"].GetValue(BoxedEntry);
        /// <summary>
        /// Bottom margin of the <see cref="Style"/> or the overriding <see cref="BoxedEntry"/>.
        /// </summary>
        public int MarginBottom => (int)Properties["marginBottom"].GetValue(BoxedEntry);

        /// <summary>
        /// Creates an empty <see cref="GUILayoutEntry"/>.
        /// </summary>
        public InternalGUILayoutEntry()
        {
            m_BoxedEntry = Activator.CreateInstance(GuiLayoutEntryType);
        }

        /// <summary>
        /// Creates a <see cref="InternalGUILayoutEntry"/> from another <see cref="GUILayoutEntry"/>.
        /// <br>This is <b>NOT</b> a copy constructor. It does not copy the <paramref name="entry"/>.</br>
        /// </summary>
        /// <param name="entry">The <see cref="GUILayoutEntry"/> to create from. This cannot be null and any other type than the <see cref="GuiLayoutEntryType"/>.</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public InternalGUILayoutEntry(object entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry), "[InternalGUILayoutEntry::ctor] Given parameter was null.");
            }

            // Type is same
            // Note : This behaviour could be unintended and will make the entries share the same value.
            // TODO : Don't do this until i create a Copy function/ctor
            // if (entry is InternalGUILayoutEntry typedEntry)
            // {
            //     m_BoxedEntry = typedEntry.BoxedEntry;
            //     return;
            // }

            if (!GuiLayoutEntryType.IsAssignableFrom(entry.GetType()))
            {
                throw new ArgumentException($"[InternalGUILayoutEntry::ctor] Given parameter's type doesn't inherit from \"{GuiLayoutEntryType}\".", nameof(entry));
            }

            m_BoxedEntry = entry;
        }

        /// <summary>
        /// Creates a <see cref="GUILayoutEntry"/> with predefined settings.
        /// </summary>
        public InternalGUILayoutEntry(float minWidth, float maxWidth, float minHeight, float maxHeight, GUIStyle style)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            Style = style;
        }
        /// <summary>
        /// Creates a <see cref="GUILayoutEntry"/> with predefined settings.
        /// </summary>
        public InternalGUILayoutEntry(float minWidth, float maxWidth, float minHeight, float maxHeight, GUIStyle style, GUILayoutOption[] options)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            Style = style;
            ApplyOptions(options);
        }

        /// <summary>
        /// Calculates the width of this <see cref="GUILayoutEntry"/> if applicable.
        /// </summary>
        /// <returns>
        /// The <see cref="MinWidth"/> that the width that was assumed to be calculated into.
        /// Since the class is internal the returned width will not be taken to account, so this method cannot be overriden.
        /// </returns>
        public float CalculateWidth()
        {
            Methods["CalcWidth"].Invoke(BoxedEntry, null);
            return MinWidth;
        }
        /// <summary>
        /// Calculates the height of this <see cref="GUILayoutEntry"/> if applicable.
        /// </summary>
        /// <returns>The <see cref="MinHeight"/> that the height that was assumed to be calculated into.</returns>
        public float CalculateHeight()
        {
            Methods["CalcHeight"].Invoke(BoxedEntry, null);
            return MinHeight;
        }

        /// <summary>
        /// Sets the <see cref="EntryRect"/> in a horizontal context.
        /// </summary>
        public void SetHorizontal(float x, float width)
        {
            Rect prevEntryRect = EntryRect;
            EntryRect = new Rect(x, prevEntryRect.y, width, prevEntryRect.height);
        }
        /// <summary>
        /// Sets the <see cref="EntryRect"/> in a horizontal context.
        /// </summary>
        public void SetVertical(float y, float height)
        {
            Rect prevEntryRect = EntryRect;
            EntryRect = new Rect(prevEntryRect.x, y, prevEntryRect.width, height);
        }

        /// <summary>
        /// Applies the given <see cref="GUILayoutOption"/> array to this entry.
        /// </summary>
        /// <param name="options">Options to apply. This method doesn't throw if the given options list is empty/null.</param>
        public void ApplyOptions(GUILayoutOption[] options)
        {
            Methods["ApplyOptions"].Invoke(BoxedEntry, new object[] { options });
        }

        /// <summary>
        /// Returns the default string override of <see cref="BoxedEntry"/>.
        /// </summary>
        public override string ToString()
        {
            return BoxedEntry.ToString();
        }

        /// <summary>
        /// Returns whether if the boxed entries are the same.
        /// </summary>
        public virtual bool Equals(InternalGUILayoutEntry other)
        {
            if (other is null)
            {
                return false;
            }

            return BoxedEntry == other.BoxedEntry;
        }
    }
}
