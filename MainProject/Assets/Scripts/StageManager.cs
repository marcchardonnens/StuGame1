using System;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine;

public enum StageResult
{
    None,
    Death,
    TimerExpired,
    EnterHomeEarly,
    SurvivorRescued,
}

public class StageManager : GameplayManagerBase
{
    public static event Action OnSceneReady = delegate { }; //stage setup, other classes can do setup
    public static event Action OnSceneCompletelyReady = delegate { }; //setup fully complete, gameplay can begin
    public float StageTimer = 900;
    public float PlayerSpawnFromHouseOffset = 15f; 
    public bool TestingOnly = false;
    public bool autoupdate = false;
    public TerrainBuilder GameplayTB;
    public NavMeshSurface Surface; 
    public float NavMeshBakRepeatTimer = 5f; 
    public bool localNavMesh = false;
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

    private StageResult Result = StageResult.None;

    public bool TerrainReady = false;
    public bool NavMeshBaked = false;

    protected override void Awake()
    {
        base.Awake();
        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault;
        TerrainReady = false;
        NavMeshBaked = false;

        Door.OnDoorInteract += OnDoorInteract;

        SetupStage();
        StartCoroutine(OnTerrainReady());

    }
    protected void OnEnable()
    {
        Door.OnDoorInteract += OnDoorInteract;
        PlayerUIController.Instance.WakeupButton.onClick.AddListener(OnWakeupButton);
        PlayerUIController.Instance.ExitGameButton.onClick.AddListener(OnExitButton);
    }

    protected void OnDisable()
    {
        Door.OnDoorInteract -= OnDoorInteract;
        PlayerUIController.Instance.WakeupButton.onClick.RemoveListener(OnWakeupButton);
        PlayerUIController.Instance.ExitGameButton.onClick.RemoveListener(OnExitButton);
    }

    public override void GiveControl()
    {
        base.GiveControl();
        OnSceneCompletelyReady?.Invoke();
    }
    private void OnDoorInteract()
    {
        Debug.Log("door interact outside");
        if (SurvivorFound)
        {
            Result = StageResult.SurvivorRescued;
            BeginTransition(GameConstants.HUBSCENE);
        }
        else
        {
            Result = StageResult.EnterHomeEarly;
            BeginTransition(GameConstants.HUBSCENE);
        }
    }

    private void OnPlayerDeath()
    {
        Result = StageResult.Death;
        BeginTransition(GameConstants.HUBSCENE);
    }

    private void OnGiveup()
    {
        Result = StageResult.Death;
        BeginTransition(GameConstants.HUBSCENE);
    }

    protected override void Update()
    {
        base.Update();

        if (GameManager.Instance.GamePaused)
        {
            StageTimer -= Time.deltaTime;
        }

        if (StageTimer <= 0)
        {
            Result = StageResult.TimerExpired;
            BeginTransition(GameConstants.HUBSCENE);
        }
    }



    public override void SetupStage()
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
        CreatePlayer();
        if (!localNavMesh)
        {
            yield return StartCoroutine(BakeNavMeshGlobal());
        }
        OnSceneReady?.Invoke();
        RaiseSceneReady();
        
        //TODO better Enemy controller
        StartCoroutine(EnemySpawner());
    }

    private IEnumerator BakeNavMeshGlobal()
    {
        Surface.BuildNavMesh();
        NavMeshBaked = true;
        yield return null;
    }


    //TODO propper enemy controller
    private IEnumerator EnemySpawner()
    {
        yield return new WaitForSeconds(EnemySpawnInitialDelay);

        while (true)
        {
            // int currentEnemies = Object.FindObjectsOfType<Enemy>().Length;
            int currentEnemies = Enemy.All.Count;
            if (currentEnemies < EnemiesMax && GameManager.Instance.Player != null)
            {
                int spawns = System.Math.Min(EnemiesSpawnedPerCycle, EnemiesMax - currentEnemies);
                for (int i = 0; i < spawns; i++)
                {
                    Vector2 circle = UnityEngine.Random.insideUnitCircle * EnemySpawnRange;
                    Vector3 randompos = new Vector3(circle.x, 0, circle.y) + GameManager.Instance.Player.transform.position;
                    if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, EnemySpawnRange, NavMesh.AllAreas))
                    {
                        Instantiate(EnemyPrefab, hit.position, Quaternion.identity);
                    }
                }
            }

            yield return new WaitForSeconds(EnemyRespawnDelay);
        }
    }

    public override PlayerController CreatePlayer()
    {
        PlayerController player = Instantiate(PlayerPrefab, null).GetComponent<PlayerController>();
        // Vector3 house = new Vector3(GameplayTB.houseGlobalPosition.x, GameplayTB.houseGlobalPosition.y, GameplayTB.houseGlobalPosition.z); //deep copy
        player.transform.position = GameplayTB.houseGlobalPosition + GameplayTB.PlayerSpawnOutsideHouseOffsetPos;
        player.transform.eulerAngles = GameplayTB.PlayerSpawnOutsideHouseRotationEuler;
        if (localNavMesh)
        {
            StartCoroutine(BakeNavMeshLocal(NavMeshBakRepeatTimer));
            NavMeshBaked = true;
        }
        player.OnDeath += OnPlayerDeath;
        return player;
    }
    
    private void OnPlayerDeath(ITakeDamage player)
    {
        EndStage(StageResult.Death);
    }

    public IEnumerator BakeNavMeshLocal(float delay)
    {
        while (true)
        {
            // GameManager.Instance.Player.Surface.BuildNavMesh();
            yield return new WaitForSeconds(delay);
        }
    }

    private void EndStage(StageResult result)
    {
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

    public override void BeginTransition(int sceneIndex)
    {
        EndStage(Result);
        TransitionToStage(sceneIndex);
    }

    protected override void OnExitButton()
    {
        Result = StageResult.Death;
        BeginTransition(GameConstants.MAINMENUSCENE);
    }

    protected override void OnWakeupButton()
    {
        Result = StageResult.Death;
        BeginTransition(GameConstants.HUBSCENE);
    }
}
