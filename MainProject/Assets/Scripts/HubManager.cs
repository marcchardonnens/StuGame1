using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : GameplayManagerBase
{  
    public Vector3 PlayerWakeupPos;
    public Vector3 PlayerWakeupRot;
    public Vector3 PlayerEnterHomePos;
    public Vector3 PlayerEnterHomeRot;
    public bool HasWokenUp = true;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Hub Manager Awake");
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

    // Start is called before the first frame update
    void Start()
    {
        SetupStage();
    }



    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }



    public override PlayerController CreatePlayer()
    {
        if (HasWokenUp)
        {
            return Instantiate(PlayerPrefab, PlayerWakeupPos, Quaternion.Euler(PlayerWakeupRot)).GetComponent<PlayerController>();
        }
        else
        {
            return Instantiate(PlayerPrefab, PlayerEnterHomePos, Quaternion.Euler(PlayerEnterHomeRot)).GetComponent<PlayerController>();
        }
    }

    public override void SetupStage()
    {
        //setup things like upgrades or resources if needed

        CreatePlayer();
        
        RaiseSceneReady();
    }



    public void UpgradeAndReplaceHouse()
    {

    }

    public void UpgradePlayerTech()
    {

    }


    public void StoryProgress()
    {

    }

    private void OnDoorInteract()
    {
        //clean up scene

        BeginTransition(GameConstants.GAMEPLAYSCENE);
    }
    public override void BeginTransition(int sceneIndex)
    {

        TransitionToStage(sceneIndex);
    }

    protected override void OnExitButton()
    {
        BeginTransition(GameConstants.MAINMENUSCENE);
    }

    protected override void OnWakeupButton()
    {
        // waking up while in the hub
        // no use for this yet, but maybe later
    }
}
