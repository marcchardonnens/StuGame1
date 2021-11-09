using System.Security.Cryptography;
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
    public static float StageTimer = 1500f;
    public float PlayerSpawnFromHouseOffset = 15f;
    public bool TestingOnly = false;
    public bool autoupdate = false;

    public TerrainBuilder GameplayTB;
    public TerrainBuilder HubTB;

    public NavMeshSurface Surface;
    public float NavMeshBakRepeatTimer = 5f;
    public bool localNavMesh = true;
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
    private static bool doorPressed = false;
    private static bool giveUp = false;
    private static bool exitGame = false;

    public int EnemiesMax = 25;

    public bool SurvivorFreed = false;


    public static bool TerrainReady = false;
    public static bool NavMeshBaked = false;

    void Awake()
    {
        GameManager.Instance.PlayerHasControl = false;
        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault;

        MakeStage();
        StartCoroutine(OnTerrainReady());
    }

    private void Update()
    {

        if (GameManager.Instance.SceneLoaded && GameManager.Instance.CurrentSceneIndex == 2)
        {
            SetupPlayer();
            StartCoroutine(EnemySpawner());
            GameManager.Instance.SceneLoaded = false;
            return;
        }
        else if(GameManager.Instance.SceneCompletelyReady && GameManager.Instance.CurrentSceneIndex == 2)
        {
            // GameManager.Instance.LockCursor();
            // GameManager.Instance.PlayerHasControl = true;
            // Player.playerUI.ShowGameplayHud();
        }

        StageTimer -= Time.deltaTime;
        if (StageTimer <= 0)
        {
            EndStage(StageResult.TimerExpired);
        }
        if (doorPressed)
        {
            doorPressed = false;
            if (SurvivorFreed)
            {
                EndStage(StageResult.SurvivorRescued);
            }
            else
            {
                EndStage(StageResult.EnterHomeEarly);
            }
        }
        if(giveUp)
        {
            giveUp = false;
            EndStage(StageResult.TimerExpired);
        }
        if(exitGame)
        {
            EndStage(StageResult.TimerExpired);
        }
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
        if (!localNavMesh)
        {
            StartCoroutine(BakeNavMeshGlobal());
        }



    }

    private IEnumerator BakeNavMeshGlobal()
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
        // Player = Instantiate(PlayerPrefab, GameplayTB.houseGlobalPosition + GameplayTB.PlayerSpawnOutsideHouseOffsetPos, Quaternion.Euler(GameplayTB.PlayerSpawnOutsideHouseRotationEuler)).GetComponent<PlayerController>();
        Player = Instantiate(PlayerPrefab, null).GetComponent<PlayerController>();
        Player.transform.position = GameplayTB.houseGlobalPosition + GameplayTB.PlayerSpawnOutsideHouseOffsetPos;
        Player.transform.eulerAngles = GameplayTB.PlayerSpawnOutsideHouseRotationEuler;
        // Player.playerUI.ShowGameplayHud();

        if (localNavMesh)
        {
            StartCoroutine(BakeNavMeshLocal(NavMeshBakRepeatTimer));
            NavMeshBaked = true;
        }
    }

    public IEnumerator BakeNavMeshLocal(float delay)
    {
        while (true)
        {
            Player.Surface.BuildNavMesh();
            yield return new WaitForSeconds(delay);
        }
    }

    public static void ExitGame()
    {
        exitGame = true;
    }

    public static void GiveUp()
    {
        giveUp = true;
    }

    public static void DoorPressed()
    {
        doorPressed = true;
    }

    public void EndStage(StageResult result)
    {
        bool showCredits = false;

        GameManager.Instance.PauseGame();

        // if (GameManager.ProfileData.FirstRun)
        // {
        //     GameManager.ProfileData.FirstRun = false;
        //     //show tips
        // }


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

            showCredits = true;
        }

        if(exitGame)
        {
            exitGame = false;
            //TODO back to main menu
            return;
        }

        StartCoroutine(GameManager.Instance.GameplayToHub(2f, 2f, showCredits));
    }
    public void OnPlayerGetMonsterXP(int amount)
    {
        MonsterXpCollected += amount;


        //set future enemy level higher if monster xp large enough;
        if (MonsterXpCollected >= MonsterXPLevelUpThreshholdCurrent && MonsterCurrentLevel < MonsterMaxLevel)
        {
            MonsterXPLevelUpThreshholdCurrent = Mathf.RoundToInt(MonsterXPLevelUpThreshholdBase * Mathf.Pow(MonsterXPLevelUpPower, MonsterCurrentLevel));
            MonsterCurrentLevel++;

            //TODO level up current enemies

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


        if (NavMesh.SamplePosition(GameplayTB.obejctiveGlobalPosition, out NavMeshHit hit, 50f, 1 << GameConstants.GROUNDLAYER))
        {
            Instantiate(SurvivorPrefab, hit.position, Quaternion.identity);
        }

        SurvivorFreed = true;



    }




}
