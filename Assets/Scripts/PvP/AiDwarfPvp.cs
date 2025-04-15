using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AiDwarfPvp : NetworkBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] float detectionRange;
    [SerializeField] Transform target;
    [SerializeField] Transform towersHolder;
    void Update()
    {
        if (!IsOwner) { return; }
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }
    void LateUpdate()
    {
        if (!IsOwner) { return; }
        Collider[] c = Physics.OverlapSphere(agent.transform.position, detectionRange, targetLayer);
        if (c.Length > 0)
        {
            foreach (var item in c)
            {
                if (item.transform != target)
                {
                    target = c[0].transform;
                    break;
                }
            }
        }
        else
        {
            target = null;
            // back to towers
        }
    }
}
