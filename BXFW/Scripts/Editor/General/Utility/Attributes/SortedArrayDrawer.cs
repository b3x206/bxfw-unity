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
    /// <summary>
    /// Draws an enforced list that is forced to be sorted.
    /// <br>Does cool clamping for numbers.</br>
    /// </summary>
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

        private const float WarningBoxHeight = 22f;
        private SortedArrayAttribute Attribute => attribute as SortedArrayAttribute;
        private const float Padding = 2f;

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
                addHeight += WarningBoxHeight;
            }
            else
            {
                // EditorGUI.GetPropertyHeight gets the height with respect to the parent type drawer.
                // (and hopefully ignores the attribute 'GetPropertyHeight's, otherwise this will crash unity)
                addHeight += EditorGUI.GetPropertyHeight(property) + Padding;
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
            position.height -= Padding;
            position.y += Padding / 2f;

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

                if (!IsSorted(parentArrayList, Attribute.Reverse))
                {
                    parentArrayList.Sort(Comparer<object>.Default);
                    if (Attribute.Reverse)
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
                    float lower = !Attribute.Reverse ?
                        (propertyIndex == 0 ? float.MinValue : (float)parentArrayList[propertyIndex - 1]) :
                        (propertyIndex == parentArrayList.Count - 1 ? float.MinValue : (float)parentArrayList[propertyIndex + 1]);
                    float upper = !Attribute.Reverse ?
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
                    int lower = !Attribute.Reverse ?
                        (propertyIndex == 0 ? int.MinValue : (int)parentArrayList[propertyIndex - 1]) :
                        (propertyIndex == parentArrayList.Count - 1 ? int.MinValue : (int)parentArrayList[propertyIndex + 1]);
                    int upper = !Attribute.Reverse ?
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
}
