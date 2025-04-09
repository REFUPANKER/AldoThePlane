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
    [SerializeField] ParticleSystem SwitchParticles;
    [Header("Variables")]
    public bool inUse = false;
    public int airState = 0;
    public float ascendLimit = 10;
    public float ascendForce = 25;
    private float firstAltitude;
    public float flightSpeed = 25;
    public float flightBoost = 10;
    public float boostMultiplier = 0.01f;
    private Vector3 v;
    [Header("Target Setup")]
    public Vector3 target;
    public CinemachineFreeLook FlightCam;

    public GameObject tailsHolder;

    private NetworkVariable<bool> switched = new NetworkVariable<bool>(value: false, readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        SwitchTarget.gameObject.SetActive(false);
    }

    [ServerRpc]
    void UServerRpc(bool s)
    {
        switched.Value = s;
        UClientRpc(s);
    }
    [ClientRpc]
    void UClientRpc(bool s)
    {
        if (IsOwner) { return; }
        ApplySwitch(!s, s);
    }
    void CheckSwitch()
    {
        if (IsOwner)
        {
            ApplySwitch(switched.Value, !switched.Value);
            UServerRpc(!switched.Value);
        }
        else
        {
            ApplySwitch(!switched.Value, switched.Value);
        }
    }
    void ApplySwitch(bool src, bool tgt)
    {
        SwitchParticles.Stop();
        SwitchParticles.Play();
        SwitchSource.gameObject.SetActive(src);
        SwitchTarget.gameObject.SetActive(tgt);
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
            hero.canAnimate = false;
            firstAltitude = ctrl.transform.position.y;

            // target selection
            HeroMovement[] heroes = FindObjectsOfType<HeroMovement>();
            foreach (HeroMovement item in heroes)
            {
                if (item != hero)
                {
                    target = item.transform.position;
                    break;
                }
            }
            //System.Random rnd = new System.Random();
            //target = target != Vector3.zero ? Vector3.zero : new Vector3(rnd.Next(-200, 200), firstAltitude, rnd.Next(-200, 200));
        }

        if (inUse)
        {
            hero.transform.LookAt(target);
            hero.anims.SetFloat("velocity", 0);
            float dif = Vector3.Distance(ctrl.transform.position, target);
            switch (airState)
            {
                case 0:
                    if (ctrl.transform.position.y - firstAltitude >= ascendLimit)
                    {
                        FlightCam.Priority += 10;
                        flightBoost = 0;
                        airState = 1;
                        CheckSwitch();
                    }
                    v.y += ascendForce * Time.deltaTime;
                    break;
                case 1:
                    if (dif <= ascendLimit)
                    {
                        airState = 2;
                    }
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
                    hero.canAnimate = true;
                    v = Vector3.zero;
                    inUse = false;
                    hero.transform.rotation = quaternion.identity;
                    hero.CanMove = true;
                    FlightCam.Priority -= 10;
                    CheckSwitch();
                    break;
            }
            ctrl.Move(v * Time.deltaTime);
        }
    }
    void StraightFlight()
    {
        v = hero.transform.forward;
        v *= flightSpeed * (1 + flightBoost);
        flightBoost += boostMultiplier;
    }
}
