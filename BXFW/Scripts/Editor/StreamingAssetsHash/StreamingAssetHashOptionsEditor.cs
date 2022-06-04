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
            var w = GetWindow<StreamingAssetHashOptionsEditor>("Streaming Asset Integrity Thing");
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
            //if (StreamingAssetHashOptions.Instance == null)
            //    StreamingAssetHashOptions.CreateEditorInstance(OptionsResourceDir, OptionsResourceName);

            EditorGUILayout.HelpBox("[StreamingAssetsHash] This is still TODO, i need to find a better structure for this asset.", MessageType.Warning);
            if (GUILayout.Button("Close"))
            {
                Close();
            }

            //// Draw the array of StreamingAssetHashOptions
            //SOCurrentHashObject.DrawCustomDefaultInspector(null);
        }
    }
}