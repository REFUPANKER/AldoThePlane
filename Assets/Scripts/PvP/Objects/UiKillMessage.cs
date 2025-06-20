using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UiKillMessage : NetworkBehaviour
{
    [SerializeField] GameNotifsManager gnm;
    [SerializeField] TextMeshProUGUI labelPlayer1, labelPlayer2;
    [SerializeField] Image holder;
    [SerializeField] Image deathType;

    public void Apply(GameNotifsManager gnm, string p1, string p2, DeathTypes deathType, Color bg)
    {
        this.gnm = gnm;
        labelPlayer1.text = p1;
        labelPlayer2.text = p2;
        holder.color = bg;
        this.deathType.sprite = GetDeathTypeSprite(deathType);
    }
    Sprite GetDeathTypeSprite(DeathTypes dt)
    {
        if (dt == DeathTypes.NormalAttack || dt == DeathTypes.Tower || dt == DeathTypes.Dwarf || dt == DeathTypes.BigDwarf)
        {
            return gnm.DeathTypeSprites[0];
        }
        foreach (var item in gnm.DeathTypeSprites)
        {
            if (item.name.Contains(dt.ToString()))
            {
                return item;
            }
        }
        return null;
    }
}
