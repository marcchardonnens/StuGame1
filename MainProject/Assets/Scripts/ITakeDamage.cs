using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
    float MaxHP { get; set; }
    float CurrentHP {get;}
    bool TakeDamage(float amount);
    Team Team {get;}
    event Action<ITakeDamage, float> OnTakeDamage;
    event Action<ITakeDamage> OnDeath;
}
