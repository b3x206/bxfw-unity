using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A class that contains information about the target gathered from a <see cref="SerializedProperty"/>.
    /// </summary>
    public class PropertyTargetInfo
    {
        /// <summary>
        /// The field information about the contained <see cref="value"/> object.
        /// </summary>
        public readonly FieldInfo fieldInfo;
        /// <summary>
        /// The target object value of the given property.
        /// </summary>
        public readonly object value;
        /// <summary>
        /// Parent object of this target.
        /// <br>If this is null, the target object is the parent object.</br>
        /// </summary>
        public readonly object parent;

        /// <summary>
        /// Tries to cast <see cref="value"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Whether if the casting was successful. Returns <see langword="false"/> if it was <b>NOT</b> successful.</returns>
        public bool TryCastValue<T>(out T value)
        {
            bool success = this.value is T;
            value = success ? (T)this.value : default;

            return success;
        }
        /// <summary>
        /// Tries to cast <see cref="parent"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <returns>Whether if the casting was successful. Returns <see langword="false"/> if it was <b>NOT</b> successful.</returns>
        public bool TryCastParent<T>(out T value)
        {
            bool success = parent is T;
            value = success ? (T)parent : default;

            return success;
        }

        /// <summary>
        /// Whether if the property is an <see cref="IEnumerable"/>.
        /// </summary>
        public bool TargetIsEnumerable => typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType);

        private PropertyTargetInfo() { }
        public PropertyTargetInfo(FieldInfo fi, object target, object parent)
        {
            fieldInfo = fi;
            value = target;
            this.parent = parent;
        }
    }

    /// <summary>
    /// Contains SerializedProperty related extension methods.
    /// </summary>
    public static class SerializedPropertyUtility
    {
        /// <summary>
        /// String token used to define a <see cref="SerializedProperty"/> array element.
        /// </summary>
        private const string SPropArrayToken = "Array.data[";

        /// <summary>
        /// Returns a string that is traversed towards parent property names.
        /// </summary>
        private static string GetParentTraversedPropertyPathString(string propertyPath, int parentDepth)
        {
            int lastIndexOfPeriod = propertyPath.LastIndexOf('.');
            for (int i = 1; i < parentDepth; i++)
            {
                lastIndexOfPeriod = propertyPath.LastIndexOf('.', lastIndexOfPeriod - 1);
            }

            if (lastIndexOfPeriod == -1)
            {
                return string.Empty;
            }

            return propertyPath.Substring(0, lastIndexOfPeriod);
        }
        /// <summary>
        /// Internal method to get parent from these given parameters.
        /// <br>Traverses <paramref name="propertyRootParent"/> using reflection and finds the target field info + 
        /// object ref (or copy if target is a <see langword="struct"/>) in <paramref name="propertyPath"/>.</br>
        /// </summary>
        /// <param name="propertyRootParent">Target (parent) object of <see cref="SerializedProperty"/>. Pass <see cref="SerializedProperty.serializedObject"/>.targetObject.</param>
        /// <param name="propertyPath">Path of the property. Pass <see cref="SerializedProperty.propertyPath"/>.</param>
        /// <exception cref="InvalidCastException"/>
        private static PropertyTargetInfo GetTarget(UnityEngine.Object propertyRootParent, string propertyPath)
        {
            if (propertyRootParent == null)
            {
                throw new ArgumentNullException(nameof(propertyRootParent), "[EditorAdditionals::GetTarget] Given argument was null.");
            }

            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                throw new ArgumentNullException(nameof(propertyPath), "[EditorAdditionals::GetTarget] Given argument was null.");
            }

            // State
            object targetParent = null;
            FieldInfo targetInfo = null;
            object target = propertyRootParent;
            bool isNextPropertyArrayIndex = false;

            // Example property path : containerClass.offsetsWithNames.m_SomeArrayData.Array.data[0].childValue.anotherChildValue
            string[] propertyNames = propertyPath.Split('.');

            for (int i = 0; i < propertyNames.Length && target != null; i++)
            {
                // Alias the string name. (but we need for for the 'i' variable)
                string propName = propertyNames[i];

                // Array targets mostly contain typeless 'IEnumerable's
                if (propName == "Array" && target is IEnumerable)
                {
                    // Arrays in property path's are seperated like -> Array.data[index]
                    isNextPropertyArrayIndex = true;
                }
                else if (isNextPropertyArrayIndex)
                {
                    // Gather -> data[index] -> the value on the 'index'
                    isNextPropertyArrayIndex = false;
                    // Maximum string length of an Int32 is 10
                    // Allocate a string as 'int.Parse' ignores whitespace
                    // The only problem here is that strings are immutable, StringBuilder allocates more garbage than needed and unsafe is bad
                    // Cannot use Marshaling (of char*) because that would end up being way slower than needed
                    // (and c# is UTF-16 for some idiotic reason, it is because of microsoft)

                    // apparently base 10 string to int parsing is easy
                    // (this is just because 'int.Parse' has no int range and we are expected to use string.Substring)
                    int arrayIndex = 0;
                    // shift the number by '10' and add the 'number' to self then add the diff of character between '9' to '0'.
                    // This way we move in a base 10 manner (note : this only works for positive integers, but indexing ints are always positive)
                    // (and to add support for negative int just negate the result after parsing the positive part)
                    // --
                    // Size of 'data[' token is 5, and start from the index 5 to parse the thing
                    for (int j = 5; j < propName.Length; j++)
                    {
                        char character = propName[j];
                        // This character means stop
                        if (character == ']')
                        {
                            break;
                        }

                        // Check if any other char
                        int charDifference = character - '0';
                        if (charDifference < 0 || charDifference > 9)
                        {
                            // In a case of an array parsing failure, this was the default behaviour.
                            // But let's just assume that is a faulty behaviour because the 'size' problem occured due to something else.

                            // --
                            // Array parse failure, should only happen on the ends of the array (i.e size field)
                            // Instead of throwing an exception, just get the object
                            // (as this may be called for the 'int size field' on the editor, for some reason)
                            // try
                            // {
                            //     targetInfo = GetField(target, propName);
                            //     targetParent = target;
                            //     target = targetInfo.GetValue(target);
                            // }
                            // catch
                            // {
                            //     // It can also have an non-existent field for some reason
                            //     // Because unity, so the method should give up (with the last information it has)
                            //     // Maybe this should print a warning, but it's not too much of a thing (just a fallback)
                            //     return new PropertyTargetInfo(targetInfo, target, targetParent);
                            // }

                            // break;
                            throw new Exception($"[SerializedPropertyUtility::GetTarget] Failed to parse array index. Property path is {propertyPath}, current name is {propName}, character is {character}. Only expected digits.");
                        }

                        // Add multiplied by 10 number to itself and add the digit itself
                        arrayIndex = (arrayIndex << 1) + (arrayIndex << 3) + charDifference;
                    }

                    if (!(target is IEnumerable targetAsArray))
                    {
                        throw new InvalidCastException(string.Format(@"[EditorAdditionals::GetTarget] Error while casting targetAsArray.
-> Invalid cast : Tried to cast type {0} as IEnumerable. Current property is {1}.", target.GetType().Name, propName));
                    }

                    IEnumerator enumerator = targetAsArray.GetEnumerator();
                    bool isSuccess = false;

                    for (int j = 0; enumerator.MoveNext(); j++)
                    {
                        object item = enumerator.Current;

                        if (arrayIndex == j)
                        {
                            // Update FieldInfo that will be returned
                            // --
                            // oh wait, that's impossible, riiight.
                            // basically FieldInfo can't point into a c# array element member,
                            // only the parent array container as it's just the object
                            // (unless we are returning a managed memory pointer, which is not really possible unless unity does it)
                            // (+ which it most likely won't because our result data is in ''safe'' FieldInfo type)

                            // If the array contains a class or a struct, and the target is a member that actually is not an array value, it updates fine though.
                            // So you could use a wrapper class that just contains the field as the target
                            // (but we can't act like that, because c# arrays are covariant and casting c# arrays is not fun)
                            // whatever just look at this : https://stackoverflow.com/questions/13790527/c-sharp-fieldinfo-setvalue-with-an-array-parameter-and-arbitrary-element-type

                            // ---------- No Array Element FieldInfo? -------------
                            // (would like to put megamind here, but git will most likely break it)
                            targetParent = target; // Set parent to previous
                            target = item;
                            isSuccess = true;

                            break;
                        }
                    }

                    // Element doesn't exist in the array
                    if (!isSuccess)
                    {
                        throw new Exception(string.Format("[EditorAdditionals::GetTarget] Couldn't find SerializedProperty '{0}' in array '{1}'. This may occur due to out of bounds indexing of the array or just the property path not existing.", propertyPath, targetAsArray));
                    }
                }
                else
                {
                    // Get next target + value.
                    targetInfo = GetField(target, propName);
                    targetParent = target;
                    target = targetInfo.GetValue(target);
                }
            }

            return new PropertyTargetInfo(targetInfo, target, targetParent);
        }

        /// <summary>
        /// Returns the c# object targets.
        /// <br>
        /// It is heavily suggested that you use <see cref="GetTargetsNoAlloc(SerializedProperty, List{PropertyTargetInfo})"/> 
        /// instead for much better performance and most likely less memory leaks.<br/>(this method calls that method internally with a newly allocated array anyways)
        /// </br>
        /// </summary>
        public static List<PropertyTargetInfo> GetTargets(this SerializedProperty prop)
        {
            var infos = new List<PropertyTargetInfo>();
            GetTargetsNoAlloc(prop, infos);
            return infos;
        }
        /// <summary>
        /// Returns the c# object targets (without allocating new arrays).
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        /// <param name="prop">Target property.</param>
        /// <param name="targetInfos">Array to write the properties into. The array is cleared then written into.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void GetTargetsNoAlloc(this SerializedProperty prop, List<PropertyTargetInfo> targetInfos)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::GetTargets] Parameter 'prop' is null.");
            }

            if (targetInfos == null)
            {
                throw new ArgumentNullException(nameof(targetInfos), "[EditorAdditionals::GetTargets] Array Parameter 'targetPairs' is null.");
            }

            targetInfos.Clear();
            targetInfos.Capacity = prop.serializedObject.targetObjects.Length;

            for (int i = 0; i < prop.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetedObject = prop.serializedObject.targetObjects[i];
                if (targetedObject == null)
                {
                    continue;
                }

                targetInfos.Add(GetTarget(targetedObject, prop.propertyPath));
            }
        }

        /// <summary>
        /// Returns the c# object parent targets.
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        public static List<PropertyTargetInfo> GetParentsOfTargets(this SerializedProperty prop, int parentDepth = 1)
        {
            List<PropertyTargetInfo> infos = new List<PropertyTargetInfo>();
            GetParentsOfTargetsNoAlloc(prop, infos, parentDepth);
            return infos;
        }
        /// <summary>
        /// Returns the c# object parent targets (without allocating new arrays).
        /// <br>
        /// Useful for cases when the "<see cref="SerializedProperty.serializedObject"/>.isEditingMultipleObjects" is true 
        /// (or for adding multi edit support for a property drawer), this will return all the object targets.
        /// </br>
        /// </summary>
        /// <param name="prop">Target property.</param>
        /// <param name="targetInfos">Array to write the properties into. The array is cleared then written into.</param>
        /// <param name="parentDepth">Depth of the target parent. Higher depths</param>
        /// <exception cref="ArgumentNullException"/>
        public static void GetParentsOfTargetsNoAlloc(this SerializedProperty prop, List<PropertyTargetInfo> targetInfos, int parentDepth = 1)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::GetParentsOfTargetsNoAlloc] Parameter 'prop' is null.");
            }

            if (targetInfos == null)
            {
                throw new ArgumentNullException(nameof(targetInfos), "[EditorAdditionals::GetParentsOfTargetsNoAlloc] Array Parameter 'targetPairs' is null.");
            }

            targetInfos.Clear();
            targetInfos.Capacity = prop.serializedObject.targetObjects.Length;

            for (int i = 0; i < prop.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetedObject = prop.serializedObject.targetObjects[i];
                if (targetedObject == null)
                {
                    continue;
                }

                targetInfos.Add(GetTarget(targetedObject, GetParentTraversedPropertyPathString(prop.propertyPath, parentDepth)));
            }
        }

        /// <summary>
        /// Returns the c# object's fieldInfo and the instance object it comes with.
        /// <br>
        /// <b>NOTE :</b> The instance object that gets returned with this method may be null.
        /// <br>In these cases use the <see langword="return"/>'s field info.</br>
        /// </br>
        /// <br/>
        /// <br>
        /// <b>NOTE 2 :</b> The <see cref="FieldInfo"/> returned may not be the exact <see cref="FieldInfo"/>, 
        /// as such case usually happens when you try to call 'GetTarget' on an array element.
        /// <br>In this case, to change the value of the array, you may need to copy the entire array,
        /// and call <see cref="FieldInfo.SetValue"/> to it.</br>
        /// </br>
        /// <br/>
        /// <br>
        /// <b>NOTE 3 :</b> Any value gathered from a normal <see langword="struct"/> child <see cref="SerializedProperty"/>
        /// (except for the 'FieldInfo') should be considered as a copy of target.
        /// <br>This is because <see cref="GetTarget(SerializedProperty)"/> cannot return struct references
        /// and it does not have low-level control of neither c# or unity.</br>
        /// </br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidCastException"/> 
        public static PropertyTargetInfo GetTarget(this SerializedProperty prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::GetTarget] Given argument 'prop' is null!");
            }

            return GetTarget(prop.serializedObject.targetObject, prop.propertyPath);
        }

        /// <summary>
        /// Returns the c# object's fieldInfo and the PARENT object it comes with. (this is useful with <see langword="struct"/>)
        /// <br>Important NOTE : The instance object that gets returned with this method may be null (or not).
        /// In these cases use the return (the FieldInfo)</br>
        /// <br/>
        /// <br>If you are using this for <see cref="CustomPropertyDrawer"/> (that is on an array otherwise this note is invalid), this class has an <see cref="FieldInfo"/> property named <c>fieldInfo</c>, 
        /// you can use that instead of the bundled field info.</br>
        /// </summary>
        /// <param name="prop">Property to get the c# object from.</param>
        /// <exception cref="NullReferenceException"/>
        /// <exception cref="InvalidCastException"/>
        public static PropertyTargetInfo GetParentOfTargetField(this SerializedProperty prop, int parentDepth = 1)
        {
            string propertyNameList = GetParentTraversedPropertyPathString(prop.propertyPath, parentDepth);

            if (string.IsNullOrEmpty(propertyNameList))
            {
                // No depth, instead return the field info from this scriptable object (use the parent scriptable object ofc)
                var fInfo = GetField(prop.serializedObject.targetObject, prop.name);

                // Return the 'serializedObject.targetObject' as target, because it isn't a field (is literally an pointer) 
                return new PropertyTargetInfo(fInfo, prop.serializedObject.targetObject, null);
            }

            var info = GetTarget(prop.serializedObject.targetObject, propertyNameList);
            return info;
        }

        /// <summary>
        /// Returns the type of the property's target.
        /// <br>If the parent '<see cref="PropertyTargetInfo.fieldInfo"/>' is an array, the child type will be the result.</br>
        /// </summary>
        /// <param name="property">Property to get type from.</param>
        public static Type GetPropertyType(this SerializedProperty property)
        {
            PropertyTargetInfo info = property.GetTarget();
            if (info.value != null)
            {
                return info.value.GetType();
            }

            return info.fieldInfo.FieldType;
        }
        /// <summary>
        /// Returns the (last array) index of this property in the array.
        /// <br>Returns <c>-1</c> if <paramref name="property"/> is not in an array.</br>
        /// </summary>
        public static int GetPropertyParentArrayIndex(this SerializedProperty property)
        {
            // Find whether if there's any array define token
            int arrayDefLastIndex = property.propertyPath.LastIndexOf(SPropArrayToken);
            // No define token
            if (arrayDefLastIndex < 0)
            {
                return -1;
            }

            // Remove the enclosing bracket ']' token
            string indStr = property.propertyPath.Substring(arrayDefLastIndex + SPropArrayToken.Length).TrimEnd(']');
            return int.Parse(indStr);
        }
        /// <summary>
        /// Internal helper method for getting field from properties.
        /// <br>Gets the target normally, if not found searches the field in <paramref name="targetType"/>'s <see cref="Type.BaseType"/>.</br>
        /// </summary>
        private static FieldInfo GetField(object target, string name, Type targetType = null)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "[EditorAdditionals::GetField] Error while getting field : Null 'target' object.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name), string.Format("[EditorAdditionals::GetField] Error while getting field : Null 'name' field. (target: '{0}', targetType: '{1}')", target, targetType));
            }

            if (targetType == null)
            {
                targetType = target.GetType();
            }

            // This won't work for struct childs (it will, but it will return a copy of the struct)
            // because GetField does the normal c# behaviour (and it's because c# structs are stackalloc)
            FieldInfo fi = targetType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // If the field info is present.
            if (fi != null)
            {
                return fi;
            }

            // If not found, search in parent
            if (targetType.BaseType != null)
            {
                return GetField(target, name, targetType.BaseType);
            }

            throw new NullReferenceException(string.Format("[EditorAdditionals::GetField] Error while getting field : Could not find '{0}' on '{1}' and it's children.", name, target));
        }

        /// <summary>
        /// Returns whether if this 'SerializedProperty' is disposed.
        /// </summary>
        public static bool IsDisposed(this SerializedProperty obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "[EditorAdditionals::IsDisposed] Target was null.");
            }

            return (IntPtr)typeof(SerializedProperty).GetField("m_NativePropertyPtr", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj) == IntPtr.Zero;
        }
        /// <summary>
        /// Returns whether if Next is callable on <paramref name="prop"/>.
        /// </summary>
        public static bool IsEndOfData(this SerializedProperty prop)
        {
            if (prop == null)
            {
                throw new ArgumentNullException(nameof(prop), "[EditorAdditionals::IsEndOfData] Target was null.");
            }

            return (bool)typeof(SerializedProperty).GetMethod("EndOfData", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(prop, null);
        }

        /// <summary>
        /// Returns the children (regardless of visibility) of the SerializedProperty.
        /// </summary>
        /// <param name="copyable">
        /// If this is true, the given child <see cref="SerializedProperty"/>ies on this iterator will be copies instead of the same reference of the different 
        /// state object, allowing for methods like <see cref="System.Linq.Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> to work properly.
        /// </param>
        /// <returns>Iterable collection of '<see cref="SerializedProperty"/>' children.</returns>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property, bool copyable = false)
        {
            property = property.Copy();
            SerializedProperty nextElement = property.Copy();

            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            // Get next child
            property.NextVisible(true);

            do
            {
                // Skipped to the next element
                if (SerializedProperty.EqualContents(property, nextElement))
                {
                    yield break;
                }

                // yield return the current gathered child property.
                if (copyable)
                {
                    using SerializedProperty copy = property.Copy();
                    yield return copy;
                }
                else
                {
                    yield return property;
                }
            }
            while (property.NextVisible(false));
        }
        /// <summary>
        /// Gets visible children of '<see cref="SerializedProperty"/>' at 1 level depth.
        /// </summary>
        /// <param name="copyable">
        /// If this is true, the given child <see cref="SerializedProperty"/>ies on this iterator will be copies instead of the same reference of the different 
        /// state object, allowing for methods like <see cref="System.Linq.Enumerable.ToArray{TSource}(IEnumerable{TSource})"/> to work properly.
        /// </param>
        /// <returns>Iterable collection of '<see cref="SerializedProperty"/>' children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty, bool copyable = false)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();     // Children iterating property
            SerializedProperty nextSiblingProperty = serializedProperty.Copy(); // Non-children property
            {
                // Move to the initial non-children visible in the next invisible sibling property
                nextSiblingProperty.NextVisible(false);
            }

            // Check initial visibility with children
            if (currentProperty.NextVisible(true))
            {
                do
                {
                    // Check if the 'currentProperty' is now equal to a 'non-children' property
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                    {
                        break;
                    }

                    // Use '.Copy' for making 'Enumerable.ToArray' work
                    // This is due to yield return'd value will be always be the same 'currentProperty' if we don't copy it
                    // But for a linear read of this IEnumerable without laying it out to an array, it will be fine

                    // tl;dr : basically copy the value to make it different instead of a 'currentProperty' pass by value.
                    if (copyable)
                    {
                        using SerializedProperty copy = currentProperty.Copy();
                        yield return copy;
                    }
                    else
                    {
                        yield return currentProperty;
                    }
                }
                while (currentProperty.NextVisible(false));
            }
        }
    }
}
