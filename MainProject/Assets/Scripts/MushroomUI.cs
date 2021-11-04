using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MushroomUI : MonoBehaviour
{
    public int mushroomCounter; 

    private TextMeshProUGUI mushroomCounterText;

    void Start() {
        mushroomCounterText = GetComponent<TextMeshProUGUI>();
        
    }

    void Update() {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            mushroomCounter += 1;
        }*/
        mushroomCounterText.text = "" + mushroomCounter; //tbd
    }

    public void SetMushroomCounter(int counter) {
        mushroomCounter = counter;
    }

}
