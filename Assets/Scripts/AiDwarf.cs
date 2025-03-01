using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class AiDwarf : MonoBehaviour
{
    private NavMeshAgent agent;
    public float enemyDetectionDistance = 10;
    public LayerMask EnemyLayer;
    public Transform EnemyCorridorTowersHolder;
    public Transform target;
    public Collider[] cols;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        BackToTower();
    }

    void Update()
    {
        cols = Physics.OverlapSphere(transform.position, enemyDetectionDistance, EnemyLayer);
        if (cols.Length > 0)
        {
            float shortest = Vector3.Distance(transform.position, cols[0].transform.position);
            for (int i = 0; i < cols.Length; i++)
            {
                float d = Vector3.Distance(transform.position, cols[i].transform.position);
                if (d <= shortest)
                {
                    shortest = d;
                    target = cols[i].transform;
                }
            }
            agent.SetDestination(target.position);
        }
        else
        {
            BackToTower();
        }
    }

    void BackToTower()
    {
        Tower[] towers = EnemyCorridorTowersHolder.GetComponentsInChildren<Tower>();
        towers = towers.Where(x => x.gameObject.activeSelf == true).ToArray();
        if (towers.Length > 0 && towers[0] != null)
        {
            target = towers[0].transform;
            agent.SetDestination(target.position);
        }
    }
}
