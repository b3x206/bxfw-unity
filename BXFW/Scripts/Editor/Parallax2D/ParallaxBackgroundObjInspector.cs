using UnityEngine;
using UnityEditor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ParallaxBackgroundLayer))]
    public class ParallaxBackgroundObjInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            using EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true);
            base.OnInspectorGUI();
        }
    }
}
