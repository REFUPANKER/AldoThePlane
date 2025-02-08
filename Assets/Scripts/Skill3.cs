using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill3 : SkillTemplate
{
    public Animation TargetsUI;
    public override void Activate()
    {
        base.Activate();
        TargetsUI.Play("HeroUltiPlane_UiTargets_In");
    }

    public override IEnumerator EnableActiveTime()
    {
        yield return base.EnableActiveTime();
        TargetsUI.Play("HeroUltiPlane_UiTargets_Out");
        Debug.Log("Target selection time over");
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
