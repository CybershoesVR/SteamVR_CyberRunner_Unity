using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerMovement : MonoBehaviour
{
    //Variables
    #region

    [SerializeField] float moveSpeed;
    [SerializeField] float sprintFactor; //The Multiplier when sprinting
    [SerializeField] float sprintTrigger; //The Input Magnitude at which sprinting is triggered
    [SerializeField] float jumpForce;
    [Space]
    [SerializeField] CapsuleCollider playerCollider; //Has to be in a child of the player because of scaling, etc.
    [SerializeField] Transform hmdCamera; //Headset Reference
    [Space]
    [SerializeField] SteamVR_Action_Vector2 moveAction;
    [SerializeField] SteamVR_Action_Vector2 moveRawLeftAction;
    [SerializeField] SteamVR_Action_Vector2 moveRawRightAction;
    [SerializeField] SteamVR_Action_Boolean jumpAction;
    [Space]
    [SerializeField] SteamVR_Input_Sources treadmillSource = SteamVR_Input_Sources.Treadmill;
    [SerializeField] SteamVR_Input_Sources handSource = SteamVR_Input_Sources.LeftHand;
    [Space]
    [SerializeField] Transform groundCheckStart; //A point at the bottom of the player
    [SerializeField] float groundLinecastLength; //The length of the line emitted from groundCheckStart.
    [SerializeField] LayerMask groundMask; //The layers that are recognized as ground.
    [SerializeField] AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f)); //X-Axis = Angle | Y-Axis = Deceleration Multiplier
    [Space]
    [SerializeField] private SteamVR_Input_Sources currentInputSource; //Movement Source. Switches between either Cybershoes or Left Hand
    [Space]
    [SerializeField] AudioSource leftFootAudioSource;
    [SerializeField] AudioSource rightFootAudioSource;
    [SerializeField] float stepSoundPlayTreshold = 0.7f;
    [SerializeField] float stepSoundResetTreshold = 0.4f;

    [HideInInspector]
    public int collectedBoosters = 0; //Used for achievement. Only counts per Run.

    private AudioSource jumpAudioSource;

    private Vector3 externalImpulse; //Used to apply an impulse from a booster
    private float impulseTimer; //Tracks the actual current duration of the impulse
    private float impulseDuration; //Duration of the current Impulse

    private Vector3 groundContactNormal;
    private float currentTargetSpeed;

    private Rigidbody rb;
    private Transform colliderTransform;

    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool isJumping = false;

    private bool leftStepPlayed = false;
    private bool rightStepPlayed = false;

    private float rawActionTimer = 11; //A timeout before step sounds are enabled.
                                       //The Raw Left/Right Trackpads in the Driver are only accessible after 11 seconds because of compatibility issues.
    private bool rawActionsEnabled = false; //Wheter the Raw Trackpads can be read yet.

    private float airTime = 0; //Used for achievement.

    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        jumpAudioSource = GetComponent<AudioSource>();

        colliderTransform = playerCollider.transform;

        StartCoroutine(WaitForRawActions());
    }

    private void Update()
    {
        FindGround(); //Updates isGrounded

        //JUMP
        if (jumpAction.GetState(currentInputSource) && isGrounded && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (rb.constraints == RigidbodyConstraints.FreezeRotation)
            {
                jumpAudioSource.Play(); //Don't play Jump sound if player can't move.
            }
        }
    }

    void FixedUpdate()
    {
        //COLLIDER UPDATE (Fit collider to player position)

        //Adjust the collider position to the x & z of the headset
        colliderTransform.position = new Vector3(hmdCamera.position.x, colliderTransform.position.y, hmdCamera.position.z);
        //Adjust the ground check position to the x & z of the headset
        groundCheckStart.position = new Vector3(hmdCamera.position.x, groundCheckStart.position.y, hmdCamera.position.z);
        //Adjust the collider height to the headset
        playerCollider.height = Mathf.Clamp(hmdCamera.localPosition.y, playerCollider.radius, 3);
        //Adjust the collider center to half it's height for an accurate position
        playerCollider.center = new Vector3(0, playerCollider.height / 2);

        //STEP SOUNDS

        if (isGrounded && rawActionsEnabled)
        {
            if (!rightFootAudioSource.isPlaying)
            {
                if (moveRawRightAction.axis.magnitude > stepSoundPlayTreshold && !rightStepPlayed)
                {
                    rightFootAudioSource.Play();
                    rightStepPlayed = true;
                }
                else if (moveRawRightAction.axis.magnitude < stepSoundResetTreshold)
                {
                    rightStepPlayed = false;
                }
            }
            
            if (!leftFootAudioSource.isPlaying)
            {
                if (moveRawLeftAction.axis.magnitude > stepSoundPlayTreshold && !leftStepPlayed)
                {
                    leftFootAudioSource.Play();
                    leftStepPlayed = true;
                }
                else if (moveRawLeftAction.axis.magnitude < stepSoundResetTreshold)
                {
                    leftStepPlayed = false;
                }
            }
        }

        //MOVEMENT

        float forwardInput = moveAction.GetAxis(currentInputSource).y;
        float sidewaysInput = moveAction.GetAxis(currentInputSource).x;

        //Sprint Check
        float sprintMultiplier = 1;
        if (moveAction.axis.magnitude >= sprintTrigger)
        {
            sprintMultiplier = sprintFactor;
        }

        currentTargetSpeed = moveSpeed * sprintMultiplier;


        //Y is cancelled out here because we don't want any height values.
        Vector3 hmdDirectionForward = hmdCamera.forward - new Vector3(0, hmdCamera.forward.y, 0);
        Vector3 hmdDirectionRight = hmdCamera.right - new Vector3(0, hmdCamera.right.y, 0);

        //Calculate the new Velocity
        Vector3 newVelocity = hmdDirectionForward * forwardInput + hmdDirectionRight * sidewaysInput;
        newVelocity *= (moveSpeed * sprintMultiplier) * SlopeMultiplier();
        newVelocity.y = rb.velocity.y;
        Debug.Log(newVelocity.magnitude);
        rb.velocity = newVelocity;

        //Increase/Decrease drag on floor/air

        if (isGrounded)
        {
            rb.drag = 10;
        }
        else
        {
            rb.drag = 0;

            if (wasGrounded && !isJumping)
            {
                StickToGround();
            }
        }

        //Impulse

        if (externalImpulse.magnitude > float.Epsilon)
        {
            rb.velocity += externalImpulse; //Add current impulse
            impulseTimer += Time.fixedDeltaTime / impulseDuration; //Count up the timer
            externalImpulse = Vector3.Lerp(externalImpulse, Vector3.zero, impulseTimer); //Lerp the impulse towards 0
        }
    }

    private void StickToGround()
    {
        //If the player is walking on a slope less that 85 degrees the velocity is readjusted along the ground.

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

            if (!wasGrounded)
            {
                airTime = 0;
            }
        }
        else
        {
            isGrounded = false;
            groundContactNormal = Vector3.up;

            airTime += Time.deltaTime;

            if (airTime >= 3f)
            {
                AchievementManager.SetAchievement("achievement_06");
            }
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

    public void SpeedBoost(Vector3 force, float duration)
    {
        externalImpulse += force;
        impulseTimer = 0;
        impulseDuration = duration;

        collectedBoosters++;
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
