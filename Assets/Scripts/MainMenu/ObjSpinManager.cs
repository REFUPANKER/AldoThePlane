using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjSpinManager : MonoBehaviour
{
    public float spinSpeed;
    [Tooltip("0:pve,1:pvp,2:freeflight,3:empty")]
    public Transform[] objs;
    Transform active;

    void Start()
    {
        empty();
    }
    void Update()
    {
        if (active != null && active.gameObject.activeSelf)
        {
            active.Rotate(0, spinSpeed * Time.deltaTime, 0);
        }
    }

    public void hoverPVE() { disableAll(); activate(0); }
    public void hoverPVP() { disableAll(); activate(1); }
    public void hoverFreeFlight() { disableAll(); activate(2); }
    public void empty() { disableAll(); activate(3); }
    void disableAll()
    {
        foreach (var item in objs)
        {
            item.gameObject.SetActive(false);
        }
    }
    void activate(int index)
    {
        active = objs[index];
        active.gameObject.SetActive(true);
    }

    public void openScene(int index)
    {
        switch (index)
        {
            case 0:
            SceneManager.LoadScene("PvEscene");
                break;
            case 1:
            SceneManager.LoadScene("PvPscene");
                break;
            case 2:
            SceneManager.LoadScene("FlightScene");
                break;
        }
    }
}
