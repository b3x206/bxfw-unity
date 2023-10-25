using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    /// <summary>
    /// Draws the '<see cref="Texture2D"/>' inspector for sprites.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(BigSpriteFieldAttribute))]
    public class BigSpriteFieldDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private float targetBoxRectHeight
        {
            get
            {
                var targetAttribute = attribute as BigSpriteFieldAttribute;

                return targetAttribute.spriteBoxRectHeight;
            }
        }
        private PropertyTargetInfo target;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.fieldInfo.FieldType != typeof(Sprite))
            {
                // Same story, calling 'GetPropertyHeight' before drawing gui or not allowing to dynamically change height while drawing is dumb
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += targetBoxRectHeight; // Hardcode the size as unity doesn't change it.
            }

            return EditorGUI.GetPropertyHeight(property, label, true) + addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (target.fieldInfo == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.fieldInfo.FieldType != typeof(Sprite))
            {
                EditorGUI.HelpBox(
                    position,
                    string.Format("Warning : Usage of 'InspectorBigSpriteFieldDrawer' on field \"{0} {1}\" even though the field type isn't sprite.", property.type, property.name),
                    MessageType.Warning
                );
                return;
            }

            EditorGUI.BeginChangeCheck();

            // fixes position.height being incorrect on some cases
            position.height = EditorGUI.GetPropertyHeight(property, label, true) + targetBoxRectHeight;
            Sprite setValue = (Sprite)EditorGUI.ObjectField(position, new GUIContent(property.displayName, property.tooltip), property.objectReferenceValue, typeof(Sprite), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (property.objectReferenceValue != null)
                {
                    Undo.RecordObject(property.objectReferenceValue, "set sprite");
                }

                property.objectReferenceValue = setValue;
            }
        }
    }

    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    public class InspectorLineDrawer : DecoratorDrawer
    {
        private InspectorLineAttribute targetAttribute;

        public override float GetHeight()
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            return targetAttribute.GetYPosHeightOffset() * 2f;
        }

        public override void OnGUI(Rect position)
        {
            targetAttribute ??= (InspectorLineAttribute)attribute;

            position.y += targetAttribute.GetYPosHeightOffset() / 2f;
            GUIAdditionals.DrawUILine(position, targetAttribute.Color, targetAttribute.LineThickness, targetAttribute.LinePadding);
        }
    }

    /// <summary>
    /// Draws the property affected by the <see cref="InspectorConditionalDrawAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(InspectorConditionalDrawAttribute))]
    public class InspectorConditionalAttributeDrawer : PropertyDrawer
    {
        private PropertyDrawer targetTypeCustomDrawer;
        private bool UseCustomDrawer => targetTypeCustomDrawer != null;
        /// <summary>
        /// The target boolean value.
        /// </summary>
        private bool drawField = true;
        /// <summary>
        /// Is true if the target field is incorrect.
        /// </summary>
        private bool drawWarning = false;
        private InspectorConditionalDrawAttribute Attribute => (InspectorConditionalDrawAttribute)attribute;

        private const float WARN_BOX_HEIGHT = 32f;
        private const BindingFlags TARGET_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // First called method before OnGUI
            // This also resets 'drawField'
            var parentPair = property.GetParentOfTargetField();
            // Try getting the FieldInfo
            FieldInfo targetBoolFieldInfo = fieldInfo.DeclaringType.GetField(Attribute.boolFieldName, TARGET_FLAGS);
            if (targetBoolFieldInfo == null)
            {
                // Try getting the PropertyInfo
                PropertyInfo targetBoolPropertyInfo = fieldInfo.DeclaringType.GetProperty(Attribute.boolFieldName, TARGET_FLAGS);

                if (targetBoolPropertyInfo == null)
                {
                    // Both failed, return the height
                    drawWarning = true;
                    return WARN_BOX_HEIGHT;
                }
                else
                {
                    drawField = (bool)targetBoolPropertyInfo.GetValue(parentPair.value);
                }
            }
            else
            {
                drawField = (bool)targetBoolFieldInfo.GetValue(parentPair.value);
            }

            // A no fail condition
            if (Attribute.ConditionInverted)
                drawField = !drawField;
            
            drawWarning = false;

            if (!drawField)
                return 0f;

            targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);
            return UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Warning
            if (drawWarning)
            {
                label = EditorGUI.BeginProperty(position, label, property);
                EditorGUI.HelpBox(position, string.Format("[ConditionalDrawAttribute] Attribute has incorrect target '{0}' for value '{1}'.", Attribute.boolFieldName, label.text), MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            // No draw
            if (!drawField)
                return;

            // Draw (with CustomDrawer)
            if (UseCustomDrawer)
            {
                targetTypeCustomDrawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyViewAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        private PropertyDrawer targetTypeCustomDrawer;
        private bool UseCustomDrawer => targetTypeCustomDrawer != null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);

            return UseCustomDrawer ? targetTypeCustomDrawer.GetPropertyHeight(property, label) : EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (EditorGUI.DisabledScope disabled = new EditorGUI.DisabledScope(true))
            {
                if (UseCustomDrawer)
                {
                    // yeah, it will display 'No GUI implemented'. definitely.
                    // Use this to not default into the defualt property drawer.
                    targetTypeCustomDrawer.OnGUI(position, property, label);
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(SortedArrayAttribute))]
    public class SortedArrayDrawer : PropertyDrawer
    {
        private class ConvertibleObjectList : List<object>
        {
            public object ToIEnumerableType(Type enumerableType)
            {
                Array array;
                if (enumerableType == typeof(IEnumerable))
                {
                    // Return an non-typesafe array.
                    array = new object[Count];
                }
                else if (enumerableType.IsGenericType && enumerableType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    // Create the typed array. (has IEnumerable with type)
                    array = Array.CreateInstance(enumerableType.GetGenericArguments()[0], Count);
                }
                else
                {
                    throw new InvalidOperationException(string.Format("[SortedArrayDrawer::ConvertibleObjectList::ToIEnumerableType] Given type '{0}' is not a IEnumerable type.", enumerableType));
                }

                for (int i = 0; i < Count; i++)
                {
                    array.SetValue(this[i], i);
                }
                return array;
            }
        }

        private const float warnHelpBoxRectHeight = 22f;
        private SortedArrayAttribute SAttribute => attribute as SortedArrayAttribute;
        private const float DR_PADDING = 2f;

        /// <summary>
        /// Must be true if the previously drawn property's type was integral or it has the 'IComparable'.
        /// </summary>
        private bool propertyTypeValid;
        private bool propertyParentTypeArray;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            propertyParentTypeArray = property.GetParentOfTargetField().fieldInfo.FieldType
                .GetInterfaces()
                .Any(i => i == typeof(IEnumerable) || i == typeof(IEnumerable<>));
            propertyTypeValid = (propertyParentTypeArray && property.GetPropertyType()
                .GetInterfaces()
                .Any(i => i == typeof(IComparable) || i == typeof(IComparable<>))) 
                || property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;

            // Since we can't intercept the 'OnGUI' of the parent array (this PropertyDrawer will be shown per element, we will just get the parent array)
            // Just give the 'GetPropertyHeight'
            if (!propertyTypeValid)
            {
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                // EditorGUI.GetPropertyHeight gets the height with respect to the parent type drawer.
                // (and hopefully ignores the attribute 'GetPropertyHeight's, otherwise this will crash unity)
                addHeight += EditorGUI.GetPropertyHeight(property) + DR_PADDING;
            }

            return addHeight;
        }

        /// <summary>
        /// Returns whether if the array is sorted.
        /// </summary>
        private static bool IsSorted(List<object> list, bool reverse)
        {
            // Assume that this method can only be called if the list has a 'IComparable'
            // But i will just use 'Comparer.Default'
            for (int iter = 0; iter < list.Count - 1; iter++)
            {
                int currentIdx = !reverse ? iter : list.Count - (iter + 1);
                int nextIdx = !reverse ? iter + 1 : list.Count - (iter + 2);

                if (Comparer<object>.Default.Compare(list[currentIdx], list[nextIdx]) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            if (propertyTypeValid)
            {
                var parentObject = property.serializedObject.targetObject; // This returns the parent object. (array is also parent)
                var parentArrayPair = property.GetTarget(); // This returns the array itself anyways (even if we call GetParentOfTargetField with 1 depth)
                // The element index to draw
                int propertyIndex = property.GetPropertyParentArrayIndex();

                // Parent array itself
                // (since normal IComparable and generic IComparable are incompatible with casting, just assume that these objects have a Method that has CompareTo)
                // Get the IEnumerable interface type
                Type arrayEnumerableType = null;
                {
                    Type[] ints = parentArrayPair.fieldInfo.FieldType.GetInterfaces();
                    foreach (Type type in ints)
                    {
                        // Calling 'GetGenericTypeDefinition' makes the type open.
                        bool breakOnType = (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || type == typeof(IEnumerable);

                        // Get the type for 'GetEnumerator'ing manually
                        if (breakOnType)
                        {
                            arrayEnumerableType = type;
                            break;
                        }
                    }
                }

                // Initilaze list (with type constraints)
                ConvertibleObjectList parentArrayList = new ConvertibleObjectList();

                // GetEnumerator();
                MethodInfo miGetEnumerator = arrayEnumerableType.GetMethod("GetEnumerator");
                object enumerator = miGetEnumerator.Invoke(fieldInfo.GetValue(parentObject), null);

                // Iterator methods (that comes with 'GetEnumerator')
                MethodInfo miMoveNext = enumerator.GetType().GetMethod("MoveNext");
                PropertyInfo miCurrentProperty = enumerator.GetType().GetProperty("Current");

                // Iterate over the elements using reflection + add them to the array.
                while ((bool)miMoveNext.Invoke(enumerator, null))
                {
                    object element = miCurrentProperty.GetValue(enumerator);
                    parentArrayList.Add(element);
                }

                if (!IsSorted(parentArrayList, SAttribute.Reverse))
                {
                    parentArrayList.Sort(Comparer<object>.Default);
                    if (SAttribute.Reverse)
                    {
                        // Reverse the sorting (if reverse attribute)
                        parentArrayList.Reverse();
                    }

                    EditorUtility.SetDirty(property.serializedObject.targetObject); // undoless 'something changed'
                    parentArrayPair.fieldInfo.SetValue(parentObject, parentArrayList.ToIEnumerableType(arrayEnumerableType));
                }

                if (property.propertyType == SerializedPropertyType.Float)
                {
                    EditorGUI.BeginChangeCheck();
                    float lower = !SAttribute.Reverse ?
                        (propertyIndex == 0 ? float.MinValue : (float)parentArrayList[propertyIndex - 1]) :
                        (propertyIndex == parentArrayList.Count - 1 ? float.MinValue : (float)parentArrayList[propertyIndex + 1]);
                    float upper = !SAttribute.Reverse ?
                        (propertyIndex == parentArrayList.Count - 1 ? float.MaxValue : (float)parentArrayList[propertyIndex + 1]) :
                        (propertyIndex == 0 ? float.MaxValue : (float)parentArrayList[propertyIndex - 1]);

                    float v = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), lower, upper);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set sorted clamped float");
                        property.floatValue = v;
                    }
                }
                else if (property.propertyType == SerializedPropertyType.Integer)
                {
                    EditorGUI.BeginChangeCheck();
                    int lower = !SAttribute.Reverse ?
                        (propertyIndex == 0 ? int.MinValue : (int)parentArrayList[propertyIndex - 1]) :
                        (propertyIndex == parentArrayList.Count - 1 ? int.MinValue : (int)parentArrayList[propertyIndex + 1]);
                    int upper = !SAttribute.Reverse ?
                        (propertyIndex == parentArrayList.Count - 1 ? int.MaxValue : (int)parentArrayList[propertyIndex + 1]) :
                        (propertyIndex == 0 ? int.MaxValue : (int)parentArrayList[propertyIndex - 1]);

                    int v = Mathf.Clamp(EditorGUI.IntField(position, label, property.intValue), lower, upper);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set sorted clamped int");
                        property.intValue = v;
                    }
                }
                else
                {
                    // Sort the array (according to the IComparable) if the field was changed.
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(position, property);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set element in sorted array");

                        parentArrayList.Sort(Comparer<object>.Default);
                        // Set the entire array to avoid issues (as IEnumerable)
                        parentArrayPair.fieldInfo.SetValue(parentObject, parentArrayList.ToIEnumerableType(arrayEnumerableType));
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "Given array type isn't valid. Please use either array of int or float (or anything that implements IComparable).", MessageType.Warning);
            }
        }
    }

    [CustomPropertyDrawer(typeof(ClampAttribute))]
    public class ClampDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private ClampAttribute CAttribute => attribute as ClampAttribute;
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Integer &&
                property.propertyType != SerializedPropertyType.Float &&
                // Supported by self types
                property.type != typeof(MinMaxValue).Name && property.type != typeof(MinMaxValueInt).Name)
            {
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }

            return addHeight;
        }

        private PropertyDrawer targetTypeCustomDrawer;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            if (property.propertyType == SerializedPropertyType.Float)
            {
                EditorGUI.BeginChangeCheck();
                // Can't just cast float to double because reasons
                if (property.type.Contains("float", StringComparison.Ordinal))
                {
                    float v = Mathf.Clamp(EditorGUI.FloatField(position, label, property.floatValue), (float)CAttribute.min, (float)CAttribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set clamped float");
                        property.doubleValue = v;
                    }
                }
                else // Assume it's a double
                {
                    double v = Math.Clamp(EditorGUI.DoubleField(position, label, property.doubleValue), CAttribute.min, CAttribute.max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "set clamped double");
                        property.doubleValue = v;
                    }
                }
            }
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                EditorGUI.BeginChangeCheck();
                long v = Math.Clamp(EditorGUI.LongField(position, label, property.intValue), (long)CAttribute.min, (long)CAttribute.max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped int");
                    property.longValue = v;
                }
            }
            // Check if property is a valid type
            // Currently supported (by the PropertyDrawer) are
            // > MinMaxValue, MinMaxValueInt
            else if (property.type == typeof(MinMaxValue).Name || property.type == typeof(MinMaxValueInt).Name)
            {
                targetTypeCustomDrawer ??= EditorAdditionals.GetTargetPropertyDrawer(this);
                targetTypeCustomDrawer.OnGUI(position, property, label);
            }
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either int or float.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(ClampVectorAttribute))]
    public class ClampVectorDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private ClampVectorAttribute CAttribute => attribute as ClampVectorAttribute;
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType != SerializedPropertyType.Vector2 &&
                property.propertyType != SerializedPropertyType.Vector3 &&
                property.propertyType != SerializedPropertyType.Vector4)
            {
                addHeight += warnHelpBoxRectHeight;
            }
            else
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            #region Vector2
            if (property.propertyType == SerializedPropertyType.Vector2)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2Field(position, label, property.vector2Value);
                var vClamped = new Vector2(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX),
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector2Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector2IntField(position, label, property.vector2IntValue);
                var vClamped = new Vector2Int(
                    Mathf.Clamp(v.x, (int)CAttribute.minX, (int)CAttribute.maxX),
                    Mathf.Clamp(v.y, (int)CAttribute.minY, (int)CAttribute.maxY)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector2IntValue = vClamped;
                }
            }
            #endregion
            #region Vector3
            else if (property.propertyType == SerializedPropertyType.Vector3)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3Field(position, label, property.vector3Value);
                var vClamped = new Vector3(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX),
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY),
                    Mathf.Clamp(v.z, (float)CAttribute.minZ, (float)CAttribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3Value = vClamped;
                }
            }
            else if (property.propertyType == SerializedPropertyType.Vector3Int)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector3IntField(position, label, property.vector3IntValue);
                var vClamped = new Vector3Int(
                    Mathf.Clamp(v.x, (int)CAttribute.minX, (int)CAttribute.maxX),
                    Mathf.Clamp(v.y, (int)CAttribute.minY, (int)CAttribute.maxY),
                    Mathf.Clamp(v.z, (int)CAttribute.minZ, (int)CAttribute.maxZ)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector3IntValue = vClamped;
                }
            }
            #endregion
            #region Vector4
            else if (property.propertyType == SerializedPropertyType.Vector4)
            {
                EditorGUI.BeginChangeCheck();
                var v = EditorGUI.Vector4Field(position, label, property.vector4Value);
                var vClamped = new Vector4(
                    Mathf.Clamp(v.x, (float)CAttribute.minX, (float)CAttribute.maxX),
                    Mathf.Clamp(v.y, (float)CAttribute.minY, (float)CAttribute.maxY),
                    Mathf.Clamp(v.z, (float)CAttribute.minZ, (float)CAttribute.maxZ),
                    Mathf.Clamp(v.w, (float)CAttribute.minZ, (float)CAttribute.maxW)
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "set clamped vector");
                    property.vector4Value = vClamped;
                }
            }
            #endregion
            else
            {
                EditorGUI.HelpBox(position, "Given type isn't valid. Please pass either Vector or VectorInt.", MessageType.Warning);
            }
        }
    }

    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorDrawer : PropertyDrawer
    {
        private const float WARN_BOX_HEIGHT = 32f;
        private const string DEFAULT_TAG = "Untagged";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
                return WARN_BOX_HEIGHT;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position,
                    string.Format("Warning : Usage of 'TagSelectorAttribute' on field \"{0} {1}\" even though the field type isn't 'System.String'.", property.type, property.name),
                    MessageType.Warning);

                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.BeginChangeCheck();

            // If the field's string value isn't untagged (default), set the value
            if (string.IsNullOrWhiteSpace(property.stringValue))
            {
                property.stringValue = DEFAULT_TAG;
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
        }
    }

    [CustomPropertyDrawer(typeof(EditDisallowCharsAttribute))]
    public class EditDisallowCharsDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private EditDisallowCharsAttribute Attribute => attribute as EditDisallowCharsAttribute;
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType == SerializedPropertyType.String)
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }
            else
            {
                addHeight += warnHelpBoxRectHeight;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginChangeCheck();
                string editString = EditorGUI.TextField(position, label, property.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!string.IsNullOrEmpty(Attribute.disallowText))
                    {
                        if (Attribute.isRegex)
                        {
                            Regex r = new Regex(Attribute.disallowText, Attribute.regexOpts);
                            // Remove all matches from the string
                            property.stringValue = r.Replace(editString, string.Empty);
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder(editString);
                            for (int i = sb.Length - 1; i >= 0; i--)
                            {
                                if (Attribute.disallowText.Any(c => c == sb[i]))
                                {
                                    sb.Remove(i, 1);
                                }
                            }
                            
                            property.stringValue = sb.ToString();
                        }
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, $"Given type isn't valid for property {label.text}. Please pass string as type.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(ObjectFieldInterfaceConstraintAttribute))]
    public class ObjectFieldInterfaceConstraintDrawer : PropertyDrawer
    {
        private const float warnHelpBoxRectHeight = 22f;
        private ObjectFieldInterfaceConstraintAttribute Attribute => attribute as ObjectFieldInterfaceConstraintAttribute;
        private const float DR_PADDING = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                addHeight += EditorGUIUtility.singleLineHeight + DR_PADDING;
            }
            else
            {
                addHeight += warnHelpBoxRectHeight;
            }

            return addHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position.height -= DR_PADDING;
            position.y += DR_PADDING / 2f;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUI.BeginChangeCheck();

                bool showMixed = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = property.hasMultipleDifferentValues; // ObjectField without 'SerializedProperty' doesn't handle this
                var objectValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, fieldInfo.FieldType, true);
                EditorGUI.showMixedValue = showMixed; 

                if (EditorGUI.EndChangeCheck())
                {
                    if (objectValue == null)
                    {
                        property.objectReferenceValue = null;
                        EditorGUI.EndProperty();
                        return;
                    }

                    bool hasType1 = Attribute.interfaceType1 == null,
                         hasType2 = Attribute.interfaceType2 == null,
                         hasType3 = Attribute.interfaceType3 == null,
                         hasType4 = Attribute.interfaceType4 == null;

                    // Find component with attributes if the object is a 'GetComponent'able one
                    Type objectValueType = objectValue.GetType();
                    MethodInfo miTryGetComponent = objectValueType.GetMethod(nameof(Component.TryGetComponent), 0, new Type[] { typeof(Type), typeof(Component).MakeByRefType() });
                    if (miTryGetComponent != null)
                    {
                        // This value is never written into, only the 'params' is directly written to.
                        // Since the object[] is a pointer array, we can just cast null to component (epic safe code)
                        object[] tryGetComponentParams = new object[] { Attribute.interfaceType1, (Component)null };

                        // This is not code, this is a travesty.
                        if (!hasType1 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType2;
                        if (!hasType2 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType3;
                        if (!hasType3 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                        tryGetComponentParams[0] = Attribute.interfaceType4;
                        if (!hasType4 && (bool)miTryGetComponent.Invoke(objectValue, tryGetComponentParams))
                        {
                            property.objectReferenceValue = (Component)tryGetComponentParams[1];
                        }
                    }
                    else
                    {
                        // Since i'm stupid and attributes don't support anything other than basic data types on ctor.
                        // This is the way i did it. If there's a better way please tell.
                        Type[] interfaces = objectValueType.GetInterfaces();

                        foreach (Type interfaceType in interfaces)
                        {
                            if (!hasType1)
                                hasType1 = interfaceType == Attribute.interfaceType1;
                            if (!hasType2)
                                hasType2 = interfaceType == Attribute.interfaceType2;
                            if (!hasType3)
                                hasType3 = interfaceType == Attribute.interfaceType3;
                            if (!hasType4)
                                hasType4 = interfaceType == Attribute.interfaceType4;
                        }
                        if (hasType1 && hasType2 && hasType3 && hasType4)
                        {
                            property.objectReferenceValue = objectValue;
                        }
                    }
                }
            }
            else
            {
                EditorGUI.HelpBox(position, $"Given type isn't valid for property {label.text}. Please use on UnityEngine.Object deriving as type fields.", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}
