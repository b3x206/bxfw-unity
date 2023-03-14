using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BXFW;

namespace BXFW
{
    /// <summary>
    /// Extensions for the <see cref="TilingSpriteRenderer"/>.
    /// </summary>
    public static class TilingSpriteRendererExtensions
    {
        /// <summary>
        /// Helper method for getting sprite alignment from sprite.
        /// </summary>
        public static SpriteAlignment GetSpriteAlignment(Sprite spriteAlign)
        {
            if (spriteAlign == null)
            {
                Debug.LogError("[TilingSpriteRenderer::GetSpriteAlignment] The sprite renderer passed is null.");
                return SpriteAlignment.Center;
            }

            // We were using box collider bounds, instead we use standard bounds.
            // var sRendCol = sRend.AddComponent<BoxCollider2D>();
            float colX = spriteAlign.bounds.center.x;
            float colY = spriteAlign.bounds.center.y;

            // Find where is the center
            if (colX > 0 && colY < 0)
                return (SpriteAlignment.TopLeft);
            else if (colX < 0 && colY < 0)
                return (SpriteAlignment.TopRight);
            else if (colX == 0 && colY < 0)
                return (SpriteAlignment.TopCenter);
            else if (colX > 0 && colY == 0)
                return (SpriteAlignment.LeftCenter);
            else if (colX < 0 && colY == 0)
                return (SpriteAlignment.RightCenter);
            else if (colX > 0 && colY > 0)
                return (SpriteAlignment.BottomLeft);
            else if (colX < 0 && colY > 0)
                return (SpriteAlignment.BottomRight);
            else if (colX == 0 && colY > 0)
                return (SpriteAlignment.BottomCenter);
            else if (colX == 0 && colY == 0)
                return (SpriteAlignment.Center);
            else
                return (SpriteAlignment.Custom);
        }

        /// <summary>
        /// Resizes an sprite mask to the size of the camera fit.
        /// </summary>
        /// <param name="relativeCam">Ortographic camera to resize.</param>
        /// <param name="sRend">The mask to resize.</param>
        /// <param name="resizeAxis">Axis to resize. Adjust accordingly. 1 means positive on that axis.</param>
        public static void ResizeSpriteMaskToScreen(this Camera relativeCam, TilingSpriteRenderer sRend, float setMultiplier = 1f, Vector2Int? resizeAxis = null)
        {
            if (resizeAxis == null)
            {
                resizeAxis = Vector2Int.one;
            }

            if (sRend == null || relativeCam == null)
            {
                Debug.LogWarning("[Additionals::ResizeSpriteToScreen] There is a null variable. Returning.");
                return;
            }

            sRend.transform.localScale = new Vector3(1, 1, 1);

            var width = sRend.tiledSprite.bounds.size.x;
            var height = sRend.tiledSprite.bounds.size.y;

            var worldScreenHeight = relativeCam.orthographicSize * 2.0f;
            float worldScreenWidth = 1f;
            if (Application.isEditor)
                worldScreenWidth = worldScreenHeight / Screen.currentResolution.height * Screen.currentResolution.width;
            else if (Application.isPlaying)
                worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

            Vector2 SetValue = new Vector2(worldScreenWidth / width, worldScreenHeight / height) * setMultiplier;
            // Debug.Log("{SetValue}");

            if (resizeAxis == Vector2Int.one)
            {
                sRend.transform.localScale =
                        new Vector3(SetValue.x, SetValue.y, sRend.transform.localScale.z);
            }
            else if (resizeAxis == Vector2Int.right)
            {
                sRend.transform.localScale =
                        new Vector3(SetValue.x, sRend.transform.localScale.y, sRend.transform.localScale.z);
            }
            else if (resizeAxis == Vector2Int.up)
            {
                sRend.transform.localScale =
                        new Vector3(sRend.transform.localScale.x, SetValue.y, sRend.transform.localScale.z);
            }
            else
            {
                Debug.LogWarning($"[TilingSpriteRenderer::ResizeSpriteToScreen] Invalid axis passed {resizeAxis}.");
            }

            //if (!sRend.CorrectScaledTransformIsCorrect())
            //    sRend.GenerateCorrectScaleParent();
        }

