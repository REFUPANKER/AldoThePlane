using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1 : SkillTemplate
{
    public ParticleSystem particles;
    public Animator anim;
    public override void Activate()
    {
        base.Activate();
        particles.Play();
    }
    public override IEnumerator EnableActiveTime()
    {
        yield return base.EnableActiveTime();
        particles.Stop();
    }

    public override void Use(GameObject target)
    {
        base.Use();
        anim.SetTrigger("Ulti1");
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
