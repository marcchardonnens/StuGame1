using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RageBar : MonoBehaviour
{
    public Slider slider;
    private TextMeshProUGUI percentage; //tbd

    public PlayerController player;

    private void Start()
    {
        slider.value = 0;
    }

    private void Update()
    {
        slider.maxValue = player.RageLevelThreshholdCurrent;
        slider.value = player.Rage;
    }

    public void SetMaxRage(float rage) {
        slider.maxValue = rage;
    }

    public void SetRage(float rage) {
        slider.value = rage;
    }

    private void SetText()
    {
        percentage.text = slider.value + "/" + slider.maxValue; //tb
    }

}
