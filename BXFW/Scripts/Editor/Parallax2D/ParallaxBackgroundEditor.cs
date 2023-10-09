using BXFW.Tools.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

namespace BXFW
{
    [CustomEditor(typeof(ParallaxBackgroundGroup))]
    public class ParallaxBackgroundEditor : Editor
    {
        private ParallaxBackgroundEditorWindow pxWindow;

        public override void OnInspectorGUI()
        {
            // Only serialize specific things
            var target = base.target as ParallaxBackgroundGroup;

            //// Editor
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.targetCamera)));
            EditorGUILayout.PropertyField
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.scrollAxis)));
            EditorGUILayout.PropertyField
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.useGlobalGroupColor)));

            if (target.useGlobalGroupColor)
            {
                EditorGUI.BeginChangeCheck();
                var cSet = EditorGUILayout.ColorField(new GUIContent("Group Color"), target.GroupColor);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Set color.");
                    target.GroupColor = cSet;
                }
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Parallax Editor",
                new GUIStyle(GUI.skin.button) { fontSize = 18, fontStyle = FontStyle.Bold }, GUILayout.Height(45f)))
            {
                if (pxWindow != null) { pxWindow.Close(); }

                pxWindow = EditorWindow.CreateWindow<ParallaxBackgroundEditorWindow>();
                pxWindow.minSize = new Vector2(650f, 500f);
                pxWindow.InspectedBGObj = target;
                pxWindow.titleContent.text = $"Px Editor | {target.name}";
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/2D Object/Parallax Group")]
        private static void CreateParallaxGroupGObj()
        {
            var g = new GameObject("Parallax Group");
            g.transform.SetParent(Selection.activeTransform);
            g.AddComponent<ParallaxBackgroundGroup>();

            Selection.activeGameObject = g;
        }
    }

    public enum ParallaxBGLayerSortMethod
    {
        SpriteRendererIndex,
        ZAxisCoords
    }

    [Serializable]
    internal struct ValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public ValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    public class ParallaxBackgroundEditorWindow : EditorWindow
    {
        // -- Page 0
        [SerializeField] private ValuePair<float, Sprite>[] ParallaxBGArray = new ValuePair<float, Sprite>[0];
        // -- Page 1
        [SerializeField] private SpriteRenderer[] ParallaxObjects = new SpriteRenderer[0];
        [SerializeField] private Transform Container;

        [SerializeField] private int BGSpriteRendererTileX = 5;
        [Range(0f, 1f)][SerializeField] private float BGLayerPrlxAmount = 0f;
        [SerializeField] private bool ModifyBGLayer = true;
        [SerializeField] private ParallaxBGLayerSortMethod CurrentSortMethod = ParallaxBGLayerSortMethod.SpriteRendererIndex;
        [SerializeField] private int BGOrderStart = -5;
        [SerializeField] private bool SetBGLayerStartAuto = true;
        [SerializeField] private bool SetZPositionAsPositive = false;
        private Vector2 ScrollPos = Vector2.zero;

        public ParallaxBackgroundGroup InspectedBGObj;
        private int CurrentToolbarIndex = 0;

        private void OrderParallaxFloatsOnArray()
        {
            for (int i = 0; i < ParallaxBGArray.Length; i++)
            {
                // Lerps between 0-1, the subtraction from 1 is because the length never completes the most background to 1.
                ParallaxBGArray[i].Key = (1f / (ParallaxBGArray.Length - 1f)) * i;
            }
        }
        /// <summary>
        /// Cleans the <see cref="ParallaxBackgroundGroup.Backgrounds"/>. <b>(from null variables.)</b>
        /// </summary>
        private void CleanBGObjectArray()
        {
            InspectedBGObj.Backgrounds.RemoveAll((d) => d == null);
        }

        private void OnGUI()
        {
            // Check null
            if (InspectedBGObj == null)
            {
                EditorGUILayout.LabelField("Please reload this window as this script cannot access the owner group.", EditorStyles.centeredGreyMiniLabel);
                Close();
                return;
            }

            // Scroll
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            // Styles
            GUIStyle customButton = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            // Editor UI
            ScriptableObject target_window = this;
            SerializedObject so = new SerializedObject(target_window);

            // Centered toolbar
            // Note : This is unaffected from the 'ScrollView'.
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            CurrentToolbarIndex = GUILayout.Toolbar(CurrentToolbarIndex,
                new[] { "Create", "Add Existing", "List Of Groups" },
                GUILayout.Width(350f), GUILayout.Height(45f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUIAdditionals.DrawUILineLayout(Color.gray);

            switch (CurrentToolbarIndex)
            {
                case 0:
                    // This has to be called after an area is created.
                    // This call makes other fields undroppable, so only do this in page 0
                    EditorAdditionals.MakeDroppableAreaGUI(() =>
                    {
                        List<Sprite> listSprite = new List<Sprite>(DragAndDrop.paths.Length);

                        foreach (var path in DragAndDrop.paths)
                        {
                            var spriteAddList = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                            if (spriteAddList == null)
                                continue;

                            listSprite.Add(spriteAddList);
                        }

                        var PrevLength = ParallaxBGArray.Length;
                        Array.Resize(ref ParallaxBGArray, PrevLength + listSprite.Count);

                        int i = Mathf.Max(0, PrevLength - 1);
                        foreach (var sprite in listSprite)
                        {
                            ParallaxBGArray[i] = new ValuePair<float, Sprite>(0f, sprite);
                            i++;
                        }

                        OrderParallaxFloatsOnArray();
                    }, new Rect(Vector2.zero, position.size));

                    EditorGUILayout.LabelField("| Sprites To Create |", EditorStyles.boldLabel);

                    EditorAdditionals.UnityArrayGUICustom(true, ref ParallaxBGArray,
                        (int i) =>
                        {
                            GUILayout.BeginHorizontal();

                            // INFO : using 'GetType()' throws exception, if your type is consistent just write typeof().
                            ParallaxBGArray[i].Value =
                                (Sprite)EditorGUILayout.ObjectField($"Sprite {i} :", ParallaxBGArray[i].Value, typeof(Sprite), false, GUILayout.MaxWidth(250f));

                            EditorGUILayout.LabelField("Px Amount :", EditorStyles.miniLabel, GUILayout.Width(100f));
                            ParallaxBGArray[i].Key =
                                EditorGUILayout.Slider(ParallaxBGArray[i].Key, 0f, 1f, GUILayout.Width(150f));

                            GUILayout.EndHorizontal();
                        });

                    EditorGUILayout.Space();

                    // Creation of the parallax group
                    // Added multiple creation.
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Order Parallax Floats", customButton, GUILayout.Width(200f), GUILayout.Height(45f)))
                    {
                        OrderParallaxFloatsOnArray();
                    }
                    if (GUILayout.Button("Reverse Parallax Sprites", customButton, GUILayout.Width(200f), GUILayout.Height(45f)))
                    {
                        Array.Reverse(ParallaxBGArray);
                    }
                    if (GUILayout.Button("Create", customButton, GUILayout.Width(200f), GUILayout.Height(45f)))
                    {
                        // Check array
                        if (ParallaxBGArray == null)
                        {
                            Debug.LogWarning("[ParallaxBGEditor] There is no background sprite.");
                            GUILayout.EndScrollView();
                            return;
                        }
                        if (ParallaxBGArray.Length <= 0)
                        {
                            Debug.LogWarning("[ParallaxBGEditor] There is no background sprite added.");
                            GUILayout.EndScrollView();
                            return;
                        }
                        if (ParallaxBGArray.All(x => x.Value == null))
                        {
                            Debug.LogWarning("[ParallaxBGEditor] There is blank/null background sprites.");
                            GUILayout.EndScrollView();
                            return;
                        }

                        CleanBGObjectArray();

                        for (int i = 0; i < ParallaxBGArray.Length; i++)
                        {
                            // Set this first as the layer can be incorrect on start.
                            if (SetBGLayerStartAuto && ModifyBGLayer)
                            {
                                BGOrderStart = (-5) - InspectedBGObj.ChildLength;
                            }

                            // Refreshable BGLayer.
                            int BGLayer = BGOrderStart - InspectedBGObj.ChildLength;

                            var ParentSet = new GameObject($"BGHolderGroup{InspectedBGObj.ChildLength}");
                            var pObj = ParentSet.AddComponent<ParallaxBackgroundLayer>();

                            ParentSet.transform.SetParent(InspectedBGObj.transform);
                            pObj.parallaxEffectAmount = ParallaxBGArray[i].Key;
                            pObj.parentGroup = InspectedBGObj;
                            // Use the tiled sprite renderer.
                            pObj.InitilazeTilingSpriteRenderer(ParallaxBGArray[i].Value);

                            switch (CurrentSortMethod)
                            {
                                case ParallaxBGLayerSortMethod.SpriteRendererIndex:
                                    pObj.TilingRendererComponent.SortOrder = BGLayer;
                                    break;
                                default:
                                case ParallaxBGLayerSortMethod.ZAxisCoords:
                                    // BGLayer is negative, so we make it positive to put it behind.
                                    ParentSet.transform.position =
                                        new Vector3(ParentSet.transform.position.x, ParentSet.transform.position.y,
                                        SetZPositionAsPositive ? BGLayer : -BGLayer);
                                    break;
                            }

                            pObj.TilingRendererComponent.AutoTile = false;
                            pObj.TilingRendererComponent.GridX = BGSpriteRendererTileX;

                            InspectedBGObj.Backgrounds.Add(pObj);
                        }

                        Debug.Log("[ParallaxBackgroundEditor] Created background(s).");
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    break;
                case 1:
                    EditorGUILayout.LabelField("Objects To Add", EditorStyles.boldLabel);
                    if (ModifyBGLayer)
                    {
                        so.UnityArrayGUI(nameof(ParallaxObjects));
                    }

                    EditorGUILayout.PropertyField(so.FindProperty(nameof(Container)));
                    EditorGUILayout.Space();

                    #region Add Existing Parallax Object
                    // Add parallax object
                    if (GUILayout.Button("Add"))
                    {
                        var BGLayer = BGOrderStart - InspectedBGObj.ChildLength;

                        if (Container == null)
                        {
                            Debug.LogWarning("[ParallaxBGEditor] There is no container.");
                            return;
                        }

                        // Check if this Container object already is registered
                        if (Container.TryGetComponent(out ParallaxBackgroundLayer objComponent))
                        {
                            for (int i = 0; i < InspectedBGObj.Backgrounds.Count; i++)
                            {
                                if (objComponent == InspectedBGObj.Backgrounds[i])
                                {
                                    Debug.Log($"[ParallaxBGEditor] There is already a parallax object with name \"{InspectedBGObj.Backgrounds[i].name}\", updated settings.");
                                    InspectedBGObj.Backgrounds.Remove(InspectedBGObj.Backgrounds[i]);
                                    break;
                                }
                            }
                        }

                        if (ModifyBGLayer)
                        {
                            if (ParallaxObjects == null)
                            {
                                Debug.LogWarning("[ParallaxBGEditor] There is no background object.");
                                GUILayout.EndScrollView();
                                return;
                            }
                            if (ParallaxObjects.Length < 0)
                            {
                                Debug.LogWarning("[ParallaxBGEditor] There is no background object added.");
                                GUILayout.EndScrollView();
                                return;
                            }
                            if (ParallaxObjects.Any(x => x == null))
                            {
                                Debug.LogWarning("[ParallaxBGEditor] There is blank/null background objects.");
                                GUILayout.EndScrollView();
                                return;
                            }
                        }

                        // Destroy previous objects.
                        while (Container.TryGetComponent(out ParallaxBackgroundLayer obj))
                        {
                            DestroyImmediate(obj);
                        }

                        var pObj = Container.gameObject.AddComponent<ParallaxBackgroundLayer>();
                        pObj.parallaxEffectAmount = BGLayerPrlxAmount;
                        pObj.parentGroup = InspectedBGObj;

                        if (ModifyBGLayer)
                        {
                            switch (CurrentSortMethod)
                            {
                                case ParallaxBGLayerSortMethod.SpriteRendererIndex:
                                    foreach (SpriteRenderer r in ParallaxObjects)
                                    {
                                        r.sortingOrder = BGLayer;
                                    }
                                    break;
                                case ParallaxBGLayerSortMethod.ZAxisCoords:
                                    Container.transform.position
                                        = new Vector3(Container.transform.position.x, Container.transform.position.y,
                                        SetZPositionAsPositive ? BGLayer : -BGLayer);
                                    break;
                            }
                        }

                        if (SetBGLayerStartAuto && ModifyBGLayer)
                        {
                            BGOrderStart = (-5) - InspectedBGObj.ChildLength;
                        }

                        InspectedBGObj.Backgrounds.Add(pObj);
                        Debug.Log("[ParallaxBackgroundEditor] Added background.");
                    }
                    #endregion
                    break;
                case 2:
                    var InspectedObjSO = new SerializedObject(InspectedBGObj);
                    // Create array field "readonly"
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(InspectedObjSO.FindProperty(nameof(ParallaxBackgroundGroup.Backgrounds)), true);
                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Destroy List"))
                    {
                        if (EditorUtility.DisplayDialog("Clear Parallax Background...",
                            "Are you sure you wanna destroy the parallax backgrounds?\n",
                            "Yes", "No"))
                        {
                            foreach (var obj in InspectedBGObj.Backgrounds)
                            {
                                if (obj == null)
                                    return;

                                DestroyImmediate(obj.gameObject);
                            }

                            InspectedBGObj.Backgrounds.Clear();
                        }
                    }
                    if (GUILayout.Button("Clear List"))
                    {
                        if (EditorUtility.DisplayDialog("Clear Parallax Background...",
                            "Are you sure you wanna clean the parallax backgrounds?\nInfo : The background gameobjects are not destroyed.",
                            "Yes", "No"))
                        {
                            foreach (var obj in InspectedBGObj.Backgrounds)
                            {
                                if (obj == null)
                                    return;

                                DestroyImmediate(obj);
                            }

                            InspectedBGObj.Backgrounds.Clear();
                        }
                    }
                    if (GUILayout.Button("Remove Nulls"))
                    {
                        CleanBGObjectArray();
                    }
                    GUILayout.EndHorizontal();
                    break;
                default:
                    Debug.LogError($"[ParallaxBackgroundEditorWindow] The menu with index {CurrentToolbarIndex} doesn't exist.");
                    break;
            }

            GUIAdditionals.DrawUILineLayout(Color.gray);
            DrawLayerOrderElemenets(so);

            so.ApplyModifiedProperties();
        }

        /// <summary>
        /// Create the 'global settings' part. Also ends the scroll bar.
        /// </summary>
        private void DrawLayerOrderElemenets(SerializedObject so)
        {
            EditorGUILayout.Space();
            var TextStyleCentered = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("-Global Settings-", TextStyleCentered);

            // Standard
            EditorGUILayout.PropertyField(so.FindProperty(nameof(ModifyBGLayer)));
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(BGSpriteRendererTileX)), new GUIContent("Sprite Renderer Tile X", "Adjust this according to your needs."));
            if (GUILayout.Button("+", GUILayout.Width(20f))) { BGSpriteRendererTileX++; }
            if (GUILayout.Button("-", GUILayout.Width(20f))) { BGSpriteRendererTileX--; }
            GUILayout.EndHorizontal();
            if (CurrentToolbarIndex != 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Note : If this value is higher, the code assumes that it is the background.\nThe background should be higher while the topmost foreground should be 0.", EditorStyles.miniBoldLabel, GUILayout.Height(30));
                EditorGUILayout.PropertyField(so.FindProperty(nameof(BGLayerPrlxAmount)));
                EditorGUI.indentLevel--;
            }

            // Sorting settings
            if (ModifyBGLayer)
            {
                EditorGUILayout.PropertyField(so.FindProperty(nameof(SetBGLayerStartAuto)));
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Layer Sort Method", GUILayout.Width(150));
                CurrentSortMethod = (ParallaxBGLayerSortMethod)EditorGUILayout.EnumPopup(CurrentSortMethod);
                GUILayout.EndHorizontal();

                if (SetBGLayerStartAuto)
                { GUI.enabled = false; }

                GUILayout.BeginHorizontal();
                switch (CurrentSortMethod)
                {
                    case ParallaxBGLayerSortMethod.SpriteRendererIndex:
                        EditorGUILayout.LabelField("Sprite Index Start", GUILayout.Width(150));
                        break;
                    case ParallaxBGLayerSortMethod.ZAxisCoords:
                        GUILayout.EndHorizontal();
                        EditorGUILayout.PropertyField(so.FindProperty(nameof(SetZPositionAsPositive)));
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Z Index Start", GUILayout.Width(150));
                        break;
                }
                BGOrderStart = EditorGUILayout.IntField(BGOrderStart);
                BGOrderStart = Mathf.Clamp(BGOrderStart, int.MinValue, -5);
                GUILayout.EndHorizontal();
                if (SetBGLayerStartAuto)
                { GUI.enabled = true; }
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Info : Sorting is disabled. To enable it again check the 'ModifyBGLayer' box.", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(250f);
            GUILayout.EndScrollView();
        }
    }
}
