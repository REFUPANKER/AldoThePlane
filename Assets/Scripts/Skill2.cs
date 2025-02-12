using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2 : SkillTemplate
{
    public Animation spinAnim;
    public Transform shieldsHolder;
    public Movement player;
    public Transform ShieldTrackPos;
    public DamageSphere sphere;

    public override void Activate()
    {
        if (!Active && !InCoolDown)
        {
            spinAnim.Play();
            shieldsHolder.gameObject.SetActive(true);
            sphere.gameObject.SetActive(true);
            base.Activate();
            player.isSpeedBoosted = true;
        }
    }
    public override void OnActiveTimeEnd()
    {
        base.OnActiveTimeEnd();
        if (!IsBlocked)
        {
            spinAnim.Stop();
            player.isSpeedBoosted = false;
            shieldsHolder.gameObject.SetActive(false);
            sphere.Play(true);
        }
    }
    void Update()
    {
        if (Active)
        {
            shieldsHolder.transform.position = ShieldTrackPos.position;
        }
    }
    
    public override void OnBlocked()
    {
        spinAnim.Stop();
        player.isSpeedBoosted = false;
        shieldsHolder.gameObject.SetActive(false);
    }
}
