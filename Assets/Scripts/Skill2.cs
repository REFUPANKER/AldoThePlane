using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2 : SkillTemplate
{
    public Animation spinAnim;
    public Transform shieldSholder;
    public Movement player;
    public Transform ShieldTrackPos;

    public override void Activate()
    {
        if (!Active && !InCoolDown)
        {
            spinAnim.Play();
            base.Activate();
            shieldSholder.gameObject.SetActive(true);
        }
    }
    public override void OnActiveTimeEnd()
    {
        base.OnActiveTimeEnd();
        spinAnim.Stop();
        player.isSpeedBoosted = false;
        shieldSholder.gameObject.SetActive(false);
    }
    void Update()
    {
        if (Active)
        {
            shieldSholder.transform.position = ShieldTrackPos.position;
            player.isSpeedBoosted = true;
        }
    }

}
