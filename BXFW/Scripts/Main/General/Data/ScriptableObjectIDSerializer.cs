using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BXFW.Data
{
    /// <summary>
    /// A serializer that only serializes the fileid's / uuids of <see cref="ScriptableObject"/>s.
    /// <br>This may only be used in cases where JSONUtility dissapoints you. (Instead of using this please use OdinSerializer)</br>
    /// </summary>
    public static class ScriptableObjectIDSerializer
    {
        // TODO : This code is a crime
        // TODO 2 : Move this to an utility script + add the ability of getting unique file identifiers on runtime?
        // but idk what it will be based on?

        /// <summary>
        /// The object / key property identifier seperator used.
        /// </summary>
        public const string OBJ_IDENTIFIER_PROPERTY_SEP = "::";
#if UNITY_EDITOR
        /// <summary>
        /// Returns the local file identifier for the <paramref name="target"/>.
        /// </summary>
        internal static long GetLocalFileIdentifier(UnityEngine.Object target)
        {
            // Get the required field, this gives the InspectorMode enum field
            PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

            // Check if target is prefab
            var prefabObject = PrefabUtility.GetCorrespondingObjectFromSource(target);
            // If the target is prefab, we also need it's parent's id times int.MaxValue and the local id.
            // For getting the proper id.
            // But for the time being leave as is because prefabs are ok (as those are not exactly scenes)

            using SerializedObject serializedObject = new SerializedObject(prefabObject != null ? prefabObject : target);
            // Setting this enables the 'm_LocalIdentifierInFile' field
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            // Get the field normally
            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");
            return localIdProp.longValue;
        }
        /// <summary>
        /// Returns the GUID + fileID combined of <paramref name="target"/>.
        /// <br>If the <paramref name="target"/> is a component or a GameObject, the scene GUID + the fileID of the objects are combined.</br>
        /// <br>If the <paramref name="target"/> is not a scene object (i.e ScriptableObject or an asset importer thing), the file already has it's own GUID + fileID.</br>
        /// </summary>
        internal static string GetUnityObjectIdentifier(UnityEngine.Object target)
        {
            string result;

            // Assume that the property's target object is atleast a component
            if (target is Component c)
            {
                // Can get 'LocalFileIdentifier' directly apparently
                result = $"{AssetDatabase.AssetPathToGUID(c.gameObject.scene.path)}{OBJ_IDENTIFIER_PROPERTY_SEP}{GetLocalFileIdentifier(c)}";
            }
            // The target value we are looking for is a GameObject though
            // Could make this code more compact but at the cost of slight performance
            // Plus this works probably fine so it's ok.
            else if (target is GameObject o)
            {
                result = $"{AssetDatabase.AssetPathToGUID(o.scene.path)}{OBJ_IDENTIFIER_PROPERTY_SEP}{GetLocalFileIdentifier(o)}";
            }
            // If this is not the case, assume it's a local filesystem asset. (can be ScriptableObject)
            // In that case use the object's own GUID
            else
            {
                // Use the normal method for local assets
                bool tryGetResult = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long fileID);
                if (!tryGetResult)
                {
                    // stupid unity does not know that the target exists on the scene
                    // but since it doesn't have any GUID's we have to improvise with
                    // !!! TODO : This needs fixing? or just leave as is?
                    guid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
                    //guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(target));
                    fileID = GetLocalFileIdentifier(target);
                }

                result = $"{guid}{OBJ_IDENTIFIER_PROPERTY_SEP}{fileID}";
            }

            return result;
        }
#endif

        // example unity json data :
        // a bare uuid setting is like (has to be defined as a generic object dict)
        // > { "uuid" : "1283098178093" }
        // -- an array object looks like this
        // {
        //   "array_value" :
        //   [
        //     'bare_uuid_def_for_index_0',
        //     'bare_uuid_def_for_index_1'
        //   ]
        // }
        private const char TK_JSON_KEY_VALUE_DEF = ':';
        private const char TK_JSON_KEY_VALUE_CONTAINER = '"';
        private const char TK_JSON_VALUE_SEP = ',';
        private const char TK_JSON_DICT_OPEN = '{';
        private const char TK_JSON_DICT_CLOSE = '}';
        private const char TK_JSON_ARRAY_OPEN = '[';
        private const char TK_JSON_ARRAY_CLOSE = ']';

        private const string UNITY_DATA_TK_GUID = "guid";
        private const string UNITY_DATA_TK_FILE_ID = "fileID";
        private const string UNITY_DATA_TK_TYPE = "type";
        // type is always 3 for ScriptableObjects
        private const string UNITY_DATA_TK_TYPE_CONST = "3";

        /// <summary>
        /// Converts the serializer elements to json, so that it can be loaded with <see cref="JsonUtility"/>.
        /// </summary>
        [Obsolete("Don't use, doesn't work and will probably get removed", false)]
        public static string ToJson<T>(T value)
            where T : GUIDScriptableObject
        {
            return ToJsonElement(value);
        }

        private static readonly Dictionary<Type, string> targetTypeArrayFieldNamesCache = new Dictionary<Type, string>(16);
        /// <summary>
        /// Converts an array to json.
        /// <br>Only supports array container types as other types won't get deserialized with <see cref="JsonUtility.FromJson{T}(string)"/>.</br>
        /// </summary>
        [Obsolete("Don't use, doesn't work and will probably get removed", false)]
        public static string ToJson<T>(IEnumerable<T> collection)
            where T : GUIDScriptableObject
        {
            // Get the target internal list name by getting all the fields
            if (!targetTypeArrayFieldNamesCache.TryGetValue(collection.GetType(), out string arrayFieldName))
            {
                // we have to traverse a reflection fields tree.
                // what
                // ok, just do this for only one depth
                // as an Int32 contains an Int32, so we really can't realistically traverse things unless we have a 'IsAtomicPrimitiveType'
                // and i don't care for more than 1 depth.
                BindingFlags instanceAllVisibility = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                FieldInfo targetArrayInfo = collection.GetType().GetFields(instanceAllVisibility)
                .SingleOrDefault(fi =>
                {
                    // o(n!) moment
                    // how fast is this? answer : no, it misses all of your precious caches and loads mspaint.exe to your l3 cache for fun
                    return fi.FieldType.GetFields(instanceAllVisibility)
                        .Any(fiChild => fiChild.FieldType.GetInterfaces()
                            .Any(fiChildElementType => fiChildElementType == typeof(IList))
                        );
                });

                // If the array does not exist, just do nothing.
                if (targetArrayInfo == null)
                    return string.Empty;

                arrayFieldName = targetArrayInfo.Name;
                targetTypeArrayFieldNamesCache.Add(collection.GetType(), arrayFieldName);
            }

            int collectionSize = collection.Count();

            // Create an object definition { .. }
            StringBuilder sb = new StringBuilder(collectionSize * 32);
            sb.Append(TK_JSON_DICT_OPEN).Append(' ')
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(arrayFieldName).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_KEY_VALUE_DEF)
                .Append(TK_JSON_ARRAY_OPEN);

            foreach (KeyValuePair<int, T> indexedPair in collection.Indexed())
            {
                int i = indexedPair.Key;
                T value = indexedPair.Value;

                sb.Append(ToJsonElement(value));

                if (i != collectionSize - 1)
                {
                    sb.Append(TK_JSON_VALUE_SEP);
                }
            }

            sb.Append(TK_JSON_ARRAY_CLOSE).Append(' ').Append(TK_JSON_DICT_CLOSE);

            return sb.ToString();
        }

        /// <summary>
        /// Converts the <see cref="GUIDScriptableObject"/> into a json element.
        /// </summary>
        [Obsolete("Don't use, doesn't work and will probably get removed", false)]
        private static string ToJsonElement<T>(T value)
            where T : GUIDScriptableObject
        {
            // max length of an guid is 32, as it contains 4 uints
            StringBuilder sb = new StringBuilder(32);

            // Both fileID, uuid and type is serialized.
            string fileID = string.Empty;
            string guid = string.Empty;
            // The GUID value is meant to be splittable
            if (value != null)
            {
                int indexOfIdentifierSep = value.GUID.IndexOf(OBJ_IDENTIFIER_PROPERTY_SEP);
                if (indexOfIdentifierSep >= 0)
                {
                    guid = value.GUID.Substring(0, value.GUID.IndexOf(OBJ_IDENTIFIER_PROPERTY_SEP));
                    fileID = value.GUID.Substring(value.GUID.IndexOf(OBJ_IDENTIFIER_PROPERTY_SEP) + OBJ_IDENTIFIER_PROPERTY_SEP.Length);
                }
            }

            sb.Append(TK_JSON_DICT_OPEN).Append(' ')
                // "guid":"2398901283",
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(UNITY_DATA_TK_GUID).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_KEY_VALUE_DEF)
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(guid).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_VALUE_SEP)
                // "fileID":"239812389",
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(UNITY_DATA_TK_FILE_ID).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_KEY_VALUE_DEF)
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(fileID).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_VALUE_SEP)
                // "type": 3
                .Append(TK_JSON_KEY_VALUE_CONTAINER).Append(UNITY_DATA_TK_TYPE).Append(TK_JSON_KEY_VALUE_CONTAINER)
                .Append(TK_JSON_KEY_VALUE_DEF)
                // (no need to surround this on quotes)
                .Append(UNITY_DATA_TK_TYPE_CONST)
              .Append(' ').Append(TK_JSON_DICT_CLOSE);

            return sb.ToString();
        }
    }
}
