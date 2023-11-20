using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using BXFW.Tools.Editor;

namespace BXFW
{
    /// <summary>
    /// Shows the window opener for the group.
    /// </summary>
    [CustomEditor(typeof(ParallaxBackgroundGroup))]
    public class ParallaxBackgroundEditor : Editor
    {
        private ParallaxBackgroundEditorWindow pxWindow;

        public override void OnInspectorGUI()
        {
            // Only serialize specific things
            var target = base.target as ParallaxBackgroundGroup;

            // Editor
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.targetCamera)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.scrollAxis)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ParallaxBackgroundGroup.useGlobalGroupColor)));

            if (target.useGlobalGroupColor)
            {
                EditorGUI.BeginChangeCheck();
                var colorSet = EditorGUILayout.ColorField(new GUIContent("Group Color"), target.GroupColor);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Set color.");
                    target.GroupColor = colorSet;
                }
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);

            if (GUILayout.Button("Open Parallax Editor", new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold }, GUILayout.Height(35f)))
            {
                // Ensure only one window
                if (pxWindow != null)
                {
                    pxWindow.Close();
                }

                pxWindow = ParallaxBackgroundEditorWindow.CreateWindow(target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("GameObject/2D Object/Parallax Group")]
        private static void CreateParallaxGroupGObj()
        {
            var g = new GameObject("Parallax Group");
            GameObjectUtility.SetParentAndAlign(g, Selection.activeGameObject);
            g.AddComponent<ParallaxBackgroundGroup>();

            Selection.activeGameObject = g;
        }
    }

    /// <summary>
    /// The sorting method to set the background layers by.
    /// <br><see cref="SpriteRendererIndex"/> =&gt; Sprite<see cref="Renderer.sortingOrder"/> based</br>
    /// <br><see cref="ZAxisCoords"/> =&gt; <see cref="Transform.position"/>.z based</br>
    /// </summary>
    public enum BackgroundLayerSortMethod
    {
        SpriteRendererIndex,
        ZAxisCoords
    }

    /// <summary>
    /// The main window to show on a <see cref="ParallaxBackgroundGroup"/>.
    /// </summary>
    public class ParallaxBackgroundEditorWindow : EditorWindow
    {
        /// <summary>
        /// A structure used to register a layer.
        /// </summary>
        [Serializable]
        private struct LayerRegistry : IComparable<LayerRegistry>
        {
            public Sprite sprite;
            [Range(0f, 1f)] public float parallaxAmount;

            public LayerRegistry(float layerParallax, Sprite layerSprite)
            {
                parallaxAmount = layerParallax;
                sprite = layerSprite;
            }

            public int CompareTo(LayerRegistry other)
            {
                return parallaxAmount.CompareTo(other.parallaxAmount);
            }
        }
        /// <summary>
        /// Editor for the <see cref="LayerRegistry"/>.
        /// </summary>
        [CustomPropertyDrawer(typeof(LayerRegistry))]
        private class LayerRegistryEditor : PropertyDrawer
        {
            private const float HEIGHT = 60f;
            private const float PADDING = 2f;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return HEIGHT + PADDING;
            }
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);

                position.height -= PADDING;
                position.y += PADDING / 2f;

                using var spSprite = property.FindPropertyRelative(nameof(LayerRegistry.sprite));
                using var spParallax = property.FindPropertyRelative(nameof(LayerRegistry.parallaxAmount));
                // Draw both horizontally
                Rect fieldRect = new Rect(position);

                fieldRect.width -= (position.width + PADDING) / 2f;
                spSprite.objectReferenceValue = (Sprite)EditorGUI.ObjectField(fieldRect, new GUIContent("Layer Sprite / Parallax Amount"), spSprite.objectReferenceValue, typeof(Sprite), false);

                fieldRect.x += (position.width + PADDING) / 2f;
                fieldRect.width += PADDING / 2f;
                fieldRect.y += (position.height - EditorGUIUtility.singleLineHeight) / 2f;
                fieldRect.height = EditorGUIUtility.singleLineHeight;

                RangeAttribute range = spParallax.GetTarget().fieldInfo.GetCustomAttribute<RangeAttribute>();
                // Using GUIContent with this causes the slider to disappear, which is dumb, why would you do this
                // It looks fine without the label
                spParallax.floatValue = EditorGUI.Slider(fieldRect, spParallax.floatValue, range.min, range.max);

                property.serializedObject.ApplyModifiedProperties();

                EditorGUI.EndProperty();
            }
        }

        /// <summary>
        /// Creates a window out of a <see cref="ParallaxBackgroundGroup"/>.
        /// </summary>
        public static ParallaxBackgroundEditorWindow CreateWindow(ParallaxBackgroundGroup group)
        {
            var window = CreateWindow<ParallaxBackgroundEditorWindow>();
            window.minSize = new Vector2(650f, 500f);
            window.targetGroup = group;
            window.titleContent.text = $"Px Editor | {group.name}";

            return window;
        }

        // -- Page 0
        [SerializeField] private LayerRegistry[] parallaxLayers = new LayerRegistry[0];
        // -- Page 1
        [SerializeField] private SpriteRenderer[] layerSpriteObjects = new SpriteRenderer[0];
        [SerializeField] private Transform container;

        [SerializeField] private int layerRendererXTileCount = 5;
        [Range(0f, 1f), SerializeField] private float layerParallaxAmount = 0f;
        [SerializeField] private bool modifyBGLayer = true;
        [SerializeField] private BackgroundLayerSortMethod layerSortMethod = BackgroundLayerSortMethod.SpriteRendererIndex;
        [SerializeField] private int layerOrderStart = -5;
        [SerializeField] private bool setLayerOrderAuto = true;
        [SerializeField] private bool setZOrderPositionAsPositive = false;
        public ParallaxBackgroundGroup targetGroup;

        private GUIStyle customButton;
        private Vector2 windowScrollPos = Vector2.zero;
        private int currentToolbarIndex = 0;

        /// <summary>
        /// Orders the parallaxing background layers according to the order.
        /// </summary>
        private void OrderParallaxFloatsOnArray()
        {
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                // Lerps between 0-1, the subtraction from 1 is because the length never completes the most background to 1.
                parallaxLayers[i].parallaxAmount = 1f / (parallaxLayers.Length - 1f) * i;
            }
        }
        /// <summary>
        /// Cleans the <see cref="ParallaxBackgroundGroup.Backgrounds"/>. <b>(from null variables.)</b>
        /// </summary>
        private void CleanBGObjectArray()
        {
            targetGroup.Backgrounds.RemoveAll(bg => bg == null);
        }

        private void DrawCreateSubMenu(SerializedObject so)
        {
            // This has to be called after an area is created.
            // This call makes other fields undroppable, so only do this in page 0
            EditorGUIAdditionals.MakeDragDropArea(() =>
            {
                List<Sprite> listSprite = new List<Sprite>(DragAndDrop.paths.Length);

                foreach (var path in DragAndDrop.paths)
                {
                    var spriteAddList = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (spriteAddList == null)
                    {
                        continue;
                    }

                    listSprite.Add(spriteAddList);
                }

                var prevLength = parallaxLayers.Length;
                Array.Resize(ref parallaxLayers, prevLength + listSprite.Count);

                int i = Mathf.Max(0, prevLength - 1);
                foreach (var sprite in listSprite)
                {
                    parallaxLayers[i] = new LayerRegistry(0f, sprite);
                    i++;
                }

                OrderParallaxFloatsOnArray();
            }, new Rect(Vector2.zero, position.size));

            EditorGUILayout.LabelField("| Sprites To Create |", EditorStyles.boldLabel);

            // Since we have a valid PropertyDrawer this will be fine
            // However the bottom method with the updated api is still fine.
            EditorGUIAdditionals.DrawArray(so, nameof(parallaxLayers));

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
                Array.Reverse(parallaxLayers);
            }
            if (GUILayout.Button("Create", customButton, GUILayout.Width(200f), GUILayout.Height(45f)))
            {
                // Check array
                if (parallaxLayers == null)
                {
                    Debug.LogWarning("[ParallaxBGEditor] There is no background sprite.");
                    return;
                }
                if (parallaxLayers.Length <= 0)
                {
                    Debug.LogWarning("[ParallaxBGEditor] There is no background sprite added.");
                    return;
                }
                if (parallaxLayers.All(x => x.sprite == null))
                {
                    Debug.LogWarning("[ParallaxBGEditor] There is blank/null background sprites.");
                    return;
                }

                CleanBGObjectArray();

                for (int i = 0; i < parallaxLayers.Length; i++)
                {
                    // Set this first as the layer can be incorrect on start.
                    if (setLayerOrderAuto && modifyBGLayer)
                    {
                        layerOrderStart = (-5) - targetGroup.ChildLength;
                    }

                    // Refreshable BGLayer.
                    int bgLayer = layerOrderStart - targetGroup.ChildLength;

                    GameObject parentSet = new GameObject($"BGHolderGroup{targetGroup.ChildLength}");
                    ParallaxBackgroundLayer layerComponent = parentSet.AddComponent<ParallaxBackgroundLayer>();

                    parentSet.transform.SetParent(targetGroup.transform);
                    layerComponent.parallaxEffectAmount = parallaxLayers[i].parallaxAmount;
                    layerComponent.parentGroup = targetGroup;
                    // Use the tiled sprite renderer.
                    layerComponent.InitilazeTilingSpriteRenderer(parallaxLayers[i].sprite);

                    switch (layerSortMethod)
                    {
                        case BackgroundLayerSortMethod.SpriteRendererIndex:
                            layerComponent.TilingRendererComponent.SortOrder = bgLayer;
                            break;
                        default:
                        case BackgroundLayerSortMethod.ZAxisCoords:
                            // BGLayer is negative, so we make it positive to put it behind.
                            parentSet.transform.position =
                                new Vector3(
                                    parentSet.transform.position.x,
                                    parentSet.transform.position.y,
                                    setZOrderPositionAsPositive ? bgLayer : -bgLayer
                                );
                            break;
                    }

                    layerComponent.TilingRendererComponent.AutoTile = false;
                    layerComponent.TilingRendererComponent.GridX = layerRendererXTileCount;

                    targetGroup.Backgrounds.Add(layerComponent);
                }

                Debug.Log("[ParallaxBackgroundEditor] Created background(s).");
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private void DrawAddExistingSubMenu(SerializedObject so)
        {
            EditorGUILayout.LabelField("Objects To Add", EditorStyles.boldLabel);

            if (modifyBGLayer)
            {
                EditorGUIAdditionals.DrawArray(so, nameof(layerSpriteObjects));
            }

            EditorGUILayout.PropertyField(so.FindProperty(nameof(container)));
            EditorGUILayout.Space();

            // Add parallax object
            if (GUILayout.Button("Add"))
            {
                var BGLayer = layerOrderStart - targetGroup.ChildLength;

                if (container == null)
                {
                    Debug.LogWarning("[ParallaxBGEditor] There is no container.");
                    return;
                }

                // Check if this Container object already is registered
                if (container.TryGetComponent(out ParallaxBackgroundLayer objComponent))
                {
                    for (int i = 0; i < targetGroup.Backgrounds.Count; i++)
                    {
                        if (objComponent == targetGroup.Backgrounds[i])
                        {
                            Debug.Log($"[ParallaxBGEditor] There is already a parallax object with name \"{targetGroup.Backgrounds[i].name}\", updated settings.");
                            targetGroup.Backgrounds.Remove(targetGroup.Backgrounds[i]);
                            break;
                        }
                    }
                }

                if (modifyBGLayer)
                {
                    if (layerSpriteObjects == null)
                    {
                        Debug.LogWarning("[ParallaxBGEditor] There is no background object.");
                        return;
                    }
                    if (layerSpriteObjects.Length < 0)
                    {
                        Debug.LogWarning("[ParallaxBGEditor] There is no background object added.");
                        return;
                    }
                    if (layerSpriteObjects.Any(x => x == null))
                    {
                        Debug.LogWarning("[ParallaxBGEditor] There is blank/null background objects.");
                        return;
                    }
                }

                // Destroy previous objects.
                while (container.TryGetComponent(out ParallaxBackgroundLayer obj))
                {
                    DestroyImmediate(obj);
                }

                var pObj = container.gameObject.AddComponent<ParallaxBackgroundLayer>();
                pObj.parallaxEffectAmount = layerParallaxAmount;
                pObj.parentGroup = targetGroup;

                if (modifyBGLayer)
                {
                    switch (layerSortMethod)
                    {
                        case BackgroundLayerSortMethod.SpriteRendererIndex:
                            foreach (SpriteRenderer r in layerSpriteObjects)
                            {
                                r.sortingOrder = BGLayer;
                            }
                            break;
                        case BackgroundLayerSortMethod.ZAxisCoords:
                            container.transform.position
                                = new Vector3(container.transform.position.x, container.transform.position.y,
                                setZOrderPositionAsPositive ? BGLayer : -BGLayer);
                            break;
                    }
                }

                if (setLayerOrderAuto && modifyBGLayer)
                {
                    layerOrderStart = (-5) - targetGroup.ChildLength;
                }

                targetGroup.Backgrounds.Add(pObj);
                Debug.Log("[ParallaxBackgroundEditor] Added background.");
            }
        }
        private void DrawListOfGroupsSubMenu()
        {
            using var targetGroupSO = new SerializedObject(targetGroup);
            // Create array field "readonly"
            GUI.enabled = false;
            EditorGUILayout.PropertyField(targetGroupSO.FindProperty(nameof(ParallaxBackgroundGroup.Backgrounds)), true);
            GUI.enabled = true;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Destroy List"))
            {
                if (EditorUtility.DisplayDialog("Destroy Parallax Background...",
                    "Are you sure you wanna destroy the parallax backgrounds?\n",
                    "Yes", "No"))
                {
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("destroy parallax background");
                    int undoGroup = Undo.GetCurrentGroup();

                    Undo.RecordObject(targetGroup, string.Empty);
                    foreach (var obj in targetGroup.Backgrounds)
                    {
                        if (obj == null)
                        {
                            return;
                        }

                        Undo.DestroyObjectImmediate(obj.gameObject);
                    }
                    targetGroup.Backgrounds.Clear();

                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
            if (GUILayout.Button("Clear List"))
            {
                if (EditorUtility.DisplayDialog("Clear Parallax Background...",
                    "Are you sure you wanna clean the parallax background components?\nInfo : The background renderers are not destroyed.",
                    "Yes", "No"))
                {
                    // Record undo
                    Undo.IncrementCurrentGroup();
                    Undo.SetCurrentGroupName("clear parallax background");
                    int undoGroup = Undo.GetCurrentGroup();

                    Undo.RecordObject(targetGroup, string.Empty);
                    foreach (var obj in targetGroup.Backgrounds)
                    {
                        if (obj == null)
                        {
                            return;
                        }

                        Undo.DestroyObjectImmediate(obj);
                    }
                    targetGroup.Backgrounds.Clear();

                    Undo.CollapseUndoOperations(undoGroup);
                }
            }
            if (GUILayout.Button("Clean Null Values"))
            {
                CleanBGObjectArray();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawLayerOrderElemenets(SerializedObject so)
        {
            EditorGUILayout.Space();
            var TextStyleCentered = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("-Global Settings-", TextStyleCentered);

            // Standard
            EditorGUILayout.PropertyField(so.FindProperty(nameof(modifyBGLayer)));
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(so.FindProperty(nameof(layerRendererXTileCount)), new GUIContent("Sprite Renderer Tile X", "Adjust this according to your needs."));
            if (GUILayout.Button("+", GUILayout.Width(20f))) { layerRendererXTileCount++; }
            if (GUILayout.Button("-", GUILayout.Width(20f))) { layerRendererXTileCount--; }
            GUILayout.EndHorizontal();
            if (currentToolbarIndex != 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Note : If this value is higher, the code assumes that it is the background.\nThe background should be higher while the topmost foreground should be 0.", EditorStyles.miniBoldLabel, GUILayout.Height(30));
                EditorGUILayout.PropertyField(so.FindProperty(nameof(layerParallaxAmount)));
                EditorGUI.indentLevel--;
            }

            // Sorting settings
            if (modifyBGLayer)
            {
                EditorGUILayout.PropertyField(so.FindProperty(nameof(setLayerOrderAuto)));
                layerSortMethod = (BackgroundLayerSortMethod)EditorGUILayout.EnumPopup("Layer Sort Method", layerSortMethod);

                using (EditorGUI.DisabledScope scope = new EditorGUI.DisabledScope(setLayerOrderAuto))
                {
                    GUILayout.BeginHorizontal();
                    switch (layerSortMethod)
                    {
                        case BackgroundLayerSortMethod.SpriteRendererIndex:
                            EditorGUILayout.LabelField("Sprite Index Start", GUILayout.Width(150));
                            break;
                        case BackgroundLayerSortMethod.ZAxisCoords:
                            GUILayout.EndHorizontal();
                            EditorGUILayout.PropertyField(so.FindProperty(nameof(setZOrderPositionAsPositive)));
                            GUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("Z Index Start", GUILayout.Width(150));
                            break;
                    }
                    layerOrderStart = EditorGUILayout.IntField(layerOrderStart);
                    layerOrderStart = Mathf.Clamp(layerOrderStart, int.MinValue, -5);
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Info : Sorting is disabled. To enable it again check the 'ModifyBGLayer' box.", MessageType.Info);
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(250f);
        }

        private void OnGUI()
        {
            // Check null
            if (targetGroup == null)
            {
                // This label is being shown just in case the 'Close' function doesn't work.
                EditorGUILayout.LabelField("Please reload this window as this script cannot access the owner group.", EditorStyles.centeredGreyMiniLabel);
                Close();
                return;
            }

            // Scroll
            windowScrollPos = GUILayout.BeginScrollView(windowScrollPos);
            // Styles
            customButton ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            // Editor UI
            using var so = new SerializedObject(this);

            // Centered toolbar
            // Note : This is unaffected from the 'ScrollView'.
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            currentToolbarIndex = GUILayout.Toolbar(currentToolbarIndex, new string[] { "Create", "Add Existing", "List Of Groups" }, GUILayout.Width(350f), GUILayout.Height(45f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUIAdditionals.DrawUILineLayout(Color.gray);

            switch (currentToolbarIndex)
            {
                case 0:
                    DrawCreateSubMenu(so);
                    break;
                case 1:
                    DrawAddExistingSubMenu(so);
                    break;
                case 2:
                    DrawListOfGroupsSubMenu();
                    break;
                default:
                    EditorGUILayout.HelpBox($"[ParallaxBackgroundEditorWindow] The menu with index {currentToolbarIndex} doesn't exist.", MessageType.Error);
                    break;
            }

            GUIAdditionals.DrawUILineLayout(Color.gray);
            DrawLayerOrderElemenets(so);

            GUILayout.EndScrollView();
            so.ApplyModifiedProperties();
        }
    }
}
