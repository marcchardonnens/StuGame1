using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage<in T>
{
    float MaxHP { get; set; }
    float CurrentHP {get;}
    bool TakeDamage(float amount);
    Team Team {get;}
    event Action<float> OnTakeDamage;
    event Func<T> OnDeath;
}
