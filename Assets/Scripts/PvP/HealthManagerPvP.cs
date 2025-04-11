using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthManagerPvP : NetworkBehaviour
{

    [SerializeField] LayerMask healthManagerLayer;
    [SerializeField] GameObject layerTargetObject;
    [SerializeField] float health;
    [SerializeField] Canvas worldUi;
    [SerializeField] bool ShowSelfWorldUi = true;
    [SerializeField] Slider worldHealthbar;
    [SerializeField] Slider UiHealthbar;
    [SerializeField] float healthbarSmoothnes = 10f;

    [Header("Took damage ui effects")]
    [SerializeField] float scaleShakeAmount = 0.8f;
    [SerializeField] float scaleDuration = 0.2f;

    private NetworkVariable<float> nvHealth = new NetworkVariable<float>();
    public override void OnNetworkSpawn()
    {
        layerTargetObject.layer = Mathf.RoundToInt(Mathf.Log(healthManagerLayer.value, 2));
        worldHealthbar.maxValue = health;
        worldHealthbar.value = health;
        if (UiHealthbar != null)
        {
            UiHealthbar.maxValue = health;
            UiHealthbar.value = health;
        }
        if (IsOwner)
        {
            UServerRpc(health);
            worldUi.gameObject.SetActive(ShowSelfWorldUi);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void UServerRpc(float h)
    {
        nvHealth.Value = h;
        UClientRpc(h);
    }
    [ClientRpc]
    void UClientRpc(float h)
    {
        if (!IsOwner)
        {
            health = h;
        }
    }

    bool playingDamageEffects = false;
    public void TakeDamage(float damage)
    {
        ApplyDamageServerRpc(damage);
        if (!playingDamageEffects)
        {
            playingDamageEffects = true;
            StartCoroutine(SmoothScaleHealthbar(worldHealthbar.transform));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(float damage)
    {
        nvHealth.Value -= damage;
        health = nvHealth.Value;
    }

    void Update()
    {
        worldHealthbar.value = Mathf.Lerp(worldHealthbar.value, nvHealth.Value, Time.deltaTime * healthbarSmoothnes);
        if (UiHealthbar != null)
        {
            UiHealthbar.value = Mathf.Lerp(UiHealthbar.value, nvHealth.Value, Time.deltaTime * healthbarSmoothnes);
        }
    }
    void LateUpdate()
    {
        worldUi.transform.LookAt(worldUi.transform.position + Camera.main.transform.forward);
        worldUi.transform.rotation = Quaternion.Euler(0f, worldUi.transform.rotation.eulerAngles.y, 0f);
    }

    IEnumerator SmoothScaleHealthbar(Transform scalingTarget)
    {
        Vector3 originalScale = scalingTarget.transform.localScale;
        Vector3 targetScale = originalScale * scaleShakeAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / (scaleDuration / 2f);
            scalingTarget.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / (scaleDuration / 2f);
            scalingTarget.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        scalingTarget.transform.localScale = originalScale;
        playingDamageEffects = false;
    }
}