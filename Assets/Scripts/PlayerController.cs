using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float speed;
    public float walkSpeed;
    public float sprintSpeed;


    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header ("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header ("Slope Handler")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;


    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask whatIsGround;
    bool grounded;


    Rigidbody rb;

    public Transform orientation;

    float xInput;
    float yInput;

    Vector3 moveDirection;

    public MovementState state;
    public enum MovementState{
        walking,
        sprinting,
        crouching,
        air
    }


    void Start(){
         rb = GetComponent<Rigidbody>();
         rb.freezeRotation = true;
         readyToJump = true;

         startYScale = transform.localScale.y;

    }

    void Update(){

        // Check ground using sphere at the bottom of character model
        grounded = Physics.CheckSphere(groundCheck.position, groundDistance, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // Handle drag for when player is on the ground
        if (grounded){
            rb.linearDamping = groundDrag;
        }
        else{
            rb.linearDamping = 0;
        }

    }

    private void FixedUpdate(){
        MovePlayer();
    }

    private void MyInput(){
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");


        // When to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Crouching
        if(Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // Stop Crouching
        if(Input.GetKeyUp(crouchKey)){
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }

    private void StateHandler(){

        // Mode - Crouching
        if (Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            speed = crouchSpeed;
        }

        // Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey)){
            state = MovementState.sprinting;
            speed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded){
            state = MovementState.walking;
            speed = walkSpeed;
        }

        // Mode - Air
        else{
            state = MovementState.air;
        }
    }

    private void MovePlayer(){
        // Calculate movement direction
        moveDirection = orientation.forward * yInput + orientation.right * xInput;

        // When on a Slope
        if(OnSlope()){
            rb.AddForce(GetSlopeMoveDirection() * speed * 20f, ForceMode.Force);
        }

        if(grounded){
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        }

        else if(!grounded){
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl(){

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // If you go faster than movement speed, find what velocity then apply
        if(flatVelocity.magnitude > speed){
            Vector3 limitedVelocity = flatVelocity.normalized * speed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }

    private void Jump(){
        // Resets y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump(){
        readyToJump = true;
    }

    private bool OnSlope(){
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, groundDistance * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection(){
        return Vector3.ProjectOnPlane(moveDirection,slopeHit.normal).normalized;
    }

}
