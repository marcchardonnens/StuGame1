using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSceneController : MonoBehaviour
{
    public Button startButton, profileButton, settingsButton, exitButton;
    public Canvas CanvasObject;

    public TMPro.TextMeshProUGUI profileText;

    public bool UnlockedProfile = false;
    

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
        startButton.onClick.AddListener(TaskStartButton);
        profileButton.onClick.AddListener(TaskProfileButton);
        settingsButton.onClick.AddListener(TaskSettingsButton);
        exitButton.onClick.AddListener(TaskExitButton);
        
        CanvasObject = GetComponent<Canvas> ();


        //profileText.text = "Toggle: Fresh Profile";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TaskStartButton()
    {
        Debug.Log("start");
        SceneManager.LoadScene("HubSceneFinal", LoadSceneMode.Single);
    }

    public void TaskProfileButton()
    {
        UnlockedProfile = !UnlockedProfile;
        
        if(UnlockedProfile)
        {
            //profileText.text = "Toggle: Unlocked Profile";
            GameManager.UnlockEverything();
        }
        else
        {

            //profileText.text = "Toggle: Fresh Profile";
            GameManager.NewProfile();
        }
        Debug.Log("profile");
    }
    
    public void TaskSettingsButton()
    {
        Debug.Log("settings");
    }
    
    public void TaskExitButton()
    {
        Debug.Log("exit");
        Application.Quit();
    }


}
