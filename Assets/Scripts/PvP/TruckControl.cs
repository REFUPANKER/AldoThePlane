using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class TruckControl : Interactible
{

    public bool canDrive;
    private NetworkVariable<bool> nvCanDrive = new NetworkVariable<bool>();

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

    public struct truckctrl : INetworkSerializable
    {
        public Quaternion w1, w2, rot;
        public float brake, torque;
        public Vector3 pos;

        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref w1);
            s.SerializeValue(ref w2);
            s.SerializeValue(ref brake);
            s.SerializeValue(ref torque);
            s.SerializeValue(ref rot);
            s.SerializeValue(ref pos);
        }
    }

    private NetworkVariable<truckctrl> tstatus = new NetworkVariable<truckctrl>();

    private void Update()
    {
        if (!IsServer)
        {
            wheelFL.motorTorque = tstatus.Value.torque;
            wheelFR.motorTorque = tstatus.Value.torque;
            wheelFL.brakeTorque = tstatus.Value.brake;
            wheelFR.brakeTorque = tstatus.Value.brake;
            wheelRL.brakeTorque = tstatus.Value.brake;
            wheelRR.brakeTorque = tstatus.Value.brake;
            wheelFLTransform.transform.rotation = tstatus.Value.w1;
            wheelFRTransform.transform.rotation = tstatus.Value.w2;
        }
        else
        {
            if (!canDrive) { return; }
            float motor = Input.GetAxis("Vertical") * motorTorque;
            float steer = Input.GetAxis("Horizontal") * steerAngle;
            bool isBraking = Input.GetAxis("Vertical") < 0;
            float brakeForce = isBraking ? brakeforce : 0f;
            wheelFL.motorTorque = isBraking ? 0f : motor;
            wheelFR.motorTorque = isBraking ? 0f : motor;
            wheelFL.brakeTorque = brakeForce;
            wheelFR.brakeTorque = brakeForce;
            wheelRL.brakeTorque = brakeForce;
            wheelRR.brakeTorque = brakeForce;
            wheelFL.steerAngle = steer;
            wheelFR.steerAngle = steer;
            UpdateWheel(wheelFL, wheelFLTransform);
            UpdateWheel(wheelFR, wheelFRTransform);
            UpdateWheel(wheelRL, wheelRLTransform);
            UpdateWheel(wheelRR, wheelRRTransform);

            truckctrl t = new truckctrl()
            {
                brake = brakeforce,
                torque = isBraking ? 0f : motor,
                pos = transform.position,
                rot = transform.rotation,
                w1 = wheelFLTransform.rotation,
                w2 = wheelFRTransform.rotation,
            };
            uStatServerRpc(t);
            if (Input.GetKeyDown(KeyCode.E))
            {
                StopInteract();
            }
        }
    }

    [ServerRpc]
    void uStatServerRpc(truckctrl t)
    {
        tstatus.Value = t;
        uStatClientRpc(t);
    }
    [ClientRpc]
    void uStatClientRpc(truckctrl t)
    {
        if (!IsOwner)
        {
            transform.position = tstatus.Value.pos;
            transform.rotation = tstatus.Value.rot;
        }
    }


    private void UpdateWheel(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    HeroMovement hero;
    public override void Interact(HeroMovement hero)
    {
        base.Interact(hero);
        this.hero = hero;
        hero.enabled = false;
        tpscam.Priority += 10;
        uCanDriveServerRpc(true);
        Debug.Log("driving car bomboclat");
    }
    public override void StopInteract()
    {
        base.StopInteract();
        hero.enabled = true;
        tpscam.Priority -= 10;
        this.hero = null;
    }

    [ServerRpc(RequireOwnership = false)]
    void uCanDriveServerRpc(bool t)
    {
        if (IsOwner)
        {
            nvCanDrive.Value = t;
            canDrive = t;
        }
        uCanDriveClientRpc(t);
    }
    [ClientRpc]
    void uCanDriveClientRpc(bool t)
    {
        if (!IsOwner) { canDrive = t; }
    }
}
