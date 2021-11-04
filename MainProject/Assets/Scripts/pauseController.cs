using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseController : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    public Button resumeButton, wakeupButton, exitButton;

    // Start is called before the first frame update
    void Start()
    {
        //CanvasObject = GetComponent<Canvas> ();
        //CanvasObject.enabled = false;
        pauseMenuUI.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        //settingsButton.onClick.AddListener(TaskSettingsButton);
        wakeupButton.onClick.AddListener(TaskWakeUpButton);
        exitButton.onClick.AddListener(TaskExitButton);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            } else {
                Pause();
            }
        }

    }

    public void Resume () {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Time.timeScale = 1f;
        
    }

    public void Pause () {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Time.timeScale = 0f;
        
    }

    public void TaskWakeUpButton() {
        Debug.Log("Wake Up");
    }

    public void TaskExitButton() {
        Debug.Log("Exit");
        Application.Quit();
    }
}
