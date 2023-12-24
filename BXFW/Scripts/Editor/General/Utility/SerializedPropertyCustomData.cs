using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using UnityEditor;
using UnityEngine;
using System.Linq;
// docfx moment
#if UNITY_EDITOR
using BXFW.Data.Editor;
#endif

namespace BXFW.Tools.Editor
{
    /// --
    /// Since you can't directly intercept the unity serializer of editor,
    /// you cannot bind your custom new datas to <see cref="SerializedProperty"/>ies.
    /// This class provides a hack to do this.
    /// --
    /// <summary>
    /// Manages the ability to bind custom serialized property datas to <see cref="SerializedProperty"/>ies.
    /// <br>Creates a <see cref="ScriptableObjectSingleton{T}"/> to use on editor.</br>
    /// </summary>
    public static class SerializedPropertyCustomData
    {
        // -- Singleton
        private static PropertyCustomDataContainer m_MainContainer;
        private const string MainContainerFolder = "Editor"; // Folder for the 'ScriptableObjectSingleton{T}'
        private const string MainContainerFileName = "SPropertyCustomData.asset";
        /// <summary>
        /// Limit of maximum values that can be contained in dictionary.
        /// </summary>
        public static int containerDictionarySizeLimit = short.MaxValue;
        /// <summary>
        /// The singleton dictionary container.
        /// <br>Directly editing this is not recommended.</br>
        /// </summary>
        public static PropertyCustomDataContainer MainContainer
        {
            get
            {
                // Access value only once as it calls 'Resources.Load'
                if (m_MainContainer == null)
                {
                    m_MainContainer = PropertyCustomDataContainer.Instance;

                    if (m_MainContainer == null)
                    {
                        m_MainContainer = PropertyCustomDataContainer.CreateEditorInstance(MainContainerFolder, MainContainerFileName);
                    }
                }

                return m_MainContainer;
            }
        }

        // -- ID Binding
        /// <summary>
        /// The MD5 generator used.
        /// <br>MD5 is not secure and should not be used if security is a high priority.</br>
        /// </summary>
        private static readonly MD5Cng m_md5 = new MD5Cng();
        /// <summary>
        /// Returns the MD5 hash of given <paramref name="s"/>.
        /// <br>This is used to bind id's.</br>
        /// </summary>
        private static string StringToMD5(string s)
        {
            byte[] hash = m_md5.ComputeHash(Encoding.Default.GetBytes(s));
            StringBuilder sb = new StringBuilder(hash.Length);

            foreach (byte b in hash)
            {
                // will return 2 char hex representation.
                sb.Append(Convert.ToString(b, 16));
            }

            return sb.ToString();
        }

        /// <summary>
        /// If this is <see langword="true"/> then the key won't be hashed and can be inspected for errors.
        /// </summary>
        public static bool keyDebugMode = false;
        /// <summary>
        /// Calls <see cref="UnityObjectIDUtility.GetUnityObjectIdentifier(UnityEngine.Object, string)"/>.
        /// <br/>
        /// <br>This is then hashed using MD5 if debug mode is disabled. (as SHA1 string is too long)</br>
        /// </summary>
        private static string GetUnityObjectIdentifier(UnityEngine.Object target)
        {
            string result = UnityObjectIDUtility.GetUnityObjectIdentifier(target);

            return !keyDebugMode ? StringToMD5(result) : result;
        }

        /// <summary>
        /// Returns a unique property identity string, usable to get an id depending on the situation;
        /// <br>A : The scene that this <paramref name="property"/> is contained in (and it's GUID).</br>
        /// <br>B : The FileID of object/component that this <paramref name="property"/> is contained in.</br>
        /// <br>C : And the property path of this current <paramref name="property"/>.</br>
        /// <br>These are combined to return a unique fingerprint of the property.</br>
        /// </summary>
        public static string GetIDString(this SerializedProperty property)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                throw new ArgumentException(@"[SerializedPropertyCustomData::GetPropertyString] Property is owned by multiple objects. Use 'GetMultiEditPropertyStrings' 
or don't call this if the 'property.serializedObject.isEditingMultipleObjects' is true.", nameof(property));
            }

            return $"{GetUnityObjectIdentifier(property.serializedObject.targetObject)}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{property.propertyPath}";
        }

