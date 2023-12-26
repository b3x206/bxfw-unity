using UnityEditor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ParallaxBackgroundLayer))]
    public class ParallaxBackgroundLayerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            using EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true);
            base.OnInspectorGUI();
        }
    }
}
