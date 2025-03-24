using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Hero_MashaMovement : MonoBehaviour
{
    public bool IsPlayerControlled = false;
    public bool CanMove = true;
    public float AttackRadius = 5;
    public LayerMask TargetLayer;

    [Header("Damage management")]
    public bool Escaping = false;
    public HealthManager health;
    public float escapeCooldown;

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

    void Start()
    {
        health.OnTakeDamage += TookDamage;
        agent.enabled = !IsPlayerControlled;
        ctrl.enabled = IsPlayerControlled;
        BackToTower();
    }

    float lastDamageTaken = 0f;
    void TookDamage()
    {
        bool cHealth = health.health <= health._h / 2;
        bool cByTower = target.name == "Parts" || target.name.Contains("Tower");
        if (cHealth || cByTower)
        {
            Escaping = true;
        }
        lastDamageTaken = Time.time;
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
                    if (Time.time - lastDamageTaken > escapeCooldown)
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
                        if (target != null)
                        {
                            if (Vector3.Distance(transform.position, target.position) > AttackRadius)
                            {
                                target = cols[0].transform;
                            }
                        }
                        else
                        {
                            target = cols[0].transform;
                        }
                    }
                    else
                    {
                        BackToTower();
                    }
                    agent.SetDestination(target.position);


                    if (target != null && Vector3.Distance(transform.position, target.position) < AttackRadius)
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
