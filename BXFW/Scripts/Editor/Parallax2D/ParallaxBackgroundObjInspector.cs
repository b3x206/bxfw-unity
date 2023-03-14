using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(ParallaxBackgroundObj))]
    public class ParallaxBackgroundObjInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}