        //public static bool CorrectScaledTransformIsCorrect(this TilingSpriteRenderer sr)
        //{
        //    return sr.CorrectScaledParent.localScale == new Vector3
        //        (1f / sr.transform.localScale.x,
        //         1f / sr.transform.localScale.y,
        //         1f / sr.transform.localScale.z);
        //}
    }

    /// <summary>
    /// TODO : Seperate classes to be base and current.
    /// Don't forget to call <see cref="GenerateGrid()"/> after resizing object.
    /// </summary>
    // @NOTE the attached sprite's position should be "Top Right" or the children will not align properly
    // Strech out the image as you need in the sprite render, the following script will auto-correct it when rendered in the game
    // Generates a nice set of repeated sprites inside a streched sprite renderer
    public class TilingSpriteRenderer : MonoBehaviour
    {
        #region Variables
        public SpriteRenderer RendererRef;
        // -- Inspector Access
        [Header("Tiling Options")]
        public bool GridOnAwake = true;
        public bool CameraResize = false;
        [SerializeField] private int _SortOrder = 0;
        public int SortOrder
        {
            get { return _SortOrder; }
            set
            {
                _SortOrder = value;

                if (TiledSpriteObjs.Values.Count <= 0)
                    return;

                foreach (var list in TiledSpriteObjs.Values)
                {
                    foreach (var rend in list)
                    {
                        if (rend == null)
                            Debug.LogWarning($"[TilingSpriteRenderer::(set)SortOrder] Renderers are null in object '{name}'.");

                        rend.sortingOrder = value;
                    }
                }
            }
        }
        [SerializeField] private Color rendColor = Color.white;
        public Color RendColor
        {
            get { return rendColor; }
            set
            {
                rendColor = value;

                if (AllRenderer.Count <= 0)
                {
                    // Get child transforms & add
                    foreach (Transform rendAdd in transform)
                    {
                        if (rendAdd.TryGetComponent(out SpriteRenderer set))
                            AllRenderer.Add(set);
                    }
                }

                foreach (var rend in AllRenderer)
                {
                    if (rend == null)
                        Debug.LogWarning($"[TilingSpriteRenderer::(set)SortOrder] Renderers are null in object '{name}'.");

                    rend.color = value;
                }
            }
        }

        private const float RESIZE_T_SET_MUL_CLAMP_MIN = 0.001f;
        public Vector2 ResizeTSetMultiplierClamp
        {
            get { return resizeTSetMultiplierClamp; }
            set
            {
                value.y = Mathf.Max(value.y, RESIZE_T_SET_MUL_CLAMP_MIN + RESIZE_T_SET_MUL_CLAMP_MIN);
                value.x = Mathf.Clamp(value.x, RESIZE_T_SET_MUL_CLAMP_MIN, value.y);

                resizeTSetMultiplierClamp = value;

                // Also update the ResizeTransformSetMultiplier
                ResizeTformSetMultiplier = ResizeTformSetMultiplier;
            }
        }
        [SerializeField] private Vector2 resizeTSetMultiplierClamp = new Vector2(RESIZE_T_SET_MUL_CLAMP_MIN, 10f); // Default value
        public float ResizeTformSetMultiplier
        {
            get { return resizeTformSetMultiplier; }
            set { resizeTformSetMultiplier = Mathf.Clamp(value, ResizeTSetMultiplierClamp.x, ResizeTSetMultiplierClamp.y); }
        }
        [SerializeField] private float resizeTformSetMultiplier = 1f; // Default value

