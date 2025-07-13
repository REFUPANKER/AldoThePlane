using System.Collections;
using UnityEngine;

public class DwarfSpawner : MonoBehaviour
{
    public Transform parent;
    public ParticleSystem particles;
    public float SpawnCooldown = 5;
    public float WaitBetweenSpawns = 0.2f;
    public GameObject[] dwarfs;
    public float spawnningNoise = 0.4f;

    void Start()
    {
        StartCoroutine(SpawnDwarfs());
    }

    IEnumerator SpawnDwarfs()
    {
        particles.Stop();
        particles.Play();

        for (int spawningIndex = 0; spawningIndex < dwarfs.Length; spawningIndex++)
        {
            GameObject newDwarf = Instantiate(dwarfs[spawningIndex], transform.position, Quaternion.identity, parent);
            newDwarf.SetActive(true);
            newDwarf.transform.Translate(Random.Range(-spawnningNoise, spawnningNoise), 0, 0);
            yield return new WaitForSeconds(WaitBetweenSpawns + Random.Range(-spawnningNoise, spawnningNoise));
        }

        StartCoroutine(ReActivate());
    }

    IEnumerator ReActivate()
    {
        yield return new WaitForSeconds(SpawnCooldown);
        StartCoroutine(SpawnDwarfs());
    }
}
