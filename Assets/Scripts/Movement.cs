using System;
using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
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

    [Header("Camera")]
    public bool InFpsCam = true;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private CinemachineBrain brainCam;
    [SerializeField] private CinemachineVirtualCameraBase aimCam;
    [Header("Aim")]
    public KeyCode AimKey = KeyCode.Mouse1;
    [SerializeField] private ParticleSystem topdownPointParticle;
    private bool isAiming = false;
    [Header("TopDownControls")]
    [SerializeField] private CinemachineVirtualCameraBase topdownCam;
    public NavMeshAgent agent;
    [SerializeField] private LayerMask groundlayer;
    public Vector3 targetPoint;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        agent.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (!isPaused)
        {
            if (InFpsCam)
            {
                Move();
                RotateWithCamera();
                Aiming();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100)) // , groundlayer
                    {
                        if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Team2") && hit.transform.gameObject.layer != LayerMask.NameToLayer("Team1"))
                        {
                            targetPoint = hit.point;
                            ParticleSystem pointing = Instantiate(topdownPointParticle, topdownPointParticle.transform.parent);
                            pointing.transform.position = hit.point;
                            pointing.Play();
                        }
                    }
                }
                if (agent.enabled)
                {
                    agent.SetDestination(targetPoint);
                    animator.SetFloat("Velocity", agent.velocity.magnitude);
                }
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                SwitchCameraMode();
            }

        }
    }
    public void SwitchCameraMode()
    {
        targetPoint = transform.position;
        aimCam.Priority = isAiming ? aimCam.Priority - 10 : aimCam.Priority;
        topdownCam.Priority = !InFpsCam ? topdownCam.Priority - 10 : topdownCam.Priority + 10;
        InFpsCam = !InFpsCam;
        animator.SetFloat("Velocity", 0);
        Cursor.lockState = InFpsCam ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = InFpsCam ? false : true;
        agent.enabled = !InFpsCam;
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
                move.Normalize();

            float speed = isSpeedBoosted ? fastRunSpeed : runSpeed;
            controller.Move(move * speed * Time.deltaTime);

            animator.SetFloat("Velocity", move.magnitude * (speed / runSpeed));

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
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
                aimCam.Priority += 10;
                isAiming = true;
            }
        }
        else if (isAiming)
        {
            aimCam.Priority -= 10;
            isAiming = false;
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : (InFpsCam ? CursorLockMode.Locked : CursorLockMode.None);
        Cursor.visible = isPaused ? true : (InFpsCam ? false : true);
        Time.timeScale = isPaused ? 0 : 1;
        brainCam.enabled = !isPaused;
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
