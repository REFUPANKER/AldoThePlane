using System;
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
    public bool canAnimate = true;
    public Animator anims;
    [Header("Camera setup")]
    [SerializeField] CinemachineBrain cmcBrain;
    [SerializeField] CinemachineFreeLook flCam;
    public Camera cam;
    [Range(300, 1000)]
    [SerializeField] float mouseSens = 300;
    [Header("Highlight")]
    [SerializeField] Outline OutlineScript;
    public Color teammateColor;
    public Color enemyColor;

    #region network transform
    public struct StructPlayer : INetworkSerializable
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 vel;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref pos);
            s.SerializeValue(ref rot);
            s.SerializeValue(ref vel);
        }
    }
    private NetworkVariable<StructPlayer> netPlayerData = new NetworkVariable<StructPlayer>(writePerm: NetworkVariableWritePermission.Server);

    [ServerRpc]
    void UpdatePlayerServerRpc(StructPlayer data)
    {
        netPlayerData.Value = data;
        UpdatePlayerClientRpc(data);
    }

    [ClientRpc]
    void UpdatePlayerClientRpc(StructPlayer data)
    {
        if (!IsOwner)
        {
            transform.position = data.pos;
            transform.rotation = data.rot;
        }
    }
    #endregion

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
        #region network transform
        if (IsOwner)
        {
            StructPlayer data = new StructPlayer
            {
                pos = transform.position,
                rot = transform.rotation,
                vel = ctrl.velocity,
            };
            UpdatePlayerServerRpc(data);
        }
        else
        {
            transform.position = netPlayerData.Value.pos;
            transform.rotation = netPlayerData.Value.rot;
            ctrl.Move(netPlayerData.Value.vel * Time.deltaTime);
        }
        #endregion

        if (!IsOwner) { return; }
        #region movement
        if (CanMove)
        {
            float ix = Input.GetAxis("Horizontal");
            float iz = Input.GetAxis("Vertical");
            Vector3 mv = transform.right * ix + transform.forward * iz;
            if (mv.magnitude > 1)
                mv.Normalize();
            ctrl.Move(mv * speed * Time.deltaTime);
            if (canAnimate)
            {
                anims.SetFloat("velocity", ctrl.velocity.magnitude, 0.05f, Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.W))
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
        #endregion

        #region pause resume
        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            cmcBrain.enabled = !Cursor.visible;
        }
        #endregion
    }
}
