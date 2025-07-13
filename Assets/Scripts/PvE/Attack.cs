using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    public Movement player;
    [SerializeField] private LayerMask TeammateLayerMask;
    [SerializeField] private LayerMask EnemyLayerMask;
    [SerializeField] private Animator anims;
    [SerializeField] private Transform cam;
    [SerializeField] private float raycastDistance = 10f;
    public float damage = 15;

    [SerializeField] private GameObject lastTarget;

    [Header("Skills")]
    [SerializeField] private Skill1 skill1;
    [SerializeField] private Skill2 skill2;
    [SerializeField] private Skill3 skill3;

    [Header("Normal Strike")]
    [SerializeField] private AnimationClip[] StrikeAnims;
    public int StrikeStack = 0;
    public bool canAnimAttack;

    void Update()
    {
        if (player.InFpsCam)
        {
            Ray ray = new Ray(cam.position, cam.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, raycastDistance, TeammateLayerMask | EnemyLayerMask))
            {
                SelectTarget(hit.transform);
            }
            else
            {
                DeselectTarget();
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, TeammateLayerMask | EnemyLayerMask))
            {
                if (Vector3.Distance(transform.position, hit.point) <= raycastDistance)
                {
                    SelectTarget(hit.transform);
                }
            }
            else
            { DeselectTarget(); }
        }





        if (Input.GetMouseButtonDown(0) &&
            lastTarget != null && !skill1.Active && !skill3.Active &&
            ((1 << lastTarget.layer) & EnemyLayerMask.value) != 0
            )
        {
            canAnimAttack = true;
            StartCoroutine(attackAnims());
        }
        if (Input.GetMouseButtonUp(0))
        {
            canAnimAttack = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !skill3.Active)
        {
            skill1.Activate();
            anims.ResetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !skill3.Active)
        {
            skill2.Activate();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            skill3.Activate();
            anims.ResetTrigger("Attack");
        }
    }

    bool inanim = false;
    IEnumerator attackAnims()
    {
        if (inanim) { yield break; }
        inanim = true;
        AnimationClip c = StrikeAnims[StrikeStack > 0 ? StrikeStack - 1 : 0];
        anims.SetFloat("StrikeIndex", StrikeStack);
        anims.SetTrigger("Attack");
        StrikeStack = StrikeStack >= StrikeAnims.Length ? 0 : StrikeStack + 1;
        if (lastTarget)
        {
            HealthManager h = lastTarget.GetComponent<HealthManager>();
            h?.TakeDamage(damage);
            Tower t = lastTarget.GetComponent<Tower>();
            t?.TakeDamage(damage);
        }
        yield return new WaitForSeconds(c.length * 0.7f);
        inanim = false;
        if (canAnimAttack)
        {
            StartCoroutine(attackAnims());
        }
        else
        {
            StrikeStack = 0;
            anims.SetFloat("StrikeIndex", StrikeStack);
            anims.ResetTrigger("Attack");
            canAnimAttack = false;
        }
    }

    void SelectTarget(Transform hit)
    {
        GameObject target = hit.gameObject;
        Outline outline = target.GetComponent<Outline>();

        if (outline)
        {
            if (lastTarget != target)
            {
                DeselectTarget();
            }
            outline.enabled = true;
        }
        lastTarget = target;
    }

    void DeselectTarget()
    {
        if (lastTarget != null)
        {
            Outline outline = lastTarget.GetComponent<Outline>();
            if (outline)
            {
                outline.enabled = false;
            }
            lastTarget = null;
        }
    }
}
