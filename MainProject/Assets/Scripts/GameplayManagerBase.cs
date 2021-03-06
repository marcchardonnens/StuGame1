using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this abstract class serves as baseline for singleton gameplay managers that controll the flow of their gameplay scene
//all of these should be singletons, because you dont want 2 managers managing the same thing
public abstract class GameplayManagerBase : MonoBehaviour, IGameplayManager
{
    public static event Action OnAnySceneReady = delegate { }; //stage setup, other classes can do setup
    public static event Action OnPauseRequest = delegate { };
    public GameObject PlayerPrefab;
    public static GameplayManagerBase Instance { get; set; } 

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }

        GameManager.Instance.CurrentManager = this;
        PlayerUIController.Instance.WakeupButton.onClick.AddListener(OnWakeupButton);
        PlayerUIController.Instance.ExitGameButton.onClick.AddListener(OnExitButton);
    }

    protected abstract void OnExitButton();
    protected abstract void OnWakeupButton();

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPauseRequest?.Invoke();
        }
    }

    public virtual PlayerController CreatePlayer()
    {
        return Instantiate(PlayerPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerController>();
    }


    public abstract void SetupStage();


    internal Vector3? GetNavmeshLocationNearPlayer(float minDistance, float maxDistance)
    {
        return null;
    }

    // protected virtual void TransitionToStage(int sceneIndex)
    // {
    //     if(sceneIndex == GameConstants.MAINMENUSCENE)
    //     {
    //         SceneTransition.TransitionToMenu();
    //     }
    //     else if(sceneIndex == GameConstants.HUBSCENE)
    //     {
    //         SceneTransition.TransitionToHub();
    //     }
    //     else if(sceneIndex == GameConstants.GAMEPLAYSCENE)
    //     {
    //         SceneTransition.TransitionToGameplay();
    //     }
    // }

    protected void RaiseSceneReady()
    {
        OnAnySceneReady?.Invoke();
    }

    public virtual void GiveControl()
    {
        GameManager.Instance.UpdateState(GameState.StagePlaying);
    }
}
