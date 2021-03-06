using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : GameplayManagerBase
{
    public static event Action OnSceneReady = delegate { }; //stage setup, other classes can do setup
    public static event Action OnSceneCompletelyReady = delegate { }; //setup fully complete, gameplay can begin
    public Vector3 PlayerWakeupPos;
    public Vector3 PlayerWakeupRot;
    public Vector3 PlayerEnterHomePos;
    public Vector3 PlayerEnterHomeRot;
    public bool HasWokenUp = true;

    protected override void Awake()
    {
        base.Awake();
        Door.OnDoorInteract += OnDoorInteract;
        PlayerUIController.Instance.WakeupButton.onClick.AddListener(OnWakeupButton);
        PlayerUIController.Instance.ExitGameButton.onClick.AddListener(OnExitButton);
    }

    protected void OnEnable()
    {

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

    public override void GiveControl()
    {
        base.GiveControl();
        OnSceneCompletelyReady?.Invoke();
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
        OnSceneReady?.Invoke();
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
        SceneTransition.TransitionToGameplay();
    }

    protected override void OnExitButton()
    {
        SceneTransition.TransitionToMenu();
    }

    protected override void OnWakeupButton()
    {
        // waking up while in the hub
        // no use for this yet, but maybe later
    }
}
