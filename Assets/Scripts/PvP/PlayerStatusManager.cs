using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerStatusManager : NetworkBehaviour
{
    [SerializeField,
    Tooltip("(Encapsulated) The objects that can be disabled or enabled for Player visibility")]
    GameObject[] VisiblityObjects;

    [SerializeField, Tooltip("when user enters vehicle,change exclude layer to interactible")] CharacterController ctrl;
    [SerializeField] Animator animator;

    #region Object Data Setup (OnNetworkSpawn)

    public statusstruct Status = new statusstruct()
    {
        CanMove = true,
        CanAnimate = true,
        Targetable = true,
        CanUseSkill = true,
    };


    [Serializable]
    public struct statusstruct : INetworkSerializable
    {
        public bool CanMove;
        public bool CanAnimate;
        public bool CanUseSkill;
        public bool InVehicle;
        public bool Targetable;
        public bool Paused;
        public bool Dead;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref CanMove);
            s.SerializeValue(ref CanAnimate);
            s.SerializeValue(ref CanUseSkill);
            s.SerializeValue(ref InVehicle);
            s.SerializeValue(ref Targetable);
            s.SerializeValue(ref Paused);
            s.SerializeValue(ref Dead);
        }
    }

    private NetworkVariable<statusstruct> _status = new NetworkVariable<statusstruct>();
    public override void OnNetworkSpawn()
    {
        SetVariables();
    }
    public void SetVariables()
    {
        if (IsOwner)
        {
            uServerRpc(Status);
        }
        else
        {
            Status = _status.Value;
        }
    }
    public void SetVariablesAsDifferentClient()
    {
        uServerRpc(Status);
    }

    [ServerRpc(RequireOwnership = false)]
    void uServerRpc(statusstruct d)
    {
        // if (_status.Value.Paused != d.Paused) { OnPaused?.Invoke(); }
        _status.Value = d;
        uClientRpc(d);
    }

    [ClientRpc]
    void uClientRpc(statusstruct d)
    {
        if (!IsOwner)
        {
            //if (Status.Paused != d.Paused) { OnPaused?.Invoke(); }
            Status = d;
        }
    }

    #endregion

    #region animations
    public void AnimateWithFloat(string name, float value, float damping = 0, float damptiming = 0)
    {
        if (!Status.CanAnimate) { return; }
        animator.SetFloat(name, value, damping, damptiming);
    }
    public void AnimateWithBool(string name, bool value)
    {
        if (!Status.CanAnimate) { return; }
        animator.SetBool(name, value);
    }
    public void AnimateWithTrigger(string name, bool isTriggered)
    {
        if (!Status.CanAnimate) { return; }
        if (isTriggered) { animator.ResetTrigger(name); }
        else { animator.SetTrigger(name); }
    }
    public float AnimatorGetFloat(string name) { return animator.GetFloat(name); }
    public bool AnimatorGetBool(string name) { return animator.GetBool(name); }
    #endregion

    // #region events
    // public delegate void _OnPaused();
    // public event _OnPaused OnPaused;
    // #endregion

    #region visibility
    public void ChangePlayerVisibility(bool visible)
    {
        visibilityServerRpc(visible);
    }
    [ServerRpc(RequireOwnership = false)]
    private void visibilityServerRpc(bool visible)
    {
        foreach (GameObject item in VisiblityObjects)
        {
            item.SetActive(visible);
        }
        visibilityClientRpc(visible);
    }
    [ClientRpc]
    private void visibilityClientRpc(bool visible)
    {
        foreach (GameObject item in VisiblityObjects)
        {
            item.SetActive(visible);
        }
    }
    #endregion

    #region parent
    public void ChangeParent(NetworkObject newParent)
    {
        SetParentServerRpc(new NetworkObjectReference(newParent));
    }
    [ServerRpc(RequireOwnership = false)]
    void SetParentServerRpc(NetworkObjectReference parentRef)
    {
        if (parentRef.TryGet(out NetworkObject parentObj))
        {
            transform.SetParent(parentObj.transform, false);
            SetParentVariables();
            SetParentClientRpc();
        }
    }
    [ClientRpc]
    void SetParentClientRpc() { if (!IsServer) { SetParentVariables(); } }
    void SetParentVariables()
    {
        ctrl.enabled = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = quaternion.identity;
    }

    public void RemoveParent(Vector3 worldOffset)
    {
        RemoveParentServerRpc(worldOffset);
    }
    [ServerRpc(RequireOwnership = false)]
    void RemoveParentServerRpc(Vector3 worldOffset)
    {
        Quaternion rot = transform.parent.rotation;
        transform.SetParent(null, false);
        RemoveParentVariables(worldOffset, rot);
        RemoveParentClientRpc(worldOffset, rot);
    }
    [ClientRpc]
    void RemoveParentClientRpc(Vector3 worldOffset, Quaternion rot) { if (!IsServer) { RemoveParentVariables(worldOffset, rot); } }
    void RemoveParentVariables(Vector3 worldOffset, Quaternion rot)
    {
        transform.position = worldOffset;
        transform.rotation = rot;
        ctrl.enabled = true;
    }
    #endregion


}
