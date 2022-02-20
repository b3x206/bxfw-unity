using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterControl2DBase : MonoBehaviour
{
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

    [Header("Movement Base Settings")]
    public float moveSpeed = 5f;
    public float jumpSpeed = 5f;

    public bool useGravity = true;
    public bool isMoving => false;

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

    // Internal values
    [SerializeField, HideInInspector] private Rigidbody2D currentRB;

    private void Awake()
    {
        
    }

    private void FixedUpdate()
    {


        // Apply velocity
        m_totalVelocity = (ExternVelocity + GravityVelocity + GetMoveVeloctity()) * Time.fixedDeltaTime;
        currentRB.MovePosition(currentRB.position + m_totalVelocity);
    }

    protected Vector2 GetMoveVeloctity()
    {

        return Vector2.zero;
    }

    protected void JumpPlayer()
    {
        // Zero out the gravity velocity and add the jump force.
    }
}
