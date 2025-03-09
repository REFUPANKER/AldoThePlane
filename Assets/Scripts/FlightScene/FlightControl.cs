using UnityEngine;

public class FlightControl : MonoBehaviour
{
    public float forwardSpeed = 20f;
    public float yawSpeed = 60f;
    public float pitchSpeed = 60f;

    public Transform cameraTransform;
    public float cameraSmoothTime = 0.1f;
    public Vector3 cameraOffset = new Vector3(0, 2, -6);
    private Vector3 cameraVelocity = Vector3.zero;

    public ParticleSystem[] weaponEffects;
    public ParticleSystem activeHitEffect;
    private bool isAttacking = false;

    void Update()
    {
        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        float yawInput = Input.GetAxis("Horizontal");
        float pitchInput = Input.GetAxis("Vertical");

        transform.Rotate(pitchInput * pitchSpeed * Time.deltaTime, 0, 0, Space.Self);
        transform.Rotate(0, 0, -yawInput * yawSpeed * Time.deltaTime, Space.Self);

        if (cameraTransform != null)
        {
            Vector3 desiredPosition = transform.position + transform.rotation * cameraOffset;
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, desiredPosition, ref cameraVelocity, cameraSmoothTime);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, transform.rotation, Time.deltaTime * 5f);
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
