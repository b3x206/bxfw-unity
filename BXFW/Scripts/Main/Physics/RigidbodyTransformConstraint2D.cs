using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    /// Works by simply registring rigidbodies to this moving transform.
    /// <summary>
    /// Constraints any rigidbody entering it's space to that transform.
    /// <br>This allows the rigidbody to freely move, while still moving with that transform.</br>
    /// <br/>
    /// <br>This class is kinda pointless and does not work as advertised. TODO maybe fix it?</br>
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RigidbodyTransformConstraint2D : MonoBehaviour
    {
        public Collider2D Collider { get; private set; }
        /// <summary>
        /// Initial body capacity.
        /// </summary>
        private const int BODY_CAPACITY = 8;
        private readonly Dictionary<Rigidbody2D, Transform> bodiesEntered = new Dictionary<Rigidbody2D, Transform>(BODY_CAPACITY);
        
        private void Awake()
        {
            Collider = GetComponent<Collider2D>();
            Collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out Rigidbody2D rb))
            {
                return;
            }

            // This body already exists.
            if (bodiesEntered.ContainsKey(rb))
            {
                return;
            }

            bodiesEntered.Add(rb, rb.transform.parent);
            rb.transform.SetParent(transform);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out Rigidbody2D rb))
            {
                return;
            }

            rb.transform.SetParent(bodiesEntered[rb]);
            bodiesEntered.Remove(rb);
        }
    }
}