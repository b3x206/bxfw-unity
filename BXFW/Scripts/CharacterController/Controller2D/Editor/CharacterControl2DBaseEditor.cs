using System.Collections;
using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(CharacterControl2DBase))]
    internal class CharacterControl2DBaseEditor : Editor
    {
        private CharacterControl2DBase Target => (CharacterControl2DBase)target;
        private SerializedObject controlBaseProperty;

        //private void OnEnable()
        //{ 
        //}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}