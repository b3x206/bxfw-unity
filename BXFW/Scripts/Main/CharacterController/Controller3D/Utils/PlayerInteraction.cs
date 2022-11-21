using UnityEngine;
using BXFW;

namespace BXFW
{
    /// <summary>
    /// Enables interaction between player and generic interface implementing components / objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        public CustomInputEvent InteractInput = new KeyCode[] { KeyCode.E };
        public Vector3 Player_InteractionPointOffset;
        public Vector3 Player_InteractionRadius = new Vector3(1.2f, 1.2f, 1.2f);
        public LayerMask Player_InteractionLayer;
        public bool Player_CanInteract = true;

        public Vector3 Player_InteractionPoint
        {
            get
            {
                return transform.position + /*transform.forward +*/ transform.TransformDirection(Player_InteractionPointOffset);
            }
        }

        private void Update()
        {
            if (!Player_CanInteract) return;

            if (InteractInput)
            {
                // Invert Interaction layer.
                // Player_InteractionLayerIgnore = ~(Player_InteractionLayerIgnore << 9);

                // Steps to interact in a tps context:
                // 1 : Create a check sphere,
                // 2 : Loop through objects to find a interface.
                // 3 : Invoke 
                var Player_EnvInteract =
                    Physics.OverlapBox(Player_InteractionPoint,
                        Player_InteractionRadius,
                        transform.rotation,
                        Player_InteractionLayer,
                        QueryTriggerInteraction.Collide);

                for (int i = 0; i < Player_EnvInteract.Length; i++)
                {
                    if (Player_EnvInteract[i].TryGetComponent(out IPlayerInteractable pInteract))
                    {
                        if (pInteract.AllowPlayerInteraction)
                        {
                            pInteract.OnPlayerInteract(this);
                        }
                    }
                }

                // TODO : Seperate fps & tps, this raycast is complete except for the error overlap sphere margin

                // Steps to interact in a fps context:
                // 1 : Create a raycast,
                // 2 : Find a interface on raycasted object (We might use spherecast on raycast point to add error margin).
                // 3 : Invoke 
                // or we could use the Vector3.Dot with tolerance, but that requires reference to all interactables
                //if (Physics.Raycast(Player_Movement.Player_Camera.transform.position, Player_Movement.Player_Camera.transform.forward, out RaycastHit h, Player_InteractionRadius.magnitude, Player_InteractionLayer))
                //{
                //    if (h.transform.TryGetComponent(out IPlayerInteractable pInteract))
                //    {
                //        if (pInteract.AllowPlayerInteraction)
                //        {
                //            pInteract.OnPlayerInteract(this);
                //        }
                //    }
                //}
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Color c = new Color(.01f, .87f, .98f, .5f); // some blue
            var prevMatrix = Gizmos.matrix;
            var prevColor = Gizmos.color;

            // Apply parent matrix to get rotation too
            Vector3 intPoint = transform.InverseTransformPoint(Player_InteractionPoint);
            Matrix4x4 trMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = trMatrix;
            Gizmos.color = c;

            // Position is correct
            // (using InverseTransformPosition, because the gizmo matrix also changes position + scale)
            Gizmos.DrawCube(intPoint, Player_InteractionRadius);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }
#endif
    }
}
