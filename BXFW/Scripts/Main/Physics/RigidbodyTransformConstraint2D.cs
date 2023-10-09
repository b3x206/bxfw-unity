using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// Works by simply registring rigidbodies to this moving transform.
    /// <summary>
    /// Constraints any rigidbody entering it's space to that transform.
    /// <br>This allows the rigidbody to freely move, while still moving with that transform.</br>
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RigidbodyTransformConstraint2D : MonoBehaviour
    {
        public Collider2D Collider { get; private set; }
        /// <summary>
        /// Initial body capacity.
        /// </summary>
        private const int BODY_CAPACITY = 64;
        private readonly Dictionary<Rigidbody2D, Transform> m_bodiesEntered = new Dictionary<Rigidbody2D, Transform>(BODY_CAPACITY);
        
        private void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Collider.isTrigger = true;
        }

        // Note + TODO : These events won't reliably detect bodies entering or not
        // To fix this there are a few approaches :
        // A : GameObject.FindObjectsOfType<Rigidbody2D>(), but this is VERY inefficient
        // B : Register Rigidbodies+Colliders that will interact with this
        // .. The interaction will be polled in Update, because this class tries to prevent input based physics mistakes
        // .. (i.e phasing through something that the player moves)
        // C : idk find better methods
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out Rigidbody2D rb))
                return;

            // This body already exists.
            if (m_bodiesEntered.ContainsKey(rb))
                return;

            m_bodiesEntered.Add(rb, rb.transform.parent);
            rb.transform.SetParent(transform);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out Rigidbody2D rb))
                return;

            rb.transform.SetParent(m_bodiesEntered[rb]);
            m_bodiesEntered.Remove(rb);
        }
    }
}