using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaManager : GameplayManagerBase
{
    public GameObject EnemyPrefab;
    private EnemyController EnemyController;

    protected override void Awake() {
        base.Awake();
        this.EnemyController = new EnemyController(this, EnemyPrefab);
    }
    public override void BeginTransition(int sceneIndex)
    {
        throw new System.NotImplementedException();
    }

    public override void SetupStage()
    {

    }

    protected override void OnExitButton()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnWakeupButton()
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        EnemyController.Tick();
    }
}
