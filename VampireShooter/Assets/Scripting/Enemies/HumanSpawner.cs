using System.Collections;
using UnityEngine;

public class HumanSpawner : MonoBehaviour
{
    public GameObject humanPrefab;
    public float respawnDelay = 5f;

    private GameObject currentHuman;

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        currentHuman = Instantiate(humanPrefab, transform.position, transform.rotation);

        // Tell the human who spawned it
        HumanBase hb = currentHuman.GetComponent<HumanBase>();
        if (hb != null)
        {
            hb.SetSpawner(this);
        }
    }

    public void OnHumanDied()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Spawn();
    }
}
