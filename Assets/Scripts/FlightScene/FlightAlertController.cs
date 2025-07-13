using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class FlightAlertController : MonoBehaviour
{
    /*
    ----- how it works ----- 
    detects objects
    and adds their radar image inside them
    holds previous image replacements on objects for not overloading
    */
    [Header("Radar")]
    public LayerMask detectables;
    public float RadarRadius = 500;
    public Transform radarItemsHolder;
    public float RadarRescanTime = 0.5f;

    [Tooltip("INWORLD CANVAS IMAGE - detectable item and image name must match.For Example : FlightMissile(Layermask) : Missile(Image)")]
    public GameObject[] radarImages;
    public float radarObjectsAltitude = 500;

    void MakeScan()
    {
        Collider[] radarCols = Physics.OverlapSphere(transform.position, RadarRadius, detectables);
        if (radarCols.Length > 0)
        {
            ReplaceMatchingIcon(radarCols);
        }
    }
    void Start()
    {
        for (int i = 0; i < radarImages.Length; i++)
        {
            radarImages[i].gameObject.SetActive(false);
        }
        StartCoroutine(RadarRescanner());
    }

    IEnumerator RadarRescanner()
    {
        MakeScan();
        yield return new WaitForSeconds(RadarRescanTime);
        StartCoroutine(RadarRescanner());
    }

    void ReplaceMatchingIcon(Collider[] cols)
    {
        for (int i = 0; i < radarItemsHolder.childCount; i++)
        {
            Destroy(radarItemsHolder.GetChild(i).gameObject);
        }

        for (int i = 0; i < cols.Length; i++)
        {
            if (!cols[i].gameObject.activeSelf) { continue; }
            GameObject getImg = GetImageByLayerName(cols[i].gameObject.layer);
            if (getImg != null)
            {
                GameObject instImg = Instantiate(getImg, radarItemsHolder);
                instImg.SetActive(true);
                instImg.transform.rotation = Quaternion.Euler(-90, 0, Math.Abs(cols[i].transform.rotation.eulerAngles.y) - 180);
                instImg.transform.position = new Vector3(cols[i].transform.position.x, radarObjectsAltitude, cols[i].transform.position.z);
            }
        }
    }

    GameObject GetImageByLayerName(int layer)
    {
        GameObject tryGetObj = radarImages.Where(i => i.layer == layer).FirstOrDefault();
        return tryGetObj;
    }
}
