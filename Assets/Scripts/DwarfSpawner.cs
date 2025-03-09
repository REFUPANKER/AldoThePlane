using System.Collections;
using UnityEngine;

public class DwarfSpawner : MonoBehaviour
{
    public Transform parent;
    public ParticleSystem particles;
    public float SpawnCooldown = 5;
    public float WaitBetweenSpawns = 0.2f;
    public GameObject[] dwarfs;
    
    void Start()
    {
        StartCoroutine(SpawnDwarfs());
    }

    IEnumerator SpawnDwarfs()
    {
        particles.Play();

        for (int spawningIndex = 0; spawningIndex < dwarfs.Length; spawningIndex++)
        {
            GameObject newDwarf = Instantiate(dwarfs[spawningIndex], transform.position, Quaternion.identity, parent);
            newDwarf.SetActive(true);
            yield return new WaitForSeconds(WaitBetweenSpawns);
        }

        StartCoroutine(ReActivate());
    }

    IEnumerator ReActivate()
    {
        yield return new WaitForSeconds(SpawnCooldown);
        StartCoroutine(SpawnDwarfs());
    }
}
