using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Allows for selection of a <see cref="MemberInfo"/> inside given <see cref="targetType"/>.
    /// <br>By default, only allows <see cref="MemberTypes.Property"/> and <see cref="MemberTypes.Field"/>, but can allow all members.</br>
    /// </summary>
    public class MemberInfoSelectorDropdown : SearchDropdown
    {
        // .. At this rate i am just going to reimplement my own crappy intellisense lol
        // TODO : Allow for item to be able to return it's own set path.

        /// <summary>
        /// A <see cref="MemberInfo"/> selection predicate delegate typedef.
        /// </summary>
        /// <param name="member">The member to select.</param>
        /// <param name="accessModifierName">The access modifier name. Can be left blank/unassigned.</param>
        /// <param name="memberTypeName">The type name. Can be left blank/unassigned.</param>
        /// <returns>Whether if this selection delegate passes the predicate test.</returns>
        public delegate bool SelectionDelegate(MemberInfo member, ref string accessModifierName, ref string memberTypeName);

        /// <summary>
        /// Iterate the given <see cref="MemberInfo"/>, if the <see cref="MemberInfo"/> contains more members to iterate.
        /// <br>The </br>
        /// </summary>
        /// <param name="iterableMemberInfo"></param>
        /// <returns></returns>
        public delegate IEnumerable<MemberInfo> IterateMemberDelegate(MemberInfo iterableMemberInfo);

        /// <summary>
        /// Item that contains extra data for selected.
        /// </summary>
        public class Item : SearchDropdownElement
        {
            /// <summary>
            /// MemberInfo that owns the <see cref="memberInfo"/>
            /// </summary>
            public readonly MemberInfo parentMemberInfo;
            /// <summary>
            /// The member info that this item contains.
            /// </summary>
            public readonly MemberInfo memberInfo;

            public Item(string label, MemberInfo rootInfo, MemberInfo info) : base(label)
            {
                memberInfo = info;
                parentMemberInfo = rootInfo;
            }

            public Item(string label, int childrenCapacity, MemberInfo rootInfo, MemberInfo info) : base(label, childrenCapacity)
            {
                memberInfo = info;
                parentMemberInfo = rootInfo;
            }
        }

        protected internal override bool AllowRichText => true;
        protected internal override bool AllowSelectionOfElementsWithChild => true;

        /// <summary>
        /// The selected target type that this dropdown was generated for.
        /// </summary>
        public readonly Type targetType;
        /// <summary>
        /// A <see cref="MemberInfo"/> selection predicate.
        /// </summary>
        public readonly SelectionDelegate memberSelectionPredicate;
        /// <summary>
        /// A child member iteration predicate.
        /// <br>This should only iterate one layer of the given <see cref="MemberInfo"/>.</br>
        /// <br/>
        /// <br>This value can be null.</br>
        /// </summary>
        public readonly IterateMemberDelegate memberChildIterationPredicate;

        /// <summary>
        /// Predicate used to select the member.
        /// <br>By default, allows <see cref="MemberTypes.Field"/> and <see cref="MemberTypes.Property"/>.</br>
        /// </summary>
        private static bool DefaultSelectionPredicate(MemberInfo info, ref string accessModifierName, ref string typeName)
        {
            if (info is PropertyInfo propertyInfo)
            {
                accessModifierName = propertyInfo.GetAccessors().First().IsPublic ? "public" : "private/protected";
                typeName = propertyInfo.PropertyType.Name;

                return propertyInfo.CanRead && propertyInfo.CanWrite;
            }
            if (info is FieldInfo fieldInfo)
            {
                accessModifierName = fieldInfo.IsPublic ? "public" : "private/protected";
                typeName = fieldInfo.FieldType.Name;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Predicate used to iterate a <see cref="MemberInfo"/>.
        /// <br>By default only iterates members with a valid "declaring" type (such as a FieldType or a PropertyType).</br>
        /// </summary>
        private static IEnumerable<MemberInfo> DefaultIterationPredicate(MemberInfo info)
        {
            if (info is FieldInfo fieldInfo)
            {
                foreach (MemberInfo childInfo in fieldInfo.FieldType.GetFields())
                {
                    yield return childInfo;
                }
            }
            if (info is PropertyInfo propertyInfo)
            {
                foreach (MemberInfo childInfo in propertyInfo.PropertyType.GetFields())
                {
                    yield return childInfo;
                }
            }
        }

        /// <summary>
        /// Recursively builds a <see cref="SearchDropdownElement"/> using a <see cref="IterateMemberDelegate"/>.
        /// </summary>
        /// <param name="parentInfo">The member info to iterate into it's children.</param>
        private Item BuildMemberInfoRecursive(MemberInfo rootInfo, MemberInfo createItemInfo)
        {
            // Accept + Create item
            string accessModifierName = string.Empty;
            string memberFieldTypeName = "<unknown>";

            if (!memberSelectionPredicate(createItemInfo, ref accessModifierName, ref memberFieldTypeName))
            {
                return null;
            }

            StringBuilder itemNameSb = new StringBuilder(64);
            if (!string.IsNullOrEmpty(accessModifierName))
            {
                itemNameSb.Append("<color=#569cd6>").Append(accessModifierName).Append("</color> ");
            }
            if (!string.IsNullOrEmpty(memberFieldTypeName))
            {
                itemNameSb.Append("<color=#2e9fa4>").Append(memberFieldTypeName).Append("</color> ");
            }
            itemNameSb.Append(createItemInfo.Name);
            Item infoElement = new Item(itemNameSb.ToString(), rootInfo, createItemInfo);

            // If the iteration predicate is empty, only use one layer depth elements.
            if (memberChildIterationPredicate == null)
            {
                return infoElement;
            }

            // Iterate the 'rootInfo'.
            foreach (MemberInfo member in memberChildIterationPredicate(createItemInfo))
            {
                Item createdMemberElement = BuildMemberInfoRecursive(rootInfo, member);
                // Don't add null members
                if (createdMemberElement == null)
                {
                    continue;
                }

                infoElement.Add(createdMemberElement);
            }

            return infoElement;
        }

        protected override SearchDropdownElement BuildRoot()
        {
            SearchDropdownElement rootItem = new SearchDropdownElement("Select Member Info");

            // Only draw public fields + properties with get+set
            MemberInfo[] rootTypeMembersArray = targetType.GetMembers();
            rootItem.ChildCapacity = rootTypeMembersArray.Length;

            foreach (MemberInfo member in rootTypeMembersArray)
            {
                Item memberItem = BuildMemberInfoRecursive(member, member);
                // MemberItem most likely didn't pass the tests.
                if (memberItem == null)
                {
                    continue;
                }

                rootItem.Add(memberItem);
            }

            return rootItem;
        }

        /// <inheritdoc cref="MemberInfoSelectorDropdown(Type, SelectionDelegate, IterateMemberDelegate)"/>
        public MemberInfoSelectorDropdown(Type target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "[MemberInfoSelectorDropdown::Create]");
            }

            targetType = target;
            memberSelectionPredicate = DefaultSelectionPredicate;
            memberChildIterationPredicate = DefaultIterationPredicate;
        }
        /// <inheritdoc cref="MemberInfoSelectorDropdown(Type, SelectionDelegate, IterateMemberDelegate)"/>
        public MemberInfoSelectorDropdown(Type target, bool allowMultiLevelMemberSelection) : this(target)
        {
            if (!allowMultiLevelMemberSelection)
            {
                memberChildIterationPredicate = null;
            }
        }
        /// <inheritdoc cref="MemberInfoSelectorDropdown(Type, SelectionDelegate, IterateMemberDelegate)"/>
        public MemberInfoSelectorDropdown(Type target, SelectionDelegate memberPredicate) : this(target)
        {
            if (memberPredicate != null)
            {
                memberSelectionPredicate = memberPredicate;
            }
        }
        /// <inheritdoc cref="MemberInfoSelectorDropdown(Type, SelectionDelegate, IterateMemberDelegate)"/>
        public MemberInfoSelectorDropdown(Type target, IterateMemberDelegate childIteratePredicate) : this(target)
        {
            memberChildIterationPredicate = childIteratePredicate;
        }
        /// <summary>
        /// Creates a MemberInfoSelectorDropdown.
        /// </summary>
        /// <param name="target">Target type to get the member infos from. This musn't be null.</param>
        /// <param name="memberPredicate">Predicate used to select the <see cref="MemberInfo"/>.</param>
        /// <param name="childIteratePredicate">
        /// Child MemberInfo iteration predicate. 
        /// This must only iterate 1 layer, anything more/recursive iterations, will cause errorenous behaviour.
        /// </param>
        public MemberInfoSelectorDropdown(Type target, SelectionDelegate memberPredicate, IterateMemberDelegate childIteratePredicate) : this(target)
        {
            if (memberPredicate != null)
            {
                memberSelectionPredicate = memberPredicate;
            }

            memberChildIterationPredicate = childIteratePredicate;
        }
    }
}
