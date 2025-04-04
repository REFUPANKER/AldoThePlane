using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class HeroMovement : NetworkBehaviour
{
    public bool CanMove = true;
    public float gravity = -9.81f;
    private Vector3 velocity;
    [SerializeField] CharacterController ctrl;
    [SerializeField] float speed = 5;
    public Animator anims;
    [Header("Camera setup")]
    [SerializeField] CinemachineFreeLook flCam;
    [SerializeField] Camera cam;
    [Range(300, 1000)]
    [SerializeField] float mouseSens = 300;
    [Header("Highlight")]
    [SerializeField] Outline OutlineScript;
    public Color teammateColor;
    public Color enemyColor;
    void Start()
    {
        if (!IsOwner)
        {
            flCam.Priority -= 1;
            cam.depth -= 1;
            AudioListener al = cam.GetComponent<AudioListener>();
            al.enabled = false;
            OutlineScript.OutlineColor = enemyColor;
        }
        else
        {
            flCam.m_XAxis.m_MaxSpeed = mouseSens;
            OutlineScript.OutlineColor = teammateColor;
        }
    }
    void Update()
    {
        if (!IsOwner) { return; }
        if (CanMove)
        {
            float ix = Input.GetAxis("Horizontal");
            float iz = Input.GetAxis("Vertical");
            Vector3 mv = transform.right * ix + transform.forward * iz;
            if (mv.magnitude > 1)
                mv.Normalize();
            ctrl.Move(mv * speed * Time.deltaTime);
            anims.SetFloat("velocity", ctrl.velocity.magnitude, 0.05f, Time.deltaTime);
            if (ctrl.velocity.magnitude >= 1)
            {
                transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
            }
            if (ctrl.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
            ctrl.Move(velocity * Time.deltaTime);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }
}
