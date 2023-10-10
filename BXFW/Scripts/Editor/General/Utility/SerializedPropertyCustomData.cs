using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// Manages the ability to bind custom serialized property datas to <see cref="SerializedProperty"/>ies.
    /// <br>Creates a <see cref="ScriptableObjectSingleton{T}"/> to use on editor.</br>
    /// </summary>
    public static class SerializedPropertyCustomData
    {
        // java ass name
        private static PropertyCustomDataContainer m_MainContainer;
        private const string MAIN_CONTAINER_FOLDER = "Editor"; // Folder for the 'ScriptableObjectSingleton{T}'
        private const string MAIN_CONTAINER_FILE_NAME = "SPropertyCustomData.asset";
        public static PropertyCustomDataContainer MainContainer
        {
            get
            {
                // Access value only once as it calls 'Resources.Load
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

        private static string ComputeMD5Hash(string str)
        {
            using (MD5Cng md5 = new MD5Cng())
            {
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(str));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns a property string depending on the;
        /// <br>A : The scene that this <paramref name="property"/> is contained in (and it's GUID).</br>
        /// <br>B : The object that this <paramref name="property"/> is contained in.</br>
        /// <br>C : And the <see cref="SerializedProperty.propertyPath"/> of <paramref name="property"/>.</br>
        /// <br>These are combined to return a md5 hash of the property.</br>
        /// </summary>
        public static string GetPropertyString(SerializedProperty property)
        {
            if (property.serializedObject.isEditingMultipleObjects)
            {
                throw new ArgumentException("[]", nameof(property));
            }

            return string.Empty;
        }
        public static string[] GetMultiEditPropertyStrings(SerializedProperty property)
        {
            // It doesn't matter if we aren't even editing multiple objects in this case
            string[] list = new string[property.serializedObject.targetObjects.Length];

            for (int i = 0; i < property.serializedObject.targetObjects.Length; i++)
            {
                UnityEngine.Object targetObject = property.serializedObject.targetObjects[i];
                // Assume that the property's target object is atleast a component
                if (targetObject is Component c)
                {
                    list[i] = AssetDatabase.AssetPathToGUID(c.gameObject.scene.path);
                }
                // If this is not the case, assume it's a local filesystem asset. (can be ScriptableObject)
                // In that case use the object's own GUID
                else
                {
                    string p = AssetDatabase.GetAssetPath(targetObject);

                    list[i] = AssetDatabase.AssetPathToGUID(p);
                }
            }

            return list;
        }

        /// <summary>
        /// Returns a saved string value.
        /// <br>If the string key does not exist, it will return <see langword="null"/>.</br>
        /// </summary>
        public static string GetString(SerializedProperty property, string key)
        {
            string value = MainContainer.savedStringValues.GetValueOrDefault(key);
            return value;
        }
    }
}
