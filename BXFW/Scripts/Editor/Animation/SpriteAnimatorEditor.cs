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
        [CustomPropertyDrawer(typeof(SpriteAnimator.SpriteAnimSequence), true)]
        public class SpriteSequenceEditor : PropertyDrawer
        {
            private readonly PropertyRectContext mainCtx = new PropertyRectContext();
            private const float ClearFramesButtonHeight = 20f;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                // Foldout Height
                float height = EditorGUIUtility.singleLineHeight + mainCtx.Padding;
                if (!property.isExpanded)
                {
                    return height;
                }

                foreach (SerializedProperty childProperty in property.GetVisibleChildren())
                {
                    height += EditorGUI.GetPropertyHeight(childProperty) + mainCtx.Padding;
                }

                // Duration display height
                height += 6; // Line + padding
                height += EditorGUIUtility.singleLineHeight + mainCtx.Padding;
                // Clear Sprites
                height += ClearFramesButtonHeight + mainCtx.Padding;

                return height;
            }
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                mainCtx.Reset();

                Rect foldoutPosition = mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight);
                label = EditorGUI.BeginProperty(position, label, property);
                property.isExpanded = EditorGUI.Foldout(foldoutPosition, property.isExpanded, label);
                if (!property.isExpanded)
                {
                    EditorGUI.EndProperty();
                    return;
                }

                foreach (SerializedProperty childProperty in property.GetVisibleChildren())
                {
                    EditorGUI.PropertyField(mainCtx.GetPropertyRect(position, childProperty), childProperty);
                }

                // Draw duration + Clear Sprites button
                GUIAdditionals.DrawUILine(mainCtx.GetPropertyRect(position, 6), EditorGUIUtility.isProSkin ? Color.gray : new Color(0.3f, 0.3f, 0.3f));
                using SerializedProperty frameSpriteArrayProperty = property.FindPropertyRelative(nameof(SpriteAnimator.SpriteAnimSequence.frameSpriteArray));
                using SerializedProperty frameMSProperty = property.FindPropertyRelative(nameof(SpriteAnimator.SpriteAnimSequence.frameMS));
                using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.FloatField(mainCtx.GetPropertyRect(position, EditorGUIUtility.singleLineHeight), "Total Duration", frameMSProperty.floatValue * frameSpriteArrayProperty.arraySize);
                }

                if (GUI.Button(mainCtx.GetPropertyRect(position, ClearFramesButtonHeight), "Clear Frames"))
                {
                    frameSpriteArrayProperty.ClearArray();
                }

                EditorGUI.EndProperty();
            }
        }

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
