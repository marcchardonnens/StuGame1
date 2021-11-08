using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene1 : MonoBehaviour
{
    public Button confirmButton;
    public TMPro.TextMeshProUGUI text;

    public bool ready = false;
    public bool start = false;
    // Start is called before the first frame update
    void Start()
    {
        confirmButton.onClick.AddListener(TaskConfirmButton);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public bool Ready()
    {
        //confirmButton.name = ready;
        text.text = "Ready";

        GameManager.Instance.UnlockCursor();

        ready = true;
        return ready;
    }

    public void TaskConfirmButton()
    {


        if(ready)
        {
            start = true;
            gameObject.SetActive(false);
            //Destroy(gameObject);
            Debug.Log("start");
        }

        //if im MenuScene gewesen
            //SceneManager.LoadScene("HubScene", LoadSceneMode.Single);
        
    }
}
