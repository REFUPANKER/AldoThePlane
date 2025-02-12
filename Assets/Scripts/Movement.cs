using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float fastRunSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public bool isMoving;
    public bool isSpeedBoosted;
    public bool CanMove = true;
    public bool ApplyGravity = true;
    [Header("Animator")]
    [SerializeField] private Animator animator;

    public CharacterController controller;
    public Vector3 velocity;
    private bool isGrounded;

    private bool isPaused = false;

    [Header("Kamera ve Dönüş")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private CinemachineBrain brainCam;

    [Header("Aim")]
    [SerializeField] private CinemachineVirtualCameraBase vcam;
    [SerializeField] public KeyCode AimKey = KeyCode.Mouse1;
    [SerializeField] private int aimPriority = 10;
    private bool isAiming = false;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (!isPaused)
        {
            Move();
            RotateWithCamera();
            Aiming();
        }
    }

    private void Move()
    {
        isGrounded = controller.isGrounded;

        if (CanMove)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            isMoving = move.magnitude > 0.1f;

            if (move.magnitude > 1)
                move.Normalize(); // Prevents diagonal speed boost

            float speed = isSpeedBoosted ? fastRunSpeed : runSpeed;
            controller.Move(move * speed * Time.deltaTime);

            animator.SetFloat("Velocity", move.magnitude * (speed / runSpeed));

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // jumping
            // if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            // {
            //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // }
        }
        else
        {
            animator.SetFloat("Velocity", 0);
        }
        if (ApplyGravity)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }

    private void RotateWithCamera()
    {
        if (cameraTransform != null && CanMove)
        {
            Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void Aiming()
    {
        if (Input.GetKey(AimKey))
        {
            if (!isAiming)
            {
                vcam.Priority += aimPriority;
                isAiming = true;
            }
        }
        else if (isAiming)
        {
            vcam.Priority -= aimPriority;
            isAiming = false;
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            brainCam.enabled = !brainCam.enabled;
        }
        else
        {
            brainCam.enabled = !brainCam.enabled;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public delegate void eOnLanded();
    public event eOnLanded OnLanded;

    public void JumpToPoint(Vector3 point, float time)
    {
        CanMove = false;
        Vector3 displacement = point - transform.position;
        // float horizontalDistance = new Vector3(displacement.x, 0, displacement.z).magnitude;
        float verticalSpeed = Mathf.Sqrt(-2 * gravity * jumpHeight);
        velocity = new Vector3(displacement.x / time, verticalSpeed, displacement.z / time);
        StartCoroutine(ResetVelocityAfterTime(time));
    }

    private IEnumerator ResetVelocityAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        velocity = Vector3.zero;
        OnLanded?.Invoke();
        yield return new WaitForSeconds(time);
        CanMove = true;
    }

}
