using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(TilingSpriteRenderer)), CanEditMultipleObjects]
    public class TilingSpriteRendererEditor : Editor
    {
        private const float IntFieldActionButtonWidth = 17.5f;
        private readonly List<Object> undoRecord = new List<Object>();

        /// <summary>
        /// Automatically registers <see cref="TilingSpriteRenderer.GenerateGrid"/> method based undos. (for an 'Editor.target', but custom targets can be passed)
        /// <br>Basically any change done to <see cref="TilingSpriteRenderer.AllRendererObjects"/> is recorded 
        /// when <paramref name="undoableGenerateAction"/> is invoked.</br>
        /// </summary>
        protected void UndoRecordGridGeneration(Action undoableGenerateAction, string undoMsg, TilingSpriteRenderer target = null)
        {
            // TODO : Merge undos into one using the Undo group creation outside the foreach
            //var Targets = targets.Cast<TilingSpriteRenderer>();
            if (target == null)
                target = (TilingSpriteRenderer)base.target;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(undoMsg);
            int undoID = Undo.GetCurrentGroup();

            // Record previous state of 'Targets'
            //foreach (var item in Targets)
            //{
            undoRecord.Add(target);
            // to be destroyed / created SpriteRenderers gameobjects
            if (target.AllRendererObjects.Count > 0)
            {
                foreach (SpriteRenderer sr in target.AllRendererObjects)
                {
                    if (sr == null)
                        continue;

                    undoRecord.Add(sr.gameObject);
                }
            }
            //}
            
            Undo.RecordObjects(undoRecord.ToArray(), string.Empty);

            undoableGenerateAction();
            //foreach (var item in Targets)
            //{
            // Register creations (for undo)
            foreach (var undoRegister in target.AllRendererObjects.Where(sr => !undoRecord.Contains(sr)))
            {
                if (undoRegister == null)
                    continue;

                Undo.RegisterCreatedObjectUndo(undoRegister.gameObject, string.Empty);
            }
            //}

            Undo.CollapseUndoOperations(undoID);
            undoRecord.Clear();
        }

        public override void OnInspectorGUI()
        {
            // -- Init
            //var Target = target as TilingSpriteRenderer;
            //var TSo = serializedObject;

            // Multiple editors (casting 'targets')
            var guiTargets = targets.Cast(x => (TilingSpriteRenderer)x).ToArray();
            DrawGUIForTargets(guiTargets, serializedObject);
        }
        protected void DrawGUIForTargets(TilingSpriteRenderer[] Targets, SerializedObject TSo)
        {
            // -- Init
            undoRecord.Clear();
            //if (undoRecord.Capacity <= 0)
            //    undoRecord.Capacity = Target.AllRendererObjects.Count + 1;
            var gEnabled = GUI.enabled;
            var showMixed = EditorGUI.showMixedValue;

            var DefaultLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };

            // -- Settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General Settings", DefaultLabelStyle);

            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(TilingSpriteRenderer.GridOnAwake)));
            EditorGUI.BeginChangeCheck();

            var checkColor = Targets[0].Color;
            EditorGUI.showMixedValue = Targets.Any(t => t.Color != checkColor);
            var tSRColor = EditorGUILayout.ColorField(nameof(TilingSpriteRenderer.Color), checkColor);
            EditorGUI.showMixedValue = showMixed;

            if (EditorGUI.EndChangeCheck())
            {
                // This one is not included in UndoRecordGridGeneration as it just modifies grid elements without destroying or creating them.
                foreach (var target in Targets)
                {
                    undoRecord.Add(target);
                    undoRecord.AddRange(target.AllRendererObjects);
                    Undo.RecordObjects(undoRecord.ToArray(), "change value RendColor");

                    target.Color = tSRColor;
                }

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }
            // Property fields already support CanEditMultipleObjects
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(TilingSpriteRenderer.TiledSprite)));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Resize Options", DefaultLabelStyle);
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(TilingSpriteRenderer.CameraResize)));

            GUI.enabled = Targets.Any(t => t.CameraResize);
            EditorGUILayout.PropertyField(TSo.FindProperty(nameof(TilingSpriteRenderer.ResizeTargetCamera)));
            GUI.enabled = gEnabled;

            // ---- Tile Options Start   ---- //
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Options", DefaultLabelStyle);
            // -- AutoTile
            EditorGUI.BeginChangeCheck();

            var checkAutoTile = Targets[0].AutoTile;
            EditorGUI.showMixedValue = Targets.Any(t => t.AutoTile != checkAutoTile);
            var tAT_Value = EditorGUILayout.Toggle(nameof(TilingSpriteRenderer.AutoTile), checkAutoTile);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.AutoTile = tAT_Value, $"change value {nameof(TilingSpriteRenderer.AutoTile)}", target);
                }

                SceneView.RepaintAll();
            }

            // -- Tile Grid X-Y && AllowGridAxis
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            TransformAxis2D tAGA_Value = Targets[0].AllowGridAxis;
            EditorGUI.showMixedValue = Targets.Any(t => t.AllowGridAxis != tAGA_Value);
            EditorGUILayout.LabelField(nameof(TilingSpriteRenderer.AllowGridAxis), GUILayout.Width(160f));
            EditorGUILayout.LabelField("X:", GUILayout.Width(15f));
            tAGA_Value |= EditorGUILayout.Toggle((tAGA_Value & TransformAxis2D.XAxis) == TransformAxis2D.XAxis) ? TransformAxis2D.XAxis : TransformAxis2D.None;
            EditorGUILayout.LabelField("Y:", GUILayout.Width(15f));
            tAGA_Value |= EditorGUILayout.Toggle((tAGA_Value & TransformAxis2D.YAxis) == TransformAxis2D.YAxis) ? TransformAxis2D.YAxis : TransformAxis2D.None;
            EditorGUI.showMixedValue = showMixed;

            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.AllowGridAxis = tAGA_Value, $"change value {nameof(TilingSpriteRenderer.AllowGridAxis)}", target);
                }

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }

            GUI.enabled = Targets.All(t => !t.AutoTile);
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var checkGridX = Targets[0].GridX;
            EditorGUI.showMixedValue = Targets.Any(t => t.GridX != checkGridX);
            var tGX_Value = EditorGUILayout.IntField(nameof(TilingSpriteRenderer.GridX), checkGridX);
            if (GUILayout.Button("+", GUILayout.Width(IntFieldActionButtonWidth))) { tGX_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IntFieldActionButtonWidth))) { tGX_Value--; }
            EditorGUI.showMixedValue = showMixed;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.GridX = tGX_Value, $"change value {nameof(TilingSpriteRenderer.GridX)}", target);
                }

                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var checkGridY = Targets[0].GridY;
            EditorGUI.showMixedValue = Targets.Any(t => t.GridY != checkGridY);
            var tGY_Value = EditorGUILayout.IntField(nameof(TilingSpriteRenderer.GridY), checkGridY);
            if (GUILayout.Button("+", GUILayout.Width(IntFieldActionButtonWidth))) { tGY_Value++; }
            if (GUILayout.Button("-", GUILayout.Width(IntFieldActionButtonWidth))) { tGY_Value--; }
            EditorGUI.showMixedValue = showMixed;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.GridY = tGY_Value, $"change value {nameof(TilingSpriteRenderer.GridY)}", target);
                }

                SceneView.RepaintAll();
            }
            GUI.enabled = true;
            TSo.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Sprites"))
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.GenerateGrid(), "call GenerateSprites", target);
                }
            }
            if (GUILayout.Button("Clear Sprites"))
            {
                foreach (var target in Targets)
                {
                    UndoRecordGridGeneration(() => target.ClearGrid(), "call ClearGrid", target);
                }
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws a gui for single target.
        /// </summary>
        protected void DrawGUIForTarget(TilingSpriteRenderer Target, SerializedObject TSo)
        {
            // -- Init
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

