using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene2 : MonoBehaviour
{
    public Button confirmButton;
    // Start is called before the first frame update
    void Start()
    {
        confirmButton.onClick.AddListener(TaskConfirmButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TaskConfirmButton()
    {
        Debug.Log("start");
        //if im HubScene gewesen
            SceneManager.LoadScene("GameplayScene", LoadSceneMode.Single);
        
    }
}
