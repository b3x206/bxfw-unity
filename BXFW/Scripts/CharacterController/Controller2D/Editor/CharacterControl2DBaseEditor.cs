using System.Collections;
using UnityEditor;
using UnityEngine;

namespace BXFW.Editor
{
    [CustomEditor(typeof(CharacterControl2DBase))]
    internal class CharacterControl2DBaseEditor : UnityEditor.Editor
    {
        private CharacterControl2DBase Target => (CharacterControl2DBase)target;


        private SerializedObject controlBaseProperty;

        private void OnEnable()
        {
            
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}