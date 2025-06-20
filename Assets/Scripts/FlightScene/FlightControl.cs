using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FlightControl : MonoBehaviour
{

    public bool paused;
    public bool canMove = true;
    public float forwardSpeed = 20f;
    public float yawSpeed = 60f;
    public float pitchSpeed = 60f;
    public float rotationAmount = 10f;

    public ParticleSystem[] weaponEffects;
    public ParticleSystem activeHitEffect;
    private bool isAttacking = false;

    public ParticleSystem flares;
    public int amountPerDeployFlare = 5;
    public float nextFlareCooldown = 0.5f;
    public float flareCooldown = 5;
    public Image flareStatus;
    [Tooltip("0:enabled,1:disabled")]
    public Color[] flareStatusColors;
    bool canFlare = true;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {

        if (canMove)
        {
            transform.position += transform.forward * forwardSpeed * Time.deltaTime;

            float yawInput = Input.GetAxis("Horizontal");
            float pitchInput = Input.GetAxis("Vertical");
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(0, -rotationAmount * Time.deltaTime, 0);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(0, rotationAmount * Time.deltaTime, 0);
            }
            transform.Rotate(pitchInput * pitchSpeed * Time.deltaTime, 0, 0, Space.Self);
            transform.Rotate(0, 0, -yawInput * yawSpeed * Time.deltaTime, Space.Self);
        }

        #region Attacking
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            StartAttack();
        }
        if (Input.GetKeyUp(KeyCode.Space) && isAttacking)
        {
            StopAttack();
        }
        if (isAttacking)
        {
            ShootRaycast();
        }
        #endregion

        #region Flares
        if (Input.GetKeyDown(KeyCode.F) && canFlare)
        {
            StartCoroutine(reactivateFlares());
        }
        #endregion


        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseResume();
        }
    }
    void PauseResume()
    {
        paused = !paused;
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        canMove = !canMove;
    }

    IEnumerator reactivateFlares()
    {
        canFlare = false;
        flareStatus.color = flareStatusColors[1];
        for (int i = 0; i < amountPerDeployFlare; i++)
        {
            ParticleSystem newflares = Instantiate(flares, transform);
            newflares.gameObject.SetActive(true);
            newflares.Play();
            newflares.transform.parent = null;
            yield return new WaitForSeconds(nextFlareCooldown);
        }
        yield return new WaitForSeconds(flareCooldown);
        canFlare = true;
        flareStatus.color = flareStatusColors[0];
    }

    #region Attacking 
    void StartAttack()
    {
        isAttacking = true;
        foreach (ParticleSystem ps in weaponEffects)
        {
            ps.Play();
        }
    }

    void StopAttack()
    {
        foreach (ParticleSystem ps in weaponEffects)
        {
            ps.Stop();
        }
        isAttacking = false;
    }
    void ShootRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {
            ParticleSystem newHitEffect = Instantiate(activeHitEffect, hit.point, Quaternion.LookRotation(Vector3.forward, Vector3.up));
            newHitEffect.transform.SetParent(null);
            newHitEffect.Play();
        }
    }

    #endregion
}
