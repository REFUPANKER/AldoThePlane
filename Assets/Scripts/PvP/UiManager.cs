using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UiManager : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(IsOwner);
    }
}
