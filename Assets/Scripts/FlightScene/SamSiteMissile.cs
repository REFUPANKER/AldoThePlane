using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class SamSiteMissile : MonoBehaviour
{
    public LayerMask missileFiredLayer;
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

    public float turnSpeed = 5;

    public bool exploded = false;
    public bool fired = false;
    public bool flared = false;
    public Vector3 target;

    public bool missedTarget = false;

    public bool SearchingTarget;
    public float NoTargetDetonationTime = 10f;

    public void Fire(Vector3 t)
    {
        missileParticles.Play();
        fired = true;
        transform.parent = null;
        StartCoroutine(landUp());
        target = t;
        gameObject.layer = (int)Math.Log(missileFiredLayer.value, 2);
    }

    IEnumerator landUp()
    {
        yield return new WaitForSeconds(landupTime);
        landedUp = true;
    }

    void Update()
    {
        if (exploded || !fired) { return; }
        DetectFlares();
        SearchTarget();
        if (!flared || !missedTarget)
        {
            if (Vector3.Distance(transform.position, target) < detonationRange) { Explode(); }
        }
    }

    void FixedUpdate()
    {
        if (exploded || !fired) { return; }
        if (!landedUp)
        {
            transform.position += transform.forward * landupSpeed * Time.deltaTime;
            return;
        }
        if (!flared || !missedTarget)
        {
            Quaternion lookRotation = Quaternion.LookRotation((target - transform.position).normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
        }
        transform.position += transform.forward * speed * Time.deltaTime;
        speed = Math.Clamp(speed + speedMultiplier, 0, maxSpeed);
    }

    Collider scanForEnemy()
    {
        Collider[] e = Physics.OverlapSphere(transform.position, range, planeLayer);
        return e.Length > 0 ? e[0] : null;
    }
    void SearchTarget()
    {
        Collider e = scanForEnemy();
        if (e != null)
        {
            target = e.transform.position;
        }
        else if (!SearchingTarget)
        {
            StartCoroutine(searchTarget());
        }
    }
    IEnumerator searchTarget()
    {
        SearchingTarget = true;
        yield return new WaitForSeconds(NoTargetDetonationTime);
        Collider e = scanForEnemy();
        if (e == null)
        {
            missedTarget = true;
            Explode();
        }
        else
        {
            target = e.transform.position;
            SearchingTarget = false;
        }
    }

    void DetectFlares()
    {
        if (!flared)
        {
            Collider[] flares = Physics.OverlapSphere(transform.position, range, flareLayer);
            flared = flares.Length > 0;
            missedTarget = flared;
        }
    }

    void Explode()
    {
        missileParticles.Stop();
        exploded = true;
        ParticleSystem exp = Instantiate(explosionParticles, transform);
        exp.transform.parent = null;
        exp.Play();
        Destroy(gameObject);
    }
}
