using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Target selecting settings")]
    public LayerMask TargetLayer;
    public List<Transform> targets = new List<Transform>();

    [Header("Attack metrics")]
    public float Cooldown;
    public float Damage = 25;

    [Header("Eye details")]
    public Transform eye;
    private Vector3 eyeCenter;
    public float eyeMovementLimit = 1f;
    public LineRenderer TargetSelectedLine;

    void Start()
    {
        eyeCenter = eye.localPosition;
        TargetSelectedLine.gameObject.SetActive(false);
    }

    void Update()
    {
        if (targets.Count > 0 && targets[0] != null)
        {
            FollowTarget();
        }
        else
        {
            ReturnToCenter();
        }
    }

    /// <summary>
    /// recrusive
    /// </summary>
    IEnumerator Attack()
    {
        if (targets.Count > 0 && targets[0] != null)
        {
            Transform target = targets[0];
            Debug.Log("TOWER ATTACK");
            switch (target.tag)
            {
                case "Player":
                Debug.Log("Attacking to Player");
                    break;
                case "Dwarf":
                Debug.Log("Attacking to Dwarf");
                    break;
            }
            yield return new WaitForSeconds(Cooldown);
            StartCoroutine(Attack());
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (((1 << col.gameObject.layer) & TargetLayer.value) != 0 && !targets.Contains(col.transform))
        {
            targets.Add(col.transform);
            StartCoroutine(Attack());
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (targets.Contains(col.transform))
        {
            targets.Remove(col.transform);
        }
        if (targets.Count > 0 && targets[0] != null)
        {
            FollowTarget();
        }
        else
        {
            ReturnToCenter();
        }
    }

    void FollowTarget()
    {
        Transform target = targets[0];
        Vector3 targetPos = new Vector3(target.position.x, eye.position.y, target.position.z);
        Vector3 desiredPos = Vector3.MoveTowards(eye.localPosition, eyeCenter + (targetPos - transform.position), eyeMovementLimit);
        eye.localPosition = new Vector3(
            Mathf.Clamp(desiredPos.x, eyeCenter.x - eyeMovementLimit, eyeCenter.x + eyeMovementLimit),
            eyeCenter.y,
            Mathf.Clamp(desiredPos.z, eyeCenter.z - eyeMovementLimit, eyeCenter.z + eyeMovementLimit)
        );
        eye.LookAt(targetPos);

        // draw line
        if (!TargetSelectedLine.gameObject.activeSelf) { TargetSelectedLine.gameObject.SetActive(true); }
        TargetSelectedLine.SetPositions(new Vector3[] { eye.position, target.position });
    }

    void ReturnToCenter()
    {
        eye.localPosition = Vector3.Lerp(eye.localPosition, eyeCenter, Time.deltaTime * 2f);

        // disable line
        if (TargetSelectedLine.gameObject.activeSelf) { TargetSelectedLine.gameObject.SetActive(false); }
        TargetSelectedLine.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
    }
}
