using BXFW;
using BXFW.Tools.Editor;
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
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.TargetCamera)));
            EditorGUILayout.PropertyField
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.LengthOffset)));
            EditorGUILayout.PropertyField
                (serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.UseGlobalGroupColor)));

            if (target.UseGlobalGroupColor)
            {
                EditorGUI.BeginChangeCheck();
                var cSet = EditorGUILayout.ColorField(new GUIContent("Group Color"), target.GroupColor);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, $"Set color.");
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
                pxWindow.InspectedBGObj = (ParallaxBackgroundGroup)target;
                pxWindow.titleContent.text = $"Px Editor | {target.name}";
            }

            //// Other
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

    [System.Serializable]
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
        [SerializeField] private int BGLayerStart = -5;
        // [SerializeField] private float BGZPositionStart = -5;
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
        /// Cleans the <see cref="ParallaxBackgroundGroup.ParallaxBGObjList"/>. <b>(from null variables.)</b>
        /// </summary>
        private void CleanBGObjectArray()
        {
            // Clear current array
            InspectedBGObj.ParallaxBGObjList.RemoveAll((ParallaxBackgroundObjRegistry d) => d.BackgroundLayer == null);
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

            // This has to be called after an area is created.
            MakeDropAreaGUI(() =>
            {
                List<Sprite> listSprite = new List<Sprite>(DragAndDrop.paths.Length);

                foreach (var path in DragAndDrop.paths)
                {
                    var spriteAddList = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (spriteAddList == null) continue;

                    listSprite.Add(spriteAddList);
                }

                var PrevLength = ParallaxBGArray.Length;
                System.Array.Resize(ref ParallaxBGArray, PrevLength + listSprite.Count);

                int i = Mathf.Max(0, PrevLength - 1);
                foreach (var sprite in listSprite)
                {
                    ParallaxBGArray[i] = new ValuePair<float, Sprite>(0f, sprite);
                    i++;
                }

                OrderParallaxFloatsOnArray();
            });

            // Scroll
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);

            // Styles (why is this a string?)
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
                        System.Array.Reverse(ParallaxBGArray);
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
                                BGLayerStart = (-5) - InspectedBGObj.ChildAmount;
                            }

                            // Refreshable BGLayer.
                            int BGLayer = BGLayerStart - InspectedBGObj.ChildAmount;

                            var ParentSet = new GameObject($"BGHolderGroup{InspectedBGObj.ChildAmount + 1}");
                            var pObj = ParentSet.AddComponent<ParallaxBackgroundObj>();

                            ParentSet.transform.SetParent(InspectedBGObj.transform);
                            pObj.ParallaxEffectAmount = ParallaxBGArray[i].Key;
                            pObj.ParentGroup = InspectedBGObj;
                            // Use the tiled sprite renderer.
                            pObj.InitilazeTilingSpriteRenderer(ParallaxBGArray[i].Value);

                            switch (CurrentSortMethod)
                            {
                                case ParallaxBGLayerSortMethod.SpriteRendererIndex:
                                    pObj.TilingSpriteRendererComponent.SortOrder = BGLayer;
                                    break;
                                default:
                                case ParallaxBGLayerSortMethod.ZAxisCoords:
                                    // BGLayer is negative, so we make it positive to put it behind.
                                    ParentSet.transform.position =
                                        new Vector3(ParentSet.transform.position.x, ParentSet.transform.position.y,
                                        SetZPositionAsPositive ? BGLayer : -BGLayer);
                                    break;
                            }

                            pObj.TilingSpriteRendererComponent.AutoTile = false;
                            pObj.TilingSpriteRendererComponent.GridX = BGSpriteRendererTileX;

                            InspectedBGObj.ParallaxBGObjList.Add(new ParallaxBackgroundObjRegistry(pObj));
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
                        var BGLayer = BGLayerStart - InspectedBGObj.ChildAmount;

                        if (Container == null)
                        {
                            Debug.LogWarning("[ParallaxBGEditor] There is no container.");
                            return;
                        }

                        // Check if this Container object already is registered
                        if (Container.TryGetComponent(out ParallaxBackgroundObj objComponent))
                        {
                            for (int i = 0; i < InspectedBGObj.ParallaxBGObjList.Count; i++)
                            {
                                if (objComponent == InspectedBGObj.ParallaxBGObjList[i].BackgroundLayer)
                                {
                                    Debug.Log($"[ParallaxBGEditor] There is already a parallax object with name \"{InspectedBGObj.ParallaxBGObjList[i].BackgroundLayer.name}\", updated settings.");
                                    InspectedBGObj.ParallaxBGObjList.Remove(InspectedBGObj.ParallaxBGObjList[i]);
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
                            if (ParallaxObjects.All(x => x == null))
                            { 
                                Debug.LogWarning("[ParallaxBGEditor] There is blank/null background objects.");
                                GUILayout.EndScrollView();
                                return; 
                            }
                        }

                        // Destroy previous objects.
                        while (Container.TryGetComponent(out ParallaxBackgroundObj obj))
                        {
                            DestroyImmediate(obj);
                        }

                        var pObj = Container.gameObject.AddComponent<ParallaxBackgroundObj>();
                        pObj.ParallaxEffectAmount = BGLayerPrlxAmount;
                        pObj.ParentGroup = InspectedBGObj;

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
                            BGLayerStart = (-5) - InspectedBGObj.ChildAmount;
                        }

                        InspectedBGObj.ParallaxBGObjList.Add(new ParallaxBackgroundObjRegistry(pObj));
                        Debug.Log("[ParallaxBackgroundEditor] Added background.");
                    }
                    #endregion
                    break;
                case 2:
                    var InspectedObjSO = new SerializedObject(InspectedBGObj);
                    // Create array field "readonly"
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(InspectedObjSO.FindProperty(nameof(ParallaxBackgroundGroup.ParallaxBGObjList)), true);
                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Destroy List"))
                    {
                        if (EditorUtility.DisplayDialog("Clear Parallax Background...",
                         "Are you sure you wanna destroy the parallax backgrounds?\n",
                         "Yes", "No"))
                        {
                            foreach (var dict in InspectedBGObj.ParallaxBGObjList)
                            {
                                if (dict.BackgroundLayer == null)
                                    return;

                                DestroyImmediate(dict.BackgroundLayer.gameObject);
                            }

                            InspectedBGObj.ParallaxBGObjList.Clear();
                        }
                    }
                    if (GUILayout.Button("Clear List"))
                    {
                        if (EditorUtility.DisplayDialog("Clear Parallax Background...",
                         "Are you sure you wanna clean the parallax backgrounds?\nInfo : The background gameobjects are not destroyed.",
                         "Yes", "No"))
                        {
                            foreach (var dict in InspectedBGObj.ParallaxBGObjList)
                            {
                                if (dict == null)
                                    return;

                                DestroyImmediate(dict.BackgroundLayer);
                            }

                            InspectedBGObj.ParallaxBGObjList.Clear();
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
        /// Create the 'global settings' part. Also sets up the scroll bar.
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
                EditorGUILayout.LabelField("Note : If this value is higher, we assume it is on more back.\nThe bg should be 1 while the most front should be 0.", EditorStyles.miniBoldLabel, GUILayout.Height(30));
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
                BGLayerStart = EditorGUILayout.IntField(BGLayerStart);
                BGLayerStart = Mathf.Clamp(BGLayerStart, int.MinValue, -5);
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

        // Array GUI Utilites in case of you didn't import the additionals.
        #region Utilites (Can be added to extensions or changed)
        /// <summary>
        /// Make gui area drag and droppable.
        /// <br>This applies to global gui layout.</br>
        /// </summary>
        public void MakeDropAreaGUI(System.Action onDragAcceptAction, Rect? customRect = null)
        {
            Event evt = Event.current;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    Rect dropArea = customRect ?? GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    /* Drawing graphics on drop area is not necessary, optional.
                    GUIStyle Boxstyle = GUI.skin.box;
                    Boxstyle.fontSize = 24;
                    Boxstyle.fontStyle = FontStyle.Bold;
                    Boxstyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Box(drop_area, "Drop sprites here...", Boxstyle);
                    */
                    if (!dropArea.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        onDragAcceptAction?.Invoke();
                    }
                    break;
            }
        }
        #endregion
    }
}