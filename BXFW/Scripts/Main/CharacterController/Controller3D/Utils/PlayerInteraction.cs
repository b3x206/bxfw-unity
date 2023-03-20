using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Enables interaction between player and generic interface implementing components / objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        public CustomInputEvent interactionInput = new KeyCode[] { KeyCode.E };
        public Vector3 interactionPointOffset;
        public Vector3 interactionBoxSize = new Vector3(1.2f, 1.2f, 1.2f);
        public LayerMask interactionLayer;
        public bool canInteract = true;

        public Vector3 InteractionPoint
        {
            get
            {
                return transform.position + transform.TransformDirection(interactionPointOffset);
            }
        }

        private void Update()
        {
            if (!canInteract) return;

            if (interactionInput)
            {
                Collider[] playerEnvInteractables =
                    Physics.OverlapBox(
                        center: transform.InverseTransformPoint(InteractionPoint),
                        halfExtents: interactionBoxSize,
                        orientation: transform.rotation,
                        interactionLayer,
                        QueryTriggerInteraction.Collide
                    );

                for (int i = 0; i < playerEnvInteractables.Length; i++)
                {
                    if (playerEnvInteractables[i].TryGetComponent(out IPlayerInteractable pInteract))
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
            Vector3 intPoint = transform.InverseTransformPoint(InteractionPoint);
            Matrix4x4 trMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = trMatrix;
            Gizmos.color = canInteract ? new Color(.01f, .87f, .98f, .5f) : new Color(.4f, .4f, .4f, .5f);

            // Position is correct
            // (using InverseTransformPosition, because the gizmo matrix also changes position + scale)
            Gizmos.DrawCube(intPoint, interactionBoxSize);

            Gizmos.matrix = prevMatrix;
            Gizmos.color = prevColor;
        }
#endif
    }
}
