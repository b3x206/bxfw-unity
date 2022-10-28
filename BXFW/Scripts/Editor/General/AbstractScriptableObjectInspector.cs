using BXFW.Tools.Editor;

using System;
using System.Reflection;

using UnityEditor;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Creates a ScriptableObject inspector.
    /// <br>Derive from this class and use the <see cref="CustomPropertyDrawer"/> attribute with same type as <typeparamref name="T"/>.</br>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractScriptableObjectInspector<T> : PropertyDrawer
        where T : ScriptableObject
    {
        /// <summary>
        /// Menu that contains the create names and delegates.
        /// </summary>
        protected GenericMenu typeMenus;
        /// <summary>
        /// The target SerializedObject.
        /// <br>This is assigned as the target object on <see langword="base"/>.<see cref="GetPropertyHeight(SerializedProperty, GUIContent)"/>.</br>
        /// </summary>
        protected SerializedObject SObject { get; private set; }

        /// <summary>
        /// Padding height between ui elements (so that it's not claustrophobic)
        /// </summary>
        protected virtual float HEIGHT_PADDING => 2;

        // TODO (low priority) :
        // AdvancedDropdown implementation on UnityEditor.IMGUI.Controls

        private float SingleLineHeight => EditorGUIUtility.singleLineHeight + HEIGHT_PADDING;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get types inheriting from player powerup on all assemblies
            if (typeMenus == null)
            {
                // Use a 'GenericMenu' as it's much more convenient than using EditorGUI.Popup
                typeMenus = new GenericMenu();

                // Get all assemblies
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsClass)
                        {
                            if (type.IsSubclassOf(typeof(T)) && type != typeof(T))
                            {
                                var pow = ScriptableObject.CreateInstance(type) as T;

                                typeMenus.AddItem(new GUIContent($"New {type.Name}"), false, () =>
                                {
                                    fieldInfo.SetValue(property.GetParentOfTargetField().Value, ScriptableObject.CreateInstance(type));
                                });
                            }
                        }
                    }
                }

                // No items
                if (typeMenus.GetItemCount() <= 0)
                    typeMenus.AddDisabledItem(new GUIContent($"Disabled (Make classes inheriting from '{typeof(T).Name}')"), true);
            }

            var propTarget = property.objectReferenceValue;
            var target = propTarget as T;

            // Check if object is null (generically)
            // If null, don't 
            if (target == null)
                return SingleLineHeight;

            if (!property.isExpanded)
                return SingleLineHeight;

            SObject ??= new SerializedObject(target);
            float h = 0f;
            SerializedProperty prop = SObject.GetIterator();
            bool expanded = true;
            while (prop.NextVisible(expanded))
            {
                if (prop.propertyPath == "m_Script")
                {
                    continue;
                }

                h += EditorGUI.GetPropertyHeight(prop, true) + HEIGHT_PADDING; // Add padding
                expanded = false; // used for the expand arrow of unity
            }

            // Add label height
            h += SingleLineHeight;

            return h;
        }

        private float currentY;
        private Rect GetPropertyRect(Rect position, SerializedProperty prop)
        {
            return GetPropertyRect(position, EditorGUI.GetPropertyHeight(prop, true));
        }
        private Rect GetPropertyRect(Rect position, float height = -1f)
        {
            // Reuse the copied struct
            position.y = currentY;
            position.height = height + HEIGHT_PADDING;

            // assuming that the height is added after first rect.
            if (height < 0f)
                height = SingleLineHeight;

            currentY += height + HEIGHT_PADDING;
            return position;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // The 'property' is for the object field that is only for assigning.
            EditorGUI.BeginProperty(position, label, property);
            currentY = position.y;

            // Gather target info
            // This works, because we are working with ScriptableObject's
            // Otherwise use BXFW's EditorAdditionals.GetTarget(SerializedProperty)
            var propTarget = property.objectReferenceValue;
            var target = propTarget as T;

            // GUI related
            var previousWidth = position.width;

            if (target == null)
            {
                EditorAdditionals.MakeDroppableAreaGUI(
                () => // OnDrag
                {
                    fieldInfo.SetValue(property.GetParentOfTargetField().Value, DragAndDrop.objectReferences[0]);
                },
                () => // ShouldAcceptDrag
                {
                    return DragAndDrop.objectReferences.Length == 1 &&
                        DragAndDrop.objectReferences[0].GetType() == typeof(T);
                }, position);

                position.width = previousWidth * .4f;
                GUI.Label(position, label);
                position.x += previousWidth * .4f;

                position.width = previousWidth * .45f;
                if (GUI.Button(position, "Assign Powerup (from child classes)", EditorStyles.popup))
                {
                    typeMenus.ShowAsContext();
                }
                position.x += previousWidth * .46f;

                position.width = previousWidth * .14f; // 1 - (.46f + .4f)
                if (GUI.Button(position, "Refresh"))
                {
                    typeMenus = null;
                }

                return;
            }

            // Property label
            Rect propFoldoutLabel = GetPropertyRect(position, SingleLineHeight); // width is equal to 'previousWidth'
            propFoldoutLabel.width = previousWidth * .85f;
            property.isExpanded = EditorGUI.Foldout(propFoldoutLabel, property.isExpanded, label);

            // Delete button (ScriptableObject)
            // This function is dangerous, and i don't care lol.
            propFoldoutLabel.x += previousWidth * .85f;
            propFoldoutLabel.width = previousWidth * .15f;
            if (GUI.Button(propFoldoutLabel, "Delete"))
            {
                Undo.DestroyObjectImmediate(target);
                EditorGUI.EndProperty();
                return;
            }

            // Main drawing
            if (property.isExpanded)
            {
                SObject ??= new SerializedObject(target);

                // Draw fields
                EditorGUI.indentLevel += 1;
                SerializedProperty prop = SObject.GetIterator();
                bool expanded = true;
                while (prop.NextVisible(expanded))
                {
                    if (prop.propertyPath == "m_Script")
                    {
                        continue;
                    }

                    EditorGUI.PropertyField(GetPropertyRect(position, prop), prop, true);

                    expanded = false;
                }
                EditorGUI.indentLevel -= 1;

                SObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }
    }
}