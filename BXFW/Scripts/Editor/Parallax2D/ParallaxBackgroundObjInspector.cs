using UnityEngine;
using UnityEditor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ParallaxBackgroundLayer))]
    public class ParallaxBackgroundObjInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var gEnabled = GUI.enabled;
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = gEnabled;
        }
    }
}
