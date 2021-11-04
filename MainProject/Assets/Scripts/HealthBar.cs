using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public bool ShowNumbers = false;
    public Slider slider;

    public void SetMaxHealth(float health) {
        slider.maxValue = health;
        //slider.value = health;

        //percentage.text = health + ""; //tb
    }

    public void SetHealth(float health) {
        slider.value = health;
        //percentage.text = health + ""; //tb
    }

}
