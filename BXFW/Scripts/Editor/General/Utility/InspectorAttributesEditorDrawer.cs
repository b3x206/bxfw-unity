using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    // TODO : Do this (very cool)
    // https://forum.unity.com/threads/drawing-a-field-using-multiple-property-drawers.479377/

    /// <summary>
    /// Draws the '<see cref="Texture2D"/>' inspector for sprites.
    /// <br>Limitations -> Doesn't support scene objects.</br>
    /// </summary>
    [CustomPropertyDrawer(typeof(BigSpriteFieldAttribute))]
    internal class BigSpriteFieldDrawer : PropertyDrawer
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
        private KeyValuePair<FieldInfo, object> target;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float addHeight = 0f;
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
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
            if (target.Key == null)
                target = property.GetTarget();

            // Draw an object field for sprite property
            if (target.Key.FieldType != typeof(Sprite))
            {
                EditorGUI.HelpBox(position,
                    string.Format("Warning : Usage of 'InspectorBigSpriteFieldDrawer' on field \"{0} {1}\" even though the field type isn't sprite.", property.type, property.name),
                    MessageType.Warning);
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
                    Undo.RecordObject(property.objectReferenceValue, "Inspector");
                }

                property.objectReferenceValue = setValue;
            }
        }
    }

    [CustomPropertyDrawer(typeof(InspectorLineAttribute))]
    internal class InspectorLineDrawer : DecoratorDrawer
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

    [CustomPropertyDrawer(typeof(ReadOnlyViewAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
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
            var gEnabled = GUI.enabled;

            GUI.enabled = false;
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
            GUI.enabled = gEnabled;
        }
    }

    [CustomPropertyDrawer(typeof(SortedArrayAttribute))]
    internal class SortedArrayDrawer : PropertyDrawer
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

            propertyParentTypeArray = property.GetParentOfTargetField().Key.FieldType.GetInterfaces().Any(i => i == typeof(IEnumerable) || i == typeof(IEnumerable<>));
            // (lol typeof(string).Assembly, they are in the same assembly so idc)
            propertyTypeValid = propertyParentTypeArray && property.GetPropertyType().GetInterfaces()
                .Any(i => i == typeof(IComparable) || i == typeof(IComparable<>)) ||
                property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float;

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
                int propertyIndex = property.GetPropertyArrayIndex();

                // Parent array itself
                // (since normal IComparable and generic IComparable are incompatible with casting, just assume that these objects have a Method that has CompareTo)
                // Get the IEnumerable interface type
                Type arrayEnumerableType = null;
                {
                    Type[] ints = parentArrayPair.Key.FieldType.GetInterfaces();
                    foreach (Type type in ints)
                    {
                        // Calling 'GetGenericTypeDefinition' makes the type open.
                        bool breakOnType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || type == typeof(IEnumerable);

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
                    parentArrayPair.Key.SetValue(parentObject, parentArrayList.ToIEnumerableType(arrayEnumerableType));
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
                        parentArrayPair.Key.SetValue(parentObject, parentArrayList.ToIEnumerableType(arrayEnumerableType));
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
    internal class ClampDrawer : PropertyDrawer
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
                if (property.type == typeof(float).Name)
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
    internal class ClampVectorDrawer : PropertyDrawer
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
}