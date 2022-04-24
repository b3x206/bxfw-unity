﻿using System.Collections;
using System.IO;
using UnityEngine;

namespace BXFW
{
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
                    // Error message not necessary, error checking should be implemented.
                    // OnInstanceNotFound();
                    // we could use delegates or temporary instances of 'T' but yeah, it's finicky at best.
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

#if UNITY_EDITOR
        /// Please do not be like me and mix runtime scripts with editor scripts.
        /// Since the nature of <see cref="ScriptableObject"/>'s are editor, this is why this method is here.
        /// <summary>
        /// <c>EDITOR ONLY : </c>
        /// Creates instance at given relative directory.
        /// <br>NOTE : Only one instance can be created. <see cref="Resources.Load(string)"/> method is called</br>
        /// </summary>
        /// <param name="relDir">Relative directory to the file. NOTE : Starts from /Resources, no need to pass '/Resources'.</param>
        public static T CreateEditorInstance(string relDir, string fileName)
        {
            if (Instance != null)
            {
                Debug.LogWarning(string.Format("[ScriptableObjectSingleton::CreateEditorInstance] Create instance called for type '{0}' even though an instance exists.", typeof(T).Name));
                return default;
            }

            // Create & serialize instance of the resource.
            // Find the directory
            var relativeParentDir = Path.Combine("Assets/Resources/", relDir.Substring(relDir.IndexOf(Tools.Editor.EditorAdditionals.ResourcesDirectory) + 1));
            var absParentDirectory = Path.Combine(Tools.Editor.EditorAdditionals.ResourcesDirectory);

            // If the relative directory isn't created, the creation will fail.
            // For that, i will actually get the combined path.
            if (!Directory.Exists(absParentDirectory))
            {
                Directory.CreateDirectory(absParentDirectory);
            }

            // Actually create the thing.
            var cInstance = CreateInstance<T>();

            UnityEditor.AssetDatabase.CreateAsset(cInstance, Path.Combine(relativeParentDir, fileName));
            UnityEditor.AssetDatabase.Refresh();

            instance = Instance;
            return instance;
        }
#endif
    }
}