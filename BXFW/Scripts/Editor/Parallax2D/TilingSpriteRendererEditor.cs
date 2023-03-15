using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TilingSpriteRenderer))]
    public class TilingSpriteRendererEditor : Editor
    {
        private const float IntFieldActionButtonWidth = 17.5f;
        private readonly List<Object> undoRecord = new List<Object>();
        /// <summary>
        /// Automatically registers <see cref="TilingSpriteRenderer.GenerateGrid"/> method based undos.
        /// <br>Basically any change done to <see cref="TilingSpriteRenderer.AllRendererObjects"/> is recorded 
        /// when <paramref name="undoableGenerateAction"/> is invoked.</br>
        /// </summary>
        private void UndoRecordGridGeneration(Action undoableGenerateAction, string undoMsg)
        {
            var Target = target as TilingSpriteRenderer;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(undoMsg);
            int undoID = Undo.GetCurrentGroup();

            // Record previous state of 'Target'
            undoRecord.Add(Target);
            // to be destroyed / created SpriteRenderers gameobjects
            if (Target.AllRendererObjects.Count > 0)
            {
                foreach (SpriteRenderer sr in Target.AllRendererObjects)
                {
                    if (sr == null)
                        continue;

                    undoRecord.Add(sr.gameObject);
                }
            }
            Undo.RecordObjects(undoRecord.ToArray(), string.Empty);

            undoableGenerateAction();
            // Register creations (for undo)
            foreach (var undoRegister in Target.AllRendererObjects.Where(sr => !undoRecord.Contains(sr)))
            {
                if (undoRegister == null)
                    continue;

                Undo.RegisterCreatedObjectUndo(undoRegister.gameObject, string.Empty);
            }

            Undo.CollapseUndoOperations(undoID);
        }

        public override void OnInspectorGUI()
        {
            // -- Init
            var Target = target as TilingSpriteRenderer;
            var TSo = serializedObject;
            undoRecord.Clear();
            if (undoRecord.Capacity <= 0)
                undoRecord.Capacity = Target.AllRendererObjects.Count + 1;
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
                // This one is not included in UndoRecordGridGeneration as it just modifies grid elements without destroying or creating them.
                undoRecord.Add(Target);
                undoRecord.AddRange(Target.AllRendererObjects);
                Undo.RecordObjects(undoRecord.ToArray(), $"change value RendColor on {Target.name}");

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
                UndoRecordGridGeneration(() => Target.AutoTile = tAT_Value, $"change value {nameof(Target.AutoTile)} on {Target.name}");

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
                UndoRecordGridGeneration(() => Target.AllowGridAxis = tAGA_Value, $"change value {nameof(Target.AllowGridAxis)} on {Target.name}");

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
                UndoRecordGridGeneration(() => Target.GridX = tGX_Value, $"change value {nameof(Target.GridX)} on {Target.name}");

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
                UndoRecordGridGeneration(() => Target.GridY = tGY_Value, $"change value {nameof(Target.GridY)} on {Target.name}");

                SceneView.RepaintAll();
            }
            GUI.enabled = true;
            TSo.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Sprites"))
            {
                UndoRecordGridGeneration(() => Target.GenerateGrid(), $"call GenerateSprites on object {Target.name}");
            }
            if (GUILayout.Button("Clear Sprites"))
            {
                UndoRecordGridGeneration(() => Target.ClearGrid(), $"call ClearGrid on object {Target.name}");
            }
            GUILayout.EndHorizontal();
        }
    }
}