        /// <summary>
        /// <inheritdoc cref="GetIDString(SerializedProperty)"/>
        /// <br>This calls the <see cref="GetUnityObjectIdentifier(UnityEngine.Object)"/> to the
        /// <see cref="SerializedProperty"/>'s <see cref="SerializedObject.targetObjects"/> and creates a key the same way.</br>
        /// </summary>
        public static string[] GetMultiIDStrings(this SerializedProperty property)
        {
            // It doesn't matter if we aren't even editing multiple objects in this case
            string[] list = new string[property.serializedObject.targetObjects.Length];

            GetMultiIDStringsNoAlloc(property, list);
            return list;
        }
        /// <summary>
        /// <inheritdoc cref="GetMultiIDStrings(SerializedProperty)"/>
        /// <br/>
        /// <br>This doesn't allocate an array. Note that the 'strings' has to have enough space allocated.</br>
        /// </summary>
        /// <returns>The size of the filled 'strings'.</returns>
        public static int GetMultiIDStringsNoAlloc(SerializedProperty property, string[] strings)
        {
            if (strings == null || strings.Length <= 0)
            {
                throw new ArgumentNullException(
                    nameof(strings),
                    "[SerializedPropertyCustomData::GetMultiEditPropertyStringsNoAlloc] Given argument was null."
                );
            }

            for (int i = 0; i < Mathf.Min(strings.Length, property.serializedObject.targetObjects.Length); i++)
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObjects[i];
                strings[i] = $"{GetUnityObjectIdentifier(targetObject)}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{property.propertyPath}";
            }

