using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SamSite : MonoBehaviour
{
    public LayerMask planeLayermask;
    public float radius = 100;
    public bool CanFire = true;
    public float FireCooldown = 5;
    public List<SamSiteMissile> missiles;
    private int missileIndex = 0;
    public float rotatingSpeed = 5;

    public bool reloading = false;
    public float ReloadCooldown = 10;

    void Update()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, planeLayermask);
        if (cols.Length > 0 && !reloading)
        {
            Vector3 dir = cols[0].transform.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-dir), Time.deltaTime * rotatingSpeed);
            FireMissile(cols[0].transform.position);
        }
    }

    void FireMissile(Vector3 t)
    {
        if (!CanFire) { return; }
        StartCoroutine(fireCooldown());
        if (missileIndex >= 0 && missileIndex < missiles.Count)
        {
            SamSiteMissile newMissile = Instantiate(missiles[missileIndex], transform);
            missiles[missileIndex].gameObject.SetActive(false);
            newMissile.gameObject.SetActive(true);
            newMissile.Fire(t);
            missileIndex += 1;
        }
        else if (missileIndex >= missiles.Count)
        {
            StartCoroutine(reload());
        }
    }

    IEnumerator fireCooldown()
    {
        CanFire = false;
        yield return new WaitForSeconds(FireCooldown);
        CanFire = true;
    }

    IEnumerator reload()
    {
        reloading = true;
        for (int i = 0; i < missiles.Count; i++)
        {
            yield return new WaitForSeconds(missiles.Count);
            missiles[i].gameObject.SetActive(true);
        }
        missileIndex = 0;
        yield return new WaitForSeconds(1);
        reloading = false;
    }
}
