using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(FreeMoveCamera))]
    public class FreeMoveCameraEditor : Editor
    {
        private new FreeMoveCamera target;

        // -- Debug Settings
        //private SerializedProperty pFieldDisableCompOnEnable;
        //// -- Camera Settings
        //private SerializedProperty pFieldMoveTransform;
        ////private SerializedProperty pFieldIsEnabled (this field is a property field, not an object)
        //private SerializedProperty pFieldCameraLookRawInput;
        //private SerializedProperty pFieldCameraLookSensitivity;
        //private SerializedProperty pFieldCameraMoveSpeed;
        //private SerializedProperty pFieldBoostedCameraMoveSpeedAdd;
        //private SerializedProperty pFieldMinMaxXRotation;
        //private SerializedProperty pFieldMinMaxYRotation;
                                 
        //// -- Input Settings -- //
        //private SerializedProperty pFieldInputAdjustMoveSpeedMouseWheel;
        //private SerializedProperty pFieldInputMoveForward;
        //private SerializedProperty pFieldInputMoveBackward;
        //private SerializedProperty pFieldInputMoveLeft;
        //private SerializedProperty pFieldInputMoveRight;
        //private SerializedProperty pFieldInputMoveBoost;
        //private SerializedProperty pFieldInputMoveDescend;
        //private SerializedProperty pFieldInputMoveAscend;
        //private SerializedProperty pFieldInputEventDisableEnable;

        private void OnEnable()
        {
            target = (FreeMoveCamera)base.target;


            // Gather 'serialized objects' for the inspector
            //pFieldDisableCompOnEnable = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));

            //pFieldMoveTransform = serializedObject.FindProperty(nameof(FreeMoveCamera.MoveTransform));
            //pFieldCameraLookRawInput = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
            //pFieldCameraLookSensitivity = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
            //pFieldCameraMoveSpeed = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
            //pFieldBoostedCameraMoveSpeedAdd = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
            //pFieldMinMaxXRotation = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
            //pFieldMinMaxYRotation = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));

            //pFieldInputAdjustMoveSpeedMouseWheel = serializedObject.FindProperty(nameof(FreeMoveCamera.DisableComponentOnEnable));
        }

        public override void OnInspectorGUI()
        {
            // Draw 'serialized objects'
            Tools.Editor.EditorAdditionals.DrawCustomDefaultInspector(serializedObject, new Dictionary<string, KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>>
                {
                    // isEnabled is private variable.
                    { "isEnabled", new KeyValuePair<Tools.Editor.MatchGUIActionOrder, System.Action>(Tools.Editor.MatchGUIActionOrder.OmitAndInvoke, () =>
                        {
                            EditorGUI.BeginChangeCheck();
                            var setIsEnabled = EditorGUILayout.Toggle(new GUIContent("Is Enabled", "Set whether if the free move camera is enabled."), target.IsEnabled);
                            if (EditorGUI.EndChangeCheck())
                            {
                                target.IsEnabled = setIsEnabled;
                            }
                        })
                    }
                });
        }
    }
}