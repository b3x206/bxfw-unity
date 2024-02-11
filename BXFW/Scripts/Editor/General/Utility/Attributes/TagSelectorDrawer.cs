using BXFW.Tools.Editor;

using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws a dropdown with the ability to select tags.
    /// <br>
    /// The selected elements are standard <see cref="SearchDropdownElement"/>s,
    /// but some special selections may require handling in the <see cref="SearchDropdown.OnElementSelectedEvent"/>.
    /// </br>
    /// </summary>
    public class TagSelectorDropdown : SearchDropdown
    {
        /// <summary>
        /// A string value of a <see cref="SearchDropdownElement"/> for the TagSelector that opens the 'Add Tag' menu.
        /// </summary>
        public const string AddTagString = "Add Tag..";

        /// <summary>
        /// A string value of a <see cref="SearchDropdownElement"/> for the TagSelector that sets the tag null.
        /// </summary>
        public const string EmptyTagString = "<set null>";

        /// <summary>
        /// Whether if this tag 
        /// </summary>
        public readonly bool addEmptyTagOption;

        protected override SearchDropdownElement BuildRoot()
        {
            // Get the tags
            SearchDropdownElement rootElement = new SearchDropdownElement("Select Tag", InternalEditorUtility.tags.Length);

            if (addEmptyTagOption)
            {
                rootElement.Add(new SearchDropdownElement(EmptyTagString));
                rootElement.Add(new SearchDropdownSeperatorElement());
            }

            foreach (string tag in InternalEditorUtility.tags)
            {
                if (tag == AddTagString || tag == EmptyTagString)
                {
                    throw new Exception($"[TagSelectorDrawer::BuildRoot] The given tag \"{tag}\" is a reserved 'TagSelectorDrawer' name. Please name your tags better.");
                }

                rootElement.Add(new SearchDropdownElement(tag));
            }

            rootElement.Add(new SearchDropdownSeperatorElement());
            rootElement.Add(new SearchDropdownElement(AddTagString));

            return rootElement;
        }

        protected override void OnElementSelected(SearchDropdownElement element)
        {
            try
            {
                if (element.content.text == AddTagString)
                {
                    // Well, I do have to do reflection. Too bad! Love to see that everything cool is 'internal'
                    // ..
                    // internal class TagManagerInspector : ProjectSettingsBaseEditor
                    // {
                    //     ..
                    //     internal enum InitialExpansionState { None=0,Tags=1,Layers=2,SortingLayers=3 }
                    //     ..
                    // }
                    // TagManagerInspector.ShowWithInitialExpansion(TagManagerInspector.InitialExpansionState.Tags);
                    // --
                    Type tagManagerType = typeof(EditorWindow).Assembly.GetType("UnityEditor.TagManagerInspector", true);
                    Type initialExpansionStateType = tagManagerType.GetNestedType("InitialExpansionState", BindingFlags.Public | BindingFlags.NonPublic);

                    MethodInfo showWithInitialExpansionMethod = tagManagerType.GetMethod("ShowWithInitialExpansion", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    // Calling this should dismiss the rest of the GUI
                    showWithInitialExpansionMethod.Invoke(null, new object[] { Enum.ToObject(initialExpansionStateType, 1) });
                }
            }
            catch
            {
                // Probably have to do this to anything that does something with reflection, as a version switching disaster could be awaiting me.
                Debug.LogError("[BXFW::TagSelectorDropdown::OnElementSelected] A reflection <b>error</b> has occured. This may happen if unity has changed somethings in their internals. Please report or try fixing if you want to.");
                throw;
            }
        }

        public TagSelectorDropdown()
        { }

        public TagSelectorDropdown(bool addEmptyTagOption)
        {
            this.addEmptyTagOption = addEmptyTagOption;
        }
    }

    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        private const float WarningBoxHeight = 32f;
        private const string DefaultSelectedTag = "Untagged";
        private TagSelectorAttribute Attribute => attribute as TagSelectorAttribute;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                return WarningBoxHeight;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary>
        /// Shows a tag selector with less boilerplate.
        /// </summary>
        private TagSelectorDropdown ShowTagSelector(Rect parentButtonRect, SerializedProperty property)
        {
            TagSelectorDropdown dropdown = new TagSelectorDropdown(Attribute.showEmptyOption);
            SerializedObject tagParentSO = new SerializedObject(property.serializedObject.targetObjects);
            SerializedProperty tagTargetProperty = tagParentSO.FindProperty(property.propertyPath);
            dropdown.OnElementSelectedEvent += (SearchDropdownElement element) =>
            {
                // Note that we (who's we? idk but this is OSS so it's our code) need to check the string
                // (if it's the special null string then don't apply)
                if (element.content.text == TagSelectorDropdown.EmptyTagString)
                {
                    tagTargetProperty.stringValue = string.Empty;
                }
                else
                {
                    tagTargetProperty.stringValue = element.content.text;
                }

                tagParentSO.ApplyModifiedProperties();

                tagParentSO.Dispose();
                tagTargetProperty.Dispose();
            };
            dropdown.OnDiscardEvent += () =>
            {
                tagParentSO.Dispose();
                tagTargetProperty.Dispose();
            };
            dropdown.Show(parentButtonRect);

            return dropdown;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(
                    position,
                    $"Warning : Usage of 'TagSelectorAttribute' on field \"{property.type} {property.name}\" even though the field type isn't 'System.String'.",
                    MessageType.Warning
                );

                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.BeginChangeCheck();

            // If the field's string value isn't untagged (default), set the value
            if (!Attribute.showEmptyOption && string.IsNullOrWhiteSpace(property.stringValue))
            {
                property.stringValue = InternalEditorUtility.tags.FirstOrDefault(DefaultSelectedTag); ;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            // To be able to show an "empty/null" tag, create a SearchDropdown, because unity's editor 'TagField' is not customizable
            // Thankfully, InternalEditorUtility.tags is public, thank you for not making me have to write reflection code again. (spoke too soon)
            // ..
            // Also have to create the 'Label' + 'Dropdown' positions manually but whatever it will do for now.

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
            EditorGUI.LabelField(labelRect, label);

            bool showMixedPrevious = EditorGUI.showMixedValue;

            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            Rect dropdownRect = new Rect(position.x + labelRect.width, position.y, Mathf.Max(position.width - labelRect.width, EditorGUIUtility.fieldWidth), position.height);
            string displayString = string.IsNullOrWhiteSpace(property.stringValue) ? "<string.Empty>" : property.stringValue;
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(displayString), FocusType.Keyboard))
            {
                ShowTagSelector(dropdownRect, property);
            }

            EditorGUI.showMixedValue = showMixedPrevious;

            // .. or if you don't want null tag capabilities, which makes the Attribute
            //    value useless, so just disable it from the attribute ..
            // property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        }
    }
}
