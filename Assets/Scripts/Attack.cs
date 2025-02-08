using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    [SerializeField] private Animator anims;
    [SerializeField] private Transform cam;
    [SerializeField] private float raycastDistance = 100f;

    private GameObject lastTarget;

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

        // Target Selecting
        if (Physics.Raycast(ray, out hit, raycastDistance) && hit.collider.CompareTag("Target"))
        {
            SelectTarget(hit);
        }
        else
        {
            DeselectTarget();
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

    void PerformStrike()
    {
        lastAttackTime = Time.time;
        anims.SetTrigger("Attack");
        StrikeStack = (StrikeStack + 1) % 3;
        StartCoroutine(AdvanceCombo());
    }

    IEnumerator AdvanceCombo()
    {
        yield return new WaitForSeconds(StrikeAnims[StrikeStack].length / 2);
        anims.SetFloat("StrikeIndex", StrikeStack);
    }

    void SelectTarget(RaycastHit hit)
    {
        GameObject target = hit.transform.gameObject;
        if (lastTarget != target)
        {
            DeselectTarget();
        }

        Outline outline = target.GetComponent<Outline>();
        if (outline)
        {
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
