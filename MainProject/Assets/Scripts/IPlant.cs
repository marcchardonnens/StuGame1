using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlant : ITakeDamage<IPlant>
{
    IEnumerator Grow(float growtime);
}
