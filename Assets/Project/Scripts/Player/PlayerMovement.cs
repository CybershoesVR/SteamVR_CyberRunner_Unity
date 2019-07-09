using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerMovement : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] float moveSpeed; //Keep the values low because of nausea. Less than 10 is optimal.
    [SerializeField] float jumpForce; //Needs to be high (~500) depending on desired jump height.
    //[SerializeField] float impulseFade = 1; //Time an impulse needs to fade away
    [Space]
    [SerializeField] CapsuleCollider playerCollider;
    [Space]
    [SerializeField] Transform hmdCamera; //Reference to the Headset Transform
    [SerializeField] SteamVR_Action_Vector2 moveAction;
    [SerializeField] SteamVR_Action_Boolean jumpAction;
    [Space]
    //Used for locating ground below the player so they can't jump mid-air.
    [SerializeField] Transform groundCheckStart; //A point at the bottom of the player
    [SerializeField] float groundLinecastLength; //The length of the line emitted from groundCheckStart.
    [SerializeField] LayerMask groundMask; //The layers that are recognized as ground.
                                           //IMPORTANT: THE PLAYER OR CHILDREN OF THE OBJECT SHOULD NOT BE ASSIGNED A GROUND LAYER (if they have colliders)

    //To read the bool from the jump action an input source is needed.
    private SteamVR_Input_Sources jumpSource = SteamVR_Input_Sources.Any;

    private Vector3 externalImpulse; //Used for Boost Pads. Fades away as defined by impulseFade.

    private Rigidbody rb;
    private Transform colliderTransform;
    private bool isGrounded = false;
    private bool isJumping = false;

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliderTransform = playerCollider.transform;
    }

    private void Update()
    {
        isGrounded = FindGround();

        //JUMP
        if (jumpAction.GetState(jumpSource) && isGrounded && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce);
        }
    }

    void FixedUpdate()
    {
        //COLLIDER UPDATE

        colliderTransform.position = new Vector3(hmdCamera.position.x, colliderTransform.position.y, hmdCamera.position.z);
        groundCheckStart.position = new Vector3(hmdCamera.position.x, groundCheckStart.position.y, hmdCamera.position.z);
        playerCollider.height = Mathf.Clamp(hmdCamera.localPosition.y, playerCollider.radius, 3);
        playerCollider.center = new Vector3(0, playerCollider.height / 2);

        //MOVEMENT

        float forward = moveAction.axis.y;
        float sideways = moveAction.axis.x;

        //Y is cancelled out here because we don't want any height values.
        Vector3 hmdDirectionForward = hmdCamera.forward - new Vector3(0, hmdCamera.forward.y, 0);
        Vector3 hmdDirectionRight = hmdCamera.right - new Vector3(0, hmdCamera.right.y, 0);

        //Sets the velocity relative to the Headset
        //Y velocity is assigned to itself to not cancel out gravitation.
        Vector3 newVelocity = ((hmdDirectionForward * forward + hmdDirectionRight * sideways) * moveSpeed) + externalImpulse;
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y + externalImpulse.y, newVelocity.z); 

        //Vector Rotation Table (For Reference)
        #region
        //(0  ,0,1  ) <> (0  ,1)   = (0  ,0,1  )
        //(0.5,0,0.5) <> (0  ,1)   = (0.5,0,0.5)
        //(0.5,0,0.5) <> (0.5,0,5) = (1  ,0,0  )
        #endregion
    }

    bool FindGround()
    {
        //Shoot a line down from the bottom of the player and check if it hits ground within the specified distance.
        bool hit = Physics.Linecast(groundCheckStart.position, groundCheckStart.position + (Vector3.down * groundLinecastLength), groundMask);

        //Disable isJumping on impact
        if (hit && isJumping && !jumpAction.GetState(jumpSource))
        {
            isJumping = false;
        }

        return hit;
    }

    public void TemporarySpeedBoost(Vector3 force, float duration)
    {
        externalImpulse += force;
        StartCoroutine(FadeExternalImpulse(force, duration));
    }

    IEnumerator FadeExternalImpulse(Vector3 impulse, float duration)
    {
        yield return new WaitForSeconds(duration);

        externalImpulse -= impulse;
    }

    private void OnDrawGizmosSelected()
    {
        //Draws a line in the editor where FindGround() will check for ground
        if (groundCheckStart != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(groundCheckStart.position, groundCheckStart.position + (Vector3.down * groundLinecastLength));
        }
    }
}
