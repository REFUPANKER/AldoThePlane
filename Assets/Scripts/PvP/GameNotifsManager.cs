using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameNotifsManager : NetworkBehaviour
{
    public float DestroyFirstAfterSeconds = 3;
    public RectTransform MessagesHolder;
    public UiKillMessage DeathMessagePrefab;
    public Sprite[] DeathTypeSprites;
    [Header("Colors")]
    public Color defaultBackground = Color.black;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            ShowKillMessage("a", "w", DeathTypes.SkillAldoPunch, defaultBackground);
        }
    }

    #region Show Kill Message

    public void ShowKillMessage(string p1, string p2, DeathTypes deathType, Color? bg = null)
    {
        if (!IsServer) { return; }
        ShowKillMessageServerRpc(p1, p2, deathType, (Color)(bg == null ? defaultBackground : bg));
    }
    [ServerRpc(RequireOwnership = false)]
    void ShowKillMessageServerRpc(string p1, string p2, DeathTypes deathType, Color bg)
    {
        ShowKillMessageClientRpc(p1, p2, deathType, bg);
    }
    [ClientRpc]
    private void ShowKillMessageClientRpc(string p1, string p2, DeathTypes deathType, Color bg)
    {
        UiKillMessage newMsg = Instantiate(DeathMessagePrefab, MessagesHolder.transform);
        newMsg.Apply(this, p1, p2, deathType, bg);
        LayoutRebuilder.ForceRebuildLayoutImmediate(MessagesHolder);
        StartCoroutine(DestroyFirstChild(newMsg));
    }
    IEnumerator DestroyFirstChild(UiKillMessage kmsg)
    {
        yield return new WaitForSeconds(DestroyFirstAfterSeconds);
        Destroy(kmsg.gameObject);
    }

    #endregion
}
