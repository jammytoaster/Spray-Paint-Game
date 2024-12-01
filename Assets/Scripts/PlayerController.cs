using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Transactions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float speed;
    public float walkSpeed;
    public float sprintSpeed;
    public Text speedText;
 
    public float groundDrag;

    [Header("Jumping")]
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
    public Vector3 slopeSlideSpeed;
    private bool isSliding;


    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;


    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask whatIsGround;
    public float playerHeight;
    bool grounded;


    Rigidbody rb;

    public Transform orientation;

    float xInput;
    float yInput;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;

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
        UpdateSpeedDisplay();

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
        // GetKey used so player can hold down space
        if(Input.GetKey(jumpKey) && readyToJump && grounded){
            bool canJump = !OnSlope() || OnSlope() && Vector3.Angle(Vector3.up, slopeHit.normal) <= maxSlopeAngle;
            if(canJump){
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }
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

    // REMOVE
    private void UpdateSpeedDisplay(){
        float speed = CalculateSpeed();
        speedText.text = $"Speed: {speed:F1} km/h";
    }
    private float CalculateSpeed()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speedMPS = rb.linearVelocity.magnitude;
            return Mathf.Round(speedMPS * 3.6f * 100f) / 100f;
        }
        return 0f;
    }

    private void StateHandler(){

        // Mode - Crouching
        if (Input.GetKey(crouchKey)){
            state = MovementState.crouching;
            speed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey)){
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

        
        // Handle on slope movement
        if (OnSlope() && readyToJump){
            //Debug.Log("on Slope");
            rb.AddForce(GetSlopeMoveDirection() * speed * 20f, ForceMode.Force);


            // Stops player sliding down a slope that is under the max slope angle
            Vector3 slopeDown = -slopeHit.normal;
            rb.AddForce(slopeDown * (rb.mass * Physics.gravity.magnitude * 0.8f), ForceMode.Acceleration);

            // Keeps player on the ground when going up slope
            if (rb.linearVelocity.y > 0){
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        else if (grounded){
            //Debug.Log("GROUND");
            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);
        }

        // in air
        else if (!grounded){
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }

        
        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl(){

        // limiting speed on slope
        if (OnSlope() && readyToJump){
            if (rb.linearVelocity.magnitude > speed)
                rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }


        // Limiting speed in air or on ground
        else{
            Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // If you go faster than movement speed, find what velocity then apply
            if (flatVelocity.magnitude > speed){
                Vector3 limitedVelocity = flatVelocity.normalized * speed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
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
        
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            if (angle > maxSlopeAngle) {
                xInput = 0f;
                yInput = 0f;
            }
            

            //Debug.Log($"Slope detected: {angle}");
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

}
