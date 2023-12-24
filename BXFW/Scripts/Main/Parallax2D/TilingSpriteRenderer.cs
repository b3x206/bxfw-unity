using UnityEngine;
using System.Collections.Generic;

namespace BXFW
{
    /// <summary>
    /// Generates a tiled series of sprites, using <see cref="GameObject"/>s and <see cref="SpriteRenderer"/>s.
    /// <br>This method prevents overdraw instead of using the 'Full Rect' for tiling <see cref="SpriteRenderer"/>s.</br>
    /// </summary>
    [ExecuteAlways]
    public class TilingSpriteRenderer : MonoBehaviour
    {
        /// <summary>
        /// Generates grid on <see cref="Awake"/> call.
        /// </summary>
        public bool gridOnAwake = true;
        /// <summary>
        /// Resizes the sprite to fit into the given <see cref="resizeTargetCamera"/>.
        /// </summary>
        public bool cameraResize = false;
        /// <summary>
        /// Camera to fit into if <see cref="cameraResize"/> is true.
        /// <br>This is set to <see cref="Camera.main"/> if there's no camera set.</br>
        /// </summary>
        public Camera resizeTargetCamera;

        [SerializeField] private int m_SortOrder = 0;
        public int SortOrder
        {
            get { return m_SortOrder; }
            set
            {
                m_SortOrder = value;

                if (m_AllRendererObjects.Count <= 0)
                {
                    // Get child transforms & add
                    foreach (Transform rendAdd in transform)
                    {
                        if (rendAdd.TryGetComponent(out SpriteRenderer set))
                        {
                            m_AllRendererObjects.Add(set);
                        }
                    }
                }

                foreach (var rend in m_AllRendererObjects)
                {
                    if (rend == null)
                    {
                        Debug.LogWarning($"[TilingSpriteRenderer::(set)SortOrder] Null renderer registered in object '{this.GetPath()}'.");
                    }

                    rend.sortingOrder = value;
                }
            }
        }
        /// <summary>
        /// Internal colors for the renderer.
        /// </summary>
        [SerializeField] private Color m_RendererColors = Color.white;
        /// <summary>
        /// Color to set the all children renderers into.
        /// </summary>
        public Color Color
        {
            get { return m_RendererColors; }
            set
            {
                m_RendererColors = value;

                if (m_AllRendererObjects.Count <= 0)
                {
                    // Get child transforms & add
                    foreach (Transform rendAdd in transform)
                    {
                        if (rendAdd.TryGetComponent(out SpriteRenderer set))
                        {
                            m_AllRendererObjects.Add(set);
                        }
                    }
                }

                foreach (var rend in m_AllRendererObjects)
                {
                    if (rend == null)
                    {
                        Debug.LogWarning($"[TilingSpriteRenderer::(set)Color] Null renderer registered in object '{this.GetPath()}'.");
                    }

                    rend.color = value;
                }
            }
        }
        [SerializeField] private Sprite m_TiledSprite;
        /// <summary>
        /// Sprite to tile.
        /// </summary>
        public Sprite TiledSprite
        {
            get { return m_TiledSprite; }
            set
            {
                Sprite prevValue = m_TiledSprite;
                m_TiledSprite = value;
                if (prevValue == null && value != null)
                {
                    GenerateGrid();
                }

                if (m_AllRendererObjects.Count <= 0)
                {
                    // Get child transforms & add
                    foreach (Transform rendAdd in transform)
                    {
                        if (rendAdd.TryGetComponent(out SpriteRenderer set))
                        {
                            m_AllRendererObjects.Add(set);
                        }
                    }
                }

                foreach (var rend in m_AllRendererObjects)
                {
                    if (rend == null)
                    {
                        Debug.LogWarning($"[TilingSpriteRenderer::(set)TiledSprite] Null renderer registered in object '{this.GetPath()}'.");
                    }

                    rend.sprite = m_TiledSprite;
                }
            }
        }
 
