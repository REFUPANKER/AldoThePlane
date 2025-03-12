using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Skill1 : SkillTemplate
{
    public LayerMask team2Layer;
    public Movement player;
    public Transform cam;
    public float attackDistance = 10f;

    [Header("Damage Stack")]
    public int DamageStack = 0;
    public TextMeshProUGUI StackText;

    [Header("Particles")]
    public ParticleSystem prtHand;
    public ParticleSystem prtGroundHit;

    [Header("Animations")]
    public Animator anim;
    public float punchAnimLength;
    public DamageSphere sphere;

    bool InAttack;
    HealthManager target;
    public override void Activate()
    {
        if (!Active && !InCoolDown)
        {
            prtHand.Play();
            base.Activate();
        }
    }

    public override void OnActiveTimeEnd()
    {
        base.OnActiveTimeEnd();
        if (!Active)
        {
            prtHand.Stop();
            Active = false;
            InAttack = false;
        }
    }

    void Update()
    {
        if (Active && !IsBlocked)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && !InAttack)
            {
                Ray ray = new Ray(cam.position, cam.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, attackDistance, team2Layer))
                {
                    target = hit.transform.GetComponent<HealthManager>();
                    anim.SetTrigger("Ulti1");
                    PassToCoolDown();
                    player.JumpToPoint(hit.transform.position, punchAnimLength);
                    InAttack = true;
                }
            }
        }
    }

    void Start()
    {
        player.OnLanded += handleLanding;
        sphere.OnEnemyKilled += enemyKilledWithSphere;
    }

    void enemyKilledWithSphere(HealthManager e)
    {
        if (e != null)
        {
            e.TakeDamage(Damage + DamageStack);
            switch (e.variation)
            {
                case Variation.Dwarf:
                    UpdateDamageStack(5);
                    break;
                case Variation.BigDwarf:
                    UpdateDamageStack(10);
                    break;
                case Variation.Monster:
                    UpdateDamageStack(15);
                    break;
                case Variation.Player:
                    UpdateDamageStack(20);
                    break;
                case Variation.BigMonster:
                    UpdateDamageStack(25);
                    break;
                case Variation.Turtle:
                case Variation.Lord:
                    UpdateDamageStack(30);
                    break;
            }
            target = null;
        }
    }

    void handleLanding()
    {
        if (IsBlocked) { return; }
        Active = false;
        InAttack = false;
        prtHand.Stop();
        prtGroundHit.Play();
        sphere.Play(true);
    }

    public override void OnBlocked()
    {
        prtHand.Stop();
    }

    public void UpdateDamageStack(int addStack)
    {
        DamageStack += addStack;
        StackText.text = DamageStack.ToString();
    }
}
