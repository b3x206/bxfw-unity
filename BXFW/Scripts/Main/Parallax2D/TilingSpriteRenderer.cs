using UnityEngine;
using System.Collections.Generic;
using System;

namespace BXFW
{
    /// <summary>
    /// Generates a tiled series of sprites, using <see cref="GameObject"/>s and <see cref="SpriteRenderer"/>s.
    /// </summary>
    [ExecuteAlways]
    public class TilingSpriteRenderer : MonoBehaviour
    {
        public bool GridOnAwake = true;
        public bool CameraResize = false;
        public Camera ResizeTargetCamera;

        [SerializeField] private int _SortOrder = 0;
        public int SortOrder
        {
            get { return _SortOrder; }
            set
            {
                _SortOrder = value;

                if (tiledSpriteObjs.Values.Count <= 0)
                {
                    return;
                }

                foreach (var list in tiledSpriteObjs.Values)
                {
                    foreach (var rend in list)
                    {
                        if (rend == null)
                        {
                            Debug.LogWarning($"[TilingSpriteRenderer::(set)SortOrder] Null renderer registered in object '{this.GetPath()}'.");
                        }

                        rend.sortingOrder = value;
                    }
                }
            }
        }
        /// <summary>
        /// Internal color for the renderer.
        /// </summary>
        [SerializeField] private Color rendererColor = Color.white;
        /// <summary>
        /// Color to set the all children renderers into.
        /// </summary>
        public Color Color
        {
            get { return rendererColor; }
            set
            {
                rendererColor = value;

                if (allRendererObjects.Count <= 0)
                {
                    // Get child transforms & add
                    foreach (Transform rendAdd in transform)
                    {
                        if (rendAdd.TryGetComponent(out SpriteRenderer set))
                        {
                            allRendererObjects.Add(set);
                        }
                    }
                }

                foreach (var rend in allRendererObjects)
                {
                    if (rend == null)
                    {
                        Debug.LogWarning($"[TilingSpriteRenderer::(set)SortOrder] Null renderer registered in object '{this.GetPath()}'.");
                    }

                    rend.color = value;
                }
            }
        }
        /// <summary>
        /// Sprite to tile.
        /// </summary>
        public Sprite TiledSprite;
        /// <summary>
        /// Bounds of a single sprite renderer.
        /// <br>Returns a dummy value if there is no sprite renderer.</br>
        /// </summary>
        public Bounds SingleBounds
        {
            get
            {
                if (allRendererObjects.Count <= 0)
                {
                    return TiledSprite != null ? TiledSprite.bounds : default;
                }

                return allRendererObjects[0].bounds;
            }
        }

        [SerializeField] private bool autoTile = false;
        public bool AutoTile
        {
            get { return autoTile; }
            set
            {
                autoTile = value;
                GenerateGrid();
            }
        }
        [SerializeField] private TransformAxis2D allowGridAxis = TransformAxis2D.XYAxis;
        public TransformAxis2D AllowGridAxis
        {
            get
            {
                return allowGridAxis;
            }
            set
            {
                allowGridAxis = value;
                GenerateGrid();
            }
        }
        public int GridX
        {
            get { return gridX; }
            set
            {
                gridX = Mathf.Clamp(value, 0, int.MaxValue);
                GenerateGrid();
            }
        }
        [SerializeField] private int gridX = 0;
        public int GridY
        {
            get { return gridY; }
            set
            {
                gridY = Mathf.Clamp(value, 0, int.MaxValue);
                GenerateGrid();
            }
        }
        [SerializeField] private int gridY = 0;

        [SerializeField] private Transform correctScaledParent;
        public Transform CorrectScaledParent
        {
            get
            {
                GenerateCorrectScaleParent();
                return correctScaledParent;
            }
        }
        /// <summary>
        /// Generates a correct scaled parent if it doesn't exist, if it does resizes it.
        /// </summary>
        public void GenerateCorrectScaleParent()
        {
            if (correctScaledParent == null)
            {
                correctScaledParent = new GameObject("CorrectScaledParent").transform;
                correctScaledParent.SetParent(transform);
            }
            // Move all 1 layer down children to the 'correctScaledParent'
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child != correctScaledParent && child.parent != correctScaledParent)
                {
                    child.SetParent(correctScaledParent);
                    i--;
                }
            }

