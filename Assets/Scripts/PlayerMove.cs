using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    private CapsuleCollider playerCollider;
    private Vector3 playerVelocity;
    private bool grounded;
    private bool sliding;

    public float playerSpeed = 5.0f;
    public float crouchSpeed = 3.25f;
    public float groundAccelRate = 0.2f;
    public float airAccelRate = 0.5f;
    public float jumpHeight = 2.0f;
    public float gravityValue = -10.0f;
    public float slopeAngle = 45.0f;
    public float stepHeight = 0.3f;
    public LayerMask whatIsGround = LayerMask.GetMask("Ground");

    private Vector2 moveInput;
    private bool wantToJump;
    private bool wantToCrouch;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private Vector3 slopeSlideVelocity;

    public List<gravityPair> GravityList;

    public void OnMove(InputAction.CallbackContext context) { moveInput = context.ReadValue<Vector2>(); }
    public void OnJump(InputAction.CallbackContext context) { wantToJump = context.ReadValueAsButton(); }
    public void OnCrouch(InputAction.CallbackContext context) { wantToCrouch = context.ReadValueAsButton(); }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCollider = GetComponent<CapsuleCollider>();
        controller.stepOffset = stepHeight;
    }

    // Update is called once per frame
    void Update()
    {
        // check if grounded
        CalculateSlide();
        Move();
    }

    private void Move()
    {
        // initialize acceleration and such
        float accel = groundAccelRate;

        // jump, damn you!
        if (grounded)
        {
            // vertical velocity
            if (playerVelocity.y < 0f) playerVelocity.y = -5f;
            if (wantToJump) playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
        else
        {
            if (sliding)
            {
                playerVelocity = slopeSlideVelocity;
            }
            accel = airAccelRate;
        }

        Vector3 lateralMove = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        lateralMove = moveInput.x * transform.right.normalized + moveInput.y * lateralMove;
        Debug.Log("lateralMove: " + lateralMove);

        Vector2 latMove = new Vector2(lateralMove.x, lateralMove.z);
        currentInputVector = Vector2.SmoothDamp(currentInputVector, latMove, ref smoothInputVelocity, accel);
        Vector3 move = new Vector3(currentInputVector.x, 0f, currentInputVector.y);
        move *= Time.deltaTime * (wantToCrouch? crouchSpeed : playerSpeed);
        Debug.Log("move: " + move);

        // jump!

        // perform gravity, eventually add dynamic gravity list
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(move + playerVelocity * Time.deltaTime);
    }
    
    private void CalculateSlide()
    {
        Debug.Log("Calculating Slide!");
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 5))
        {
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);
            Debug.Log("ground slope" + angle);
            if (angle >= controller.slopeLimit)
            {
                slopeSlideVelocity = Vector3.ProjectOnPlane(new Vector3(0f, gravityValue, 0f), hitInfo.normal);
                sliding = true;
                grounded = false;
                return;
            }
            else if (hitInfo.transform.gameObject.layer == whatIsGround.value)
            {
                sliding = false;
                grounded = true;
                return;
            }
        }
        Debug.Log("No Ground!");

        sliding = false;
        grounded = false;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
    }
}

public struct gravityPair
{
    public string source;
    public Vector3 gravity;
}
