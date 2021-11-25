using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : GameplayManagerBase
{
    public static event Action OnSceneReady = delegate { };
    public static event Action OnSceneCompletelyReady = delegate { };
    public GameObject EnemyPrefab;
    private EnemyController EnemyController;

    protected override void Awake()
    {
        base.Awake();
        this.EnemyController = new EnemyController(this, EnemyPrefab);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        EnemyController.Tick();
    }
    public override void BeginTransition(int sceneIndex)
    {
    }

    public override void SetupStage()
    {
        RaiseSceneReady();
        OnSceneReady?.Invoke();
    }

    protected override void OnExitButton()
    {
    }

    protected override void OnWakeupButton()
    {
    }

    public override PlayerController CreatePlayer()
    {
        return base.CreatePlayer();
    }

    protected override void TransitionToStage(int sceneIndex)
    {
        base.TransitionToStage(sceneIndex);
    }

    public override void GiveControl()
    {
        base.GiveControl();
        OnSceneCompletelyReady?.Invoke();
    }
}
