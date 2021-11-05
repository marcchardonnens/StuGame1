using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndSceneClick : MonoBehaviour
{

    public Button endButton;
    // Start is called before the first frame update
    void Start()
    {
        endButton.onClick.AddListener(TaskExitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TaskExitGame () {
        Debug.Log("exit");
        Application.Quit();
    }
}
