using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Target selecting settings")]
    public LayerMask TargetLayer;

    [Header("Attack metrics")]
    public float Cooldown = 1f;
    public float Damage = 25f;
    private bool canAttack = true;
    public TowerBullet bullet;

    [Header("Eye details")]
    public Transform eye;
    public LineRenderer TargetSelectedLine;
    public float eyeMovementLimit = 1f;
    private Vector3 eyeCenter;
    private Transform target;

    void Start()
    {
        eyeCenter = eye.localPosition;
        TargetSelectedLine.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target)
            FollowTarget();
        else
            ReturnToCenter();
    }

    void OnTriggerStay(Collider col)
    {
        if (((1 << col.gameObject.layer) & TargetLayer.value) == 0)
            return;

        target = col.transform;
        if (canAttack)
            StartCoroutine(AttackCoolDown());
    }

    void OnTriggerExit(Collider col)
    {
        if (col.transform == target)
            target = null;
    }

    IEnumerator AttackCoolDown()
    {
        canAttack = false;
        TowerBullet newBullet = Instantiate(bullet, transform);
        newBullet.Follow(eye.position, target);
        yield return new WaitForSeconds(Cooldown);
        canAttack = true;
    }

    void FollowTarget()
    {
        Vector3 targetPos = new Vector3(target.position.x, eye.position.y, target.position.z);
        Vector3 desiredPos = Vector3.MoveTowards(eye.localPosition, eyeCenter + (targetPos - transform.position), eyeMovementLimit);
        eye.localPosition = new Vector3(
            Mathf.Clamp(desiredPos.x, eyeCenter.x - eyeMovementLimit, eyeCenter.x + eyeMovementLimit),
            eyeCenter.y,
            Mathf.Clamp(desiredPos.z, eyeCenter.z - eyeMovementLimit, eyeCenter.z + eyeMovementLimit)
        );
        eye.LookAt(targetPos);

        TargetSelectedLine.gameObject.SetActive(true);
        TargetSelectedLine.SetPositions(new Vector3[] { eye.position, target.position });
    }

    void ReturnToCenter()
    {
        eye.localPosition = Vector3.Lerp(eye.localPosition, eyeCenter, Time.deltaTime * 2f);
        TargetSelectedLine.gameObject.SetActive(false);
    }
}
