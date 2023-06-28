﻿using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Represents a scrollable by camera movement, parallaxed <see cref="TilingSpriteRenderer"/>.
    /// </summary>
    [RequireComponent(typeof(TilingSpriteRenderer))]
    public class ParallaxBackgroundObj : MonoBehaviour
    {
        private float Length;
        private Vector2 StartPos;
        public float ParallaxEffectAmount;
        public ParallaxBackgroundGroup ParentGroup;

        public TilingSpriteRenderer TilingSpriteRendererComponent
        { 
            get { return tilingSpriteRendComp; } 
        }
        [SerializeField] private TilingSpriteRenderer tilingSpriteRendComp;

        public void InitilazeTilingSpriteRenderer(Sprite rendSprite)
        {
            if (!TryGetComponent(out tilingSpriteRendComp))
            {
                tilingSpriteRendComp = gameObject.AddComponent<TilingSpriteRenderer>();
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
            Vector3 positionDelta = Vector3.zero;
            if ((ParentGroup.ScrollAxis & TransformAxis2D.XAxis) == TransformAxis2D.XAxis)
            {
                float Temp =
                    ParentGroup.TargetCamera.transform.position.x *
                    (1 - ParallaxEffectAmount);
                float Dist =
                    ParentGroup.TargetCamera.transform.position.x *
                    ParallaxEffectAmount;

                positionDelta.x += Dist;

                if (Temp > StartPos.x + Length) StartPos.x += Length;
                else if (Temp < StartPos.x - Length) StartPos.x -= Length;
            }
            if ((ParentGroup.ScrollAxis & TransformAxis2D.YAxis) == TransformAxis2D.YAxis)
            {
                // Follow the camera position in Y too
                //float yTemp = ParentGroup.TargetCamera.transform.position.y *
                //    (1 - ParallaxEffectAmount);
                // Note that we don't need to restart / tile on Y parallax, so the yTemp can be commented
                // Both axis parallax works the same.
                float yDist = ParentGroup.TargetCamera.transform.position.y *
                    ParallaxEffectAmount;

                positionDelta.y += yDist;
            }

            transform.position = new Vector3(StartPos.x + positionDelta.x, StartPos.y + positionDelta.y, transform.position.z);
        }
    }
}