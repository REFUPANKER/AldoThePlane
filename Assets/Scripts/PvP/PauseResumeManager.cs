using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PauseResumeManager : NetworkBehaviour
{
    public bool isPaused = false;
    [SerializeField] CinemachineVirtualCameraBase[] cams;
    [SerializeField] HeroMovement hero;

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
        isPaused = !isPaused;
        hero.CanMove = !isPaused;
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        foreach (var item in cams)
        {
            item.enabled = !isPaused;
        }
    }
}
