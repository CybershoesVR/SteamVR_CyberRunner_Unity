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
    [SerializeField] bool flight = false;
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
    [SerializeField] float slopeRayHeight;
    [SerializeField] float maxSlopeSteepness;
    [SerializeField] float slopeThreshold = 0.01f;

    //To read the bool from the jump action an input source is needed.
    private SteamVR_Input_Sources jumpSource = SteamVR_Input_Sources.Any;

    private Vector3 externalImpulse; //Used for Boost Pads. Fades away as defined by impulseFade.
    private float impulseTimer;
    private float impulseDuration;

    private Rigidbody rb;
    private Transform colliderTransform;
    private bool isGrounded = false;
    private bool isJumping = false;

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliderTransform = playerCollider.transform;

        if (flight)
        {
            rb.useGravity = false;
        }
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
        Vector3 newVelocity;

        if (!flight)
        {
            newVelocity = ((hmdDirectionForward * forward + hmdDirectionRight * sideways) * moveSpeed) + externalImpulse;
            newVelocity.y = rb.velocity.y + externalImpulse.y;
        }
        else
        {
            newVelocity = ((hmdCamera.forward * forward + hmdDirectionRight * sideways) * moveSpeed) + externalImpulse;
        }

        if (SlopesFlat(transform.position, new Vector3(newVelocity.x, 0, newVelocity.z), 10f)) // filter the y out, so it only checks forward... could get messy with the cosine otherwise.
        {
            rb.velocity = newVelocity;
        }

        //Vector Rotation Table (For Reference)
        #region
        //(0  ,0,1  ) <> (0  ,1)   = (0  ,0,1  )
        //(0.5,0,0.5) <> (0  ,1)   = (0.5,0,0.5)
        //(0.5,0,0.5) <> (0.5,0,5) = (1  ,0,0  )
        #endregion

        if (externalImpulse.magnitude > 0)
        {
            impulseTimer += Time.fixedDeltaTime / impulseDuration;
            externalImpulse = Vector3.Lerp(externalImpulse, Vector3.zero, impulseTimer);
        }
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

    bool SlopesFlat(Vector3 pos, Vector3 targetDir, float distance)
    {
        Ray slopeRay = new Ray(pos, targetDir); // cast a Ray from the position of our gameObject into our desired direction. Add the slopeRayHeight to the Y parameter.

        RaycastHit hit;

        if (Physics.Raycast(slopeRay, out hit, distance, groundMask))
        {
            float slopeAngle = Mathf.Deg2Rad * Vector3.Angle(Vector3.up, hit.normal); // Here we get the angle between the Up Vector and the normal of the wall we are checking against: 90 for straight up walls, 0 for flat ground.

            float radius = Mathf.Abs(slopeRayHeight / Mathf.Sin(slopeAngle)); // slopeRayHeight is the Y offset from the ground you wish to cast your ray from.

            if (slopeAngle >= maxSlopeSteepness * Mathf.Deg2Rad) //You can set "maxSlopeSteepness" to any angle you wish.
            {
                if (hit.distance - playerCollider.radius > Mathf.Abs(Mathf.Cos(slopeAngle) * radius) + slopeThreshold) // Magical Cosine. This is how we find out how near we are to the slope / if we are standing on the slope. as we are casting from the center of the collider we have to remove the collider radius.
                                                                                                                 // The slopeThreshold helps kills some bugs. ( e.g. cosine being 0 at 90° walls) 0.01 was a good number for me here
                {
                    return true; // return true if we are still far away from the slope
                }

                return false; // return false if we are very near / on the slope && the slope is steep
            }

            return true; // return true if the slope is not steep
        }

        return true; //Return True if no collision is imminent at all
    }

    public void SpeedBoost(Vector3 force, float duration)
    {
        externalImpulse += force;
        impulseTimer = 0;
        impulseDuration = duration;
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
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
