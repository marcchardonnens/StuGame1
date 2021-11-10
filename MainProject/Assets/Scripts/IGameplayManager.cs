using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayManager
{
    PlayerController CreatePlayer(Vector3 pos, Vector3 rot); 
    void SetupStage();
    void OnSceneLoaded();
    void OnSceneCompletelyReady();

}
