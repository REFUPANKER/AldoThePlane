using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill3 : SkillTemplate
{
    public Animation TargetsUI;
    public Color32 UiTargetSelectedColor = new Color32(20, 255, 40, 255);
    public Image[] TargetsUiImages;
    public bool TargetSelected = false;
    public Transform enemiesHolder;
    public EnemyDwarf[] enemies;
    void Start()
    {
        enemies = enemiesHolder.GetComponentsInChildren<EnemyDwarf>();
        if (enemies.Length >= TargetsUiImages.Length)
        {
            for (int i = 0; i < TargetsUiImages.Length; i++)
            {
                TargetsUiImages[i].sprite = enemies[i].ThumbnailImage;
            }
        }
    }

    public override void Activate()
    {
        if (!Active && !InCoolDown)
        {
            TargetsUI.Play("HeroUltiPlane_UiTargets_In");
            base.Activate();
        }
    }

    public override void OnActiveTimeEnd()
    {
        base.OnActiveTimeEnd();
        if (!TargetSelected)
        {
            TargetsUI.Play("HeroUltiPlane_UiTargets_Out");
            Active = false;
        }
    }

    bool alpha3InputFix;
    void Update()
    {
        if (!alpha3InputFix && Input.GetKeyUp(KeyCode.Alpha3))
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
    }

    void SelectUiTarget(int index)
    {
        PassToCoolDown();
        TargetsUiImages[index].color = UiTargetSelectedColor;
        TargetsUI.Play("HeroUltiPlane_UiTargets_Out");
        TargetSelected = true;
        Active = false;
        Debug.Log((index+1) + " selected");
    }
}
