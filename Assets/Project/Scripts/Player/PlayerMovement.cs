using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerMovement : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] float moveSpeed = 10; //Keep the values low because of nausea.
    [SerializeField] float sprintFactor;
    [SerializeField] float sprintTrigger = 0.7f;
    public float minSpeed;
    public float maxSpeed;
    [SerializeField] float jumpForce; //Needs to be high (~500) depending on desired jump height.
    [SerializeField] float jumpPush = 5;
    [SerializeField] float pushDuration = 1;
    [Space]
    [SerializeField] CapsuleCollider playerCollider;
    [Space]
    [SerializeField] Transform hmdCamera; //Reference to the Headset Transform
    [SerializeField] SteamVR_Action_Vector2 moveAction;
    [SerializeField] SteamVR_Action_Vector2 moveRawLeftAction;
    [SerializeField] SteamVR_Action_Vector2 moveRawRightAction;
    [SerializeField] SteamVR_Action_Boolean jumpAction;
    [SerializeField] SteamVR_Input_Sources treadmillSource = SteamVR_Input_Sources.Treadmill;
    [SerializeField] SteamVR_Input_Sources handSource = SteamVR_Input_Sources.LeftHand;
    [Space]
    //Used for locating ground below the player so they can't jump mid-air.
    [SerializeField] Transform groundCheckStart; //A point at the bottom of the player
    [SerializeField] float groundLinecastLength; //The length of the line emitted from groundCheckStart.
    [SerializeField] LayerMask groundMask; //The layers that are recognized as ground.
                                           //IMPORTANT: THE PLAYER OR CHILDREN OF THE OBJECT SHOULD NOT BE ASSIGNED A GROUND LAYER (if they have colliders)
    [SerializeField] AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
    //[SerializeField] float slopeRayHeight;
    //[SerializeField] float maxSlopeSteepness;
    //[SerializeField] float slopeThreshold = 0.01f;
    [Space]
    [SerializeField] private SteamVR_Input_Sources currentInputSource;
    [Space]
    [SerializeField] AudioSource leftFootAudioSource;
    [SerializeField] AudioSource rightFootAudioSource;

    private AudioSource jumpAudioSource;
    private bool rightFoot = true;

    private Vector3 externalImpulse; //Used for Boost Pads. Fades away as defined by impulseFade.
    private float impulseTimer;
    private float impulseDuration;

    private Vector3 groundContactNormal;
    private float currentTargetSpeed;

    private Rigidbody rb;
    private Transform colliderTransform;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool isJumping = false;

    private bool leftFootPlayed = false;
    private bool rightFootPlayed = false;

    private float rawActionTimer = 11;
    private bool rawActionsEnabled = false;

    #endregion

    void Start()
    {
        //currentSource = handSource;

        rb = GetComponent<Rigidbody>();
        jumpAudioSource = GetComponent<AudioSource>();
        colliderTransform = playerCollider.transform;
    }

    private void Update()
    {
        FindGround();

        //JUMP
        if (jumpAction.GetState(currentInputSource) && isGrounded && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //SpeedBoost((hmdCamera.forward - new Vector3(0,hmdCamera.forward.y,0)) * jumpPush, pushDuration);

            if (rb.constraints == RigidbodyConstraints.FreezeRotation)
            {
                jumpAudioSource.Play();
            }
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

        //Step Sounds
        if (isGrounded && rawActionsEnabled)
        {
            if (moveRawLeftAction.axis.magnitude > 0.7f && !leftFootAudioSource.isPlaying && !leftFootPlayed)
            {
                //Debug.Log("Playing Left Foot: " + moveRawLeftAction.axis.magnitude);

                leftFootAudioSource.Play();
                leftFootPlayed = true;
            }
            else if (moveRawLeftAction.axis.magnitude < 0.3f)
            {
                leftFootPlayed = false;
            }

            if (moveRawRightAction.axis.magnitude > 0.7f && !rightFootAudioSource.isPlaying && !rightFootPlayed)
            {
                //Debug.Log("Right Raw: " + moveRawRightAction.axis.magnitude);

                rightFootAudioSource.Play();
                rightFootPlayed = true;
            }
            else if (moveRawRightAction.axis.magnitude < 0.3f)
            {
                rightFootPlayed = false;
            }
        }


        float forwardInput = moveAction.GetAxis(currentInputSource).y;
        float sidewaysInput = moveAction.GetAxis(currentInputSource).x;

        float sprintMultiplier = 1;

        if (moveAction.axis.magnitude >= sprintTrigger)
        {
            sprintMultiplier = sprintFactor;
        }

        currentTargetSpeed = moveSpeed * sprintMultiplier;


        //Y is cancelled out here because we don't want any height values.
        //Vector3 hmdDirectionForward = hmdCamera.forward - new Vector3(0, hmdCamera.forward.y, 0);
        //Vector3 hmdDirectionRight = hmdCamera.right - new Vector3(0, hmdCamera.right.y, 0);

        //Sets the velocity relative to the Headset
        //Y velocity is assigned to itself to not cancel out gravitation.

        Vector3 newVelocity = hmdCamera.forward * forwardInput + hmdCamera.right * sidewaysInput;// * (moveSpeed * sprintMultiplier)) + externalImpulse;
        newVelocity = Vector3.ProjectOnPlane(newVelocity, groundContactNormal).normalized;
        //newVelocity.y = rb.velocity.y + externalImpulse.y;
        newVelocity *= (moveSpeed * sprintMultiplier);

        if (rb.velocity.magnitude < currentTargetSpeed)
        {
            rb.AddForce(newVelocity * SlopeMultiplier(), ForceMode.Impulse);
        }

        if (isGrounded)
        {
            rb.drag = 10;
        }
        else
        {
            rb.drag = 2;

            if (!isJumping && wasGrounded)
            {
                StickToGroundHelper();
            }
        }

        //if (SlopesFlat(groundCheckStart.position, new Vector3(newVelocity.x, 0, newVelocity.z), 10f)) // filter the y out, so it only checks forward... could get messy with the cosine otherwise.
        //{
        //    rb.velocity = newVelocity;
        //}

        if (externalImpulse.magnitude > float.Epsilon)
        {
            rb.AddForce(externalImpulse, ForceMode.Impulse);
            impulseTimer += Time.fixedDeltaTime / impulseDuration;
            externalImpulse = Vector3.Lerp(externalImpulse, Vector3.zero, impulseTimer);
        }
    }

    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, playerCollider.radius * (1.0f - 0.1f), Vector3.down, out hitInfo,
                               ((playerCollider.height / 2f) - playerCollider.radius) +
                               0.5f, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rb.velocity = Vector3.ProjectOnPlane(rb.velocity, hitInfo.normal);
            }
        }
    }

    void FindGround()
    {
        wasGrounded = isGrounded;

        RaycastHit hitInfo;

        //Shoot a line down from the bottom of the player and check if it hits ground within the specified distance.
        bool hit = Physics.Linecast(groundCheckStart.position, groundCheckStart.position + (Vector3.down * groundLinecastLength), out hitInfo, groundMask);

        if (hit)
        {
            isGrounded = true;
            groundContactNormal = hitInfo.normal;
        }
        else
        {
            isGrounded = false;
            groundContactNormal = Vector3.up;
        }

        //Disable isJumping on impact
        if (hit && isJumping && !jumpAction.GetState(currentInputSource))
        {
            isJumping = false;
        }
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(groundContactNormal, Vector3.up);
        return slopeCurveModifier.Evaluate(angle);
    }

    //bool SlopesFlat(Vector3 pos, Vector3 targetDir, float distance)
    //{
    //    Ray slopeRay = new Ray(pos, targetDir); // cast a Ray from the position of our gameObject into our desired direction. Add the slopeRayHeight to the Y parameter.

    //    RaycastHit hit;

    //    if (Physics.Raycast(slopeRay, out hit, distance, groundMask))
    //    {
    //        float slopeAngle = Mathf.Deg2Rad * Vector3.Angle(Vector3.up, hit.normal); // Here we get the angle between the Up Vector and the normal of the wall we are checking against: 90 for straight up walls, 0 for flat ground.

    //        float radius = Mathf.Abs(slopeRayHeight / Mathf.Sin(slopeAngle)); // slopeRayHeight is the Y offset from the ground you wish to cast your ray from.

    //        if (slopeAngle >= maxSlopeSteepness * Mathf.Deg2Rad) //You can set "maxSlopeSteepness" to any angle you wish.
    //        {
    //            if (hit.distance - playerCollider.radius > Mathf.Abs(Mathf.Cos(slopeAngle) * radius) + slopeThreshold) // Magical Cosine. This is how we find out how near we are to the slope / if we are standing on the slope. as we are casting from the center of the collider we have to remove the collider radius.
    //                                                                                                             // The slopeThreshold helps kills some bugs. ( e.g. cosine being 0 at 90° walls) 0.01 was a good number for me here
    //            {
    //                return true; // return true if we are still far away from the slope
    //            }

    //            return false; // return false if we are very near / on the slope && the slope is steep
    //        }

    //        return true; // return true if the slope is not steep
    //    }

    //    return true; //Return True if no collision is imminent at all
    //}

    public void SpeedBoost(Vector3 force, float duration)
    {
        externalImpulse += force;
        impulseTimer = 0;
        impulseDuration = duration;
    }

    public float SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        return moveSpeed;
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    public float AddSpeed(float amount)
    {
        float newSpeed = moveSpeed + amount;

        if (newSpeed < minSpeed)
        {
            newSpeed = minSpeed;
        }
        else if (newSpeed > maxSpeed)
        {
            newSpeed = maxSpeed;
        }

        moveSpeed = newSpeed;

        return moveSpeed;
    }

    public void SwitchController(bool cybershoesActive)
    {
        if (cybershoesActive)
        {
            currentInputSource = treadmillSource;
        }
        else
        {
            currentInputSource = handSource;
        }
    }

    IEnumerator WaitForRawActions()
    {
        yield return new WaitForSeconds(rawActionTimer);
        rawActionsEnabled = true;
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
