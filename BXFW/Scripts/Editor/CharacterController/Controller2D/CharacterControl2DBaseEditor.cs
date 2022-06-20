using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(CharacterControl2DBase), true)]
    internal class CharacterControl2DBaseEditor : Editor
    {
        private new CharacterControl2DBase target { get { return (CharacterControl2DBase)base.target; } }
        private SerializedObject controlBaseProperty;

        //private void OnEnable()
        //{ 
        //}

        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();

            switch (target.moveAxis)
            {
                case TransformAxis2D.None:
                default:
                    break;
                // Hide jump if player can move on all axis
                case TransformAxis2D.XYAxis:
                    dict.Add(nameof(CharacterControl2DBase.jumpAxis), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    dict.Add(nameof(CharacterControl2DBase.jumpSpeed), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    dict.Add(nameof(CharacterControl2DBase.MoveJumpInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                    break;
            }

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}