        public bool AutoTile
        {
            get { return autoTile; }
            set
            {
                autoTile = value;
                GenerateGrid();
            }
        }
        public Vector2Int AllowGridAxis
        {
            get { return allowGridAxis; }
            set
            {
                allowGridAxis = value;
                GenerateGrid();
            }
        }
        [SerializeField] private Vector2Int allowGridAxis = Vector2Int.one; // Allow grid on both axis. (Default value desc.)
        [SerializeField] private bool autoTile = false;
        public int GridX
        {
            get { return gridX; }
            set
            {
                gridX = Mathf.Clamp(value, 0, int.MaxValue);
                GenerateGrid();
            }
        }
        [SerializeField] private int gridX = 0;                             // Default value
        public int GridY
        {
            get { return gridY; }
            set
            {
                gridY = Mathf.Clamp(value, 0, int.MaxValue);
                GenerateGrid();
            }
        }
        [SerializeField] private int gridY = 0;                             // Default value

        // Auto resize options
        //public Vector2Int MaskResizeAxis 
        //{ 
        //    get { return maskResizeAxis; } 
        //    set { maskResizeAxis = value; ResizeObj(); } 
        //}
        //[SerializeField] private Vector2Int maskResizeAxis = Vector2Int.right; // Only resize the mask on x. (Default value desc.)

        [Header("Sprite")]
        public Sprite tiledSprite;

        // -- Script access
        [SerializeField] private Transform correctScaledTransform;
        //public Transform CorrectScaledParent 
        //{ 
        //    get { return correctScaledTransform; } 
        //    private set { correctScaledTransform = value; } 
        //}
        /// <summary>
        /// Access the <see cref="TiledSpriteObjs"/> dictionary.
        /// </summary>
        /// <param name="key">Key axis. If passed invalid key you will get null object</param>
        public List<SpriteRenderer> this[Vector2Int key]
        {
            get { return TiledSpriteObjs[key]; }
            private set { TiledSpriteObjs[key] = value; }
        }
        public SpriteRenderer this[int key]
        {
            get { return AllRenderer[key]; }
        }


        [SerializeField]
        private SerializableDictionary<Vector2Int, List<SpriteRenderer>> TiledSpriteObjs =
            new SerializableDictionary<Vector2Int, List<SpriteRenderer>>();
        [SerializeField] private List<SpriteRenderer> AllRenderer = new List<SpriteRenderer>();
        public Camera ResizeTargetCamera;

        // ---- Tempoary ---- //
        private bool AwakeCalledInit = false;
        #endregion

        private void Awake()
        {
            Initilaze();
            AwakeCalledInit = true;
        }

        /// <summary>
        /// Initilazes the <see cref="TilingSpriteRenderer"/>.
        /// </summary>
        public void Initilaze()
        {
            // Set main camera
            ResizeTargetCamera = Camera.main;
            // Create correct scaled parent.
            //GenerateCorrectScaleParent();

            // Set Tiled objects default value
            TiledSpriteObjs = new SerializableDictionary<Vector2Int, List<SpriteRenderer>>();

            // -- Awake only events
            if (!AwakeCalledInit)
            {
                if (GridOnAwake)
                {
                    GenerateGrid();
                }
            }
        }
        //public void GenerateCorrectScaleParent()
        //{
        //    if (CorrectScaledParent == null)
        //    {
        //        CorrectScaledParent = new GameObject("CorrectScaledParent").transform;
        //        CorrectScaledParent.SetParent(transform);
        //    }

        //    CorrectScaledParent.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);
        //}

