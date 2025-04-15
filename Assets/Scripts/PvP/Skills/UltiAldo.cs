using System;
using Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class UltiAldo : NetworkBehaviour
{
    [SerializeField] PlayerStatusManager psm;
    [SerializeField] float damage = 50;
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
    [SerializeField] LayerMask healthManagerLayer;
    public Transform target;
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

        if (!inUse && Input.GetKeyDown(KeyCode.Alpha3) && ctrl.isGrounded && psm.Status.CanUseSkill)
        {
            // target selection
            PlayerStatusManager[] psms = FindObjectsOfType<PlayerStatusManager>();
            foreach (PlayerStatusManager item in psms)
            {
                if (item.gameObject != ctrl.gameObject && item.Status.Targetable)
                {
                    target = item.transform;
                    break;
                }
            }

            if (target == null) { return; }

            airState = 0;
            inUse = true;
            v.y = 0;
            psm.AnimateWithFloat("velocity", 0);
            psm.Status.CanMove = false;
            psm.Status.CanAnimate = false;
            psm.SetVariables();
            firstAltitude = target.transform.position.y;

            //System.Random rnd = new System.Random();
            //target = target != Vector3.zero ? Vector3.zero : new Vector3(rnd.Next(-200, 200), firstAltitude, rnd.Next(-200, 200));
        }

        if (inUse)
        {
            ctrl.transform.LookAt(target);
            //hero.anims.SetFloat("velocity", 0);
            float dif = Vector3.Distance(ctrl.transform.position, target.position);
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
                    psm.Status.CanMove = true;
                    psm.Status.CanAnimate = true;
                    psm.SetVariables();
                    v.y = -ascendForce * Time.deltaTime;
                    inUse = false;
                    ctrl.transform.rotation = quaternion.identity;
                    FlightCam.Priority -= 10;
                    CheckSwitch();
                    AfterLanded();
                    target = null;
                    break;
            }
            ctrl.Move(v * Time.deltaTime);
        }
    }
    void StraightFlight()
    {
        v = ctrl.transform.forward;
        v *= flightSpeed * (1 + flightBoost);
        flightBoost += boostMultiplier;
    }

    void AfterLanded()
    {
        Collider[] col = Physics.OverlapSphere(ctrl.transform.position, 10, healthManagerLayer);
        foreach (var item in col)
        {
            if (item.transform.root != ctrl.transform)
            {
                HealthManagerPvP h = item.GetComponent<HealthManagerPvP>();
                h?.TakeDamage(damage);
            }
        }
    }
}
