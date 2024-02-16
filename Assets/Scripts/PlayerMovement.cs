using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float maxGroundForce;
    [SerializeField] private float maxAirForce;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float crouchJumpMultiplier;
    private bool grounded;
    private bool isCrouching;
    private Vector2 move;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        isCrouching = context.ReadValueAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();
    }

    /* Figure out how to fix moving when should be stationary on slope.
     */
    private void Move()
    {
        // find target velocity
        Vector3 currentVelocity = rb.velocity;
        Vector3 targetVelocity = new Vector3(move.x, 0, move.y);
        targetVelocity *= isCrouching? crouchSpeed : speed;

        // align direction
        targetVelocity = transform.TransformDirection(targetVelocity);

        // calculate forces
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        // limit force
        velocityChange = Vector3.ClampMagnitude(velocityChange, grounded? maxGroundForce : maxAirForce);

        if (grounded || move.magnitude > 0f) rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void Jump()
    {
        Vector3 jumpForces = rb.velocity;

        float jumpVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y * (isCrouching ? crouchJumpMultiplier : 1f));

        if (grounded)
        {
            jumpForces.y = jumpVelocity;
        }

        rb.velocity = jumpForces;
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

    public float GetMovementRatio()
    {
        return (new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude / speed);
    }
}
