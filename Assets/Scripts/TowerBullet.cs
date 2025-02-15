using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBullet : MonoBehaviour
{
    public Vector3 startPos;
    public float damage;
    public Transform _target;
    public float speed = 10f;
    public Transform bulletsHolder;
    public float FollowLimit = 30;

    private float basedis;
    void Start()
    {
        transform.parent = bulletsHolder;
    }

    public void Follow(Vector2 StartPos, Transform target)
    {
        gameObject.SetActive(true);
        _target = target;
        this.startPos = StartPos;
    }

    void Update()
    {
        if (_target != null)
        {
            basedis = Vector3.Distance(startPos, _target.position);
            transform.LookAt(_target);
            transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);

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
        //Debug.Log($"Bullet reached : {damage}+{basedis} : {damage + basedis}");
        Destroy(gameObject);
    }
}
