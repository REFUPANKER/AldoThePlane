using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Skill3 : SkillTemplate
{
    public Animation TargetsUI;
    public Color32 UiTargetSelectedColor = new Color32(20, 255, 40, 255);
    public Image[] TargetsUiImages = new Image[5];

    [Header("Enemies"), Tooltip("Auto Collecting on start")]
    public Transform enemiesHolder;
    public HealthManager[] enemies = new HealthManager[] { };

    public HealthManager target;
    private Vector3 lastPoint;
    public bool TargetSelected = false;
    public Movement player;

    [Header("Morph")]
    public GameObject playerObj;
    public GameObject planeObj;
    public ParticleSystem morphSmoke;
    public bool morphCompleted = false;

    [Header("Block skills")]
    public Skill1 dSkill1;
    public Skill2 dSkill2;

    public float AscendLimit = 5;
    public int InAirState = 0;

    [Header("Flight Speed"), Tooltip("Effects Damage : adds half of it to damage")]
    public float FlightSpeed = 4;
    public float MaxSpeed = 40;
    public float SpeedMultiplier = 0.1f;
    private float _BaseSpeed = 4;

    [Header("Hit to target point")]
    public DamageSphere damageSphere;
    public ParticleSystem groundHitParticles;

    void Start()
    {
        _BaseSpeed = FlightSpeed;
    }

    private void InitEnemies()
    {
        HealthManager[] es = enemiesHolder.GetComponentsInChildren<HealthManager>();
        enemies = es;
        for (int i = 0; i < TargetsUiImages.Length; i++)
        {
            if (i < es.Length)
            {
                TargetsUiImages[i].sprite = enemies[i].ThumbnailImage;
                TargetsUiImages[i].color = enemies[i].health <= 0 ||
                            !enemies[i].gameObject.activeInHierarchy ? Color.red : Color.white;
            }
            else
            {
                TargetsUiImages[i].sprite = null;
                TargetsUiImages[i].color = Color.white;
            }
        }
    }

    public override void Activate()
    {
        if (!Active && !InCoolDown && !TargetSelected)
        {
            InitEnemies();
            TargetsUI.Play("HeroUltiPlane_UiTargets_In");
            base.Activate();
        }
    }

    public override void OnActiveTimeEnd()
    {
        if (!TargetSelected)
        {
            TargetsUI.Play("HeroUltiPlane_UiTargets_Out");
        }
        Active = TargetSelected;
        alpha3InputFix = false;
    }

    Vector3 groundPoint;
    bool alpha3InputFix;
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            alpha3InputFix = true;
        }
        if (Active && alpha3InputFix)
        {
            if (!TargetSelected)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) { SelectUiTarget(0); }
                if (Input.GetKeyDown(KeyCode.Alpha2)) { SelectUiTarget(1); }
                if (Input.GetKeyDown(KeyCode.Alpha3)) { SelectUiTarget(2); }
                if (Input.GetKeyDown(KeyCode.Alpha4)) { SelectUiTarget(3); }
                if (Input.GetKeyDown(KeyCode.Alpha5)) { SelectUiTarget(4); }
            }
        }

        // --- FLIGHT---
        if (TargetSelected && target != null)
        {
            player.transform.LookAt(new Vector3(target.pos.x, target.pos.y + 1, target.pos.z));
        }
        if (target != null)
        {
            lastPoint = target.pos;
        }
        if (InAirState > 0)
        {
            groundPoint = new Vector3(player.transform.position.x, 0, player.transform.position.z);
        }
        switch (InAirState)
        {
            case 1:
                if (morphCompleted && Vector3.Distance(player.transform.position, groundPoint) >= AscendLimit)
                {
                    player.velocity.y = 0;
                    InAirState = 2;
                }
                break;
            case 2:
                float dif = Vector3.Distance(player.transform.position, lastPoint);
                if (planeObj.activeSelf && dif <= 1.5f)
                {
                    StartCoroutine(MorphTo(1));
                }
                if (dif <= 1)
                {
                    // APPLY DAMAGE
                    player.controller.excludeLayers = LayerMask.GetMask("Nothing");
                    player.velocity = Vector3.zero;
                    damageSphere.Play(true);
                    if (target != null)
                    {
                        target.TakeDamage(Damage + dSkill1.DamageStack + (FlightSpeed / 2));
                    }
                    groundHitParticles.Play();
                    InAirState = 3;

                }
                else
                {
                    player.velocity = player.transform.forward;
                    player.velocity *= FlightSpeed;
                    FlightSpeed += SpeedMultiplier;
                    FlightSpeed = Mathf.Clamp(FlightSpeed, _BaseSpeed, MaxSpeed);
                }
                break;
            case 3:
                FlightSpeed = _BaseSpeed;
                InAirState = 0;

                Active = false;
                target = null;
                TargetSelected = false;
                dSkill1.UnBlock();
                dSkill2.UnBlock();
                UnBlock();
                player.ApplyGravity = true;
                player.CanMove = true;
                break;
        }
    }

    /// <summary>
    /// dont forget to start coroutine<br></br>
    /// opt obj <br></br>
    /// 1   player<br></br>
    /// 2   plane
    /// </summary>
    /// <param name="opt"></param>
    IEnumerator MorphTo(int opt)
    {
        morphCompleted = false;
        morphSmoke.Play();
        switch (opt)
        {
            case 1:
                planeObj.SetActive(false);
                playerObj.SetActive(true);
                break;
            case 2:
                playerObj.SetActive(false);
                planeObj.SetActive(true);
                break;
        }
        yield return new WaitForSeconds(morphSmoke.main.duration / 3);
        morphCompleted = true;
    }

    void SelectUiTarget(int index)
    {
        if (index < enemies.Length && enemies[index].health > 0 && enemies[index].gameObject.activeInHierarchy)
        {
            alpha3InputFix = false;
            this.Block();
            dSkill1.Block();
            dSkill2.Block();
            PassToCoolDown();
            TargetsUiImages[index].color = UiTargetSelectedColor;
            TargetsUI.Play("HeroUltiPlane_UiTargets_Out");
            target = enemies[index];
            TargetSelected = true;
            player.CanMove = false;
            player.ApplyGravity = false;
            // prepare to flight
            player.controller.excludeLayers = 1;
            InAirState = 1;
            player.velocity.y = Mathf.Sqrt(-4f * player.gravity * player.jumpHeight);
            StartCoroutine(MorphTo(2));
        }
    }
}
