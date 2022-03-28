using UnityEditor;
using UnityEngine;

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

        private void OnGUI()
        {
            
        }
    }
}