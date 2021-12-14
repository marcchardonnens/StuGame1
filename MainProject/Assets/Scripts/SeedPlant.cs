using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPlant : PlantBase//, IInteractable
{
    public static event Action<int> OnSeedPlantHarvest = delegate{};
    public float UpgradeGrowtimeMultiplier = 0.5f;
    public int SeedRefillAmount = 1;
    [HideInInspector] public List<HarvestableSeed> Seeds = new List<HarvestableSeed>();
    public Light Light;
    void Awake()
    {
        Seeds.AddRange(GetComponentsInChildren<HarvestableSeed>(true));
        foreach (var seed in Seeds)
        {
            seed.Plant = this;
        }
    }

    public void RemoveSeed(HarvestableSeed seed)
    {
        Seeds.Remove(seed);
        Destroy(seed.gameObject);
        if(Seeds.Count <= 0)
        {
            Destroy(gameObject);
        }
    }

    public override IEnumerator Grow()
    {

        if(GameManager.ProfileData.HasSeedUpgrade)
        {
            GrowTime *= UpgradeGrowtimeMultiplier;
        }
        yield return base.Grow();
        Light.enabled = true;
        Util.SetChildrenActive(transform, true);

    }
}
