using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    /// <summary>
    /// uses collider's include/exclude layer systems<br></br>
    /// for example = in case of enemy layer is Team2<br></br>
    /// Include layer : Team2Layer                   <br></br>
    /// Exclude layer : everything except Team2Layer <br></br>
    /// also switch objects holder's layer to opposite layer (not root > usually "parts" obj)
    /// </summary>
    [Header("Target selecting settings")]
    public LayerMask TargetLayer;

    [Header("Attack metrics")]
    public float Cooldown = 1f;
    private bool canAttack = true;
    public TowerBullet bullet;
    public float range;

    [Header("Eye details")]
    public Transform eye;
    public LineRenderer TargetSelectedLine;
    public float eyeMovementLimit = 1f;
    private Vector3 eyeCenter;
    private Transform target;

    [Header("Health")]
    public float Health;
    public Slider healthbar;

    void Start()
    {
        healthbar.maxValue = Health;
        healthbar.value = Health;
        eyeCenter = eye.localPosition;
        TargetSelectedLine.gameObject.SetActive(false);
    }

    void Update()
    {
        if (target && target.gameObject.activeSelf)
            FollowTarget();
        else
            ReturnToCenter();

        Collider[] cols = Physics.OverlapSphere(transform.position, range, TargetLayer);
        if (cols.Length > 0)
        {
            target = cols[0].transform;
            if (canAttack)
            {
                StartCoroutine(AttackCoolDown());
            }
        }
        else
        {
            target = null;
        }
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
        TargetSelectedLine.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        TargetSelectedLine.gameObject.SetActive(false);
    }


    public void TakeDamage(float damage)
    {
        Health -= damage;
        healthbar.value = Health;
        if (Health <= 0)
        {
            Debug.Log("TOWER DOWN !!!");
        }
    }
}
