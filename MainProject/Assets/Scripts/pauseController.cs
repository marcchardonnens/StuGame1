using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pauseController : MonoBehaviour
{
    private Canvas CanvasObject; // Assign in inspector


    // Start is called before the first frame update
    void Start()
    {
        CanvasObject = GetComponent<Canvas> ();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key was pressed");
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("Escape key was released");
            CanvasObject.enabled = !CanvasObject.enabled;
        }

    }
}
