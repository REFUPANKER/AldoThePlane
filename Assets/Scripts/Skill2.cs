using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill2 : SkillTemplate
{
    public Animation shields;
    public override void Use()
    {
        base.Use();
        shields.Play();
    }
    public override IEnumerator EnableActiveTime()
    {
        yield return base.EnableActiveTime();
        Debug.Log("Shield off");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