        /// <summary>
        /// Bounds of a single sprite renderer.
        /// <br>Returns a <see langword="default"/> value if there is no sprite renderer.</br>
        /// </summary>
        public Bounds SingleBounds
        {
            get
            {
                if (m_AllRendererObjects.Count <= 0)
                {
                    return m_TiledSprite != null ? m_TiledSprite.bounds : default;
                }

                return m_AllRendererObjects[0].bounds;
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
        [SerializeField] private TransformAxis2D m_AllowGridAxis = TransformAxis2D.XYAxis;
        public TransformAxis2D AllowGridAxis
        {
            get
            {
                return m_AllowGridAxis;
            }
            set
            {
                m_AllowGridAxis = value;
                GenerateGrid();
            }
        }
        public int GridX
        {
            get { return m_GridX; }
            set
            {
                int prev = m_GridX;
                m_GridX = Mathf.Clamp(value, 0, int.MaxValue);

                if (prev != m_GridX)
                {
                    GenerateGrid();
                }
            }
        }
        [SerializeField] private int m_GridX = 0;
        public int GridY
        {
            get { return m_GridY; }
            set
            {
                int prev = m_GridY;
                m_GridY = Mathf.Clamp(value, 0, int.MaxValue);
                
                if (prev != m_GridY)
                {
                    GenerateGrid();
                }
            }
        }
        [SerializeField] private int m_GridY = 1;

        [SerializeField] private Transform m_CorrectScaledParent;
        public Transform CorrectScaledParent
        {
            get
            {
                GenerateCorrectScaleParent();
                return m_CorrectScaledParent;
            }
        }
        /// <summary>
        /// Generates a correct scaled parent if it doesn't exist, if it does resizes it.
        /// </summary>
        public void GenerateCorrectScaleParent()
        {
            if (m_CorrectScaledParent == null)
            {
                m_CorrectScaledParent = new GameObject("CorrectScaledParent").transform;
                m_CorrectScaledParent.SetParent(transform);
            }
            // Move all 1 layer down children to the 'correctScaledParent'
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child != m_CorrectScaledParent && child.parent != m_CorrectScaledParent)
                {
                    child.SetParent(m_CorrectScaledParent);
                    i--;
                }
            }

            m_CorrectScaledParent.localPosition = Vector3.zero;
            m_CorrectScaledParent.localRotation = Quaternion.identity;
            // prevent 0 scale
            if (Mathf.Approximately(transform.localScale.Abs().MinAxis(), 0f))
            {
                Debug.LogWarning($"[TilingSpriteRenderer::GenerateCorrectScaleParent] Invalid scale '{transform.localScale}' for transform '{name}'. Setting scale to Vector3.one.");
                transform.localScale = Vector3.one;
            }

            m_CorrectScaledParent.localScale = new Vector3(1f / transform.localScale.x, 1f / transform.localScale.y, 1f / transform.localScale.z);
        }

        [SerializeField] private List<SpriteRenderer> m_AllRendererObjects = new List<SpriteRenderer>();
        public SpriteRenderer this[int key]
        {
            get { return m_AllRendererObjects[key]; }
        }
        /// <summary>
        /// All of the sprite renderers, queue and placement agnostic.
        /// </summary>
        public IReadOnlyList<SpriteRenderer> AllRendererObjects
        {
            get
            {
                return m_AllRendererObjects;
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

            if (gridOnAwake)
            {
                GenerateGrid();
            }
        }
        private void Update()
        {
            if (transform.hasChanged)
            {
                GenerateCorrectScaleParent();

                if (AutoTile && m_TiledSprite != null)
                {
                    GridX = (Mathf.CeilToInt(Mathf.Abs(transform.lossyScale.x) / m_TiledSprite.bounds.size.x) * 2) - 1;
                    GridY = (Mathf.CeilToInt(Mathf.Abs(transform.lossyScale.y) / m_TiledSprite.bounds.size.y) * 2) - 1;
                }
            }
        }

        /// <summary>
        /// Initilazes the <see cref="TilingSpriteRenderer"/>.
        /// </summary>
        public void Initilaze()
        {
            // Set main camera
            if (resizeTargetCamera == null)
            {
                resizeTargetCamera = Camera.main;
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
            if (CorrectScaledParent == null || resizeTargetCamera == null || m_AllRendererObjects == null)
            {
                Initilaze();
            }

            if (m_TiledSprite == null)
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
                m_GridX = 1;
                m_GridY = 1;

                // Calculate bounds, split bounds and ceil the value. (to avoid spaces)
                m_GridX = (Mathf.CeilToInt(Mathf.Abs(transform.lossyScale.x) / m_TiledSprite.bounds.size.x) * 2) - m_GridX;
                m_GridY = (Mathf.CeilToInt(Mathf.Abs(transform.lossyScale.y) / m_TiledSprite.bounds.size.y) * 2) - m_GridY;
            }

            // Generate Object
            if ((m_GridX <= 0 || m_GridY <= 0) && !AutoTile)
            {
                return false; // No grid
            }

            // Grid count
            int gX = ((AllowGridAxis & TransformAxis2D.XAxis) == TransformAxis2D.XAxis) ? m_GridX : 1;
            int gY = ((AllowGridAxis & TransformAxis2D.YAxis) == TransformAxis2D.YAxis) ? m_GridY : 1;

            for (int y = 0; y < gY; y++)
            {
                int x;
                bool tileUp = y % 2 == 1;

                for (x = 0; x < gX; x++)
                {
                    bool tileRight = x % 2 == 1;

                    SpriteRenderer sRend = new GameObject($"Tile({x}, {y})").AddComponent<SpriteRenderer>();
                    sRend.transform.SetParent(CorrectScaledParent);
                    sRend.sprite = m_TiledSprite;
                    sRend.sortingOrder = m_SortOrder;
                    sRend.transform.localPosition = new Vector3(
                        sRend.bounds.size.x * Mathf.CeilToInt(x / 2f) * (tileRight ? 1f : -1f),
                        sRend.bounds.size.y * Mathf.CeilToInt(y / 2f) * (tileUp ? 1f : -1f)
                    );
                    sRend.transform.localRotation = Quaternion.identity;

                    sRend.transform.localScale = Vector3.one;

                    m_AllRendererObjects.Add(sRend);
                }
            }

            // Set renderable colors
            Color = m_RendererColors;

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

            if (m_AllRendererObjects != null && m_AllRendererObjects.Count > 0)
            {
                m_AllRendererObjects.Clear();
            }
        }
    }
}
