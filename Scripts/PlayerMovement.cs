using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float gravity = -9.8f;
    private float verticalVelocity;

    private CharacterController controller;

    private Vector2 moveInput;

    public Transform modelTransform;
    public Transform cameraPivot;

    private Animator animator;

    private bool isRunning;
    private bool isMoving;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    public float jumpForce = 5f;

    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundDetectionDistance = 3f;
    [SerializeField] private float minFallDistance = 2f;

    private bool hasJumped;
    private bool isFalling;
    private float highestY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        highestY = transform.position.y;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        float currentSpeed = (isRunning && isMoving) ? runSpeed : walkSpeed;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 forward = cameraPivot.forward;
        Vector3 right = cameraPivot.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDir = (forward * moveInput.y + right * moveInput.x);

        if (desiredMoveDir.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMoveDir);
            modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, targetRotation, 15f * Time.deltaTime);
        }


        Vector3 velocity = desiredMoveDir * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        OnFalling();
        UpdateAnimations();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            verticalVelocity = jumpForce;

            hasJumped = true;
            isFalling = false;

            animator.SetTrigger("Jump");
        }
    }

    public void OnFalling()
    {
        if (controller.isGrounded)
        {
            hasJumped = false;
            isFalling = false;
            highestY = transform.position.y;
            return;
        }

        highestY = Mathf.Max(highestY, transform.position.y);

        if (verticalVelocity >= -0.1f || isFalling)
        {
            return;
        }

        bool groundNearby = HasGroundBelow();

        float fallDistance = highestY - transform.position.y;

        bool fellOffEdge = !hasJumped && !groundNearby;

        bool longFall = fallDistance >= minFallDistance;

        if (fellOffEdge || longFall)
        {
            isFalling = true;
            UnityEngine.Debug.Log("Animacion de caida activada");
        }
    }

     private bool HasGroundBelow()
    {
        Bounds bounds = controller.bounds;

        Vector3 origin = new Vector3(bounds.center.x, bounds.min.y + 0.15f, bounds.center.z);

        float rayDistance = groundDetectionDistance + 0.15f;

        bool groundFound = Physics.Raycast(
            origin,
            Vector3.down,
            rayDistance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );

        UnityEngine.Debug.DrawRay(
            origin,
            Vector3.down * rayDistance,
            groundFound ? Color.green : Color.red
        );

        return groundFound;
    }


    public void OnSprint(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    public void UpdateAnimations()
    {
        isMoving = moveInput.magnitude > 0.1f;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsRunning", isRunning && isMoving);
        animator.SetBool("Grounded", controller.isGrounded);
        animator.SetFloat("VerticalVelocity", verticalVelocity);
        animator.SetBool("IsFalling", isFalling);
    }

}
