using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BXFW
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterControl2DBase : MonoBehaviour
    {
        [Header("Movement Base Settings")]
        public float moveSpeed = 5f;
        public float jumpSpeed = 5f;
        public TransformAxis2D moveAxis = TransformAxis2D.XYAxis; // Axis that we are allowed to move.
        public TransformAxis2D jumpAxis = TransformAxis2D.YAxis;  // Axis to apply the jump force to.

        public Vector2 gravity = Physics2D.gravity;
        public bool useGravity = true;

        // Runtime variables
        public bool isMoving => false;
        /// <summary>
        /// Whether if we are moving using input.
        /// <br>Set by <see cref="GetMoveVelocity"/>.</br>
        /// </summary>
        public bool isMovingInput { get; private set; }
        public Vector2 Velocity
        {
            get
            {
                return m_totalVelocity;
            }
            set
            {
                ExternVelocity = value;
            }
        }
        /// <summary>
        /// Total applied velocity.
        /// <br>Returns the all of the currently applied velocities.</br>
        /// </summary>
        private Vector2 m_totalVelocity;
        /// <summary>
        /// Externally applied velocity. (i.e <see cref="Velocity"/>'s setter setts this value.)
        /// </summary>
        public Vector2 ExternVelocity { get; private set; }
        public Vector2 GravityVelocity { get; private set; }
        public Rigidbody2D CharacterRB
        {
            get
            {
                if (currentRB == null)
                    currentRB = GetComponent<Rigidbody2D>();

                // Apply / Verify Rigidbody settings.
                currentRB.isKinematic = true;
                currentRB.sleepMode = RigidbodySleepMode2D.NeverSleep;

                return currentRB;
            }
        }

        // Internal values
        [SerializeField, HideInInspector] private Rigidbody2D currentRB;

        private void Awake()
        {

        }

        private void FixedUpdate()
        {


            // Apply velocity
            m_totalVelocity = (ExternVelocity + GravityVelocity + GetMoveVelocity()) * Time.fixedDeltaTime;
            currentRB.MovePosition(currentRB.position + m_totalVelocity);
        }

        protected Vector2 GetMoveVelocity()
        {

            return Vector2.zero;
        }

        protected void JumpPlayer()
        {
            // Zero out the gravity velocity and add the jump force.
            GravityVelocity = Vector3.zero;

            GravityVelocity = jumpAxis.GetVectorFromTransformAxis(Vector3.zero, new Vector2(jumpSpeed, jumpSpeed));
        }
    }
}