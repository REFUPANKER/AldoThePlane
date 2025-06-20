using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamSite : MonoBehaviour
{
    public LayerMask planeLayermask;
    public float range = 100;
    public bool CanFire = true;
    public float FireCooldown = 5;
    public List<SamSiteMissile> missiles;

    void Update()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, range, planeLayermask);
        if (CanFire && cols.Length > 0)
        {
            FireMissile(cols[0].transform.position);
        }
    }

    void FireMissile(Vector3 t)
    {
        StartCoroutine(fireCooldown());
        for (int i = 0; i < missiles.Count; i++)
        {
            if (missiles[i] != null)
            {
                missiles[i].Fire(t);
                missiles.RemoveAt(i);
                break;
            }
        }
    }
    IEnumerator fireCooldown()
    {
        CanFire = false;
        yield return new WaitForSeconds(FireCooldown);
        CanFire = true;
    }
}
