using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackAldo : NetworkBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] GameObject lastTarget;
    [SerializeField] LayerMask healthManagerLayer;

    [Header("Attack Attributes")]
    [SerializeField] float attackDistance = 10;
    [SerializeField] float damage = 25;
    public override void OnNetworkSpawn()
    {
        cam = Camera.main.transform;
    }

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
        if (Input.GetKeyDown(KeyCode.Mouse0) && Physics.Raycast(ray, out hit, attackDistance, healthManagerLayer))
        {
            HealthManagerPvP h = hit.transform.GetComponent<HealthManagerPvP>();
            h?.TakeDamage(damage);
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
