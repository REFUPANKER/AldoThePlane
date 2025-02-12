using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public Sprite ThumbnailImage;
    public Vector3 pos;
    public float health = 100;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (health > 0)
        {
            pos = transform.position;
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health > 0)
        {

        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
