using Cinemachine;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private float fastRunSpeed = 8f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    public bool isMoving;
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [SerializeField] private CharacterController controller;
    private Vector3 velocity;
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

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        isMoving = move.magnitude > 0.1f;

        if (move.magnitude > 1)
            move.Normalize(); // Prevents diagonal speed boost

        float speed = Input.GetKey(KeyCode.LeftShift) ? fastRunSpeed : runSpeed;
        controller.Move(move * speed * Time.deltaTime);

        animator.SetFloat("Velocity", move.magnitude * (speed / runSpeed));

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void RotateWithCamera()
    {
        if (cameraTransform != null)
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
}
