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
    public bool HasGrenadeUpgrade = true;
    public bool HasTurretUpgrade = true;
    public bool HasShieldUpgrade = true;
    public bool HasSeedUpgrade = true;
    public bool HasMeleeUpgrade = true;
    public bool HasHouseUpgrade = true;
    public bool HasSeedContainerUpgrade = true;
    public bool HasWoodInventoryUpgrade = true;

    public void SaveToFile()
    {
    }

    public void LoadFromFile()
    {

    }



}
