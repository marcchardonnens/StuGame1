using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPlant : PlantBase, IInteractable
{
    public float UpgradeGrowtimeMultiplier = 0.5f;
    public int SeedRefillAmount = 1;

    public Light Light;
    public GameObject bloom;

    // Start is called before the first frame update
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Grow(GrowTime));
    }

    public void Interact()
    {

        Debug.Log("Seedplant Interact");

        if (grown)
        {
            player.RefillSeeds(SeedRefillAmount);
            Destroy(gameObject);
        }
    }

    public override IEnumerator Grow(float growTime)
    {

        if(GameManager.ProfileData.HasSeedUpgrade)
        {
            growTime *= UpgradeGrowtimeMultiplier;
        }

        yield return base.Grow(growTime);

        Light.enabled = true;
        bloom.SetActive(true);
    
    }

    public string UiText()
    {
        return PlayerUIController.InteractPrefix + "Harvest Seeds + " + SeedRefillAmount;
    }
}
