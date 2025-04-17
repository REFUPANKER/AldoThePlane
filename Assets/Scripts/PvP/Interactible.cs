using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Interactible : NetworkBehaviour
{
    public KeyCode IntKey;
    public string DefText;
    public bool CanInteract = true;
    public PlayerStatusManager psm;
    private NetworkVariable<bool> _CanInteract = new NetworkVariable<bool>(true);

    public delegate void onStopInteract();
    public event onStopInteract OnStopInteract;

    // dont forget to set psm
    public virtual void Interact()
    {
        uServerRpc(false);
    }
    public virtual void StopInteract()
    {
        OnStopInteract?.Invoke();
        uServerRpc(true);
    }

    [ServerRpc(RequireOwnership = false)]
    void uServerRpc(bool t)
    {
        _CanInteract.Value = t;
        CanInteract = t;
        uClientRpc(t);
    }
    [ClientRpc]
    void uClientRpc(bool t)
    {
        if (!IsOwner)
        {
            CanInteract = t;
        }
    }
}
