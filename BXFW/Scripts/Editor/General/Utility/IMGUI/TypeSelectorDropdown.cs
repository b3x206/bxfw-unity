using System;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A type selector that categorizes and has the ability to select types.
    /// </summary>
    public class TypeSelectorDropdown : SearchDropdown
    {
        /// <summary>
        /// An 'SearchDropdownElement' that contains extra data.
        /// </summary>
        public class Item : SearchDropdownElement
        {
            /// <summary>
            /// Assembly qualified (and forklift certified) name for the given type.
            /// </summary>
            public string assemblyQualifiedName;

            public Item(string name, string typeAssemblyQualifiedName) : base(name)
            {
                assemblyQualifiedName = typeAssemblyQualifiedName;
            }
            public Item(string name, string tooltip, string typeAssemblyQualifiedName) : base(name, tooltip)
            {
                assemblyQualifiedName = typeAssemblyQualifiedName;
            }
        }

        protected internal override bool AllowRichText => true;

        private readonly Type m_selectedType;
        private readonly bool m_addNoneOption = true;
        private readonly Predicate<Type> m_filterPredicate;
        /// <summary>
        /// List of assembly flags to filter.
        /// </summary>
        public AssemblyFlags filterFlags = AssemblyFlags.All & ~AssemblyFlags.Dynamic;

        protected internal override StringComparison SearchComparison => StringComparison.OrdinalIgnoreCase;

        protected override SearchDropdownElement BuildRoot()
        {
            SearchDropdownElement rootItem = new SearchDropdownElement("Type Categories");
            if (m_addNoneOption)
            {
                rootItem.Add(new Item("None", string.Empty));
            }

            foreach (KeyValuePair<AssemblyFlags, Type[]> domainCategoryType in TypeListProvider.DomainTypesList)
            {
                if ((domainCategoryType.Key & filterFlags) != domainCategoryType.Key)
                {
                    continue;
                }

                SearchDropdownElement categoryItem = new SearchDropdownElement($"<color=#a2d4a3>{domainCategoryType.Key}</color>", domainCategoryType.Value.Length);
                rootItem.Add(categoryItem);

                foreach (Type t in domainCategoryType.Value)
                {
                    if (!m_filterPredicate(t))
                    {
                        continue;
                    }

                    string typeIdentifier = string.Empty;
                    if (t.IsClass)
                    {
                        typeIdentifier = "<color=#4ec9b0>C</color>";
                    }
                    else if (t.IsEnum)
                    {
                        // Enum is also value type, so do it before?
                        typeIdentifier = "<color=#b8d797>E</color>";
                    }
                    else if (t.IsValueType)
                    {
                        typeIdentifier = "<color=#86b86a>S</color>";
                    }
                    else if (t.IsInterface)
                    {
                        typeIdentifier = "<color=#b8d7a3>I</color>";
                    }

                    Item categoryChildItem = new Item($"{typeIdentifier} | <color=white>{t.FullName}</color>", t.FullName, t.AssemblyQualifiedName);
                    categoryChildItem.Selected = t == m_selectedType;
                    categoryItem.Add(categoryChildItem);
                }

                // Make element non-selectable if the predicate ignored all
                if (!categoryItem.HasChildren)
                {
                    rootItem.Remove(categoryItem);
                }
            }

            return rootItem;
        }

        private static bool DefaultFilterPredicate(Type t)
        {
            return t.IsPublic;
        }

        public TypeSelectorDropdown(Type selected)
        {
            m_selectedType = selected;
            m_filterPredicate = DefaultFilterPredicate;
        }
        public TypeSelectorDropdown(Type selected, bool addNoneOption)
        {
            m_selectedType = selected;
            m_addNoneOption = addNoneOption;
            m_filterPredicate = DefaultFilterPredicate;
        }
        public TypeSelectorDropdown(Type selected, Predicate<Type> filterPredicate)
        {
            m_selectedType = selected;
            m_filterPredicate = filterPredicate ?? DefaultFilterPredicate;
        }
        public TypeSelectorDropdown(Type selected, Predicate<Type> filterPredicate, bool addNoneOption)
        {
            m_selectedType = selected;
            m_addNoneOption = addNoneOption;
            m_filterPredicate = filterPredicate ?? DefaultFilterPredicate;
        }
    }
}
