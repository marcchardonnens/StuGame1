using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum StageResult
{
    Death,
    TimerExpired,
    EnterHomeEarly,
    SurvivorRescued,
}


//this class is responsible for managing the Gameplay in the gameplay Scene
//setting up the stage, gameplay (spawning mobs, handling events), and cleaning up the stage again
public class StageManager : MonoBehaviour
{
    public static PlayerController Player;
    public float PlayerSpawnFromHouseOffset = 15f;
    [SerializeField] private PlayerController playerInScene;
    public bool TestingOnly = false;
    public bool autoupdate = false;

    public TerrainBuilder GameplayTB;
    public TerrainBuilder HubTB;

    public NavMeshSurface Surface;
    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject SurvivorPrefab;
    public GameObject BossPrefab;

    public bool PreSpawnEnemies = true;
    public float EnemySpawnInitialDelay = 15f;
    public float EnemyRespawnDelay = 10f;
    public float EnemySpawnRange = 250f;
    public int EnemiesSpawnedPerCycle = 10;
    public int MonsterXPLevelUpThreshholdBase = 250;
    public int MonsterXPLevelUpThreshholdCurrent = 250;
    public float MonsterXPLevelUpPower = 1.1f;
    public int MonsterMaxLevel = 10;
    public int MonsterCurrentLevel = 0;

    public float BossKillRemainingTimerMultiplier = 2f;

    private int MonsterXpCollected = 0;

    public int WoodMaxDefault = 50;
    public int WoodMaxUpgraded = 100;
    private int WoodMax;
    private int WoodCollected = 0;

    private int CalciumCollected = 0;
    private int LuciferinCollected = 0;
    private int OxygenCollected = 0;

    private bool BossKilled = false;
    private bool SurvivorFound = false;

    public int EnemiesMax = 25;

    public bool SurvivorFreed = false;


    private GameManager GameManager;

    public bool TerrainReady = false;
    public bool NavMeshBaked = false;

    void Awake()
    {
        GameManager findgamemanager = FindObjectOfType<GameManager>();
        if (findgamemanager)
        {
            GameManager = findgamemanager;
        }
        else
        {
            GameManager = Instantiate(new GameObject()).AddComponent<GameManager>();
        }
        PlayerController findplayer = FindObjectOfType<PlayerController>();
        if (findplayer)
        {
            Player = findplayer;
        }
        else
        {
            Player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
        }

        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault;

        MakeStage();
        StartCoroutine(OnTerrainReady());
    }

    private void Update()
    {

    }



    public void MakeStage()
    {
        if (TestingOnly)
        {
            return;
        }

        GameplayTB.MakeTerrain();
        TerrainReady = true;


    }

    private IEnumerator OnTerrainReady()
    {
        yield return new WaitUntil(() => TerrainReady);
        StartCoroutine(BakeNavMesh());
        SetupPlayer();
        StartCoroutine(EnemySpawner());

    }

    private IEnumerator BakeNavMesh()
    {
        Surface.BuildNavMesh();
        NavMeshBaked = true;
        yield return null;

    }

    private IEnumerator EnemySpawner()
    {
        yield return new WaitForSeconds(EnemySpawnInitialDelay);

        while (true)
        {
            Player = playerInScene;
            int currentEnemies = Object.FindObjectsOfType<Enemy>().Length;
            if (currentEnemies < EnemiesMax && Player != null)
            {
                int spawns = System.Math.Min(EnemiesSpawnedPerCycle, EnemiesMax - currentEnemies);
                for (int i = 0; i < spawns; i++)
                {
                    Vector2 circle = Random.insideUnitCircle * EnemySpawnRange;
                    Vector3 randompos = new Vector3(circle.x, 0, circle.y) + Player.transform.position;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randompos, out hit, EnemySpawnRange, NavMesh.AllAreas))
                    {
                        Instantiate(EnemyPrefab, hit.position, Quaternion.identity);
                    }
                }
            }



