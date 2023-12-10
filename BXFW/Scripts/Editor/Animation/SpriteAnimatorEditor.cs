using UnityEditor;
using UnityEngine;

using System.Linq;
using System.Collections.Generic;
using BXFW.Tools.Editor;

namespace BXFW.ScriptEditor
{
    [CustomEditor(typeof(SpriteAnimator))]
    public class SpriteAnimatorEditor : Editor
    {
        // TODO : Create field editor for SpriteAnimSequence (show how long will the animation take etc.)

        public override void OnInspectorGUI()
        {
            var target = base.target as SpriteAnimator;
            var currentAnimName = target.CurrentAnimation?.name;
            var dict = new Dictionary<string, KeyValuePair<MatchGUIActionOrder, System.Action>>()
            {
                // this is a private field
                { "_currentAnimIndex", new KeyValuePair<MatchGUIActionOrder, System.Action>(MatchGUIActionOrder.OmitAndInvoke,
                    () =>
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUILayout.BeginHorizontal();
                        var cAnimIndexSet = EditorGUILayout.IntField(new GUIContent("Current Animation Index", "Sets the index from 'animation' array."),
                            target.CurrentAnimIndex);
                        EditorGUILayout.LabelField(new GUIContent($"AnimName={currentAnimName}"));

                        EditorGUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Set Current Animation Index");

                            target.CurrentAnimIndex = cAnimIndexSet;
                        }
                    })
                },
                { nameof(SpriteAnimator.animations), new KeyValuePair<MatchGUIActionOrder, System.Action>(MatchGUIActionOrder.Before,
                    () =>
                    {
                        // Check & warn duplicate ID values
                        // This method also returns what keys are duplicate
                        var duplicates = target.animations
                            .GroupBy(i => i.name)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key);

                        if (duplicates.Count() > 0)
                        {
                            // Have duplicates, show a warning
                            EditorGUILayout.HelpBox("Warning : Array 'animations' has duplicate values. Please remove.", MessageType.Warning);
                            // For now don't do anything fancy (as it's not necessary)
                        }
                    })
                }
            };

            // this is a bad way of checking whether if the nulls are valid
            // both fields will be drawn if the target is ambigious
            bool isInvalidNullConf = false;
            if (target.animateSprite != null && target.animateImage != null)
            {
                dict.Add(nameof(SpriteAnimator.animateImage), new KeyValuePair<MatchGUIActionOrder, System.Action>(MatchGUIActionOrder.After, () =>
                {
                    // Show warning if an invalid null config
                    EditorGUILayout.HelpBox("Warning : Both fields of target objects are filled. Please empty one of them.", MessageType.Warning);
                }));

                isInvalidNullConf = true;
            }

            if (target.animateSprite != null && !isInvalidNullConf)
            {
                dict.Add(nameof(SpriteAnimator.animateImage), new KeyValuePair<MatchGUIActionOrder, System.Action>(MatchGUIActionOrder.Omit, null));
            }
            if (target.animateImage != null && !isInvalidNullConf)
            {
                dict.Add(nameof(SpriteAnimator.animateSprite), new KeyValuePair<MatchGUIActionOrder, System.Action>(MatchGUIActionOrder.Omit, null));
            }


            serializedObject.DrawCustomDefaultInspector(dict);
        }
    }
}
