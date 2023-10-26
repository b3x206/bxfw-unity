#if UNITY_EDITOR
using UnityEngine;
using System.Reflection;
using UnityEditor;

// - editor related data utils - //
namespace BXFW.Data.Editor
{
    /// <summary>
    /// <c>[ EDITOR ONLY ]</c> A utility script that only returns the FileID's / UUID's of <see cref="UnityEngine.Object"/>s.<br/>
    /// (Instead of using this for serialization purposes please use OdinSerializer) <br/>
    /// </summary>
    public static class UnityObjectIDUtility
    {
        /// <summary>
        /// The default object / key property identifier seperator used.
        /// </summary>
        public const string DefaultObjIdentifierValueSeperator = "::";
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
        /// <param name="valueSeperator">The value seperator to use if you have a custom one.</param>
        public static string GetUnityObjectIdentifier(UnityEngine.Object target, string valueSeperator = DefaultObjIdentifierValueSeperator)
        {
            string result;

            // Assume that the property's target object is atleast a component
            if (target is Component c)
            {
                // Can get 'LocalFileIdentifier' directly apparently
                result = $"{AssetDatabase.AssetPathToGUID(c.gameObject.scene.path)}{valueSeperator}{GetLocalFileIdentifier(c)}";
            }
            // The target value we are looking for is a GameObject though
            // Could make this code more compact but at the cost of slight performance
            // Plus this works probably fine so it's ok.
            else if (target is GameObject o)
            {
                result = $"{AssetDatabase.AssetPathToGUID(o.scene.path)}{valueSeperator}{GetLocalFileIdentifier(o)}";
            }
            // If this is not the case, assume it's a local filesystem asset. (can be ScriptableObject)
            // In that case use the object's own GUID
            else
            {
                // Use the normal method for local assets
                bool tryGetResult = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(target, out string guid, out long fileID);
                if (!tryGetResult)
                {
                    // stupid unity ignores the fact that the target exists on the scene
                    // but since it doesn't have any GUID's we have to improvise with this crap hack
                    // which will not make unique objects on other scenes
                    // --
                    // this is probably the best solution for objects that have no GUID
                    // No serialization of these as reference of guid+file id pair is possible
                    guid = string.Empty;
                    fileID = GetLocalFileIdentifier(target);
                }

                result = $"{guid}{valueSeperator}{fileID}";
            }

            return result;
        }
    }
}
#endif
