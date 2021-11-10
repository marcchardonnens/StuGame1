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
            SetupPlayer();
        }
        else if (GameManager.Instance.SceneCompletelyReady && GameManager.Instance.CurrentSceneIndex == 1)
        {
            GameManager.Instance.SceneCompletelyReady = false;
        }
    }

    public void SetupPlayer()
    {
        if (HasWokenUp)
        {
            Player = Instantiate(PlayerPrefab, PlayerWakeupPos, Quaternion.Euler(PlayerWakeupRot)).GetComponent<PlayerController>();
        }
        else
        {
            Player = Instantiate(PlayerPrefab, PlayerEnterHomePos, Quaternion.Euler(PlayerEnterHomeRot)).GetComponent<PlayerController>();
        }
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
