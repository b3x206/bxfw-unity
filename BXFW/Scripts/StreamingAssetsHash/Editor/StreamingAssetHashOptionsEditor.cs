using System.IO;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.Tools
{
    public class StreamingAssetHashOptionsEditor : EditorWindow
    {
        [MenuItem("Tools/Streaming Asset Hash")]
        public static void OpenHashFileEditor()
        {
            var w = CreateInstance<StreamingAssetHashOptionsEditor>();
            w.Show();
        }

        private static SerializedObject soCurrentHashObject;
        private static SerializedObject SOCurrentHashObject
        {
            get
            {
                if (soCurrentHashObject == null)
                    soCurrentHashObject = new SerializedObject(StreamingAssetHashOptions.Instance);

                return soCurrentHashObject;
            }
        }

        public const string OptionsResourceDir = ""; // Relative dir
        public const string OptionsResourceName = "AssetHashOptions.asset"; // File name

        private void OnGUI()
        {
            // Create 'StreamingAssetHashOptions' if it doesn't exist.
            if (StreamingAssetHashOptions.Instance == null)
                StreamingAssetHashOptions.CreateEditorInstance(OptionsResourceDir, OptionsResourceName);

            // Draw the array of StreamingAssetHashOptions
            SOCurrentHashObject.DrawCustomDefaultInspector(null);
        }
    }
}