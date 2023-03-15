using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// The parallax background sprite renderer group object.
    /// </summary>
    [RequireComponent(typeof(TilingSpriteRenderer))]
    public class ParallaxBackgroundObj : MonoBehaviour
    {
        private float Length;
        private Vector2 StartPos;
        public float ParallaxEffectAmount;
        public ParallaxBackgroundGroup ParentGroup;

        public TilingSpriteRenderer TilingSpriteRendererComponent
        { get { return tilingSpriteRendComp; } private set { tilingSpriteRendComp = value; } }
        [SerializeField] private TilingSpriteRenderer tilingSpriteRendComp;

        public void InitilazeTilingSpriteRenderer(Sprite rendSprite)
        {
            if (TilingSpriteRendererComponent == null)
            {
                TilingSpriteRendererComponent = GetComponent<TilingSpriteRenderer>();
            }
            if (rendSprite == null)
            {
                Debug.LogError("[ParallaxBackgroundObj] Null sprite was passed.");
                return;
            }

            TilingSpriteRendererComponent.TiledSprite = rendSprite;
            TilingSpriteRendererComponent.AllowGridAxis = TransformAxis2D.XAxis;
            TilingSpriteRendererComponent.ResizeTargetCamera = ParentGroup.TargetCamera;
            TilingSpriteRendererComponent.CameraResize = true;
            TilingSpriteRendererComponent.AutoTile = true;
            //TilingSpriteRendererComponent.ResizeTformSetMultiplier = 3f;
        }

        private void Start()
        {
            if (ParentGroup.TargetCamera == null)
            {
                Debug.LogError($"[ParallaxBackground] A parent with name \"{ParentGroup.name}\" doesn't have a target camera assigned.");
            }

            StartPos.x = transform.position.x;
            StartPos.y = transform.position.y;
            Length = TilingSpriteRendererComponent.SingleBounds.size.x;
        }

        private void Update()
        {
            Scroll();
        }

        public void Scroll()
        {
            float Temp =
                ParentGroup.TargetCamera.transform.position.x *
                (1 - ParallaxEffectAmount);
            float Dist =
                ParentGroup.TargetCamera.transform.position.x *
                ParallaxEffectAmount;

            // Follow the camera position in Y too
            //float yTemp = ParentGroup.TargetCamera.transform.position.y *
            //    (1 - ParallaxEffectAmount);
            // Note that we don't need to restart / tile on Y parallax.
            float yDist = ParentGroup.TargetCamera.transform.position.y *
                ParallaxEffectAmount;

            transform.position = new Vector3(StartPos.x + Dist, StartPos.y + yDist, transform.position.z);

            if (Temp > StartPos.x + Length) StartPos.x += Length;
            else if (Temp < StartPos.x - Length) StartPos.x -= Length;
        }
    }
}
