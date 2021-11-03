using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeedUI : MonoBehaviour
{
    public int seedCounter;
    public int seedAmount; 

    private TextMeshProUGUI seedAmountText;

    void Start() {
        seedAmountText = GetComponent<TextMeshProUGUI>();
        
    }

    void Update() {
        //seedCounter = 1;
        //seedAmount = 10;
        seedAmountText.text = seedCounter + " / " + seedAmount;
    }

    public void SetSeedCounter(int counter) {
        seedCounter = counter;
    }
    
    public void SetSeedAmount(int amount) {
        seedAmount = amount;
    }

}
