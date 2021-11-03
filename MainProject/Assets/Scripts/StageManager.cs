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




    private int MonsterXpCollected = 0;
    private int WoodCollected = 0;


    void Awake()
    {
        Player = (PlayerController) GameObject.FindObjectOfType(typeof(PlayerController), false);

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
        if (result == StageResult.Death || result == StageResult.TimerExpired)
        {
            //lose all except monster xp
        }
        else if (result == StageResult.EnterHomeEarly)
        {
            //keep monster xp and resources
        }
        else if (result == StageResult.SurvivorRescued)
        {
            //"win" aplly major upgrade etc...
        }

    }

    public void OnPlayerGetMonsterXP(int amount)
    {
        MonsterXpCollected += amount;


        //set future enemy level higher if monster xp large enough;


    }

    public void OnPlayerGetWood(int amount)
    {
        WoodCollected += amount;
    }




}
