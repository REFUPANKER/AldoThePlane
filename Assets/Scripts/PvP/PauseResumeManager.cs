using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PauseResumeManager : NetworkBehaviour
{
    [SerializeField] PlayerStatusManager psm;
    [SerializeField] CinemachineVirtualCameraBase[] cams;
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.P)) { prAct(); }
    }
    public void prAct()
    {
        psm.AnimateWithFloat("velocity", 0);
        psm.Status.Paused = !psm.Status.Paused;
        psm.Status.CanUseSkill = !psm.Status.Paused;
        psm.Status.CanAnimate = !psm.Status.Paused;
        psm.Status.Targetable = !psm.Status.Paused;
        psm.Status.CanMove = psm.Status.Dead ? false : !psm.Status.Paused;
        psm.SetVariables();
        Cursor.visible = psm.Status.Paused;
        Cursor.lockState = psm.Status.Paused ? CursorLockMode.None : CursorLockMode.Locked;
        foreach (var item in cams)
        {
            item.enabled = !psm.Status.Paused;
        }
    }
}
