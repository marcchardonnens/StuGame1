using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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
    public TerrainBuilder Terrain;

    public int WoodMaxDefault = 50;
    public int WoodMaxUpgraded = 100;

    private int MonsterXpCollected = 0;
    private int WoodMax;
    private int WoodCollected = 0;

    private int CalciumCollected = 0;
    private int LuciferinCollected = 0;
    private int OxygenCollected = 0;

    private bool BossKilled = false;
    private bool SurvivorFound = false;


    void Awake()
    {
        Player = (PlayerController) GameObject.FindObjectOfType(typeof(PlayerController), false);
        WoodMax = GameManager.ProfileData.HasWoodInventoryUpgrade ? WoodMaxUpgraded : WoodMaxDefault; 

        if(GameManager.ProfileData.FirstRun)
        {

            //show tips
        }


        //debug only
        new GameObject().AddComponent<GameManager>();

    }


    public void MakeStage()
    {
        if (TestingOnly)
        {
            return;
        }


        //Terrain.MakeTerrain();






    }

    public void SetupPlayer(/*PlayerSafeData*/)
    {

    }

    
    public void EndStage(StageResult result)
    {
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

    }

    public void OnPlayerGetMonsterXP(int amount)
    {
        MonsterXpCollected += amount;


        //set future enemy level higher if monster xp large enough;


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




}