            correctScaledParent.localPosition = Vector3.zero;
            correctScaledParent.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);
        }

        [SerializeField] private List<SpriteRenderer> allRendererObjects = new List<SpriteRenderer>();
        public SpriteRenderer this[int key]
        {
            get { return allRendererObjects[key]; }
        }
        /// <summary>
        /// All of the sprite renderers, queue and placement agnostic.
        /// </summary>
        public IReadOnlyList<SpriteRenderer> AllRendererObjects
        {
            get
            {
                return allRendererObjects;
            }
        }

        /// <summary>
        /// List wrapper for unity to serialize the tiled objects.
        /// </summary>
        [Serializable]
        public class SpriteRendererList : List<SpriteRenderer>
        {
            public SpriteRendererList()
            { }
            public SpriteRendererList(int capacity) : base(capacity)
            { }
            public SpriteRendererList(IEnumerable<SpriteRenderer> collection) : base(collection)
            { }
        }
        [SerializeField] private SerializableDictionary<int, SpriteRendererList> tiledSpriteObjs = new SerializableDictionary<int, SpriteRendererList>();
        /// <summary>
        /// Get the X tiled sprite renderers on their Y index.
        /// <br>Index of the X tiles are sequential, but their object placements are not sequential.</br>
        /// </summary>
        public IReadOnlyDictionary<int, SpriteRendererList> TiledSpriteObjects
        {
            get
            {
                return tiledSpriteObjs;
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            Initilaze();

            if (GridOnAwake)
            {
                GenerateGrid();
            }
        }
        private void Update()
        {
            if (transform.hasChanged)
            {
                GenerateCorrectScaleParent();
            }
        }

        /// <summary>
        /// Initilazes the <see cref="TilingSpriteRenderer"/>.
        /// </summary>
        public void Initilaze()
        {
            // Set main camera
            if (ResizeTargetCamera == null)
            {
                ResizeTargetCamera = Camera.main;
            }

            // Create correct scaled parent.
            GenerateCorrectScaleParent();
        }
        /// <summary>
        /// Method to regenerate grid.
        /// </summary>
        /// <returns><see langword="true"/> if the generation was successful. Note that this method calls <see cref="ClearGrid"/> no matter the result.</returns>
        public bool GenerateGrid()
        {
            // Call this first to avoid destroying existing stuff
            ClearGrid();

            // Prepare
            if (CorrectScaledParent == null || ResizeTargetCamera == null || tiledSpriteObjs == null)
            {
                Initilaze();
            }

            if (TiledSprite == null)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError($"[TilingSpriteRenderer::GenerateGrid] The tiledSprite variable is null on object \"{name}\".");
                }

                return false;
            }

            // Call autotile statement after resize to get correct bound scale.
            if (AutoTile)
            {
                gridX = 1;
                gridY = 1;

                // Calculate bounds, split bounds and ceil the value. (to avoid spaces)
                // TODO : Use SpriteMaskComponent.bounds for proper auto tiling
                gridX = (Mathf.CeilToInt(transform.lossyScale.x / TiledSprite.bounds.size.x) * 2) - gridX;
                gridY = (Mathf.CeilToInt(transform.lossyScale.y / TiledSprite.bounds.size.y) * 2) - gridY;
            }

            // Generate Object
            if ((gridX <= 0 || gridY <= 0) && !AutoTile)
            {
                return false; // No grid
            }

            bool tileRightOrUp(int currTile)
            {
                return (currTile % 2) == 1;
            }

            // Grid count
            int gX = ((AllowGridAxis & TransformAxis2D.XAxis) == TransformAxis2D.XAxis) ? gridX : 1;
            int gY = ((AllowGridAxis & TransformAxis2D.YAxis) == TransformAxis2D.YAxis) ? gridY : 1;

            for (int y = 0; y < gY; y++)
            {
                int x;

                var ListTile = new SpriteRendererList(gridX);

                for (x = 0; x < gX; x++)
                {
                    SpriteRenderer sRend = new GameObject($"Tile({x}, {y})").AddComponent<SpriteRenderer>();
                    sRend.transform.SetParent(CorrectScaledParent);
                    sRend.sprite = TiledSprite;
                    sRend.sortingOrder = _SortOrder;
                    sRend.transform.localPosition = new Vector3(
                        sRend.bounds.size.x * Mathf.CeilToInt(x / 2f) * (tileRightOrUp(x) ? 1f : -1f),
                        sRend.bounds.size.y * Mathf.CeilToInt(y / 2f) * (tileRightOrUp(y) ? 1f : -1f)
                    );

                    sRend.transform.localScale = Vector3.one;

                    ListTile.Add(sRend);
                    allRendererObjects.Add(sRend);
                }

                tiledSpriteObjs.Add(y, ListTile);
            }

            // Set renderable colors
            Color = rendererColor;

            return true;
        }

        /// <summary>
        /// Clears the sprites on current arrays.
        /// </summary>
        public void ClearGrid()
        {
            if (CorrectScaledParent.childCount > 0)
            {
                // transform.childCount updates when an object is destroyed
                // keep in current state for the exact amount of children to be destroyed.
                var childCount = CorrectScaledParent.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    var t = CorrectScaledParent.GetChild(0);

                    if (t == null)
                    {
                        continue;
                    }

                    if (t == CorrectScaledParent)
                    {
                        continue;
                    }
#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        UnityEditor.Undo.DestroyObjectImmediate(t.gameObject);
                    }
                    else
#endif
                    {
                        // Destroy does not work, because making assets that generate objects on scene
                        // is a bad thing, you need to fiddle with Mesh component instead
                        DestroyImmediate(t.gameObject);
                    }
                }
            }

            if (tiledSpriteObjs != null && tiledSpriteObjs.Count > 0)
            {
                tiledSpriteObjs.Clear();
            }
            if (allRendererObjects != null && allRendererObjects.Count > 0)
            {
                allRendererObjects.Clear();
            }
        }
    }
}
