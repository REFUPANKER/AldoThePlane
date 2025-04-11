using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TeamManager : NetworkBehaviour
{
    [SerializeField] LayerMask layer;
    [Header("Object Colors")]
    [SerializeField] Material ColorMaterial;
    [SerializeField] Renderer[] objs;
    [Header("UI Colors")]
    [SerializeField] Color ColorUi;
    [SerializeField] Image[] imgs;
    [Header("Setup")]
    [SerializeField] bool ApplyOnStart = true;

    void Start()
    {
        if (ApplyOnStart)
        {
            ApplyTeam(layer);
        }
    }
    public override void OnNetworkSpawn()
    {
        if (!ApplyOnStart && IsOwner)
        {
            ApplyTeam(layer);
        }
    }

    public void ApplyTeam(LayerMask layer)
    {
        int l = Mathf.RoundToInt(Mathf.Log(layer.value, 2));
        gameObject.layer = l;
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject c = transform.GetChild(i).gameObject;
            if (c.layer == 0)
            {
                c.layer = l;
            }
        }
        foreach (var item in objs)
        {
            item.material = ColorMaterial;
        }
        foreach (var item in imgs)
        {
            item.color = ColorUi;
        }
    }

}
