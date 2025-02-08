using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTemplate : MonoBehaviour
{
    public bool InUse;
    public float ActiveTime;
    public float Cooldown;
    private void ItsInUse()
    {
        if (InUse)
            return;
        InUse = true;
        StartCoroutine(EnableActiveTime());
    }
    public virtual void Activate()
    {
        ItsInUse();
        // setup skill
    }
    public virtual IEnumerator EnableActiveTime()
    {
        yield return new WaitForSeconds(ActiveTime);
        InUse = false;
    }

    public virtual void Use()
    {
        ItsInUse();
    }

    public virtual void Use(GameObject target)
    {
        ItsInUse();
    }
}
