using System.IO;
using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Singleton that is for <see cref="ScriptableObject"/>'s.
    /// <br>Loads asset using <see cref="Resources.Load"/></br>
    /// </summary>
    public class ScriptableObjectSingleton<T> : ScriptableObject
        where T : ScriptableObject
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                // If instance isn't loaded, we need to load it.
                // Simplest way to find instance is to call Resources.LoadAll<>() with a empty directory.
                // While inefficient and 'u call load on MonoBehaviour Constructor thats illegal' types of error prone, it will work for now.
                var soCurrent = Resources.LoadAll<T>(string.Empty);

                if (soCurrent.Length <= 0)
                {
                    // Error message not necessary, error checking should be implemented. (on target code)
                    return null;
                }
                if (soCurrent.Length > 1)
                {
                    Debug.LogWarning(string.Format("[ScriptableObjectSingleton::Instance] There is multiple scriptable object found in resources with type '{0}'. Loading the first one.", typeof(T).Name));
                }

                instance = soCurrent[0];
                return instance;
            }
        }

        /// <summary>
        /// <c>EDITOR ONLY : </c>
        /// Creates instance at given relative directory.
        /// <br>NOTE : Only one instance can be created. <see cref="Resources.Load(string)"/> method is called</br>
        /// </summary>
        /// <param name="relativeDir">Relative directory to the file. NOTE : Starts from /Resources, no need to pass '/Resources'.</param>
        /// <param name="fileName">Name of the file to create.</param>
        public static T CreateEditorInstance(string relativeDir, string fileName)
        {
#if UNITY_EDITOR
            if (Instance != null)
            {
                Debug.LogWarning(string.Format("[ScriptableObjectSingleton::CreateEditorInstance] Create instance called for type '{0}' even though an instance exists.", typeof(T).Name));
                return default;
            }

            // Create & serialize instance of the resource.
            // Find the directory
            var checkedRelativeDir = relativeDir.Substring(relativeDir.IndexOf(Tools.Editor.EditorAdditionals.ResourcesDirectory) + 1); // This relative directory omits the '/resources' junk.
            var relativeParentDir = Path.Combine("Assets/Resources/", checkedRelativeDir);
            var absoluteParentDir = Path.Combine(Tools.Editor.EditorAdditionals.ResourcesDirectory, checkedRelativeDir);

            // If the relative directory isn't created, the creation will fail.
            // For that, i will actually get the combined path.
            if (!Directory.Exists(absoluteParentDir))
            {
                Directory.CreateDirectory(absoluteParentDir);
            }

            // Actually create the thing.
            var cInstance = CreateInstance<T>();

            UnityEditor.AssetDatabase.CreateAsset(cInstance, Path.Combine(relativeParentDir, fileName));
            UnityEditor.AssetDatabase.Refresh();

            instance = Instance;
            return instance;
#else
            throw new System.Exception("[ScriptableObjectSingleton] Called editor method in runtime!");
#endif
        }
    }
}