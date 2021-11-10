using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Start is called before the first frame update
    void Start()
    {

    }

    

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (GameManager.Instance.SceneLoaded && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneLoaded = false;
            // CreatePlayer();
        }
        else if (GameManager.Instance.SceneCompletelyReady && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneCompletelyReady = false;
        }
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
        //might not be needed
        StageReady = true;
    }

    public void InitiateNewRun()
    {

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

}
