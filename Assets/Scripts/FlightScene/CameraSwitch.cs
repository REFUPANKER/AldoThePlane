using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public FlightControl self;
    public FlightControl target;
    public GameObject cam;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            self.canMove = false;
            target.canMove = true;
            cam.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
