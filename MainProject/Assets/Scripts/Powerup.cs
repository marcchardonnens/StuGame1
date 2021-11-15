using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
    IronShroom,
    RedShroom,
    BlueShroom,
    GreenShroom,
    GoldShroom,
    WoodShroom,
    StoneShroom,
    //TransparentShroom,
    //YellowShroom,
    //PlantShroom,
}

[RequireComponent(typeof(Collider))]
public class Powerup : MonoBehaviour
{
    public PowerupType Type;
    public float EffectScale = 1f;

    void OnTriggerEnter(Collider collider)
    {
        PlayerController player = collider.GetComponent<PlayerController>();
        if (player)
        {
            player.ConsumeShroom(this);
            Destroy(gameObject);
        }
    }

}
