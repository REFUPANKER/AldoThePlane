using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSphere : MonoBehaviour
{
    [SerializeField] private Animation anim;

    public float AnimLength { get { return anim.clip.length; } }

    public delegate void AnimEnd();
    public event AnimEnd OnAnimationEnded;

    public float Damage;

    public delegate void EnemyKilled(HealthManager enemy);
    public event EnemyKilled OnEnemyKilled;

    public void Play(bool disableAfterPlay)
    {
        gameObject.SetActive(true);
        anim.Play();
        if (disableAfterPlay)
        {
            StartCoroutine(disableAfterPlayed());
        }
    }

    IEnumerator disableAfterPlayed()
    {
        yield return new WaitForSeconds(AnimLength);
        anim.Stop();
        gameObject.SetActive(false);
        OnAnimationEnded?.Invoke();
    }

    public void Stop()
    {
        anim.Stop();
    }

    void OnTriggerEnter(Collider col)
    {
        Tower t = col.GetComponent<Tower>();
        t?.TakeDamage(Damage);
        HealthManager h = col.GetComponent<HealthManager>();
        h?.TakeDamage(Damage);
        if (h?.health <= 0)
        {
            OnEnemyKilled?.Invoke(h);
        }
    }
}
