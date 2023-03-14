using UnityEditor;
using UnityEngine;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TilingSpriteRenderer))]
    public class TilingSpriteRendererEditor : Editor
    {
        /// <summary>
        /// The script property name from unity to create a field for it.
        /// </summary>
        private const string UDefault_ScriptPFieldName = "m_Script";
        private const float IFieldIncDecBtnWidth = 17.5f;
        public override void OnInspectorGUI()
        {
            // -- Init
            var Target = target as TilingSpriteRenderer;
            var TSo = serializedObject;

            var DefaultLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            // -- End Init

            // ---- Unity Default //
            GUI.enabled = false;
            EditorGUILayout.PropertyField(TSo.FindProperty(UDefault_ScriptPFieldName));
            GUI.enabled = true;

            #region ---- Settings Begin //
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.GridOnAwake)));
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.ResizeTargetCamera)));

            // ---- AutoResize Options Start ---- //
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Resize Options", DefaultLabelStyle);
            // -- CameraResize
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.CameraResize)));
            // -- ResizeTransformSetMultiplier
            GUI.enabled = Target.CameraResize;
            EditorGUI.BeginChangeCheck();
            var tRTSM_Value = EditorGUILayout.FloatField(nameof(Target.ResizeTformSetMultiplier), Target.ResizeTformSetMultiplier);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, $"Change value ResizeTransformSetMultiplier on {Target.name}");

                Target.ResizeTformSetMultiplier = tRTSM_Value;

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }
            EditorGUI.BeginChangeCheck();
            var tRTSMC_Value = EditorGUILayout.Vector2Field(nameof(Target.ResizeTSetMultiplierClamp), Target.ResizeTSetMultiplierClamp);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, $"Change value ResizeTransformSetMultiplierClamp on {Target.name}");

                Target.ResizeTSetMultiplierClamp = tRTSMC_Value;

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }
            EditorGUI.BeginChangeCheck();
            var tSRColor = EditorGUILayout.ColorField(nameof(Target.RendColor), Target.RendColor);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(Target, $"Change value RendColor on {Target.name}");

                Target.RendColor = tSRColor;

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }

            // -- MaskResizeAxis
            // EditorGUI.BeginChangeCheck();
            // EditorGUILayout.BeginHorizontal();

            // Mask resizing
            //var tMRA_Value = new Vector2Int();
            //EditorGUILayout.LabelField(nameof(Target.MaskResizeAxis), GUILayout.Width(160f));
            //EditorGUILayout.LabelField("X:", GUILayout.Width(15f));
            //tMRA_Value.x = System.Convert.ToInt32(EditorGUILayout.Toggle(Target.MaskResizeAxis.x == 1));
            //EditorGUILayout.LabelField("Y:", GUILayout.Width(15f));
            //tMRA_Value.y = System.Convert.ToInt32(EditorGUILayout.Toggle(Target.MaskResizeAxis.y == 1));

            //EditorGUILayout.EndHorizontal();
            //if (EditorGUI.EndChangeCheck())
            //{
            //    Undo.RegisterCompleteObjectUndo(Target, $"Change value MaskResizeAxis on {Target.name}");

            //    Target.MaskResizeAxis = tMRA_Value;

            //    if (GUI.changed)
            //    {
            //        SceneView.RepaintAll();
            //    }
            //}
            GUI.enabled = true;
            // ---- AutoResize Options End   ---- //

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

            var tAGA_Value = new Vector2Int();
            EditorGUILayout.LabelField(nameof(Target.AllowGridAxis), GUILayout.Width(160f));
            EditorGUILayout.LabelField("X:", GUILayout.Width(15f));
            tAGA_Value.x = System.Convert.ToInt32(EditorGUILayout.Toggle(Target.AllowGridAxis.x == 1));
            EditorGUILayout.LabelField("Y:", GUILayout.Width(15f));
            tAGA_Value.y = System.Convert.ToInt32(EditorGUILayout.Toggle(Target.AllowGridAxis.y == 1));
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

            #region -- Grid X-Y
            GUI.enabled = !Target.AutoTile;
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var tGX_Value = EditorGUILayout.IntField(nameof(Target.GridX), Target.GridX);
            if (GUILayout.Button("+", GUILayout.Width(IFieldIncDecBtnWidth))) { tGX_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IFieldIncDecBtnWidth))) { tGX_Value--; }

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
            if (GUILayout.Button("+", GUILayout.Width(IFieldIncDecBtnWidth))) { tGY_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IFieldIncDecBtnWidth))) { tGY_Value--; }

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
            #endregion
            // ---- Tile Options End     ---- //

            #endregion // Settings End

            // ---- Sprite begin //
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(Target.tiledSprite)));
            // ---- Sprite end   //

            // Apply property fields.
            TSo.ApplyModifiedProperties();

            #region ---- Target gen begin
            //if (Target.CorrectScaledParent == null || !Target.CorrectScaledTransformIsCorrect())
            //{
            //    // Re generate correct scale parent.
            //    Target.GenerateCorrectScaleParent();
            //}
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
            #endregion // Target gen end
        }
    }
}
