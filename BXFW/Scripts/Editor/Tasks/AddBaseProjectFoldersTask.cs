﻿using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace BXFW.Tools.Editor
{
    [System.Serializable]
    public class AddBaseProjectFoldersTask : EditorTask
    {
        /// <summary>
        /// The root asset folder, but for unity relative directories.
        /// </summary>
        private const string UnityRootAssetFolder = "Assets";
        /// <summary>
        /// The root asset folder, as an absolute path.
        /// </summary>
        private string AbsoluteRootAssetFolder => Path.Combine(Directory.GetCurrentDirectory(), UnityRootAssetFolder, RootDirectory);
        /// <summary>
        /// Default lists of paths to generate.
        /// </summary>
        private static readonly IReadOnlyList<string> DefaultGenerateFolders = new[]
        {
            "Fonts",
            "Materials/Shaders",
            "Prefabs", "Resources",
            "Scripts",
            "3DModel", "Sounds", "Textures"
        };
        // disallow illegal folder chars (except for directory seperators for making directories)
        [Tooltip("List of folders to generate."), DisallowChars("?<>:*|\"")]
        public List<string> generateFolderPaths;
        [Tooltip("Root directory to generate the folders into."), DisallowChars("?<>:*|\"")]
        public string RootDirectory = string.Empty;

        private void OnEnable()
        {
            generateFolderPaths = new List<string>(DefaultGenerateFolders);
        }

        public override void Run()
        {
            foreach (string path in generateFolderPaths)
            {
                // Clean string in list
                string pathClean = path.TrimStart(' ', '/', '\\').TrimEnd(' ', '/', '\\');
                string absPath = Path.Combine(AbsoluteRootAssetFolder, pathClean);
                // Create directory if it does not exist (use System.IO instead of UnityEditor because the latter does not work)
                // Directory.CreateDirectory creates the parent directory if it does not exist.
                if (!Directory.Exists(absPath))
                {
                    Directory.CreateDirectory(absPath);
                }
            }

            // Forcefully update as all the 
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }
    }
}
