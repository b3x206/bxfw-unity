using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BXFW.Data
{
    /// <summary>
    /// A utility script that only returns the FileID's / UUID's of <see cref="ScriptableObject"/>s.
    /// <br>This may only be used in cases where JSONUtility dissapoints you and you have full access to the 'ScriptableObject'.
    /// (Instead of using this please use OdinSerializer)
    /// </br>
    /// </summary>
    internal static class ScriptableObjectIDUtility
    {
#if UNITY_EDITOR
        /// <summary>
        /// The object / key property identifier seperator used.
        /// </summary>
        public const string OBJ_IDENTIFIER_PROPERTY_SEP = "::";
        /// <summary>
        /// Returns the local file identifier for the <paramref name="target"/>.
        /// </summary>
        public static long GetLocalFileIdentifier(UnityEngine.Object target)
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
        public static string GetUnityObjectIdentifier(UnityEngine.Object target)
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
                    // TODO : This needs fixing? or just leave as is?
                    guid = AssetDatabase.AssetPathToGUID(SceneManager.GetActiveScene().path);
                    //guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(target));
                    fileID = GetLocalFileIdentifier(target);
                }

                result = $"{guid}{OBJ_IDENTIFIER_PROPERTY_SEP}{fileID}";
            }

            return result;
        }
#endif
    }
}
