using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvestableSeed : MonoBehaviour, IInteractable
{
    public static event Action<int> OnSeedHarvest = delegate{};
    public SeedPlant Plant;
    public string Name => "Seed";

    public bool Enabled { get; set; } = true;

    public void Interact()
    {
        if(!Enabled) return;
        OnSeedHarvest?.Invoke(Plant.SeedRefillAmount);
        Enabled = false;
        Plant.RemoveSeed(this);
    }

    public string UiText()
    {
        if(!Enabled) return "";
        return PlayerUIController.InteractPrefix + "Harvest Seed + " + Plant.SeedRefillAmount;
    }
}
