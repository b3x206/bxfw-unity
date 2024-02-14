using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace BXFW
{
    /// <summary>
    /// Contains <see cref="Type"/> utilities and other related utilities.
    /// </summary>
    public static class TypeUtility
    {
        // -- Type
        /// <summary>
        /// Returns whether if the <paramref name="typeName"/> has an integral name.
        /// <br>Useful to compare <see cref="UnityEditor.SerializedProperty.type"/> to check.</br>
        /// </summary>
        public static bool IsTypeNameInteger(string typeName)
        {
            return typeName == typeof(byte).Name ||
                typeName == typeof(sbyte).Name ||
                typeName == typeof(short).Name ||
                typeName == typeof(ushort).Name ||
                typeName == typeof(int).Name ||
                typeName == typeof(uint).Name ||
                typeName == typeof(long).Name ||
                typeName == typeof(ulong).Name ||
                typeName == typeof(IntPtr).Name ||
                typeName == typeof(UIntPtr).Name ||
                // SerializedProperty.type
                typeName == "int" ||
                typeName == "long";
        }
        /// <summary>
        /// Returns whether if the type name is a floating point number type.
        /// <br>Compares <paramref name="typeName"/> to <see cref="float"/>, <see cref="double"/> or <see cref="decimal"/>.</br>
        /// </summary>
        public static bool IsTypeNameFloat(string typeName)
        {
            return typeName == typeof(float).Name ||
                typeName == typeof(double).Name ||
                typeName == typeof(decimal).Name ||
                // SerializedProperty.type
                typeName == "float" ||
                typeName == "double";
        }
        /// <summary>
        /// Returns whether if the <paramref name="typeName"/> has a numerical name.
        /// <br>The difference between <see cref="IsTypeNameInteger(string)"/> is that the type name is also compared against <see cref="float"/> and <see cref="double"/>.</br>
        /// <br>Useful to compare <see cref="UnityEditor.SerializedProperty.type"/> to check.</br>
        /// </summary>
        public static bool IsTypeNameNumerical(string typeName)
        {
            return IsTypeNameInteger(typeName) || IsTypeNameFloat(typeName);
        }

        /// <summary>
        /// A backtick used in the generic name definitions.
        /// </summary>
        private const char GenericArgumentsDefinitionChar = '`';
        /// <summary>
        /// A list of global type aliases/predefined types used by C# for System types.
        /// <br>For example, <c>System.Int32</c> being aliased to <see cref="int"/>.</br>
        /// <br/>
        /// <br>These types are also called built-in types as well.</br>
        /// </summary>
        private static readonly Dictionary<Type, string> GlobalTypeAliasesMap = new Dictionary<Type, string>
        {
            // Value
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(sbyte), "sbyte" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            // These types are pointer types which can be explicitly written
            // And these shorthand names are more confusing anyways
            // { typeof(nint), "nint" },   // Which is System.IntPtr
            // { typeof(nuint), "nuint" }, // Which is System.UIntPtr
            // Reference
            { typeof(object), "object" },
            { typeof(string), "string" },
        };
        /// <summary>
        /// Returns the pretty name/type definition string of the c# type.
        /// <br>(like <c>Foo&lt;Parameter&gt;</c> instead of <c>Foo`1[[typeof(Parameter).QualifiedAssemblyName]]</c>).</br>
        /// </summary>
        /// <param name="type">Type to return it's name.</param>
        /// <param name="includeNamespace">Whether to include the type's namespace in the start. This applies to all types.</param>
        /// <param name="usePredefinedTypeAliases">Whether to use the shorthand aliases/type definitions for default c# types. Applies to all types.</param>
        public static string GetTypeDefinitionString(this Type type, bool includeNamespace = false, bool usePredefinedTypeAliases = true)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "[Additionals::GetTypeDefinitionString] Given argument was null.");
            }

            // Type exists on global aliases/predefined types
            if (usePredefinedTypeAliases && GlobalTypeAliasesMap.TryGetValue(type, out string alias))
            {
                return alias;
            }

            string typeName = includeNamespace ? type.FullName : type.Name;
            // type.Name or type.FullName can be null if the given type is weird
            int indexOfGenericArgsChar = typeName?.IndexOf(GenericArgumentsDefinitionChar) ?? -1;
            if (indexOfGenericArgsChar < 0)
            {
                return typeName;
            }

            Type[] typeGenericArgs = type.GetGenericArguments();
            StringBuilder typeNameSb = new StringBuilder(typeName.Substring(0, indexOfGenericArgsChar));
            typeNameSb.Append('<');
            for (int i = 0; i < typeGenericArgs.Length; i++)
            {
                Type genericArg = typeGenericArgs[i];
                // Open type adding, add semicolons
                if (genericArg.IsGenericParameter)
                {
                    if (i != typeGenericArgs.Length - 1)
                    {
                        // (all GenericArguments are likely open if one of them is open too, so no space needed)
                        typeNameSb.Append(',');
                    }
                    continue;
                }

                typeNameSb.Append(GetTypeDefinitionString(genericArg, includeNamespace, usePredefinedTypeAliases));
                if (i != typeGenericArgs.Length - 1)
                {
                    // Leave a space for closed types
                    typeNameSb.Append(", ");
                }
            }
            typeNameSb.Append('>');

            return typeNameSb.ToString();
        }

        /// <summary>
        /// Returns whether if <paramref name="type"/> is a built-in c# integer type.
        /// </summary>
        public static bool IsTypeInteger(Type type)
        {
            return IsTypeNameInteger(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is a built-in c# integer type.
        /// </summary>
        public static bool IsTypeInteger<T>()
        {
            return IsTypeInteger(typeof(T));
        }
        /// <summary>
        /// Returns whether if <paramref name="type"/> is a built-in c# floating point number type.
        /// </summary>
        public static bool IsTypeFloat(Type type)
        {
            return IsTypeNameFloat(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is a built-in c# floating point number type.
        /// </summary>
        public static bool IsTypeFloat<T>()
        {
            return IsTypeFloat(typeof(T));
        }
        /// <summary>
        /// Returns whether if <paramref name="type"/> is an numerical type.
        /// <br>Checks type name against <see cref="float"/> and <see cref="double"/> also, unlike <see cref="IsTypeInteger(Type)"/></br>
        /// </summary>
        public static bool IsTypeNumerical(Type type)
        {
            return IsTypeNameNumerical(type.Name);
        }
        /// <summary>
        /// Returns whether if <typeparamref name="T"/> is an numerical type.
        /// <br>Checks type name against <see cref="float"/> and <see cref="double"/> also, unlike <see cref="IsTypeInteger{T}()"/></br>
        /// </summary>
        public static bool IsTypeNumerical<T>()
        {
            return IsTypeNumerical(typeof(T));
        }

        /// <summary>
        /// Returns whether if the type is a nullable one.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsNullable(this Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t), "[SerializableDictionary::TypeIsNullable] Given argument was null.");
            }

            return !t.IsValueType || Nullable.GetUnderlyingType(t) != null;
        }
        /// <summary>
        /// Returns whether if the given type is assignable from generic type <paramref name="openGenericType"/>.
        /// <br>Can be used/tested against <b>open generic</b> types and <paramref name="target"/>'s base types are checked recursively.</br>
        /// </summary>
        /// <param name="target">The target type to test to whether if it's assignable from <paramref name="openGenericType"/>. This cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        public static bool IsAssignableFromOpenGeneric(this Type target, Type openGenericType)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "[Additionals::IsAssignableFromOpenGeneric] Given argument was null.");
            }
            if (openGenericType == null)
            {
                return false;
            }

            // target      => given type
            // genericType => Generic<> (open type)

            // Can be assigned using interface (can be checked only once, GetInterfaces returns all interfaces)
            if (target.GetInterfaces().Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == openGenericType))
            {
                return true;
            }

            // Can be assigned using BaseType inheritance (class inheritance)
            Type iterTarget = target;
            do
            {
                // Can be assigned directly (with open type)
                if (iterTarget.IsGenericType && iterTarget.GetGenericTypeDefinition() == openGenericType)
                {
                    return true;
                }

                iterTarget = iterTarget.BaseType;
            }
            while (iterTarget != null);

            // Reached end of base type
            return false;
        }
        /// <summary>
        /// Returns whether if the <paramref name="genericArg"/> allows the <paramref name="checkType"/> as it's value on the given class/struct/whatever's generic argument.
        /// <br>Note : This is not as good as the CLR checks of <see cref="Type.MakeGenericType(Type[])"/>. </br>
        /// <br>If there is a smaller amount of types available for you to test for
        /// use the <see cref="Type.MakeGenericType(Type[])"/> method in a <c>try {} catch</c> block instead of using this method.</br>
        /// </summary>
        /// <param name="genericArg">The generic argument to check against. This cannot be null and must be a <see cref="Type.IsGenericType"/>.</param>
        /// <param name="checkType">The type to check against. This cannot be null.</param>
        /// <returns>The result of whether if the <paramref name="checkType"/> is assignable to the <paramref name="genericArg"/>'s constraints.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public static bool GenericArgumentAllowsType(Type genericArg, Type checkType)
        {
            if (genericArg == null)
            {
                throw new ArgumentNullException(nameof(genericArg), "[Additionals::GenericArgumentAllowsType] Given argument was null.");
            }
            if (!genericArg.IsGenericParameter)
            {
                throw new ArgumentException($"[Additionals::GenericArgumentAllowsType] Given 'genericArg ({genericArg})' is not an open generic type.", nameof(genericArg));
            }
            if (checkType == null)
            {
                throw new ArgumentNullException(nameof(checkType), "[Additionals::GenericArgumentAllowsType] Given argument was null.");
            }

            // Get generic arg parameter attributes
            Type[] baseTypes = checkType.GetBaseTypes().ToArray();
            bool hasStructConstraint = (genericArg.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) == GenericParameterAttributes.NotNullableValueTypeConstraint;
            bool hasNewConstructorConstraint = hasStructConstraint || (genericArg.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) == GenericParameterAttributes.DefaultConstructorConstraint;
            bool hasClassConstraint = !hasStructConstraint && (genericArg.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == GenericParameterAttributes.ReferenceTypeConstraint;

            // Check if the parameter attributes pass
            // TODO : Some attributes are missing the checks (i.e covariance, whatever that is)
            if (hasStructConstraint && !checkType.IsValueType)
            {
                return false;
            }
            if (hasClassConstraint && !checkType.IsNullable() && !checkType.IsByRef)
            {
                return false;
            }
            if (hasNewConstructorConstraint && checkType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null) == null)
            {
                return false;
            }

            // Get the first class that provides the constraints
            // The 'GetGenericParameterConstraints' return the actual class constraints
            foreach (Type constraint in genericArg.GetGenericParameterConstraints())
            {
                // Check if base types contain the type for inheritance checking
                if (!baseTypes.Contains(constraint))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Returns a list of base generic types inside given <paramref name="type"/>,
        /// mapped accordingly to the dictionary of it's base inheriting generic types.
        /// <br>The keys of the given dictionary is open generic types and the values are the keys generic arguments.</br>
        /// </summary>
        public static Dictionary<Type, Type[]> GetBaseGenericTypeArguments(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type), "[Additionals::GetBaseGenericTypeArguments] Given argument was null.");
            }

            Dictionary<Type, Type[]> baseTypePairs = new Dictionary<Type, Type[]>(4);

            Type iterTarget = type;
            do
            {
                if (iterTarget.IsGenericType)
                {
                    // Set dictionary not null if element was added.
                    baseTypePairs.Add(iterTarget.GetGenericTypeDefinition(), iterTarget.GetGenericArguments());
                }

                iterTarget = iterTarget.BaseType;
            }
            while (iterTarget != null);

            return baseTypePairs;
        }

        /// <summary>
        /// Get an iterator of the base types + interfaces implemented of <paramref name="type"/>.
        /// <br>Returns a blank iterator if no base type + interfaces.</br>
        /// </summary>
        public static IEnumerable<Type> GetBaseTypes(this Type type)
        {
            if (type.BaseType == null)
            {
                return type.GetInterfaces();
            }

            return Enumerable.Repeat(type.BaseType, 1)
                             .Concat(type.GetInterfaces())
                             .Concat(type.GetInterfaces().SelectMany(GetBaseTypes))
                             .Concat(type.BaseType.GetBaseTypes());
        }
        /// <summary>Get types that has the <paramref name="attributeType"/> attribute from <see cref="Assembly"/> <paramref name="attbAsm"/>.</summary>
        /// <returns>The types with the attribute <paramref name="attributeType"/>.</returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Type attributeType, Assembly attbAsm = null)
        {
            if (attbAsm == null)
            {
                attbAsm = attributeType.Assembly;
            }

            foreach (Type type in attbAsm.GetTypes())
            {
                if (type.GetCustomAttributes(attributeType, true).Length > 0)
                {
                    yield return type;
                }
            }
        }
        /// <summary>
        /// Gets types that inherit from 'T'.
        /// <br>Only uses the assembly of <typeparamref name="T"/>.</br>
        /// <br>Use the <c>TypeListProvider</c> (editor only) if you need all assemblies.</br>
        /// </summary>
        public static IEnumerable<Type> GetInheritingTypes<T>() where T : class
        {
            return Assembly.GetAssembly(typeof(T))
                .GetTypes()
                .Where((Type myType) => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
        }

        /// <summary>
        /// An utility method used to get the typed <see cref="EqualityComparer{T}.Default"/>'s comparison result with the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">
        /// Type of the both <paramref name="lhs"/> and <paramref name="rhs"/>.
        /// An <see cref="ArgumentException"/> will be thrown (by the reflection utility) on invocation if the types mismatch.
        /// </param>
        /// <param name="lhs">First object to compare. This method returns whether if this value is equal to <paramref name="rhs"/>.</param>
        /// <param name="rhs">Other way around. Method returns if this is equal to <paramref name="lhs"/>.</param>
        public static bool TypedEqualityComparerResult(Type type, object lhs, object rhs)
        {
            // Because apparently there's no typeless EqualityComparer?
            // EqualityComparer is used because of the IEquatable check and other things
            // ----- No Typeless EqualityComparer? -----
            Type typedComparerType = typeof(EqualityComparer<>).MakeGenericType(type);
            object typedComparer = typedComparerType.GetProperty(nameof(EqualityComparer<object>.Default), BindingFlags.Public | BindingFlags.Static).GetValue(null);
            MethodInfo typedComparerEqualsMethod = typedComparerType.GetMethod(nameof(EqualityComparer<object>.Equals), 0, new Type[] { type, type });
            return (bool)typedComparerEqualsMethod.Invoke(typedComparer, new object[] { lhs, rhs });
        }
    }
}
