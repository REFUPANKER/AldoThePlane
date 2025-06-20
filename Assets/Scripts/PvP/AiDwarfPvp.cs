
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.AI;

// public class AiDwarfPvp : NetworkBehaviour
// {
//     [SerializeField] NavMeshAgent agent;
//     [SerializeField] LayerMask targetLayer;
//     [SerializeField] float detectionRange;
//     [SerializeField] Transform towersHolder;
//     [SerializeField] Animator anims;
//     [SerializeField] strtarget _target;
//     //TODO: fix ai reselecting target when HOST gets into range of agent when its following different target
//     struct strtarget : INetworkSerializable
//     {
//         public Vector3 pos;
//         public bool canFollow;

//         public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
//         {
//             s.SerializeValue(ref pos);
//             s.SerializeValue(ref canFollow);
//         }
//     }

//     NetworkVariable<strtarget> target = new NetworkVariable<strtarget>();
//     void Update()
//     {
//         if (IsServer && target.Value.canFollow)
//         {
//             agent.SetDestination(target.Value.pos);
//             anims.SetFloat("velocity",agent.velocity.magnitude);
//         }
//     }
//     void LateUpdate()
//     {
//         if (!IsServer) { return; }
//         Collider[] c = Physics.OverlapSphere(agent.transform.position, detectionRange, targetLayer);
//         if (c.Length > 0)
//         {
//             bool selectable = c[0] != null && c[0].gameObject.activeSelf;
//             setTargetServerRpc(selectable ? c[0].transform.position : Vector3.zero);
//         }
//     }

//     [ServerRpc(RequireOwnership = false)]
//     void setTargetServerRpc(Vector3 pos, bool cf = true)
//     {
//         target.Value = new strtarget { pos = pos, canFollow = cf };
//         setTargetClientRpc(pos, cf);
//     }
//     [ClientRpc]
//     void setTargetClientRpc(Vector3 pos, bool cf)
//     {
//         _target.pos = pos;
//         _target.canFollow = cf;
//     }

// }


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class AiDwarfPvp : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator anims;

    public float attackCooldown = 5;
    public float damage = 15;
    bool canAttack = true;

    public float enemyDetectionDistance = 10;
    public LayerMask EnemyLayer;
    public Transform EnemyCorridorTowersHolder;
    public Transform target;
    public Collider[] cols;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anims = GetComponent<Animator>();
        //BackToTower();
        BackToTowerServerRpc();
    }

    void Update()
    {
        if (!IsServer) { return; }
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
            //BackToTower();
            BackToTowerServerRpc();
        }
        anims.SetFloat("velocity", agent.velocity.magnitude);
    }
    void LateUpdate()
    {
        if (IsServer)
        {
            if (target != null && Vector3.Distance(transform.position, target.position) <= agent.stoppingDistance)
            {
                AttackServerRpc();
            }
        }
    }

    [ServerRpc]
    void AttackServerRpc()
    {
        Attack();
        AttackClientRpc();
    }
    [ClientRpc]
    void AttackClientRpc()
    {
        Attack();
    }
    void Attack()
    {
        if (!canAttack) { return; }
        anims.ResetTrigger("attack");
        anims.SetTrigger("attack");
        canAttack = false;
        transform.LookAt(new Vector3(target.position.x, 0, target.position.z));

        HealthManagerPvP h = target.GetComponentInChildren<HealthManagerPvP>();
        h?.TakeDamage(damage);

        Tower t = target.GetComponent<Tower>();
        t?.TakeDamage(damage);

        StartCoroutine(reactivateCanAttack());
    }
    IEnumerator reactivateCanAttack()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
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
    [ServerRpc]
    void BackToTowerServerRpc()
    {
        BackToTower();
        BackToTowerClientRpc();
    }
    [ClientRpc]
    void BackToTowerClientRpc()
    {
        BackToTower();
    }
}
