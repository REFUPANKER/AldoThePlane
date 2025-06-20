using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamSiteMissile : MonoBehaviour
{
    public ParticleSystem missileParticles;
    public ParticleSystem explosionParticles;
    public LayerMask planeLayer;
    public LayerMask flareLayer;

    public bool landedUp;
    public float landupTime = 1;
    public float landupSpeed = 150;

    public float range = 100;
    public float detonationRange = 10;

    public float speed = 100;
    public float speedMultiplier = 0.1f;
    public float maxSpeed = 200;

    public bool exploded = false;
    public bool fired = false;
    public bool flared = false;
    public Vector3 target;

    public bool missedTarget = false;
    public float missTargetDetonationTime = 10;

    public void Fire(Vector3 t)
    {
        missileParticles.Play();
        fired = true;
        StartCoroutine(landUp());
        target = t;
    }

    void Update()
    {
        if (exploded || !fired) { return; }
        if (!landedUp)
        {
            transform.position += transform.forward * landupSpeed * Time.deltaTime;
            return;
        }
        Collider[] cols = Physics.OverlapSphere(transform.position, range, planeLayer);
        Collider[] flares = Physics.OverlapSphere(transform.position, range, flareLayer);
        flared = flares.Length > 0;
        if (cols.Length > 0 && !flared && !missedTarget)
        {
            target = cols[0].transform.position;
            transform.LookAt(target);
            if (Vector3.Distance(transform.position, target) < detonationRange)
            {
                Explode();
            }
        }
        else
        {
            StartCoroutine(missTargetDetonation());
        }
        transform.position += transform.forward * speed * Time.deltaTime;
        speed = Math.Clamp(speed + speedMultiplier, 0, maxSpeed);
    }

    IEnumerator landUp()
    {
        yield return new WaitForSeconds(landupTime);
        landedUp = true;
    }

    IEnumerator missTargetDetonation()
    {
        missedTarget = true;
        yield return new WaitForSeconds(missTargetDetonationTime);
        missileParticles.Stop();
        Explode();
    }

    void Explode()
    {
        exploded = true;
        ParticleSystem exp = Instantiate(explosionParticles, transform);
        exp.transform.parent = null;
        exp.Play();
        Destroy(gameObject);
    }
}
