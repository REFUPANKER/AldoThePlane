using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : SkillTemplate
{
    public Movement player;
    public Transform cam;
    public Attack attack;
    public float attackDistance = 10f;
    public ParticleSystem prtHand;
    public ParticleSystem prtGroundHit;
    public Animator anim;
    public AnimationClip punchAnim;

    public override void Activate()
    {
        if (!Active && !InCoolDown)
        {
            prtHand.Play();
            base.Activate();
        }
    }

    public override void OnActiveTimeEnd()
    {
        base.OnActiveTimeEnd();
        if (!Active)
        {
            prtHand.Stop();
            prtGroundHit.Play();
            Active = true;
        }
    }
    void Update()
    {
        if (Active)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Ray ray = new Ray(cam.position, cam.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, attackDistance))
                {
                    anim.SetTrigger("Ulti1");
                    PassToCoolDown();
                    player.JumpToPoint(hit.point, punchAnim.length);
                }
            }
        }
    }

}
