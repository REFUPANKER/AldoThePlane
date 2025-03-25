using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Hero_MashaMovement : MonoBehaviour
{
    public bool IsPlayerControlled;
    public bool CanMove = true;
    public float AttackRadius = 5;
    public LayerMask TargetLayer;

    [Header("Chasing")]
    public Transform lastTarget;
    public bool isChasing;
    public float ChaseTimeout = 3;

    [Header("Damage management")]
    public bool Escaping;
    public HealthManager health;
    public float escapeCooldown;
    public float safetyDistance = 7;

    [Header("Movement type setup")]
    public Animator anims;
    public NavMeshAgent agent;
    public CharacterController ctrl;

    [Header("Camera")]
    public Camera cam;

    [Header("Towers Holder")]
    public Transform towers;
    public Transform selfBaseTower;

    [Header("Targetting")]
    public Transform target;

    [Header("Attack")]
    public bool isAttacking;

    void Start()
    {
        health.OnTakeDamage += TookDamage;
        agent.enabled = !IsPlayerControlled;
        ctrl.enabled = IsPlayerControlled;
        BackToTower();
    }

    float escapeStart = 0f;
    float chaseStart = 0f;
    void TookDamage()
    {
        bool cHealth = health.health <= health._h / 2;
        bool cByTower = target.name == "Parts" || target.name.Contains("Tower");
        if (cHealth || cByTower)
        {
            Escaping = true;
        }
        escapeStart = Time.time;
    }

    void Update()
    {
        if (CanMove)
        {
            if (IsPlayerControlled)
            {

            }
            else
            {
                if (Escaping)
                {
                    if (Time.time - escapeStart > escapeCooldown || (target != null && Vector3.Distance(transform.position, target.position) >= safetyDistance))
                    {
                        Escaping = false;
                    }
                    agent.SetDestination(selfBaseTower.position);
                }
                else
                {
                    Collider[] cols = Physics.OverlapSphere(transform.position, AttackRadius, TargetLayer);
                    if (cols.Length > 0)
                    {
                        lastTarget = target;
                        target = cols[0].transform;
                        int decision = Random.Range(0, 2);
                        switch (decision)
                        {
                            case 0:
                                if (lastTarget == target && !isChasing && target.name != "Parts" && !target.name.Contains("Tower"))
                                {
                                    chaseStart = Time.time;
                                    isChasing = true;
                                }
                                break;
                            case 1:
                                if (!isAttacking)
                                {
                                    Debug.Log("Attack");
                                    isAttacking = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        BackToTower();
                    }
                    if (target != null)
                    {
                        agent.SetDestination(target.position);
                    }

                    if (isChasing && Time.time - chaseStart > ChaseTimeout)
                    {
                        isChasing = false;
                        Escaping = true;
                        escapeStart = Time.time;
                    }

                    if (target != null && Vector3.Distance(transform.position, target.position) < AttackRadius / 2)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(target.position - transform.position), Time.deltaTime * 100f);
                    }
                }
                anims.SetFloat("Velocity", agent.velocity.magnitude);
            }
        }
    }

    void BackToTower()
    {
        Tower[] t = towers.GetComponentsInChildren<Tower>();
        t = t.Where(x => x.gameObject.activeSelf == true).ToArray();
        if (t.Length > 0 && t[0] != null)
        {
            target = t[0].transform;
            agent.SetDestination(target.position);
        }
    }
}
