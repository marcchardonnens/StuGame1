using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileData
{
    public bool FirstRun = true;

    public int StoryDeathProgress = 0;
    public int StoryReturnProgress = 0;
    public int StorySuccessProgress = 0;

    public bool UnlockedLightbulb = false;

    public int MonsterXPTotal = 0;
    public int MonsterXPCurrent = 0;
    public int WoodTotal = 0;
    public int WoodCurrent = 0;
    public int CalciumTotal = 0;
    public int CalciumCurrent = 0;
    public int LuciferinTotal = 0;
    public int LuciferinCurrent = 0;
    public int OxygenTotal = 0;
    public int OxygenCurrent = 0;
    public int HouseUpgradeLevel = 0;
    public bool HasGrenadeUpgrade = false;
    public bool HasTurretUpgrade = false;
    public bool HasShieldUpgrade = false;
    public bool HasSeedUpgrade = false;
    public bool HasMeleeUpgrade = false;
    public bool HasSeedContainerUpgrade = false;
    public bool HasWoodInventoryUpgrade = false;

    public ProfileData(bool unlocked)
    {
        if (unlocked)
        {
            this.HouseUpgradeLevel = 3;
            this.HasGrenadeUpgrade = true;
            this.HasTurretUpgrade = true;
            this.HasShieldUpgrade = true;
            this.HasSeedUpgrade = true;
            this.HasMeleeUpgrade = true;
            this.HasSeedContainerUpgrade = true;
            this.HasWoodInventoryUpgrade = true;
        }
    }

    public void SaveToFile()
    {

    }

    public void LoadFromFile()
    {

    }



}
