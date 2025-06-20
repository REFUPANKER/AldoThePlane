using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class TruckControl : Interactible
{
    public Rigidbody carrb;
    public Vector3 ExitOffset = new Vector3(-2, 0, 0);
    public CinemachineFreeLook tpscam;

    public float motorTorque = 1500f;
    public float steerAngle = 30f;
    public float brakeforce = 3000f;

    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public Transform wheelFLTransform;
    public Transform wheelFRTransform;
    public Transform wheelRLTransform;
    public Transform wheelRRTransform;

    public GameObject backLightsHolder;
    public KeyCode frontlightKey = KeyCode.F;
    public GameObject frontLightsHolder;

    [Serializable]
    public struct truckctrl : INetworkSerializable
    {
        public Quaternion rot;
        public float brake, torque, steer;
        public Vector3 pos;
        public bool backlights, frontlights;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref steer);
            s.SerializeValue(ref brake);
            s.SerializeValue(ref torque);
            s.SerializeValue(ref rot);
            s.SerializeValue(ref pos);
            s.SerializeValue(ref backlights);
            s.SerializeValue(ref frontlights);
        }
    }

    private NetworkVariable<truckctrl> tstatus = new NetworkVariable<truckctrl>();
    private NetworkVariable<bool> nvCanDrive = new NetworkVariable<bool>();
    private NetworkVariable<bool> nvStopping = new NetworkVariable<bool>();
    private NetworkVariable<ulong> nvDriver = new NetworkVariable<ulong>();

    void Start()
    {
        backLightsHolder.SetActive(false);
        frontLightsHolder.SetActive(false);
        UpdateAllWheels();
    }

    public override void OnNetworkSpawn()
    {
        // uCanDriveServerRpc(false, ulong.MaxValue);
        // truckctrl t = new truckctrl()
        // {
        //     pos = transform.position,
        //     rot = transform.rotation,
        // };
        // uStatServerRpc(t);
        ResetCarSpeedServerRpc();
    }

    bool keyUp = false;
    private void Update()
    {
        if (nvStopping.Value || (psm != null && psm.Status.Paused))
        {
            ResetCarSpeedServerRpc();
            uStoppingServerRpc(false);
        }

        if (nvDriver.Value != NetworkManager.Singleton.LocalClientId)
        {
            wheelFL.motorTorque = tstatus.Value.torque;
            wheelFR.motorTorque = tstatus.Value.torque;
            wheelFL.brakeTorque = tstatus.Value.brake;
            wheelFR.brakeTorque = tstatus.Value.brake;
            wheelRL.brakeTorque = tstatus.Value.brake;
            wheelRR.brakeTorque = tstatus.Value.brake;
            wheelFL.steerAngle = tstatus.Value.steer;
            wheelFR.steerAngle = tstatus.Value.steer;
            UpdateAllWheels();
            transform.position = tstatus.Value.pos;
            transform.rotation = tstatus.Value.rot;
            backLightsHolder.SetActive(tstatus.Value.backlights);
            frontLightsHolder.SetActive(tstatus.Value.frontlights);
        }
        else if (nvCanDrive.Value)
        {
            float steer = Input.GetAxis("Horizontal") * steerAngle;
            bool isBraking = Input.GetKey(KeyCode.Space);
            bool backLightsTriggered = isBraking || Input.GetKey(KeyCode.S);
            bool frontLightsTriggered = Input.GetKey(frontlightKey);
            float currentBrakeForce = isBraking ? brakeforce : 0f;
            float motor = isBraking ? 0 : Input.GetAxis("Vertical") * motorTorque;
            if (Input.GetKeyUp(IntKey)) { keyUp = true; }
            if (keyUp && Input.GetKeyDown(IntKey)) { StopInteract(); return; }
            if (carrb.velocity.magnitude <= 0.3f && Input.GetKeyDown(KeyCode.R))
            {
                transform.position = transform.position + Vector3.up * 2f;
                transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            }
            wheelFL.motorTorque = motor;
            wheelFR.motorTorque = motor;
            wheelFL.brakeTorque = currentBrakeForce;
            wheelFR.brakeTorque = currentBrakeForce;
            wheelRL.brakeTorque = currentBrakeForce;
            wheelRR.brakeTorque = currentBrakeForce;
            wheelFL.steerAngle = steer;
            wheelFR.steerAngle = steer;
            UpdateAllWheels();

            backLightsHolder.SetActive(backLightsTriggered);
            frontLightsHolder.SetActive(frontLightsTriggered);

            truckctrl t = new truckctrl()
            {
                brake = currentBrakeForce,
                torque = motor,
                pos = transform.position,
                rot = transform.rotation,
                steer = steer,
                backlights = backLightsTriggered,
                frontlights = frontLightsTriggered
            };
            uStatServerRpc(t);
        }

    }
    void LateUpdate()
    {
        tpscam.enabled = psm != null && !psm.Status.Paused;
    }
    [ServerRpc(RequireOwnership = false)]
    void uStatServerRpc(truckctrl t)
    {
        tstatus.Value = t;
    }

    [ServerRpc(RequireOwnership = false)]
    void uStoppingServerRpc(bool s)
    {
        nvStopping.Value = s;
    }

    [ServerRpc(RequireOwnership = false)]
    void ResetCarSpeedServerRpc()
    {
        ResetCarSpeed();
        ResetCarSpeedClientRpc();
    }
    [ClientRpc]
    void ResetCarSpeedClientRpc() { if (!IsServer) { ResetCarSpeed(); } }
    void ResetCarSpeed()
    {
        wheelFL.motorTorque = 0;
        wheelFR.motorTorque = 0;
        carrb.velocity = Vector3.zero;
        carrb.angularVelocity = Vector3.zero;
        truckctrl nt = new truckctrl()
        {
            pos = transform.position,
            rot = transform.rotation,
            brake = 9999f
        };
        uStatServerRpc(nt);
    }

    private void UpdateWheel(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
    void UpdateAllWheels()
    {
        UpdateWheel(wheelFL, wheelFLTransform);
        UpdateWheel(wheelFR, wheelFRTransform);
        UpdateWheel(wheelRL, wheelRLTransform);
        UpdateWheel(wheelRR, wheelRRTransform);
    }

    public override void Interact()
    {
        base.Interact();
        if (psm == null) { return; }
        psm.AnimateWithFloat("velocity", 0);
        psm.Status.InVehicle = true;
        psm.Status.CanMove = false;
        psm.Status.Targetable = false;
        tpscam.enabled = true;
        if (!psm.Status.Paused) { tpscam.Priority += 10; }
        psm.SetVariables();
        psm.ChangePlayerVisibility(false);
        psm.ChangeParent(NetworkObject);
        uCanDriveServerRpc(true, psm.NetworkObject.OwnerClientId);
        ResetCarSpeedServerRpc();
    }
    public override void StopInteract()
    {
        if (psm == null) { return; }
        psm.Status.InVehicle = false;
        psm.Status.Targetable = true;
        psm.Status.CanMove = true;
        psm.SetVariables();
        tpscam.enabled = false;
        if (!psm.Status.Paused) { tpscam.Priority -= 10; }
        uStoppingServerRpc(true);
        uCanDriveServerRpc(false, ulong.MaxValue);
        keyUp = false;
        psm.ChangePlayerVisibility(true);
        psm.RemoveParent(transform.position - (-transform.right * ExitOffset.x) - (transform.forward * ExitOffset.z));
        psm = null;
        base.StopInteract();
    }

    [ServerRpc(RequireOwnership = false)]
    void uCanDriveServerRpc(bool t, ulong player)
    {
        nvCanDrive.Value = t;
        nvDriver.Value = player == ulong.MaxValue ? Convert.ToUInt64(NetworkManager.Singleton.ConnectedClients.Count + 1) : player;
    }
}
