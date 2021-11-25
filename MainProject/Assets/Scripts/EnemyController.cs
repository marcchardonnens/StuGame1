using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyController
{
    // private MonoBehaviour monoBehaviour; //maybe

    //constants
    private bool active = false;
    private const int MAXENEMIES = 15;
    private const int SPAWNINTERVAL = 10; //seconds to milis
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
        CheckEnemySpawn();
    }

    private void OnRageLevelUp(int oldLevel, int newLevel)
    {
        CurrentEnemyLevel = newLevel;
    }

    public void Tick()
    {

    }

    private async void CheckEnemySpawn()
    {
        while (active)
        {
            await Task.Delay(Util.SecondsToMillis(SPAWNINTERVAL));
            Debug.Log("Check Enemy Spawn");
            if (Enemy.All.Count < MAXENEMIES)
            {
                SpawnEnemies();
            }
        }
    }
    private void SpawnEnemies()
    {
        Debug.Log("Spawning Enemies");
        int toSpawn = Math.Min(GROUPSIZE, MAXENEMIES - Enemy.All.Count);
        for (int i = 0; i < toSpawn; i++)
        {
            // Manager.GetNavmeshLocationNearPlayer(SPAWNDISTANCEPLAYERMIN, SPAWNDISTANCEPLAYERMAX);
            // Vector2 circle = UnityEngine.Random.insideUnitCircle * (SPAWNDISTANCEPLAYERMAX - SPAWNDISTANCEPLAYERMIN);
            // Vector3 randompos = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.Player.transform.position;
            Vector3 randompos = new Vector3(Random.Range(-50f, 50f), 0, Random.Range(-50f, 50f));
            //TODO check min distance
            if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, SPAWNDISTANCEPLAYERMAX, NavMesh.AllAreas))
            {
                UnityEngine.Object.Instantiate(EnemyPrefab, hit.position, Quaternion.identity);
            }
            else
            {
                i--; //try again
            }
        }
    }

    private void ForceEnemyBehaviour(Enemy enemy, int level)
    {
        enemy.ChangeBehaviour(level);
    }

    private void AnalyseState()
    {

    }



}
