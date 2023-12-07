using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Singleton that is for <see cref="ScriptableObject"/>'s.
    /// <br>Loads asset using <see cref="Resources.Load"/>.</br>
    /// </summary>
    public class ScriptableObjectSingleton<T> : ScriptableObject
        where T : ScriptableObject
    {
        [NonSerialized]
        private static T m_Instance;
        public static T Instance
        {
            get
            {
                if (m_Instance != null)
                {
                    return m_Instance;
                }

                // If instance isn't loaded, we need to load it.
                // Simplest way to find instance is to call Resources.LoadAll<>() with a empty directory.
                // While inefficient and 'u call load on MonoBehaviour Constructor thats illegal' types of error prone, it will work for now.
                T[] soCurrent = Resources.LoadAll<T>(string.Empty);

                if (soCurrent.Length <= 0)
                {
                    // Error message not necessary, error checking should be implemented. (on target code)
                    return null;
                }
                if (soCurrent.Length > 1)
                {
                    Debug.LogWarning(string.Format("[ScriptableObjectSingleton::Instance] There is multiple scriptable object found in resources with type '{0}'. Loading the first one.", typeof(T).Name));
                }

                m_Instance = soCurrent[0];
                return m_Instance;
            }
        }

        /// <summary>
        /// <c>EDITOR ONLY : </c>
        /// Creates instance at given relative directory. Handles <see cref="UnityEditor.AssetDatabase"/> related methods.
        /// <br>NOTE : Only one instance can be created. <see cref="Resources.Load(string)"/> method is called.</br>
        /// </summary>
        /// <param name="relativeDir">Relative directory to the file. NOTE : Starts from /Resources, no need to pass '/Resources'.</param>
        /// <param name="fileName">Name of the file to create.</param>
        // we cannot fix the 'Scriptable Object size not same !!!11!'
        // but the load also works fine, wtf? this seems to be some sort of bug that i can't solve because i don't know what's going on
        // The built game and it's data structures look completely fine (checked using AssetRipper)
        // And also throw compiler errors while compiling so that we don't have to rely on runtime exceptions solely
        [MethodImpl(MethodImplOptions.NoOptimization)]
#if UNITY_EDITOR
        public
#else
        private
#endif
        static T CreateEditorInstance(string relativeDir, string fileName, bool enforceAssetPrefix = true)
        {
#if UNITY_EDITOR
            if (Instance != null)
            {
                Debug.LogWarning(string.Format("[ScriptableObjectSingleton::CreateEditorInstance] Create instance called for type '{0}' even though an instance exists.", typeof(T).Name));
                return default;
            }

            // Create & serialize instance of the resource.
            // Find the directory
            string loadableResourcesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "/Assets/Resources");
            string checkedRelativeDir = relativeDir.Substring(relativeDir.IndexOf(loadableResourcesDirectory) + 1); // This relative directory omits the '/resources' junk.
            string relativeParentDir = Path.Combine("Assets/Resources/", checkedRelativeDir);
            string absoluteParentDir = Path.Combine(loadableResourcesDirectory, checkedRelativeDir);

            // If the relative directory isn't created, the creation will fail.
            // For that, i will actually get the combined path.
            if (!Directory.Exists(absoluteParentDir))
            {
                Directory.CreateDirectory(absoluteParentDir);
            }

            // Actually create the thing.
            T cInstance = CreateInstance<T>();

            string AssetExtensionPrefix = ".asset";
            if (enforceAssetPrefix && !fileName.EndsWith(AssetExtensionPrefix))
            {
                fileName += AssetExtensionPrefix;
            }

            UnityEditor.AssetDatabase.CreateAsset(cInstance, Path.Combine(relativeParentDir, fileName));
            UnityEditor.AssetDatabase.Refresh();

            m_Instance = Instance;
            return m_Instance;
#else
            // Now with the terrible workaround this should be only thrown via reflection
            throw new System.InvalidOperationException("[ScriptableObjectSingleton::CreateEditorInstance] Called editor method in runtime!");
#endif
        }
    }
}
