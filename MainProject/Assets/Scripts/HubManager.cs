using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public static PlayerController Player;
    public Vector3 PlayerWakeupPos;
    public Vector3 PlayerWakeupRot;
    public Vector3 PlayerEnterHomePos;
    public Vector3 PlayerEnterHomeRot;
    public bool HasWokenUp = true;

    private void Awake()
    {
        Debug.Log("Hub Manager Awake");

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.SceneLoaded && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneLoaded = false;
            SetupHubscene();
        }
        else if (GameManager.Instance.SceneCompletelyReady && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneCompletelyReady = false;
            // GameManager.Instance.PlayerHasControl = true;
            // GameManager.Instance.LockCursor();
            // Player.playerUI.ShowHubHud();
            // Player.playerCamera.enabled = true;
        }
    }

    public void SetupHubscene()
    {
        if (HasWokenUp)
        {
            Player = Instantiate(PlayerPrefab, PlayerWakeupPos, Quaternion.Euler(PlayerWakeupRot)).GetComponent<PlayerController>();
        }
        else
        {
            Player = Instantiate(PlayerPrefab, PlayerEnterHomePos, Quaternion.Euler(PlayerEnterHomeRot)).GetComponent<PlayerController>();
        }
        GameManager.Instance.PlayerHasControl = false;
        Player.playerCamera.enabled = true;
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
