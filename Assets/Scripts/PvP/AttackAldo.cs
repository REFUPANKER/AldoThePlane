using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackAldo : NetworkBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] float attackDistance = 10;
    [SerializeField] GameObject lastTarget;

    void Update()
    {
        if (!IsOwner) { return; }
        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, attackDistance))
        {
            if (hit.transform != transform)
            {
                SelectTarget(hit.transform);
            }
        }
        else
        {
            DeselectTarget();
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
        else
        {
            DeselectTarget();
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
