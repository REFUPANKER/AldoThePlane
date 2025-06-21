using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SamSite : MonoBehaviour
{
    public LayerMask planeLayermask;
    public float range = 100;
    public bool CanFire = true;
    public float FireCooldown = 5;
    public List<SamSiteMissile> missiles;
    public float rotatingSpeed = 5;
    void Update()
    {
        if (missiles.Count == 0)
        {
            this.enabled = false;
        }
        Collider[] cols = Physics.OverlapSphere(transform.position, range, planeLayermask);
        if (cols.Length > 0)
        {
            Vector3 dir = cols[0].transform.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-dir), Time.deltaTime * rotatingSpeed);
            if (CanFire)
            {
                FireMissile(cols[0].transform.position);
            }
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
