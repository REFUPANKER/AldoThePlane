using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// make sure call base functions
/// </summary>
public class SkillTemplate : MonoBehaviour
{
    [Header("Parameters")]
    public bool Active;
    public bool InCoolDown;
    [SerializeField] private float ActiveTime;
    [SerializeField] private float Cooldown;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI UiText;
    [SerializeField] private Image UltiSlot;
    [SerializeField] private Color32 DisabledUltiColor = new Color32(50, 50, 50, 255);
    [SerializeField] private Color32 ActivatedTextColor = new Color32(255, 180, 0, 255);

    /// <summary>
    /// Countdown variables
    /// <br></br>_cd : Cooldown
    /// <br></br>_at : Active time
    /// </summary>
    private float _cd, _at;

    public virtual void Activate()
    {
        if (Active || InCoolDown)
            return;
        Active = true;
        InCoolDown = true;
        _cd = Cooldown;
        _at = ActiveTime;
        UiText.color = ActivatedTextColor;
        SwitchUiState(Active);
        StartCoroutine(EnableActiveTime());
    }

    public virtual IEnumerator EnableActiveTime()
    {
        if (_at < 1)
        {
            Active = false;
            UiText.color = Color.white;
            OnActiveTimeEnd();
            if (_cd > 0)
            {
                StartCoroutine(EnableCooldown());
            }
            else
            {
                InCoolDown = false;
                SwitchUiState(InCoolDown);
                UiText.text = "";
                OnCoolDownEnd();
            }
            yield break;
        }
        UiText.text = _at.ToString();
        _at--;
        _cd--;
        yield return new WaitForSeconds(1);
        StartCoroutine(EnableActiveTime());
    }

    public virtual IEnumerator EnableCooldown()
    {
        if (_cd < 1)
        {
            InCoolDown = false;
            SwitchUiState(InCoolDown);
            UiText.text = "";
            OnCoolDownEnd();
            yield break;
        }
        UiText.text = _cd.ToString();
        _cd--;
        yield return new WaitForSeconds(1);
        StartCoroutine(EnableCooldown());
    }

    public void PassToCoolDown()
    {
        _at = 0;
    }

    public virtual void OnActiveTimeEnd()
    {

    }

    public virtual void OnCoolDownEnd()
    {

    }

    /// <summary>
    /// Change ulti slot color by cooldown status
    /// </summary>
    public void SwitchUiState(bool state)
    {
        if (state)
        {
            UltiSlot.color = DisabledUltiColor;
        }
        else
        {
            UltiSlot.color = Color.white;
        }
    }
}
