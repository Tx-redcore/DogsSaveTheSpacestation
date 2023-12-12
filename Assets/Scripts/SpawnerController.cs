using System.Collections;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    public GameObject spawnEnemy;
    public bool isTimedRespawn = false;
    public int spawnerSeconds = 5;

    private GameObject refEnemy;
    private bool isTimeToSpawn = false;
    private bool coroutineWait = false;

    void Start()
    {
        GameController.onRestart += OnRestart;
        var spawn = Instantiate(spawnEnemy, transform.position, Quaternion.identity);
        refEnemy = spawn;
    }

    void FixedUpdate()
    {
        if (isTimedRespawn && refEnemy == null)
        {
            if (isTimeToSpawn)
            {
                SpawnNew();
                isTimeToSpawn = false;
            }
            else
            {
                if (!coroutineWait)
                {
                    StartCoroutine(setOfTimer());
                }
            }
        }
       
    }

    void OnRestart()
    {
        if(refEnemy != null)
        {
            Destroy(refEnemy);
        }
        SpawnNew();
    }

    private void SpawnNew()
    {
        var spawn = Instantiate(spawnEnemy, transform.position, Quaternion.identity);
        refEnemy = spawn;
    }

    IEnumerator setOfTimer()
    {
        coroutineWait = true;
        yield return new WaitForSeconds(spawnerSeconds);
        isTimeToSpawn = true;
        coroutineWait = false;
        yield return true;
    }
}