        /// <summary>
        /// Method to regenerate grid.
        /// </summary>
        public void GenerateGrid()
        {
            /// WARNING : Always set the private (<see cref="gridX"/>/<see cref="gridY"/>) variables inside <see cref="GenerateGrid"/>!
            /// If you don't do that something bad will happen. (stack overflow exception) 
            /// The reason is that public values call this method.

            #region GenerateGrid Prepare
            if (/*(CorrectScaledParent == null ||*/ ResizeTargetCamera == null ||
                TiledSpriteObjs == null)
            {
                Initilaze();
            }

            //ResizeObj();

            //if (!this.CorrectScaledTransformIsCorrect())
            //{
            //    GenerateCorrectScaleParent();
            //}

            // Call autotile statement after resize to get correct bound scale.
            if (AutoTile)
            {
                gridX = 1;
                gridY = 1;

                // Calculate bounds
                // Split bounds and ceil the value. (to avoid spaces)
                // Debug.Log($"Bound mask : {SpriteMaskComponent.bounds.size.x} / {tiledSprite.bounds.size.x} -> {SpriteMaskComponent.bounds.size.x / tiledSprite.bounds.size.x}");
                gridX = (Mathf.CeilToInt(transform.lossyScale.x / tiledSprite.bounds.size.x) * 2) - gridX;
                gridY = (Mathf.CeilToInt(transform.lossyScale.y / tiledSprite.bounds.size.y) * 2) - gridY;
            }

            if (tiledSprite == null)
            {
                Debug.LogError($"[TilingSpriteRenderer] The tiledSprite variable is null on object \"{name}\".");
                return;
            }
            #endregion

            // Destroy if grid is set to 0? idk
            ClearGrid();

            #region GenerateGrid Generate Object
            if ((GridX <= 0 || GridY <= 0) && !AutoTile) return; // No grid

            /// Returns true if the number is odd.
            /// Delegate.
            /// Info : If unity complains about this method being static remove the static keyword and ignore visual studio. seems to work in 2020 unity.
            static bool tileRightOrUp(int currTile)
            {
                return (currTile % 2) == 1;
            }

            var gX = (AllowGridAxis.x == 1) ? GridX : 1;
            var gY = (AllowGridAxis.y == 1) ? GridY : 1;

            for (int y = 0; y < gY; y++)
            {
                var ListTile = new List<SpriteRenderer>(GridX);
                AllRenderer.Clear();

                int x;
                for (x = 0; x < gX; x++)
                {
                    if (IsClearing) return;

                    var sRend = new GameObject($"Tile({x}, {y})").AddComponent<SpriteRenderer>();
                    //sRend.transform.SetParent(CorrectScaledParent);
                    sRend.transform.SetParent(transform);
                    sRend.sprite = tiledSprite;
                    sRend.sortingOrder = _SortOrder;
                    sRend.maskInteraction = SpriteMaskInteraction.None;
                    sRend.transform.localPosition = new Vector3(
                        sRend.bounds.size.x * Mathf.CeilToInt(x / 2f) * (tileRightOrUp(x) ? 1f : -1f),
                        sRend.bounds.size.y * Mathf.CeilToInt(y / 2f) * (tileRightOrUp(y) ? 1f : -1f)
                        );

                    sRend.transform.localScale = Vector3.one;

                    if (RendererRef == null)
                    {
                        RendererRef = sRend;
                    }
                    ListTile.Add(sRend);
                    AllRenderer.Add(sRend);
                }

                TiledSpriteObjs.Add(new Vector2Int(x, y), ListTile);
            }

            RendColor = rendColor;
            #endregion

            #region This place is a scrapped bounds calculating thing
            /*
            // Destroy all childs.
            foreach (Transform t in transform)
            {
                if (t == transform) continue;

                Destroy(t.gameObject);
            }

            // Set component settings
            sRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            sMask.sprite = sprite;

            // Check alignment
            var align = GetSpriteAlignment(sRenderer);
            if (align != SpriteAlignment.TopRight)
            {
                Debug.LogWarning($"[TilingSpriteRenderer::Awake] You forgot change the sprite pivot to Top Right. The pivot gathered was : \"{align}\".");
            }


            // math
            Vector2 spriteSize_wu = new Vector2(sRenderer.bounds.size.x / transform.localScale.x, 
                sRenderer.bounds.size.y / transform.localScale.y);
            Vector3 scale = new Vector3(1.0f, 1.0f, 1.0f);

            if (gridX != 0.0f) 
            {
                float width_wu = sRenderer.bounds.size.x / gridX;
                scale.x = width_wu / spriteSize_wu.x;
                spriteSize_wu.x = width_wu;
            }
            if (gridY != 0.0f) 
            {
                float height_wu = sRenderer.bounds.size.y / gridY;
                scale.y = height_wu / spriteSize_wu.y;
                spriteSize_wu.y = height_wu;
            }

            // Create gameobject 'prefab'
            GameObject childPrefab = new GameObject("BGTilePrefab");
            SpriteRenderer childSprite = childPrefab.AddComponent<SpriteRenderer>();
            childPrefab.transform.position = transform.position;
            childSprite.sprite = sRenderer.sprite;

            GameObject child;

            Debug.Log($"For loop conditions // y bounds : i * {(int)spriteSize_wu.y} < {Mathf.RoundToInt(sRenderer.bounds.size.y)} | x bounds : j * {(int)spriteSize_wu.x} < {Mathf.RoundToInt(sRenderer.bounds.size.x)}");

            // TODO : Fix bound calculation.
            for (int i = 0, h = Mathf.RoundToInt(sRenderer.bounds.size.y); i * spriteSize_wu.y < h; i++) 
            {
                Debug.Log($"{i} * {(int)spriteSize_wu.y / 2} < {Mathf.RoundToInt(sRenderer.bounds.size.y)}");

                for (int j = 0, w = Mathf.RoundToInt(sRenderer.bounds.size.x); j * spriteSize_wu.x < w; j++) 
                {
                    child = Instantiate(childPrefab);
                    child.transform.position = transform.position - (new Vector3(spriteSize_wu.x * j, spriteSize_wu.y * i, 0f));
                    child.transform.localScale = scale;
                    child.transform.parent = transform;
                }
            }

            // Destroy tempoary prefab.
            Destroy(childPrefab);

            // Disable this SpriteRenderer and let the prefab children render themselves
            // Note : We only use the sprite renderer for bounds, so i might create a bounds class that is only used for determining bounds.
            sRenderer.enabled = false; */
            #endregion
        }

