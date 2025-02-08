using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Attack : MonoBehaviour
{
    [SerializeField] private Movement player;
    [SerializeField] private Animator anims;
    [SerializeField] private Transform cam;
    [SerializeField] private float raycastDistance = 100f;

    private GameObject lastTarget;

    [Header("Skill Slots")]
    [SerializeField] private bool[] SkillesEnabled = { false, false, false, false };
    [SerializeField] private Image[] SkillSlots;
    [SerializeField] private Color SkillActivatedColor;
    [SerializeField] private TextMeshProUGUI[] SkillCooldownTexts;
    [SerializeField] private float[] SkillCooldowns = { 0f, 4f, 12f, 16f };

    [Header("Skills")]
    [SerializeField] private Skill1 skill1;
    [SerializeField] private Skill2 skill2;
    [SerializeField] private Skill3 skill3;

    [Header("Normal Strike")]
    [SerializeField] private AnimationClip[] StrikeAnims;
    public int StrikeStack = 0;
    private float lastAttackTime;
    [SerializeField] private float comboResetTime = 1.5f; // Kombo sıfırlama süresi

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

        // Skill Usage
        if (SkillesEnabled[3])
        {
            Debug.Log("Use Skill 3");
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && !SkillesEnabled[1])
            {
                StartCoroutine(SkillSlotCoroutine(1));
                skill1.Activate();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && !SkillesEnabled[2] && !SkillesEnabled[3])
            {
                StartCoroutine(SkillSlotCoroutine(2));
                skill2.Activate();
                skill2.Use();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && !SkillesEnabled[3])
            {
                StartCoroutine(SkillSlotCoroutine(3));
                skill3.Activate();
            }
        }


        // Strike
        // TODO: CHECK FOR TARGET
        if (Input.GetKeyDown(KeyCode.Mouse0) && !SkillesEnabled[3] && SkillesEnabled[1] && lastTarget != null)
        {
            skill1.Use(lastTarget);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && !SkillesEnabled[3] && !SkillesEnabled[1])
        {
            PerformStrike();
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

    IEnumerator SkillSlotCoroutine(int slot)
    {
        StartCoroutine(SkillCountdownText(slot, SkillCooldowns[slot]));
        SkillesEnabled[slot] = true;
        SkillSlots[slot].color = SkillActivatedColor;
        yield return new WaitForSeconds(SkillCooldowns[slot]);
        SkillSlots[slot].color = Color.white;
        SkillesEnabled[slot] = false;
    }

    IEnumerator SkillCountdownText(int slot, float cooldown)
    {
        SkillCooldownTexts[slot].text = cooldown == 0 ? "" : cooldown.ToString();
        yield return new WaitForSeconds(1);
        if (cooldown > 0)
        {
            StartCoroutine(SkillCountdownText(slot, cooldown - 1));
        }
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