            yield return new WaitForSeconds(EnemyRespawnDelay);
        }
    }


    public void SetupPlayer()
    {
        //Player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
        Player = FindObjectOfType<PlayerController>();
        Player.transform.position = GameplayTB.houseGlobalPosition + GameplayTB.PlayerSpawnOutsideHouseOffsetPos;
        Player.transform.eulerAngles = GameplayTB.PlayerSpawnOutsideHouseRotationEuler;
    }





    public void EndStage(StageResult result)
    {

        // Debug.Log("end stage");
        // Loadingscreen.text.text = "Loading Scene";
        // Loadingscreen.gameObject.SetActive(true);

        GameManager.PauseGame();

        if (GameManager.ProfileData.FirstRun)
        {
            GameManager.ProfileData.FirstRun = false;
            //show tips
        }
        if (result == StageResult.Death || result == StageResult.TimerExpired)
        {
            //lose all except monster xp
            GameManager.ProfileData.MonsterXPTotal += MonsterXpCollected;
            GameManager.ProfileData.MonsterXPCurrent += MonsterXpCollected;

            GameManager.ProfileData.StoryDeathProgress++;
        }
        else if (result == StageResult.EnterHomeEarly)
        {
            //keep monster xp and resources
            GameManager.ProfileData.MonsterXPTotal += MonsterXpCollected;
            GameManager.ProfileData.MonsterXPCurrent += MonsterXpCollected;
            GameManager.ProfileData.WoodCurrent += WoodCollected;
            GameManager.ProfileData.WoodTotal += WoodCollected;
            GameManager.ProfileData.OxygenCurrent += OxygenCollected;
            GameManager.ProfileData.OxygenTotal += OxygenCollected;
            GameManager.ProfileData.LuciferinCurrent += LuciferinCollected;
            GameManager.ProfileData.LuciferinTotal += LuciferinCollected;
            GameManager.ProfileData.CalciumCurrent += CalciumCollected;
            GameManager.ProfileData.CalciumTotal += CalciumCollected;

            GameManager.ProfileData.StoryReturnProgress++;

        }
        else if (result == StageResult.SurvivorRescued)
        {
            //"win" aplly major upgrade etc...
            GameManager.ProfileData.StorySuccessProgress++;
            GameManager.ProfileData.UnlockedLightbulb = true;

            SceneManager.LoadScene("EndScreenScene", LoadSceneMode.Single);
        }


        // SceneManager.LoadScene("HubsceneFinal", LoadSceneMode.Single);
        //swap scene

    }

    public void LoadGameplay()
    {
        Debug.Log("loadgameplay");
        PlayerController.UnlockCursor();
    }

    public void OnPlayerGetMonsterXP(int amount)
    {
        MonsterXpCollected += amount;


        //set future enemy level higher if monster xp large enough;
        if (MonsterXpCollected >= MonsterXPLevelUpThreshholdCurrent && MonsterCurrentLevel < MonsterMaxLevel)
        {
            MonsterXPLevelUpThreshholdCurrent = Mathf.RoundToInt(MonsterXPLevelUpThreshholdBase * Mathf.Pow(MonsterXPLevelUpPower, MonsterCurrentLevel));
            MonsterCurrentLevel++;

            //maybe level up current enemies?

        }

    }

    public void OnPlayerGetWood(int amount)
    {
        if (WoodCollected + amount <= WoodMax)
        {
            WoodCollected += amount;
        }
        else
        {
            WoodCollected = WoodMax;
        }
    }

    public void OnBossKilled()
    {

        //survivor follow player
        //double remaining timer


        NavMeshHit hit;
        if (NavMesh.SamplePosition(GameplayTB.obejctiveGlobalPosition, out hit, 50f, 1 << GameConstants.GROUNDLAYER))
        {
            Instantiate(SurvivorPrefab);
        }

        SurvivorFreed = true;



    }




}
