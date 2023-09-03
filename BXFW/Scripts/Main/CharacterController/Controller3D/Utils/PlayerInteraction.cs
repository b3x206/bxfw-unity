using UnityEngine;

namespace BXFW
{
    /// <summary>
    /// Enables interaction between player and generic interface implementing <see cref="IPlayerInteractable"/> components / objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        public bool canInteract = true;
        public CustomInputEvent interactionInput = new KeyCode[] { KeyCode.E };
        public Vector3 interactionPointOffset;
        public Vector3 interactionBoxSize = new Vector3(1.2f, 1.2f, 1.2f);
        public LayerMask interactionLayer;
        [SerializeField, Clamp(1, ushort.MaxValue), Tooltip("Maximum amount of physics objects/colliders to interact with at one time.\nUsed for the Physics.OverlapBoxNoAlloc()")]
        private int m_MaxInteractCount = 16;
        /// <summary>
        /// Maximum interaction count for this interaction.
        /// </summary>
        public int MaxInteractCount
        {
            get { return m_MaxInteractCount; }
            set
            {
                m_MaxInteractCount = Mathf.Clamp(value, 1, ushort.MaxValue);

#if UNITY_EDITOR
                // No arrays while not playing
                if (!Application.isPlaying)
                {
                    return;
                }
#endif
                // Create array without original data (who cares)
                overlapBoxInteractables = new Collider[m_MaxInteractCount];
            }
        }
        /// <summary>
        /// The local point of interaction for this <see cref="PlayerInteraction"/>.
        /// </summary>
        public Vector3 InteractionPoint
        {
            get
            {
                return transform.position + transform.TransformDirection(interactionPointOffset);
            }
        }
        private Collider[] overlapBoxInteractables;

        private void Start()
        {
            overlapBoxInteractables = new Collider[MaxInteractCount];
        }

        private void Update()
        {
            if (!canInteract)
                return;

            if (interactionInput)
            {
                Interact();
            }
        }

        /// <summary>
        /// Interacts with <see cref="IPlayerInteractable"/>'s with the parameters on this <see cref="PlayerInteraction"/> component.
        /// <br>By default, called on the <see cref="Update"/> method with the <see cref="interactionInput"/> trigger.</br>
        /// <br>Does nothing if the <see cref="canInteract"/> is <see langword="false"/>.</br>
        /// </summary>
        public void Interact()
        {
            if (!canInteract)
                return;

            int overlapBoxInteractedCount = Physics.OverlapBoxNonAlloc(
                center: transform.InverseTransformPoint(InteractionPoint),
                halfExtents: interactionBoxSize, 
                results: overlapBoxInteractables,
                orientation: transform.rotation,
                interactionLayer,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < overlapBoxInteractedCount; i++)
            {
                if (overlapBoxInteractables[i].TryGetComponent(out IPlayerInteractable pInteract))
                {
                    if (pInteract.AllowPlayerInteraction)
                    {
                        pInteract.OnPlayerInteract(this);
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
