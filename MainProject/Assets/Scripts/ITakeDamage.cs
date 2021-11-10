using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITakeDamage
{
    bool TakeDamage(float amount);
    Team Team {get;}
}
