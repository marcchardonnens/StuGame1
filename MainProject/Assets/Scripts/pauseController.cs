using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        //tbd
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //playerCon.UnlockCursor();
            if (GameIsPaused)
            {
                //playerCon.UnlockCursor();
                //tbd better
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Resume();
            } else {
                Pause();
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;
            }
        }

    }

    public void Resume () {
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        
    }

    public void Pause () {
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        Time.timeScale = 0f;        
    }

    public void TaskWakeUpButton() {
        Debug.Log("Wake Up");
        //TBD Spielstand speichern
        SceneManager.LoadScene("HubScene", LoadSceneMode.Single);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void TaskExitButton() {
        Debug.Log("Exit");
        Application.Quit();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
