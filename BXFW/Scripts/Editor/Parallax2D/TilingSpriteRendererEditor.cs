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
        private const float NUM_FIELD_ACTION_BTN_WIDTH = 17.5f;
        private readonly List<Object> m_undoRecord = new List<Object>();

        private void OnSceneGUI()
        {
            // Draw a bounding box for one target (targets doesn't work on OnSceneGUI)
            var hMatrix = Handles.matrix;
            var target = base.target as TilingSpriteRenderer;
            // Only do drawing if the target is autotile
            if (!target.AutoTile)
                return;

            Transform tTrs = target.transform;

            // Only position + rotation
            Handles.matrix = Matrix4x4.TRS(tTrs.position, tTrs.rotation, Vector3.one);
            // just draw a cube + rectangle showing bounds
            // because when i try to do scaling the thing it always tries to work additively making it fly away
            // This is because i have skill issue
            EditorGUI.BeginChangeCheck();
            Vector3 topRight = new Vector3(tTrs.lossyScale.x, tTrs.lossyScale.y, tTrs.position.z);
            Handles.CubeHandleCap(0, topRight, Quaternion.identity, HandleUtility.GetHandleSize(topRight) * .3f, EventType.Repaint);

            Vector3 topLeft = new Vector3(-tTrs.lossyScale.x, tTrs.lossyScale.y, 0f);
            Handles.CubeHandleCap(0, topLeft, Quaternion.identity, HandleUtility.GetHandleSize(topLeft) * .3f, EventType.Repaint);

            Vector3 bottomRight = new Vector3(tTrs.lossyScale.x, -tTrs.lossyScale.y, 0f);
            Handles.CubeHandleCap(0, bottomRight, Quaternion.identity, HandleUtility.GetHandleSize(bottomRight) * .3f, EventType.Repaint);

            Vector3 bottomLeft = new Vector3(-tTrs.lossyScale.x, -tTrs.lossyScale.y, 0f);
            Handles.CubeHandleCap(0, bottomLeft, Quaternion.identity, HandleUtility.GetHandleSize(bottomLeft) * .3f, EventType.Repaint);

            Handles.DrawSolidRectangleWithOutline(new Vector3[] { bottomLeft, bottomRight, topRight, topLeft }, new Color(.4f, .4f, .4f, .1f), Color.black);

            Handles.matrix = hMatrix;
        }

        /// <summary>
        /// Automatically registers <see cref="TilingSpriteRenderer.GenerateGrid"/> method based undos. (for an 'Editor.target', but custom targets can be passed)
        /// <br>Basically any change done to <see cref="TilingSpriteRenderer.AllRendererObjects"/> is recorded 
        /// when <paramref name="undoableGenerateAction"/> is invoked.</br>
        /// </summary>
        protected void UndoRecordGridGeneration(Action undoableGenerateAction, string undoMsg, TilingSpriteRenderer target = null)
        {
            if (EditorApplication.isPlaying)
            {
                undoableGenerateAction();
                return;
            }

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
            m_undoRecord.Add(target);
            // to be destroyed / created SpriteRenderers gameobjects
            foreach (SpriteRenderer sr in target.AllRendererObjects)
            {
                if (sr == null)
                    continue;

                m_undoRecord.Add(sr.gameObject);
            }
            //}
            
            Undo.RecordObjects(m_undoRecord.ToArray(), string.Empty);

            undoableGenerateAction();
            //foreach (var item in Targets)
            //{
            // Register creations (for undo)
            foreach (var undoRegister in target.AllRendererObjects.Where(sr => !m_undoRecord.Contains(sr.gameObject)))
            {
                if (undoRegister == null)
                    continue;

                Undo.RegisterCreatedObjectUndo(undoRegister.gameObject, string.Empty);
            }
            //}

            Undo.CollapseUndoOperations(undoID);
            m_undoRecord.Clear();
        }

        public override void OnInspectorGUI()
        {
            // -- Init
            //var target = base.target as TilingSpriteRenderer;
            //var tso = serializedObject;

            // Multiple editors (casting 'targets')
            var guiTargets = targets.Cast<TilingSpriteRenderer>().ToArray();
            DrawGUIForTargets(guiTargets, serializedObject);
        }
        protected void DrawGUIForTargets(TilingSpriteRenderer[] targets, SerializedObject so)
        {
            // -- Init
            m_undoRecord.Clear();
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

            EditorGUILayout.PropertyField(so.FindProperty(nameof(TilingSpriteRenderer.gridOnAwake)));
            EditorGUI.BeginChangeCheck();

            var checkColor = targets[0].Color;
            EditorGUI.showMixedValue = targets.Any(t => t.Color != checkColor);
            var tSRColor = EditorGUILayout.ColorField(nameof(TilingSpriteRenderer.Color), checkColor);
            EditorGUI.showMixedValue = showMixed;

            if (EditorGUI.EndChangeCheck())
            {
                // This one is not included in UndoRecordGridGeneration as it just modifies grid elements without destroying or creating them.
                foreach (var target in targets)
                {
                    if (!EditorApplication.isPlaying)
                    {
                        m_undoRecord.Add(target);
                        m_undoRecord.AddRange(target.AllRendererObjects);
                        Undo.RecordObjects(m_undoRecord.ToArray(), "change value RendColor");
                    }

                    target.Color = tSRColor;
                }

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }
            // Property fields already support CanEditMultipleObjects
            // -- AutoTile
            EditorGUI.BeginChangeCheck();

            var checkTiledSprite = targets[0].TiledSprite;
            EditorGUI.showMixedValue = targets.Any(t => t.TiledSprite != checkTiledSprite);
            var tTiledSpriteValue = EditorGUILayout.ObjectField("Sprite", checkTiledSprite, typeof(Sprite), false) as Sprite;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.TiledSprite = tTiledSpriteValue, $"change value {nameof(TilingSpriteRenderer.TiledSprite)}", target);
                }

                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Camera Resize Options", DefaultLabelStyle);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(TilingSpriteRenderer.cameraResize)));

            GUI.enabled = targets.Any(t => t.cameraResize);
            EditorGUILayout.PropertyField(so.FindProperty(nameof(TilingSpriteRenderer.resizeTargetCamera)));
            GUI.enabled = gEnabled;

            // ---- Tile Options Start   ---- //
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Options", DefaultLabelStyle);
            // -- AutoTile
            EditorGUI.BeginChangeCheck();

            var checkAutoTile = targets[0].AutoTile;
            EditorGUI.showMixedValue = targets.Any(t => t.AutoTile != checkAutoTile);
            var tAutoTileValue = EditorGUILayout.Toggle("Auto Tile", checkAutoTile);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.AutoTile = tAutoTileValue, $"change value {nameof(TilingSpriteRenderer.AutoTile)}", target);
                }

                SceneView.RepaintAll();
            }

            // -- Tile Grid X-Y && AllowGridAxis
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();

            TransformAxis2D tAllowAxisValue = targets[0].AllowGridAxis;
            EditorGUI.showMixedValue = targets.Any(t => t.AllowGridAxis != tAllowAxisValue);
            EditorGUILayout.LabelField(nameof(TilingSpriteRenderer.AllowGridAxis), GUILayout.Width(160f));
            EditorGUILayout.LabelField("X:", GUILayout.Width(15f));
            bool allowXAxis = EditorGUILayout.Toggle((tAllowAxisValue & TransformAxis2D.XAxis) == TransformAxis2D.XAxis);
            tAllowAxisValue = (allowXAxis ? (tAllowAxisValue | TransformAxis2D.XAxis) : (tAllowAxisValue & ~TransformAxis2D.XAxis));
            EditorGUILayout.LabelField("Y:", GUILayout.Width(15f));
            bool allowYAxis = EditorGUILayout.Toggle((tAllowAxisValue & TransformAxis2D.YAxis) == TransformAxis2D.YAxis);
            tAllowAxisValue = (allowYAxis ? (tAllowAxisValue | TransformAxis2D.YAxis) : (tAllowAxisValue & ~TransformAxis2D.YAxis));

            EditorGUI.showMixedValue = showMixed;

            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.AllowGridAxis = tAllowAxisValue, $"change value {nameof(TilingSpriteRenderer.AllowGridAxis)}", target);
                }

                if (GUI.changed)
                {
                    SceneView.RepaintAll();
                }
            }

            GUI.enabled = targets.All(t => !t.AutoTile);
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var checkGridX = targets[0].GridX;
            EditorGUI.showMixedValue = targets.Any(t => t.GridX != checkGridX);
            var tGridXValue = EditorGUILayout.IntField(nameof(TilingSpriteRenderer.GridX), checkGridX);
            if (GUILayout.Button("+", GUILayout.Width(NUM_FIELD_ACTION_BTN_WIDTH))) { tGridXValue++; }
            if (GUILayout.Button("-", GUILayout.Width(NUM_FIELD_ACTION_BTN_WIDTH))) { tGridXValue--; }
            EditorGUI.showMixedValue = showMixed;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.GridX = tGridXValue, $"change value {nameof(TilingSpriteRenderer.GridX)}", target);
                }

                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            var checkGridY = targets[0].GridY;
            EditorGUI.showMixedValue = targets.Any(t => t.GridY != checkGridY);
            var tGridYValue = EditorGUILayout.IntField(nameof(TilingSpriteRenderer.GridY), checkGridY);
            if (GUILayout.Button("+", GUILayout.Width(NUM_FIELD_ACTION_BTN_WIDTH))) { tGridYValue++; }
            if (GUILayout.Button("-", GUILayout.Width(NUM_FIELD_ACTION_BTN_WIDTH))) { tGridYValue--; }
            EditorGUI.showMixedValue = showMixed;

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.GridY = tGridYValue, $"change value {nameof(TilingSpriteRenderer.GridY)}", target);
                }

                SceneView.RepaintAll();
            }
            GUI.enabled = true;
            so.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Sprites"))
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.GenerateGrid(), "call GenerateSprites", target);
                }
            }
            if (GUILayout.Button("Clear Sprites"))
            {
                foreach (var target in targets)
                {
                    UndoRecordGridGeneration(() => target.ClearGrid(), "call ClearGrid", target);
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}
