using System;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class UltiAldo : NetworkBehaviour
{
    [SerializeField] HeroMovement hero;
    [SerializeField] CharacterController ctrl;
    [Header("Switch transforms")]
    [SerializeField] Transform SwitchSource;
    [SerializeField] Transform SwitchTarget;
    [SerializeField] ParticleSystem particles;
    [Header("Variables")]
    public bool inUse = false;
    public int airState = 0;
    public float ascendLimit = 10;
    public float ascendForce = 5;
    private float firstAltitude;
    public float flightSpeed = 25;
    private float flightBoost = 0;
    private float boostMultiplier = 0.01f;
    private Vector3 v;
    [Header("Target Setup")]
    public Vector3 target;
    public CinemachineFreeLook FlightCam;
    // default scale
    private Vector3 dsSource, dsTarget;


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }
        dsSource = SwitchSource.lossyScale;
        dsTarget = SwitchTarget.lossyScale;
        UpdateScaleServerRpc(dsSource, Vector3.zero);
    }

    [ServerRpc]
    void UpdateScaleServerRpc(Vector3 d1, Vector3 d2)
    {
        if (IsOwner)
        {
            SwitchSource.localScale = d1;
            SwitchTarget.localScale = d2;
        }
        UpdateScaleClientRpc(d1, d2);
    }

    [ClientRpc]
    void UpdateScaleClientRpc(Vector3 d1, Vector3 d2)
    {
        if (!IsOwner)
        {
            SwitchSource.localScale = d1;
            SwitchTarget.localScale = d2;
        }
    }

    void Update()
    {
        if (!IsOwner) { return; }
        if (!inUse && Input.GetKeyDown(KeyCode.Alpha3) && ctrl.isGrounded)
        {
            airState = 0;
            inUse = true;
            v.y = 0;
            hero.CanMove = false;
            hero.anims.SetFloat("velocity", 0);
            firstAltitude = ctrl.transform.position.y;

            System.Random rnd = new System.Random();
            target = new Vector3(rnd.Next(-200, 200), firstAltitude + ascendLimit, rnd.Next(-200, 200));

            FlightCam.Priority += 10;
        }

        if (inUse)
        {
            float dif = Vector3.Distance(ctrl.transform.position, target);
            switch (airState)
            {
                case 0:
                    if (ctrl.transform.position.y - firstAltitude >= ascendLimit)
                    {
                        flightBoost = 0;
                        airState = 1;
                        // Switch
                        UpdateScaleServerRpc(Vector3.zero, dsTarget);
                    }
                    v.y += ascendForce * Time.deltaTime;
                    break;
                case 1:
                    if (dif <= ascendLimit)
                    {
                        airState = 2;
                        target.y -= ascendLimit;
                    }
                    hero.transform.LookAt(target);
                    StraightFlight();
                    break;
                case 2:
                    if (dif <= 1)
                    {
                        airState = 3;
                    }
                    v.y -= ascendForce * Time.deltaTime;
                    StraightFlight();
                    break;
                default:
                    v = Vector3.zero;
                    UpdateScaleServerRpc(dsSource, Vector3.zero);
                    inUse = false;
                    hero.transform.rotation = quaternion.identity;
                    hero.CanMove = true;
                    FlightCam.Priority -= 10;
                    break;
            }
            ctrl.Move(v * Time.deltaTime);
        }
    }
    void StraightFlight()
    {
        v = hero.transform.forward;
        float speed = flightSpeed * (1 + flightBoost);
        v *= Math.Clamp(speed, 0, 100);
        flightBoost += boostMultiplier;
    }
}
