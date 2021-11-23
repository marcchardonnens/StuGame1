using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.AI;

public class EnemyController
{
    // private MonoBehaviour monoBehaviour; //maybe

    //constants
    private bool active = false;
    private const int MAXENEMIES = 20;
    private const int SPAWNINTERVAL = 10 * 1000; //seconds to milis
    private const int GROUPSIZE = 5;
    private const float SPAWNDISTANCEPLAYERMIN = 25f;
    private const float SPAWNDISTANCEPLAYERMAX = 100f;

    //Object references
    private readonly GameplayManagerBase Manager;
    private readonly GameObject EnemyPrefab;


    //fields
    private int CurrentEnemyLevel = 0;

    public EnemyController(GameplayManagerBase manager, GameObject enemyPrefab)
    {
        active = true;
        Manager = manager;
        EnemyPrefab = enemyPrefab;
        PlayerController.OnRageLevelUp += OnRageLevelUp;
    }

    private void OnRageLevelUp(int oldLevel, int newLevel)
    {
        CurrentEnemyLevel = newLevel;
        //TODO set statemachine
    }

    public void Tick()
    {

    }

    private async void CheckEnemySpawn()
    {
        while (active)
        {
            if (Enemy.All.Count > MAXENEMIES)
            {
                SpawnEnemies();
            }
            await Task.Delay(SPAWNINTERVAL);
        }
    }
    private void SpawnEnemies()
    {
        int toSpawn = Math.Min(GROUPSIZE, MAXENEMIES - Enemy.All.Count);
        for (int i = 0; i < toSpawn; i++)
        {
            // Manager.GetNavmeshLocationNearPlayer(SPAWNDISTANCEPLAYERMIN, SPAWNDISTANCEPLAYERMAX);
            Vector2 circle = UnityEngine.Random.insideUnitCircle * (SPAWNDISTANCEPLAYERMAX - SPAWNDISTANCEPLAYERMIN);
            Vector3 randompos = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.Player.transform.position;
            //TODO check min distance
            if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, SPAWNDISTANCEPLAYERMAX, NavMesh.AllAreas))
            {
                UnityEngine.Object.Instantiate(EnemyPrefab, hit.position, Quaternion.identity);
            }
            UnityEngine.Object.Instantiate(EnemyPrefab);
        }
    }

    private void AnalyseState()
    {

    }



}
