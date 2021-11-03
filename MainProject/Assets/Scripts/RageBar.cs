using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RageBar : MonoBehaviour
{
    public Slider slider;
    private TextMeshProUGUI percentage; //tbd

    public void SetMaxRage(float rage) {
        slider.maxValue = rage;
        slider.value = rage;
        //percentage.text = health + ""; //tb
    }

    public void SetRage(float rage) {
        slider.value = rage;
        percentage.text = rage + ""; //tb
    }

}
