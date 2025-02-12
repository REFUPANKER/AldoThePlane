using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : SkillTemplate
{
    public Movement player;
    public Transform cam;
    public Attack attack;
    public float attackDistance = 10f;
    [Header("Particles")]
    public ParticleSystem prtHand;
    public ParticleSystem prtGroundHit;
    [Header("Animations")]
    public Animator anim;
    public float punchAnimLength;
    public DamageSphere sphere;
    
    bool InAttack;
    Enemy target;
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
            Active = false;
            InAttack = false;
        }
    }

    void Update()
    {
        if (Active && !IsBlocked)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !InAttack)
            {
                Ray ray = new Ray(cam.position, cam.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, attackDistance) && hit.transform.tag == "Target")
                {
                    target = hit.transform.GetComponent<Enemy>();
                    anim.SetTrigger("Ulti1");
                    PassToCoolDown();
                    player.JumpToPoint(hit.point, punchAnimLength);
                    InAttack = true;
                }
            }
        }
    }

    void Start()
    {
        player.OnLanded += handleLanding;
    }

    void handleLanding()
    {
        if (IsBlocked) { return; }
        Active = false;
        InAttack = false;
        prtHand.Stop();
        prtGroundHit.Play();
        sphere.Play(true);
        if (target != null)
        {
            target.TakeDamage(Damage);
            target = null;
        }
    }
    public override void OnBlocked()
    {
        prtHand.Stop();
    }
}
