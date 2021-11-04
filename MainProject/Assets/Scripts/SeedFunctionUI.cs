using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeedFunctionUI : MonoBehaviour
{
    
private TextMeshProUGUI seedFunctionText;
private string seedFunction;

    void Start() {
        seedFunctionText = GetComponent<TextMeshProUGUI>();
        
    }

    void Update() {
        //seedFunctionText.text = "no attack";
    }

    public void SetFunctionName(int functionName) {
        switch (functionName) {
            case 1: 
                seedFunctionText.text = "grenade";
            break;
            case 2:
                seedFunctionText.text = "place shield";
            break;
            case 3:
                seedFunctionText.text = "place turret";
            break;
            case 4:
                seedFunctionText.text = "plant";
            break;
            default:
                seedFunctionText.text = "";
            break;

        }

        //seedFunctionText.text = seedFunction;
        
    }
}
