using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InteractionIndicatorsUI : NetworkBehaviour
{
    [SerializeField] PlayerStatusManager psm;
    Transform cam;
    public bool InInteraction = false;
    [SerializeField] LayerMask interactibleLayer;
    [SerializeField] float detectionDistance = 10;
    [SerializeField] GameObject IndicatorCanvas;
    [SerializeField] Text keyLabel;
    [SerializeField] Text definitionLabel;
    [SerializeField] float resetCanInteractCooldown = 1;
    public override void OnNetworkSpawn()
    {
        cam = Camera.main.transform;
        IndicatorCanvas.SetActive(false);
    }
    private Transform lastobj;
    private Interactible lastobjITB;
    void Update()
    {
        if (!IsOwner) { return; }
        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;
        if (!psm.Status.Paused && !InInteraction && Physics.Raycast(ray, out hit, detectionDistance, interactibleLayer))
        {
            if (hit.transform != lastobj)
            {
                lastobjITB = hit.transform.GetComponent<Interactible>();
                if (lastobjITB != null && lastobjITB.CanInteract)
                {
                    IndicatorCanvas.SetActive(true);
                    keyLabel.text = lastobjITB.IntKey.ToString();
                    definitionLabel.text = lastobjITB.DefText;
                    lastobjITB.OnStopInteract += onStopInteract;
                }
                else
                {
                    IndicatorCanvas.SetActive(false);
                }
            }
        }
        else
        {
            IndicatorCanvas.SetActive(false);
            lastobj = null;
            lastobjITB = null;
        }
        if (lastobjITB != null && lastobjITB.CanInteract && Input.GetKeyDown(lastobjITB.IntKey))
        {
            InInteraction = true;
            lastobjITB.psm = psm;
            lastobjITB.Interact();
        }
    }
    void onStopInteract()
    {
        StartCoroutine(ResetCanInteract());
    }

    IEnumerator ResetCanInteract()
    {
        yield return new WaitForSeconds(resetCanInteractCooldown);
        InInteraction = false;
        if (lastobjITB != null)
        {
            lastobjITB.OnStopInteract -= onStopInteract;
        }
    }
}