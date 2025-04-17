using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class TruckControl : Interactible
{
    public Rigidbody carrb;

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
        public Quaternion w1, w2, rot;
        public float brake, torque;
        public Vector3 pos;
        public bool backlights, frontlights;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref w1);
            s.SerializeValue(ref w2);
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
        uCanDriveServerRpc(false, ulong.MaxValue);
    }

    bool keyUp = false;
    private void Update()
    {
        if (nvStopping.Value || (psm != null && psm.Status.Paused))
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
            wheelRL.brakeTorque = brakeforce;
            wheelRR.brakeTorque = brakeforce;
            UpdateWheel(wheelFL, wheelFLTransform);
            UpdateWheel(wheelFR, wheelFRTransform);
            backLightsHolder.SetActive(true);
            if (carrb.velocity.magnitude <= 1)
            {
                carrb.velocity = Vector3.zero;
                carrb.angularVelocity = Vector3.zero;
                truckctrl nt = new truckctrl()
                {
                    pos = transform.position,
                    rot = transform.rotation
                };
                uStatServerRpc(nt);
                uStoppingServerRpc(false);
                backLightsHolder.SetActive(false);
            }
            return;
        }

        if (nvDriver.Value != NetworkManager.Singleton.LocalClientId)
        {
            wheelFL.motorTorque = tstatus.Value.torque;
            wheelFR.motorTorque = tstatus.Value.torque;
            wheelRL.brakeTorque = tstatus.Value.brake;
            wheelRR.brakeTorque = tstatus.Value.brake;
            wheelFLTransform.transform.rotation = tstatus.Value.w1;
            wheelFRTransform.transform.rotation = tstatus.Value.w2;
            backLightsHolder.SetActive(tstatus.Value.backlights);
            frontLightsHolder.SetActive(tstatus.Value.frontlights);
        }
        if (nvCanDrive.Value)
        {
            float steer = Input.GetAxis("Horizontal") * steerAngle;
            bool isBraking = Input.GetKey(KeyCode.Space);
            bool backLightsTriggered = isBraking || Input.GetKey(KeyCode.S);
            bool frontLightsTriggered = Input.GetKey(frontlightKey);
            float currentBrakeForce = isBraking ? brakeforce : 0f;
            float motor = isBraking ? 0 : Input.GetAxis("Vertical") * motorTorque;

            if (Input.GetKeyUp(IntKey)) { keyUp = true; }
            if (keyUp && Input.GetKeyDown(IntKey)) { StopInteract(); return; }

            wheelFL.motorTorque = motor;
            wheelFR.motorTorque = motor;
            wheelRL.brakeTorque = currentBrakeForce;
            wheelRR.brakeTorque = currentBrakeForce;
            wheelFL.steerAngle = steer;
            wheelFR.steerAngle = steer;
            UpdateWheel(wheelFL, wheelFLTransform);
            UpdateWheel(wheelFR, wheelFRTransform);

            backLightsHolder.SetActive(backLightsTriggered);
            frontLightsHolder.SetActive(frontLightsTriggered);

            truckctrl t = new truckctrl()
            {
                brake = currentBrakeForce,
                torque = motor,
                pos = transform.position,
                rot = transform.rotation,
                w1 = wheelFLTransform.rotation,
                w2 = wheelFRTransform.rotation,
                backlights = backLightsTriggered,
                frontlights = frontLightsTriggered
            };
            uStatServerRpc(t);
        }

    }
    void LateUpdate()
    {
        tpscam.gameObject.SetActive(psm != null && !psm.Status.Paused);
    }
    [ServerRpc(RequireOwnership = false)]
    void uStatServerRpc(truckctrl t)
    {
        tstatus.Value = t;
        transform.position = t.pos;
        transform.rotation = t.rot;
    }

    [ServerRpc(RequireOwnership = false)]
    void uStoppingServerRpc(bool s)
    {
        nvStopping.Value = s;
    }

    private void UpdateWheel(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
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
        if (!psm.Status.Paused)
        {
            tpscam.Priority += 10;
        }
        psm.SetVariables();
        uCanDriveServerRpc(true, psm.NetworkObject.OwnerClientId);
    }
    public override void StopInteract()
    {
        if (psm == null) { return; }
        psm.Status.InVehicle = false;
        psm.Status.Targetable = true;
        psm.Status.CanMove = true;
        psm.SetVariables();
        tpscam.enabled = false;
        if (!psm.Status.Paused)
        {
            tpscam.Priority -= 10;
        }
        psm = null;
        uStoppingServerRpc(true);
        uCanDriveServerRpc(false, ulong.MaxValue);
        keyUp = false;
        base.StopInteract();
    }

    [ServerRpc(RequireOwnership = false)]
    void uCanDriveServerRpc(bool t, ulong player)
    {
        if (IsOwner)
        {
            nvCanDrive.Value = t;
            nvDriver.Value = player == ulong.MaxValue ? Convert.ToUInt64(NetworkManager.Singleton.ConnectedClients.Count + 1) : player;
        }
    }
}
