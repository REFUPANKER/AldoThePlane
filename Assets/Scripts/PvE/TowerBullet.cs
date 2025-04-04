using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBullet : MonoBehaviour
{
    private Vector3 _startPos;
    public float damage;
    public Transform _target;
    public float speed = 10f;
    public float FollowLimit = 30;
    private float _v = 1f;
    public float velocityPerStep = 0.005f;

    private float basedis;

    public void Follow(Vector3 StartPos, Transform target)
    {
        gameObject.SetActive(true);
        transform.position = StartPos;
        this._startPos = StartPos;
        _target = target;
    }

    void Update()
    {
        if (_target != null)
        {
            basedis = Vector3.Distance(_startPos, _target.position);
            transform.LookAt(_target);
            transform.position = Vector3.MoveTowards(transform.position, _target.position, _v * speed * Time.deltaTime);
            _v = Math.Clamp(_v, velocityPerStep, speed);
            if (Vector3.Distance(transform.position, _target.position) <= 0.1f)
            {
                HitTarget();
            }
            if (basedis > FollowLimit)
            {
                Destroy(gameObject);
            }
        }
    }

    void HitTarget()
    {
        Destroy(gameObject);
        HealthManager h = _target.GetComponent<HealthManager>();
        if (h)
        {
            h.TakeDamage(damage);
            // h.variation 
        }
    }
}
