using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    [SerializeField] private LayerMask TeammateLayerMask;
    [SerializeField] private LayerMask EnemyLayerMask;
    [SerializeField] private Animator anims;
    [SerializeField] private Transform cam;
    [SerializeField] private float raycastDistance = 100f;

    [SerializeField] private GameObject lastTarget;

    [Header("Skills")]
    [SerializeField] private Skill1 skill1;
    [SerializeField] private Skill2 skill2;
    [SerializeField] private Skill3 skill3;

    [Header("Normal Strike")]
    [SerializeField] private AnimationClip[] StrikeAnims;
    public int StrikeStack = 0;
    private float lastAttackTime;
    [SerializeField] private float comboResetTime = 1.5f;

    void Update()
    {
        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, raycastDistance, TeammateLayerMask | EnemyLayerMask))
        {
            SelectTarget(hit);
        }
        else
        {
            DeselectTarget();
        }


        if (Input.GetMouseButtonDown(0) && lastTarget != null && !skill1.Active && !skill3.Active)
        {
            if (((1 << lastTarget.layer) & EnemyLayerMask.value) != 0)
            {
                Debug.Log("attacking");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) && !skill3.Active)
        {
            skill1.Activate();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !skill3.Active)
        {
            skill2.Activate();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            skill3.Activate();
        }

        if (Time.time - lastAttackTime > comboResetTime)
        {
            StrikeStack = 0;
            anims.SetFloat("StrikeIndex", StrikeStack);
        }
    }

    void SelectTarget(RaycastHit hit)
    {
        GameObject target = hit.transform.gameObject;
        Outline outline = target.GetComponent<Outline>();

        if (outline)
        {
            if (lastTarget != target)
            {
                DeselectTarget();
            }

            outline.enabled = true;
            lastTarget = target;
        }
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
