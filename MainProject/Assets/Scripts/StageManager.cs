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


    public GameObject pauseMenuUI;
    [SerializeField] private PlayerController playerInScene;
    public bool TestingOnly = false;
    public bool autoupdate = false;
    
    public LoadingScene1 Loadingscreen;
    
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

    public int EnemiesMax = 25;

    public bool SurvivorFreed = false;


    //public bool GameIsPaused = true;



    void Awake()
    {
        //PlayerController.UnlockCursor();
        playerInScene.gameObject.SetActive(false);
        Time.timeScale = 0;
        //TB = Object.Instantiate(new TerrainBuilder());
        //Player = (PlayerController) GameObject.FindObjectOfType(typeof(PlayerController), false);
        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault;
        //if(GameManager.ProfileData.FirstRun)
        //{

        //    //show tips
        //}


        //debug only
        //new GameObject().AddComponent<GameManager>();


        pauseMenuUI.SetActive(false);

        MakeStage();


    }

    private void Update()
    {
        //if(TB.IsHubScene && Input.GetKeyDown(KeyCode.E))
        //{
        //    Loadingscreen.gameObject.SetActive(true);
        //    StartCoroutine(loagGameplay());
        //}

        //Time.timeScale = 1;
        //StartCoroutine(EnemySpawner());

        if(Loadingscreen.start)
        {
            Loadingscreen.start = false;
            Loadingscreen.ready = false;
            Loadingscreen.text.text = "Generating Level .....";
            PlayerController.UnlockCursor();
            Time.timeScale = 1;
            //IsPaused = false;
            Loadingscreen.gameObject.SetActive(false);
            playerInScene.gameObject.SetActive(true);
            if (SceneManager.GetActiveScene().name == "GameplayFinal" && TB.finished)
            {
                TB.finished = false;
                SetupPlayer();
                StartCoroutine(EnemySpawner());
                Instantiate(BossPrefab, TB.obejctiveGlobalPosition, Quaternion.identity);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {

            if(pauseMenuUI.activeSelf)
            {
                pauseMenuUI.SetActive(false);
                playerInScene.canMove = true;
                Time.timeScale = 1;
                PlayerController.LockCursor();
            }
            else
            {
                pauseMenuUI.SetActive(true);
                playerInScene.canMove = false;
                Time.timeScale = 0;

                PlayerController.UnlockCursor();
            }

            Debug.Log("k");

            //if (pauseController.pauseMenuUI.gameObject.activeSelf)
            //{
            //    //unpause
            //    pauseController.pauseMenuUI.gameObject.SetActive(false);
            //    PlayerController.LockCursor();
            //    Time.timeScale = 0;
            //    //playerInScene.gameObject.SetActive(true);
            //    playerInScene.canMove = true;
            //    //pauseController.Resume();
            //}
            //else
            //{
            //    //pause
            //    pauseController.pauseMenuUI.gameObject.SetActive(true);
            //    PlayerController.UnlockCursor();
            //    Time.timeScale = 0;
            //    //playerInScene.gameObject.SetActive(false);
            //    playerInScene.canMove = false;
            //    //pauseController.Pause();
            //}

        }

    }

    private IEnumerator loagGameplay()
    {
        Debug.Log("changing scene in 1f");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("GameplayFinal");
        Debug.Log("loading newsdsadf");
    }

    public void MakeStage()
    {
        if (TestingOnly)
        {
            return;
        }


        if (SceneManager.GetActiveScene().name == "GameplayFinal")
        {
            TB.MakeTerrain();
        }





        Loadingscreen.Ready();




    }

    private IEnumerator EnemySpawner()
    {
        //yield return new WaitForSeconds(15f);

        while (true)
        {
            Player = playerInScene;
            int currentEnemies = Object.FindObjectsOfType<Enemy>().Length;
            if(currentEnemies < EnemiesMax && Player != null)
            {
                int spawns = System.Math.Min(10, EnemiesMax - currentEnemies);
                for (int i = 0; i < spawns; i++)
                    {
                    Vector2 circle = Random.insideUnitCircle * 250f;
                    Vector3 randompos =  new Vector3(circle.x, 0, circle.y) + Player.transform.position;
                    NavMeshHit hit;
                    if(NavMesh.SamplePosition(randompos, out hit, 250f, NavMesh.AllAreas))
                    {
                        Instantiate(EnemyPrefab, hit.position, Quaternion.identity);
                    }
                }
            }



            yield return new WaitForSeconds(3f);
        }
    }


    public void SetupPlayer()
    {
        //Player = Instantiate(PlayerPrefab).GetComponent<PlayerController>();
        Player = FindObjectOfType<PlayerController>();
        Player.transform.position = TB.House.position;
        Player.transform.position += TB.House.forward * 15f;
        Player.transform.forward = TB.House.forward;
        Player.transform.position = new Vector3(Player.transform.position.x, 5f, Player.transform.position.z);
    }





    public void EndStage(StageResult result)
    {

        Debug.Log("end stage");
        Loadingscreen.text.text = "Loading Scene";
        Loadingscreen.gameObject.SetActive(true);

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


        SceneManager.LoadScene("HubsceneFinal", LoadSceneMode.Single);
        //swap scene

    }

    public void LoadGameplay()
    {
        Debug.Log("loadgameplay");
        Loadingscreen.gameObject.SetActive(true);
        PlayerController.UnlockCursor();
        SceneManager.LoadScene("GameplayFinal");
    }

    public void ShowLoadingScreen()
    {

        //Loadingscreen = new LoadingScene1();
        Loadingscreen.gameObject.SetActive(true);
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

        SurvivorFreed = true;



    }




}
