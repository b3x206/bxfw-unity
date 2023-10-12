using System;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace BXFW.Tools.Editor
{
    /// Since you can't directly intercept the unity serializer of editor,
    /// you cannot bind your custom new datas to <see cref="SerializedProperty"/>ies.
    /// This class provides a hack to do this.
    /// <summary>
    /// Manages the ability to bind custom serialized property datas to <see cref="SerializedProperty"/>ies.
    /// <br>Creates a <see cref="ScriptableObjectSingleton{T}"/> to use on editor.</br>
    /// </summary>
    public static class SerializedPropertyCustomData
    {
        // -- Singletons
        private static PropertyCustomDataContainer m_MainContainer;
        private const string MAIN_CONTAINER_FOLDER = "Editor"; // Folder for the 'ScriptableObjectSingleton{T}'
        private const string MAIN_CONTAINER_FILE_NAME = "SPropertyCustomData.asset";
        /// <summary>
        /// The singleton dictionary container.
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
                        m_MainContainer = PropertyCustomDataContainer.CreateEditorInstance(MAIN_CONTAINER_FOLDER, MAIN_CONTAINER_FILE_NAME);
                    }
                }

                return m_MainContainer;
            }
        }

        // -- ID Binding
        /// <summary>
        /// The object / key property identifier seperator used.
        /// </summary>
        public const string OBJ_IDENTIFIER_PROPERTY_SEP = "::";
        /// <summary>
        /// Returns the GUID + fileID combined of <paramref name="target"/>.
        /// <br>If the <paramref name="target"/> is a component or a GameObject, the scene GUID + the fileID of the objects are combined.</br>
        /// <br>If the <paramref name="target"/> is not a scene object (i.e ScriptableObject or an asset importer thing), the file already has it's own GUID + fileID.</br>
        /// </summary>
        private static string GetUnityObjectIdentifier(UnityEngine.Object target)
        {
            // Assume that the property's target object is atleast a component
            if (target is Component c)
            {
                // Cannot get 'LocalFileIdentifier' directly so have to assert a 'Try' method.
                Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(c.gameObject, out string _, out long fileID), "[SerializedPropertyCustomData::GetUnityObjectIdentifier] => TryGetGUIDAndLocalFileIdentifier is false.");
                return $"{AssetDatabase.AssetPathToGUID(c.gameObject.scene.path)}{OBJ_IDENTIFIER_PROPERTY_SEP}{fileID}";
            }
            // The target value we are looking for is a GameObject though
            // Could make this code more compact but at the cost of slight performance
            // Plus this works probably fine so it's ok.
            else if (target is GameObject o)
            {
                Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out string _, out long fileID), "[SerializedPropertyCustomData::GetUnityObjectIdentifier] => TryGetGUIDAndLocalFileIdentifier is false.");
                return $"{AssetDatabase.AssetPathToGUID(o.scene.path)}{OBJ_IDENTIFIER_PROPERTY_SEP}{fileID}";
            }
            // If this is not the case, assume it's a local filesystem asset. (can be ScriptableObject)
            // In that case use the object's own GUID
            else
            {
                Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long fileID), "[SerializedPropertyCustomData::GetUnityObjectIdentifier] => TryGetGUIDAndLocalFileIdentifier is false.");
                return $"{guid}{OBJ_IDENTIFIER_PROPERTY_SEP}{fileID}";
            }
        }

        /// <summary>
        /// Returns a unique property identity string depending on the;
        /// <br>A : The scene that this <paramref name="property"/> is contained in (and it's GUID).</br>
        /// <br>B : The object that this <paramref name="property"/> is contained in.</br>
        /// <br>C : And the <see cref="SerializedProperty.propertyPath"/> of <paramref name="property"/>.</br>
        /// <br>These are combined to return a unique fingerprint of the property (not hashed).</br>
        /// </summary>
        public static string GetPropertyString(SerializedProperty property)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                throw new ArgumentException(@"[SerializedPropertyCustomData::GetPropertyString] Property is owned by multiple objects. Use 'GetMultiEditPropertyStrings' 
or don't call this if the 'property.serializedObject.isEditingMultipleObjects' is true.", nameof(property));
            }

            UnityEngine.Object targetObject = property.serializedObject.targetObject;
            return GetUnityObjectIdentifier(targetObject);
        }
        /// <summary>
        /// <inheritdoc cref="GetPropertyString(SerializedProperty)"/>
        /// <br>This calls the <see cref="GetPropertyString(SerializedProperty)"/> to the
        /// <see cref="SerializedProperty"/>'s <see cref="SerializedObject.targetObjects"/>.</br>
        /// </summary>
        public static string[] GetMultiPropertyStrings(SerializedProperty property)
        {
            // It doesn't matter if we aren't even editing multiple objects in this case
            string[] list = new string[property.serializedObject.targetObjects.Length];

            GetMultiPropertyStringsNoAlloc(property, list);
            return list;
        }
        /// <summary>
        /// <inheritdoc cref="GetMultiPropertyStrings(SerializedProperty)"/>
        /// <br/>
        /// <br>This doesn't allocate an array. Note that the 'strings' has to have enough space allocated.</br>
        /// </summary>
        /// <returns>The size of the filled 'strings'.</returns>
        public static int GetMultiPropertyStringsNoAlloc(SerializedProperty property, string[] strings)
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
                strings[i] = GetUnityObjectIdentifier(targetObject);
            }

            return property.serializedObject.targetObjects.Length;
        }
        /// <summary>
        /// <inheritdoc cref="GetMultiPropertyStrings(SerializedProperty)"/>
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
            for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObjects[i];
                strings.Add(GetUnityObjectIdentifier(targetObject));
            }
        }
        
        // -- Data Binding + Adding
        // FIXME : Generalize the way of getting a keyed data seperation? or this is fine (but still fragile)
        /// <summary>
        /// General purpose no-alloc list container.
        /// </summary>
        private static readonly List<string> m_noAllocPropertyStrings = new List<string>();
        /// <summary>
        /// Returns whether if the 'SerializedProperty' target object contains the key.
        /// <br>For 'SerializedProperties' that are editing multiple objects it returns whether if the all targets contain the key.</br>
        /// </summary>
        public static bool HasDataKey(this SerializedProperty property, string key)
        {
            foreach (var propertyTarget in property.serializedObject.targetObjects)
            {
                string saveKey = $"{GetUnityObjectIdentifier(propertyTarget)}{OBJ_IDENTIFIER_PROPERTY_SEP}{key}";

                if (!MainContainer.savedIntValues.ContainsKey(saveKey))
                    return false;
                if (!MainContainer.savedStringValues.ContainsKey(saveKey))
                    return false;
            }

            return true;
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
            // 'Get' methods should not throw if all the values are the same on the list.
            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            string initialValue = MainContainer.savedStringValues.GetValueOrDefault($"{m_noAllocPropertyStrings[0]}{OBJ_IDENTIFIER_PROPERTY_SEP}{key}");
            // Ensure all values with keys are the same
            for (int i = 1; i < m_noAllocPropertyStrings.Count; i++)
            {
                var propString = MainContainer.savedStringValues.GetValueOrDefault($"{m_noAllocPropertyStrings[i]}{OBJ_IDENTIFIER_PROPERTY_SEP}{key}");
                if (propString != initialValue)
                {
                    throw new ArgumentException($"[SerializedPropertyCustomData::GetString] Different value values on a multi edit 'SerializedProperty' with key : {key}, Values were {propString} != {initialValue}", nameof(property));
                }
            }

            // Values are same, can safely return.
            return initialValue;
        }
        /// <summary>
        /// Returns the list of saved string values.
        /// <br>The corresponding objects of <see cref="SerializedObject.targetObjects"/> are sequential to the given list.</br>
        /// </summary>
        public static string[] GetMultiStrings(this SerializedProperty property, string key)
        {
            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            string[] values = new string[m_noAllocPropertyStrings.Count];
            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                values[i] = MainContainer.savedStringValues.GetValueOrDefault($"{m_noAllocPropertyStrings[i]}{OBJ_IDENTIFIER_PROPERTY_SEP}{key}");
            }

            return values;
        }

        /// <summary>
        /// Sets a string to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects all keys are set to the same value.</br>
        /// </summary>
        public static void SetString(this SerializedProperty property, string key, string value)
        {
            if (property == null || property.IsDisposed())
                throw new ArgumentNullException(nameof(property), "[SerializedPropertyCustomData::SetString] Given property is either null or disposed.");
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "[SerializedPropertyCustomData::SetString] Given key is null.");

            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);
            
            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                MainContainer.savedStringValues.Add($"{m_noAllocPropertyStrings[i]}{OBJ_IDENTIFIER_PROPERTY_SEP}{key}", value);
            }
        }
        /// <summary>
        /// Sets a string to <paramref name="property"/>.
        /// <br>If the <paramref name="property"/> is editing multiple objects the <paramref name="keyReturnPredicate"/> will be used to refer to other objects.</br>
        /// </summary>
        public static void SetString(this SerializedProperty property, Func<int, string> keyReturnPredicate, string value)
        {
            if (property == null || property.IsDisposed())
                throw new ArgumentNullException(nameof(property), "[SerializedPropertyCustomData::SetString] Given property is either null or disposed.");
            if (keyReturnPredicate == null)
                throw new ArgumentNullException(nameof(keyReturnPredicate), "[SerializedPropertyCustomData::SetString] Given keyReturnPredicate is null.");

            GetMultiPropertyStringsNoAlloc(property, m_noAllocPropertyStrings);

            for (int i = 0; i < m_noAllocPropertyStrings.Count; i++)
            {
                MainContainer.savedStringValues.Add($"{m_noAllocPropertyStrings[i]}{OBJ_IDENTIFIER_PROPERTY_SEP}{keyReturnPredicate(i)}", value);
            }

        }
        /// <inheritdoc cref="SetString(SerializedProperty, Func{int, string}, string)"/>
        public static void SetString(this SerializedProperty property, Func<UnityEngine.Object, string> keyReturnPredicate, string value)
        {
            SetString(property, (int i) => keyReturnPredicate(property.serializedObject.targetObjects[i]), value);
        }
    }
}
