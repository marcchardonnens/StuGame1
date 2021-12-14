using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayManager 
{
    PlayerController CreatePlayer();
    void SetupStage();
    void GiveControl();
}