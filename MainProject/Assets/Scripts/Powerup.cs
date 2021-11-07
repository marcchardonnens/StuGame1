using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
