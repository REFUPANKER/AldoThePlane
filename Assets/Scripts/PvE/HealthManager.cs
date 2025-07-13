using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Variation variation;
    public Sprite ThumbnailImage;
    public Vector3 pos;
    [Header("Health")]
    public float health = 100;
    private float dhealth;
    public Slider healthbar;
    public bool DestroyAfterDeath = true;
    public bool DeathIsHandled = false;
    public delegate void OnTakekDamage();
    public event OnTakekDamage OnTakeDamage;
    public delegate void OnDeath();
    public event OnDeath OnDied;

    [Header("Rotate ui to camera")]
    public Transform cam;
    public Transform ui;
    void Start()
    {
        dhealth = health;
        cam = Camera.main.transform;
        healthbar.maxValue = health;
        healthbar.value = health;
    }

    void LateUpdate()
    {
        if (health > 0)
        {
            pos = transform.position;
            Collider[] c = Physics.OverlapSphere(transform.position, 5);
            if (c.Length > 0)
            {
                ui.LookAt(cam.position);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthbar.value = health;
        OnTakeDamage?.Invoke();
        if (health > 0)
        {
            // damage anims
        }
        else
        {
            OnDied?.Invoke();
            if (!DeathIsHandled)
            {
                if (DestroyAfterDeath)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    public void Respawn()
    {
        health = dhealth;
        healthbar.value = health;
    }
}
