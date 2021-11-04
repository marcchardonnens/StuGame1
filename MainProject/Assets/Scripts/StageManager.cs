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

    public bool TestingOnly = false;
    public bool autoupdate = false;
    public TerrainBuilder TB;

    public GameObject PlayerPrefab;
    public GameObject EnemyPrefab;
    public GameObject SurvivorPrefab;
    public GameObject BossPrefab;

    public const int MonsterXPLevelUpThreshholdBase = 250;
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

    public int EnemiesMax = 0;



    void Awake()
    {
        //TB = Object.Instantiate(new TerrainBuilder());
        //Player = (PlayerController) GameObject.FindObjectOfType(typeof(PlayerController), false);
        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault;
        //if(GameManager.ProfileData.FirstRun)
        //{

        //    //show tips
        //}


        //debug only
        new GameObject().AddComponent<GameManager>();


        MakeStage();

    }


    public void MakeStage()
    {
        if (TestingOnly)
        {
            return;
        }


        TB.MakeTerrain();



        //SetupPlayer();


        StartCoroutine(EnemySpawner());


        Instantiate(BossPrefab, TB.obejctiveGlobalPosition, Quaternion.identity);


    }

    private IEnumerator EnemySpawner()
    {
        //yield return new WaitForSeconds(15f);

        while (true)
        {
            int currentEnemies = Object.FindObjectsOfType<Enemy>().Length;
            if(currentEnemies < EnemiesMax)
            {
                int spawns = System.Math.Min(10, EnemiesMax - currentEnemies);
                for (int i = 0; i < spawns; i++)
                    {
                    Vector3 randompos = Random.insideUnitSphere + Player.transform.position;
                    NavMeshHit hit;
                    if(NavMesh.SamplePosition(randompos, out hit, 250f, 1 << GameConstants.GROUNDLAYER))
                    {
                        Instantiate(EnemyPrefab);
                    }
                }
            }



            yield return new WaitForSeconds(10f);
        }
    }


    public void SetupPlayer()
    {
        Player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
        Player.transform.position = TB.House.position;
        Player.transform.position += TB.House.forward * 5;
        Player.transform.forward = TB.House.forward;
    }





    public void EndStage(StageResult result)
    {
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
        }



        SceneManager.LoadScene("HubScene", LoadSceneMode.Single);
        //swap scene

    }

    public void OnPlayerGetMonsterXP(int amount)
    {
        MonsterXpCollected += amount;


        //set future enemy level higher if monster xp large enough;
        if(MonsterXpCollected >= MonsterXPLevelUpThreshholdCurrent && MonsterCurrentLevel < MonsterMaxLevel)
        {
            MonsterXPLevelUpThreshholdCurrent = Mathf.RoundToInt(MonsterXPLevelUpThreshholdBase * Mathf.Pow(MonsterXPLevelUpPower, MonsterCurrentLevel));
            MonsterCurrentLevel++;

            //maybe level up current enemies?

        }

    }

    public void OnPlayerGetWood(int amount)
    {
        if(WoodCollected + amount <= WoodMax)
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
        if (NavMesh.SamplePosition(TB.obejctiveGlobalPosition, out hit, 50f, 1 << GameConstants.GROUNDLAYER))
        {
            Instantiate(SurvivorPrefab);
        }





    }




}
