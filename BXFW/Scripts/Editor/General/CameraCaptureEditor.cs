using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(CameraCapture))]
    public class CameraCaptureInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var target = base.target as CameraCapture;

            serializedObject.DrawCustomDefaultInspector(new Dictionary<string, KeyValuePair<MatchGUIActionOrder, Action>>
            {
                { nameof(CameraCapture.CaptureKey), new KeyValuePair<MatchGUIActionOrder, Action>(MatchGUIActionOrder.After, 
                    () =>
                    {
                        if (GUILayout.Button("Capture"))
                        {
                            target.TakeCameraShot();
                        }
                    }) 
                }
            });
        }
    }
}