using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

//this class is responsible for managing the Gameplay in the gameplay Scene
//setting up the stage, gameplay (spawning mobs, handling events), and cleaning up the stage again
public class StageManager : MonoBehaviour
{
    public bool autoupdate = false;

    public NavMeshSurface Surface;
    public NavMeshModifierVolume HighMod;
    public NavMeshModifierVolume LowMod;


    public void MakeStage()
    {




    }

}
