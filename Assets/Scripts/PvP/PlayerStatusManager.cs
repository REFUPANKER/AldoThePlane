using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatusManager : NetworkBehaviour
{
    public statusstruct Status = new statusstruct()
    {
        CanMove = true,
        CanAnimate = true,
        Targetable = true,
        CanUseSkill = true,
    };

    public Animator animator;

    [Serializable]
    public struct statusstruct : INetworkSerializable
    {
        public bool CanMove;
        public bool CanAnimate;
        public bool CanUseSkill;
        public bool Targetable;
        public bool Paused;
        public bool Dead;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref CanMove);
            s.SerializeValue(ref CanAnimate);
            s.SerializeValue(ref CanUseSkill);
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
        _status.Value = d;
        uClientRpc(d);
    }

    [ClientRpc]
    void uClientRpc(statusstruct d)
    {
        if (!IsOwner)
        {
            Status = d;
        }
    }

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
}
