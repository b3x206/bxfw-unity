using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TilingSpriteRenderer))]
    public class TilingSpriteRendererEditor : Editor
    {
        private const float IntFieldActionButtonWidth = 17.5f;
        
        public override void OnInspectorGUI()
        {
            // -- Init
            var Target = target as TilingSpriteRenderer;
            var TSo = serializedObject;
            var gEnabled = GUI.enabled;

            var DefaultLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            // -- Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Settings", DefaultLabelStyle);

            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.GridOnAwake)));
            EditorGUI.BeginChangeCheck();
            var tSRColor = EditorGUILayout.ColorField(nameof(Target.Color), Target.Color);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, $"Change value RendColor on {Target.name}");

                Target.Color = tSRColor;

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.TiledSprite)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Resize Options", DefaultLabelStyle);
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.CameraResize)));
            
            GUI.enabled = Target.CameraResize;
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.ResizeTargetCamera)));
            GUI.enabled = gEnabled;

            // ---- Tile Options Start   ---- //
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Options", DefaultLabelStyle);
            // -- AutoTile
            EditorGUI.BeginChangeCheck();
            var tAT_Value = EditorGUILayout.Toggle(nameof(Target.AutoTile), Target.AutoTile);
            if (EditorGUI.EndChangeCheck())
            {
                // Register undo before
                string UndoMsg = $"Change value {nameof(Target.AutoTile)} on {Target.name}";
                Undo.RegisterCompleteObjectUndo(Target, UndoMsg);
                Undo.RegisterCompleteObjectUndo(Target.transform, UndoMsg);

                Target.AutoTile = tAT_Value;
                SceneView.RepaintAll();
            }

            // -- Tile Grid X-Y && AllowGridAxis
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            TransformAxis2D tAGA_Value = TransformAxis2D.None;
            EditorGUILayout.LabelField(nameof(Target.AllowGridAxis), GUILayout.Width(160f));
            EditorGUILayout.LabelField("X:", GUILayout.Width(15f));
            tAGA_Value |= EditorGUILayout.Toggle((Target.AllowGridAxis & TransformAxis2D.XAxis) == TransformAxis2D.XAxis) ? TransformAxis2D.XAxis : TransformAxis2D.None;
            EditorGUILayout.LabelField("Y:", GUILayout.Width(15f));
            tAGA_Value |= EditorGUILayout.Toggle((Target.AllowGridAxis & TransformAxis2D.YAxis) == TransformAxis2D.YAxis) ? TransformAxis2D.YAxis : TransformAxis2D.None;
            
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, $"Change value {nameof(Target.AllowGridAxis)} on {Target.name}");

                Target.AllowGridAxis = tAGA_Value;

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }

            GUI.enabled = !Target.AutoTile;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var tGX_Value = EditorGUILayout.IntField(nameof(Target.GridX), Target.GridX);
            if (GUILayout.Button("+", GUILayout.Width(IntFieldActionButtonWidth))) { tGX_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IntFieldActionButtonWidth))) { tGX_Value--; }

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                // Register undo before
                string UndoMsg = $"Change value {nameof(Target.GridX)} on {Target.name}";
                Undo.RecordObject(Target.transform, UndoMsg);
                //Undo.RegisterCompleteObjectUndo(Target, UndoMsg);
                //Undo.RegisterCompleteObjectUndo(Target.transform, UndoMsg);

                Target.GridX = tGX_Value;
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var tGY_Value = EditorGUILayout.IntField(nameof(Target.GridY), Target.GridY);
            if (GUILayout.Button("+", GUILayout.Width(IntFieldActionButtonWidth))) { tGY_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IntFieldActionButtonWidth))) { tGY_Value--; }

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                // Register undo before
                string UndoMsg = $"Change value {nameof(Target.GridY)} on {Target.name}";
                Undo.RecordObject(Target.transform, UndoMsg);
                //Undo.RegisterCompleteObjectUndo(Target, UndoMsg);
                //Undo.RegisterCompleteObjectUndo(Target.transform, UndoMsg);

                Target.GridY = tGY_Value;
                SceneView.RepaintAll();
            }
            GUI.enabled = true;
            TSo.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Sprites"))
            {
                Undo.RegisterCompleteObjectUndo(Target.transform, $"Undo call GenerateSprites on object {Target.name}");

                Target.GenerateGrid();
            }
            if (GUILayout.Button("Clear Sprites"))
            {
                Undo.RegisterCompleteObjectUndo(Target.transform, $"Undo call ClearGrid on object {Target.name}");

                Target.ClearGrid();
            }
            GUILayout.EndHorizontal();
        }
    }
}
