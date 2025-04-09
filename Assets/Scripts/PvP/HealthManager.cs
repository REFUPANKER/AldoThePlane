using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
namespace PvP
{
    public class HealthManager : NetworkBehaviour
    {

        [SerializeField] HeroMovement hero;
        [SerializeField] float health;
        [SerializeField] Canvas worldUi;
        [SerializeField] bool ShowSelfWorldUi = true;
        [SerializeField] Slider worldHealthbar;
        [SerializeField] Slider UiHealthbar;
        [SerializeField] float healthbarSmoothnes = 10f;

        private NetworkVariable<float> nvHealth = new NetworkVariable<float>();

        public override void OnNetworkSpawn()
        {
            worldUi.gameObject.layer = LayerMask.NameToLayer("Healthbar");
            worldHealthbar.maxValue = health;
            worldHealthbar.value = health;
            UiHealthbar.maxValue = health;
            UiHealthbar.value = health;
            nvHealth.Value = health;
            if (IsOwner)
            {
                worldUi.gameObject.SetActive(ShowSelfWorldUi);
            }
        }

        [ServerRpc]
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

        public void TakeDamage(float damage)
        {
            if (IsOwner)
            {
                health -= damage;
                UServerRpc(health);
            }
            else
            {
                health = nvHealth.Value - damage;
            }
        }

        void Update()
        {
            worldHealthbar.value = Mathf.Lerp(worldHealthbar.value, IsOwner ? health : nvHealth.Value, Time.deltaTime * healthbarSmoothnes);
            UiHealthbar.value = Mathf.Lerp(UiHealthbar.value, IsOwner ? health : nvHealth.Value, Time.deltaTime * healthbarSmoothnes);

            if (!IsOwner) { return; }
            if (Input.GetKeyDown(KeyCode.T))
            {
                TakeDamage(10);
            }
        }
        void LateUpdate()
        {
            worldUi.transform.LookAt(worldUi.transform.position + hero.cam.transform.forward);
            worldUi.transform.rotation = Quaternion.Euler(0f, worldUi.transform.rotation.eulerAngles.y, 0f);
        }
    }
}