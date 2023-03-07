using System;
using System.Collections.Generic;
using BXFW.Tools.Editor;
using UnityEngine;
using UnityEditor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(PlayerMovement))]
    public class PlayerMovementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            var target = base.target as PlayerMovement;

            if (!target.canMove)
            {
                dict.Add(nameof(target.canMove), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Before, () => EditorGUILayout.HelpBox("Player will not move. These settings won't change anything.", MessageType.Info)));
            }
            if (!target.useInternalInputMove)
            {
                dict.Add(nameof(target.moveForwardInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveBackwardInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveLeftInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveRightInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveCrouchInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveJumpInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.moveRunInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }
            if (!target.UseGravity)
            {
                dict.Add(nameof(target.gravity), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(target.groundMask), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }
            switch (target.currentCameraView)
            {
                case PlayerMovement.PlayerViewType.TPS:
                case PlayerMovement.PlayerViewType.FreeRelativeCam:
                    break;

                default:
                    dict.Add(nameof(target.targetCamera), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () => 
                    {
                        var gEnabled = GUI.enabled;

                        GUI.enabled = false;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.targetCamera)));
                        GUI.enabled = gEnabled;
                    }));
                    break;
            }

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
