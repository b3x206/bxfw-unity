using UnityEngine;

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
                return transform.position + transform.TransformDirection(Player_InteractionPointOffset);
            }
        }

        private void Update()
        {
            if (!Player_CanInteract) return;

            if (InteractInput)
            {
                // For the time being, use the tps method as it will
                // work fine with the PlayerFPSCamera class we have
                Collider[] Player_EnvInteract =
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
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // The shown gizmo draws the desired interaction bounds
            // If the interaction logic on 'Update' doesn't work as intended
            // update with the correct behaviour (OnDrawGizmosSelected OverlapBox)
            var prevMatrix = Gizmos.matrix;
            var prevColor = Gizmos.color;

            // Apply parent matrix to get rotation too
            Vector3 intPoint = transform.InverseTransformPoint(Player_InteractionPoint);
            Matrix4x4 trMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = trMatrix;
            Gizmos.color = new Color(.01f, .87f, .98f, .5f);  // some nice blue

            // Position is correct
            // (using InverseTransformPosition, because the gizmo matrix also changes position + scale)
            Gizmos.DrawCube(intPoint, Player_InteractionRadius);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }
#endif
    }
}
