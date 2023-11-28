using System;
using System.Collections.Generic;
using BXFW.Tools.Editor;
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
                dict.Add(nameof(PlayerMovement.moveForwardInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveBackwardInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveLeftInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveRightInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveCrouchInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveJumpInput), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.moveRunInput), EditorAdditionals.OmitAction);
            }
            if (!targets.All(m => m.UseGravity))
            {
                dict.Add(nameof(PlayerMovement.gravity), EditorAdditionals.OmitAction);
                dict.Add(nameof(PlayerMovement.groundMask), EditorAdditionals.OmitAction);
            }
            if (targets.Any(m => m.currentCameraView != PlayerMovement.CamViewType.TPS && m.currentCameraView != PlayerMovement.CamViewType.FreeRelativeCam))
            {
                dict.Add(nameof(PlayerMovement.targetCamera), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.OmitAndInvoke, () =>
                {
                    using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlayerMovement.targetCamera)));
                    }
                }));
            }

            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