        ///// <summary>
        ///// Resizes the object according to the camera.
        ///// </summary>
        //public void ResizeObj()
        //{
        //    if (CameraResize)
        //    {
        //        ResizeTargetCamera.ResizeSpriteMaskToScreen(this, ResizeTformSetMultiplier, maskResizeAxis);
        //    }
        //}

        /// <summary>
        /// <see cref="ClearGrid"/> should only call SET on this.
        /// Get whether the function <see cref="ClearGrid"/> is running.
        /// </summary>
        private bool IsClearing = false;
        public void ClearGrid()
        {
            IsClearing = true;
            if (transform.childCount > 0)
            {
                // Cache the child count (that updates with the bool condition)
                var childCount = transform.childCount;

                for (int i = 0; i < childCount; i++)
                {
                    /// Q: Why is there a constant '0'????
                    /// A: Well, unity changed stuff.
                    /// Previously the 'foreach' iterator of a transform does not update it's indexes whenever there is a null member.
                    /// Since we perform destroy action inside a foreach, our iterator moves 2 steps forward instead of ignoring the 
                    /// null member (what we mean is the iterator list is stripped from nulls).
                    /// Whenever this null members are stripped, the array shifts and it does not use the intended value to remove, 
                    /// leaving half of the transform content intact.
                    /// This just removes whatever is index 0 now. It should work on all unity versions as index 0 shifts like the 
                    /// unity 2020 transform iterator.
                    /// ----
                    /// TL;DR : this is just a simple hack to mitigate a hard to debug problem, and that's why we have a very long
                    /// comment here.
                    var t = transform.GetChild(0);

                    if (t == null)
                    {
                        Debug.Log("Transform is null. Continuing.");
                        continue;
                    }

                    if (Application.isEditor)
                        DestroyImmediate(t.gameObject);
                    else
                        Destroy(t.gameObject);
                }
            }
            /// It now works (goddamnit unity)
            IsClearing = false;
            if (TiledSpriteObjs != null)
            {
                if (TiledSpriteObjs.Count > 0)
                {
                    TiledSpriteObjs.Clear();
                }
            }
        }
    }
}