            return property.serializedObject.targetObjects.Length;
        }
        /// <summary>
        /// <inheritdoc cref="GetMultiIDStrings(SerializedProperty)"/>
        /// <br/>
        /// <br>This clears the <paramref name="strings"/> parameter and refills it again.</br>
        /// </summary>
        public static void GetMultiPropertyStringsNoAlloc(SerializedProperty property, List<string> strings)
        {
            if (strings == null)
            {
                throw new ArgumentNullException(
                    nameof(strings),
                    "[SerializedPropertyCustomData::GetMultiEditPropertyStringsNoAlloc] Given argument was null."
                );
            }

            strings.Clear();
            strings.Capacity = property.serializedObject.targetObjects.Length;
            for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObjects[i];
                strings.Add($"{GetUnityObjectIdentifier(targetObject)}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{property.propertyPath}");
            }
        }

        // -- Data Binding + Adding
        // Generic methods of manipulating a dictionary should use '
        /// <summary>
        /// General purpose no-alloc list container.
        /// </summary>
        private static readonly List<string> m_noAllocPropertyStrings = new List<string>();
        // - Generic Impl
        /// <summary>
        /// Returns whether if the 'SerializedProperty' target object contains the key.
        /// <br>For 'SerializedProperties' that are editing multiple objects it returns whether if the all targets contain the key.</br>
        /// </summary>
        private static bool HasDataKey<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, string key)
        {
            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                // propertyId = m_noAllocPropertyStrings[i];
                string saveKey = $"{m_noAllocPropertyStrings[i]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{key}";

                if (!targetDict.ContainsKey(saveKey))
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Returns a saved value from given dictionary.
        /// <br>If the string key does not exist, it will return <see langword="null"/>.</br>
        /// <br/>
        /// <br><see cref="ArgumentException"/> = Thrown when the <paramref name="property"/> is editing multiple objects and same key has different values.</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        private static T GetValue<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, string key)
        {
            // 'Get' methods should not throw if all the values are the same on the list.
            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            T initialValue = targetDict.GetValueOrDefault($"{m_noAllocPropertyStrings[0]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{key}");
            // Ensure all values with keys are the same
            for (int i = 1; i < m_noAllocPropertyStrings.Count; i++)
            {
                T propValue = targetDict.GetValueOrDefault($"{m_noAllocPropertyStrings[i]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{key}");
                if (EqualityComparer<T>.Default.Equals(propValue, initialValue))
                {
                    throw new ArgumentException($"[SerializedPropertyCustomData::Get{typeof(T).GetTypeDefinitionString(false, false)}] Different value values on a multi edit 'SerializedProperty' with key : {key}, Values were {propValue} != {initialValue}", nameof(property));
                }
            }

            // Values are same, can safely return.
            return initialValue;
        }
        /// <summary>
        /// Returns the list of saved values from given dictionary.
        /// <br>The corresponding objects of <see cref="SerializedObject.targetObjects"/> are sequential to the given list.</br>
        /// </summary>
        private static T[] GetMultiValues<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, string key)
        {
            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            T[] values = new T[m_noAllocPropertyStrings.Count];
            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                values[i] = targetDict.GetValueOrDefault($"{m_noAllocPropertyStrings[i]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{key}");
            }

            return values;
        }

        /// <summary>
        /// Sets a value to <paramref name="targetDict"/> using <paramref name="property"/> + <paramref name="key"/> as the key.
        /// <br>If the <paramref name="property"/> is editing multiple objects all keys are set to the same value.</br>
        /// </summary>
        private static void SetValue<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, string key, T value)
        {
            if (property == null || property.IsDisposed())
            {
                throw new ArgumentNullException(nameof(property), $"[SerializedPropertyCustomData::Set{typeof(T).GetTypeDefinitionString(false, false)}] Given property is either null or disposed.");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key), $"[SerializedPropertyCustomData::Set{typeof(T).GetTypeDefinitionString(false, false)}] Given key is null.");
            }

            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                string propertyKey = $"{m_noAllocPropertyStrings[i]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{key}";
                if (targetDict.ContainsKey(propertyKey))
                {
                    targetDict[propertyKey] = value;
                }
                else
                {
                    // Ensure that the dictionary is in size limit
                    if (targetDict.Count >= containerDictionarySizeLimit)
                    {
                        targetDict.Remove(targetDict.First().Key);
                    }
                    // Then add the value as Dictionary.Remove's removal order is undefined.
                    targetDict.Add(propertyKey, value);
                }
            }
        }
        /// <summary>
        /// Sets a value to <paramref name="targetDict"/> using <paramref name="property"/> + <paramref name="key"/> as the key.
        /// <br>If the <paramref name="property"/> is editing multiple objects the <paramref name="keyReturnPredicate"/> will be used to refer to other objects.</br>
        /// </summary>
        /// <param name="keyReturnPredicate">The key to return according to the given object.</param>
        private static void SetValue<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, Func<int, string> keyReturnPredicate, T value)
        {
            if (property == null || property.IsDisposed())
            {
                throw new ArgumentException($"[SerializedPropertyCustomData::Set{typeof(T).GetTypeDefinitionString(false, false)}] Given property is either null or disposed.", nameof(property));
            }

            if (keyReturnPredicate == null)
            {
                throw new ArgumentNullException(nameof(keyReturnPredicate), $"[SerializedPropertyCustomData::Set{typeof(T).GetTypeDefinitionString(false, false)}] Given argument is null.");
            }

            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                string indexValue = keyReturnPredicate(i);
                if (string.IsNullOrWhiteSpace(indexValue))
                {
                    throw new NullReferenceException($"[SerializedPropertyCustomData::Set{typeof(T).GetTypeDefinitionString()}] Key predicate returned null string at index {i}.");
                }

                string propertyKey = $"{m_noAllocPropertyStrings[i]}{UnityObjectIDUtility.DefaultObjIdentifierValueSeperator}{indexValue}";
                if (targetDict.ContainsKey(propertyKey))
                {
                    targetDict[propertyKey] = value;
                }
                else
                {
                    if (targetDict.Count >= containerDictionarySizeLimit)
                    {
                        targetDict.Remove(targetDict.First().Key);
                    }

                    targetDict.Add(propertyKey, value);
                }
            }
        }
        /// <inheritdoc cref="SetValue{T}(in SerializableDictionary{string, T}, SerializedProperty, Func{int, string}, T)"/>
        public static void SetValue<T>(in SerializableDictionary<string, T> targetDict, SerializedProperty property, Func<UnityEngine.Object, string> keyReturnPredicate, T value)
        {
            SetValue(targetDict, property, (int i) => keyReturnPredicate(property.serializedObject.targetObjects[i]), value);
        }

        /// <summary>
        /// Clears all binded datas.
        /// </summary>
        public static void Clear()
        {
            MainContainer.Reset();
        }

        // - Typed Impl
        /// <inheritdoc cref="HasDataKey{T}(in SerializableDictionary{string, T}, SerializedProperty, string)"/>
        public static bool HasStringKey(this SerializedProperty property, string key)
        {
            return HasDataKey(MainContainer.savedStringValues, property, key);
        }
        /// <summary>
        /// Returns a saved string value.
        /// <br>If the string key does not exist, it will return <see langword="null"/>.</br>
        /// <br/>
        /// <br><see cref="ArgumentException"/> = Thrown when the <paramref name="property"/> is editing multiple objects and same key has different values.</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static string GetString(this SerializedProperty property, string key)
        {
            return GetValue(MainContainer.savedStringValues, property, key);
        }
        /// <inheritdoc cref="GetString(SerializedProperty, string)"/>
        public static string GetString(this SerializedProperty property, string key, string defaultValue)
        {
            if (!HasStringKey(property, key))
            {
                return defaultValue;
            }

            return GetValue(MainContainer.savedStringValues, property, key);
        }
        /// <summary>
        /// Returns the list of saved string values.
        /// <br>The corresponding objects of <see cref="SerializedObject.targetObjects"/> are sequential to the given list.</br>
        /// </summary>
        public static string[] GetMultiStrings(this SerializedProperty property, string key)
        {
            return GetMultiValues(MainContainer.savedStringValues, property, key);
        }
        /// <summary>
        /// Sets a string to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects all keys are set to the same value.</br>
        /// </summary>
        public static void SetString(this SerializedProperty property, string key, string value)
        {
            SetValue(MainContainer.savedStringValues, property, key, value);
        }
        /// <summary>
        /// Sets a string to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects the <paramref name="keyReturnPredicate"/> will be used to refer to other objects.</br>
        /// </summary>
        public static void SetString(this SerializedProperty property, Func<int, string> keyReturnPredicate, string value)
        {
            SetValue(MainContainer.savedStringValues, property, keyReturnPredicate, value);
        }
        /// <inheritdoc cref="SetString(SerializedProperty, Func{int, string}, string)"/>
        public static void SetString(this SerializedProperty property, Func<UnityEngine.Object, string> keyReturnPredicate, string value)
        {
            SetValue(MainContainer.savedStringValues, property, keyReturnPredicate, value);
        }

        /// <inheritdoc cref="HasDataKey{T}(in SerializableDictionary{string, T}, SerializedProperty, string)"/>
        public static bool HasLongKey(this SerializedProperty property, string key)
        {
            return HasDataKey(MainContainer.savedIntValues, property, key);
        }
        /// <summary>
        /// Returns a saved long value.
        /// <br>If the string key does not exist, it will return <see langword="null"/>.</br>
        /// <br/>
        /// <br><see cref="ArgumentException"/> = Thrown when the <paramref name="property"/> is editing multiple objects and same key has different values.</br>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        public static long GetLong(this SerializedProperty property, string key)
        {
            return GetValue(MainContainer.savedIntValues, property, key);
        }
        /// <inheritdoc cref="GetLong(SerializedProperty, string)"/>
        public static long GetLong(this SerializedProperty property, string key, long defaultValue)
        {
            if (!HasLongKey(property, key))
            {
                return defaultValue;
            }

            return GetValue(MainContainer.savedIntValues, property, key);
        }
        /// <summary>
        /// Returns the list of saved long values.
        /// <br>The corresponding objects of <see cref="SerializedObject.targetObjects"/> are sequential to the given list.</br>
        /// </summary>
        public static long[] GetMultiLongs(this SerializedProperty property, string key)
        {
            return GetMultiValues(MainContainer.savedIntValues, property, key);
        }
        /// <summary>
        /// Sets a long to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects all keys are set to the same value.</br>
        /// </summary>
        public static void SetLong(this SerializedProperty property, string key, long value)
        {
            SetValue(MainContainer.savedIntValues, property, key, value);
        }
        /// <summary>
        /// Sets a long to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects the <paramref name="keyReturnPredicate"/> will be used to refer to other objects.</br>
        /// </summary>
        public static void SetLong(this SerializedProperty property, Func<int, string> keyReturnPredicate, long value)
        {
            SetValue(MainContainer.savedIntValues, property, keyReturnPredicate, value);
        }
        /// <inheritdoc cref="SetString(SerializedProperty, Func{int, string}, string)"/>
        public static void SetLong(this SerializedProperty property, Func<UnityEngine.Object, string> keyReturnPredicate, long value)
        {
            SetValue(MainContainer.savedIntValues, property, keyReturnPredicate, value);
        }
    }
}
