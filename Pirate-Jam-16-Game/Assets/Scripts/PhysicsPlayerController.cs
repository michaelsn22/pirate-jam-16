using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public bool isGrounded;
    public bool isGroundedCenter;
    public Transform groundCheckCenter;
    public float groundDistance = 0.4f;
    public Transform cam;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;
    public float speed = 6f;
    public float burstValueUpwards = 20f;
    public float dashSpeed = 500f;
    private bool canDash = true;

    [SerializeField] private LayerMask groundMask;

    private Vector3 movement;
    private bool jumpRequested = false;
    private bool dashRequested = false;
    private bool isDashing = false;
    private float dashDuration = 0.2f; // Duration of the dash in seconds
    private float dashCooldown = 2f;
    private float currentTime = 0f;
    private float timeSinceLastJump = 0f;
    private float timeOfLastJump = 0f;
    private int jumpCounter = 0;


    void Start()
    {
        //Cache reference to our rigidbody
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        timeSinceLastJump =  currentTime - timeOfLastJump;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Debug.Log("current time is:"+currentTime);
        //Debug.Log("time of last jump is: "+timeOfLastJump);
        //Debug.Log("timeSinceLastJump: "+timeSinceLastJump);
        //Debug.Log("timeSinceLastJump - currentTime = "+temp);

        // Input detection so the player can move.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movement = new Vector3(horizontal, 0f, vertical).normalized;

        //if we are on the ground.... reset the jump counter.
        if (jumpCounter != 0 && CheckGroundDistance() && rb.velocity.y <= -0.5f)
        {
            //Debug.Log("resetting jump counter");
            jumpCounter = 0;
        }

        // Jump input detection (if we are grounded or within tolerance distance, or we have extra jumps available, THEN JUMP!)
        if (Input.GetKeyDown(KeyCode.Space) && CheckGroundDistance() || Input.GetKeyDown(KeyCode.Space) && isGrounded || Input.GetKeyDown(KeyCode.Space) && timeSinceLastJump >= 0.5f && jumpCounter < 3)//isGrounded
        {
            if (jumpCounter >= 3)
            {
                //Debug.Log("not allowing a jump, too mny jumps inputted!");
            }
            else
            {
                jumpCounter++;
                timeOfLastJump = Time.time;
                //Debug.Log("time of last jump is: "+timeOfLastJump);
                jumpRequested = true;
            }
            
        }

        //dash input detection
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            dashRequested = true;
        }

        isGroundedCenter = Physics.CheckSphere(groundCheckCenter.position, groundDistance, groundMask);

        isGrounded = isGroundedCenter;
    }

    void FixedUpdate()
    {
        MovePlayer();
        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
        TryToDash();
    }

    void TryToDash()
    {
        if(!dashRequested)
        {
            return;
        }
        dashRequested = false;
        canDash = false;

        StartCoroutine(DashCoroutine());
    }

    void MovePlayer(float? speedOverride = null)
    {
        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            float targetSpeed = speedOverride ?? speed;
            rb.velocity = new Vector3(moveDir.x * targetSpeed, rb.velocity.y, moveDir.z * targetSpeed);
        }
        else //DECIDE WHETHER TO KEEP MOMENTUM OR NOT!
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    void Jump()
    {
        //Debug.Log("jumping");
        rb.velocity = new Vector3(0, 0f, 0);
        rb.AddForce(transform.up * burstValueUpwards, ForceMode.Impulse);
    }


    //method to check if we are a certain distance from the ground.
    private bool CheckGroundDistance()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheckCenter.position, Vector3.down, out hit))
        {
            float distanceToGround = hit.distance;
            if (distanceToGround <= 0.4f || distanceToGround <= 2f && rb.velocity.y <= -13f)
            {
                //Debug.Log("Distance to ground is below 3f: " + distanceToGround);
                return true;
            }
            else
            {
                return false;
            }
        }
        else{
            return false;
        }
    }

    //debug method
    private void CheckGroundDistance2()
    {
        RaycastHit hit;
        if (Physics.Raycast(groundCheckCenter.position, Vector3.down, out hit))
        {
            float distanceToGround = hit.distance;
            //Debug.Log(distanceToGround);
        }
    }

    IEnumerator DashCoroutine() //Apply force over some duration 'dashDuration' to the player.
    {
        isDashing = true;

        float elapsedTime = 0f;

        //clear the existing velocity for a more controlled dash
        rb.velocity = Vector3.zero;

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.fixedDeltaTime;

            //Apply a portion of the dash force each frame
            rb.AddForce(transform.forward * (dashSpeed / dashDuration) * Time.fixedDeltaTime, ForceMode.VelocityChange);

            //Wait for the next FixedUpdate
            yield return new WaitForFixedUpdate();
        }

        isDashing = false;
        Invoke(nameof(DashCooldownHandler), dashCooldown);
    }

    private void DashCooldownHandler()
    {
        canDash = true;
    }
}
