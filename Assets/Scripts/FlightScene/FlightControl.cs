using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlightControl : MonoBehaviour
{

    public bool paused;
    public bool canMove = true;
    public float forwardSpeed = 20f;
    public float yawSpeed = 60f;
    public float pitchSpeed = 60f;

    public ParticleSystem[] weaponEffects;
    public ParticleSystem activeHitEffect;
    private bool isAttacking = false;

    public Transform planeA10;
    public Transform planeB2;
    public CinemachineFreeLook vcamPlaneB2;

    void Update()
    {
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        if (canMove)
        {
            float yawInput = Input.GetAxis("Horizontal");
            float pitchInput = Input.GetAxis("Vertical");

            transform.Rotate(pitchInput * pitchSpeed * Time.deltaTime, 0, 0, Space.Self);
            transform.Rotate(0, 0, -yawInput * yawSpeed * Time.deltaTime, Space.Self);
        }

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

        if (Input.GetKeyDown(KeyCode.P))
        {
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            paused = !paused;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (planeA10.gameObject.activeSelf)
            {
                planeA10.gameObject.SetActive(false);
                planeB2.gameObject.SetActive(true);
                vcamPlaneB2.Priority+=10;
            }
            else
            {
                planeA10.gameObject.SetActive(true);
                planeB2.gameObject.SetActive(false);
                vcamPlaneB2.Priority-=10;
            }
        }
    }

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
}
