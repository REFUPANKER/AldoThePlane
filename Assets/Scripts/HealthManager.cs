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
    public RectTransform healthBar;
    private float _h;
    private float _hw;

    [Header("Rotate ui to camera")]
    public Transform cam;
    public Transform ui;
    void Start()
    {
        cam = Camera.main.transform;
        _h = health;
        _hw = healthBar.rect.width;
    }

    void LateUpdate()
    {
        if (health > 0)
        {
            pos = transform.position;
            if (Vector3.Distance(cam.position, transform.position) <= 10)
            {
                ui.LookAt(cam.position);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthBar.sizeDelta = new Vector2(health > 0 ? health * 100 / _h / 100 * _hw : 0, healthBar.rect.height);
        if (health > 0)
        {

        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
