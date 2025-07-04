using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class HeroMovement : NetworkBehaviour
{

    [SerializeField] PlayerStatusManager psm;

    public Camera cam;
    public float gravity = -9.81f;
    private Vector3 velocity;
    public CharacterController ctrl;
    [SerializeField] float speed = 5;
    [Header("Camera setup")]
    [SerializeField] CinemachineFreeLook flCam;
    [SerializeField] CinemachineVirtualCameraBase[] tpsCams;
    [Tooltip("auto set to main camera after spawn")]
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

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerServerRpc(StructPlayer data)
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

    public override void OnNetworkSpawn()
    {
        cam = Camera.main;
        if (IsOwner)
        {
            flCam.m_XAxis.m_MaxSpeed = mouseSens;
            OutlineScript.OutlineColor = teammateColor;
        }
        else
        {
            foreach (var item in tpsCams)
            {
                item.gameObject.SetActive(false);
            }
            cam.depth -= 1;
            AudioListener a = cam.GetComponent<AudioListener>();
            a.enabled = false;
            OutlineScript.OutlineColor = enemyColor;
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
        else if (ctrl.enabled)
        {
            transform.position = netPlayerData.Value.pos;
            transform.rotation = netPlayerData.Value.rot;
            ctrl.Move(netPlayerData.Value.vel * Time.deltaTime);
        }
        #endregion

        if (!IsOwner) { return; }
        #region movement
        if (ctrl.enabled && psm.Status.CanMove && !psm.Status.Dead)
        {
            float ix = Input.GetAxis("Horizontal");
            float iz = Input.GetAxis("Vertical");
            Vector3 mv = transform.right * ix + transform.forward * iz;
            if (mv.magnitude > 1)
                mv.Normalize();
            ctrl.Move(mv * speed * Time.deltaTime);
            if (psm.Status.CanAnimate)
            {
                psm.AnimateWithFloat("velocity", ctrl.velocity.magnitude, 0.05f, Time.deltaTime);
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
    }

    public void Kill(ulong id)
    {
        killServerRpc(id);
    }

    [ServerRpc(RequireOwnership = false)]
    void killServerRpc(ulong id)
    {
        if (NetworkManager.Singleton.ConnectedClients.ContainsKey(id))
        {
            var player = NetworkManager.Singleton.ConnectedClients[id].PlayerObject;
            PlayerStatusManager psmX = player.GetComponent<PlayerStatusManager>();
            psmX.Status.Dead = true;
            psmX.SetVariables();
            Debug.Log("reached to client");
        }
    }
}
