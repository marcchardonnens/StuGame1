using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneController : MonoBehaviour
{
    public Button startButton, profileButton, settingsButton, exitButton;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
        startButton.onClick.AddListener(TaskStartButton);
        profileButton.onClick.AddListener(TaskProfileButton);
        settingsButton.onClick.AddListener(TaskSettingsButton);
        exitButton.onClick.AddListener(TaskExitButton);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TaskStartButton()
    {
        Debug.Log("start");
        //SceneManager.LoadScene("newSceneTest", LoadSceneMode.Single);
        //SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }

    public void TaskProfileButton()
    {
        Debug.Log("profile");
    }
    
    public void TaskSettingsButton()
    {
        Debug.Log("settings");
    }
    
    public void TaskExitButton()
    {
        Debug.Log("exit");
    }


}
