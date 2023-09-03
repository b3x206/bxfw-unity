using System;
using System.Collections.Generic;
using BXFW.Tools.Editor;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(PlayerMovement)), CanEditMultipleObjects]
    public class PlayerMovementEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>();
            var targets = base.targets.Cast<PlayerMovement>().ToArray();

            if (!targets.All(m => m.canMove))
            {
                dict.Add(nameof(PlayerMovement.canMove), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Before, () => EditorGUILayout.HelpBox("Player will not move. These settings won't change anything.", MessageType.Info)));
            }
            if (!targets.All(m => m.useInternalInputMove))
            {
                dict.Add(nameof(PlayerMovement.moveForwardInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveBackwardInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveLeftInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveRightInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveCrouchInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveJumpInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.moveRunInput), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }
            if (!targets.All(m => m.UseGravity))
            {
                dict.Add(nameof(PlayerMovement.gravity), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
                dict.Add(nameof(PlayerMovement.groundMask), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.Omit, null));
            }
            if (targets.Any(m => m.currentCameraView != PlayerMovement.CamViewType.TPS && m.currentCameraView != PlayerMovement.CamViewType.FreeRelativeCam))
            {
                dict.Add(nameof(PlayerMovement.targetCamera), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
                {
                    var gEnabled = GUI.enabled;

                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlayerMovement.targetCamera)));
                    GUI.enabled = gEnabled;
                }));
            }

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
