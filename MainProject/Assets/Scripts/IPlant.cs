using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlant : ITakeDamage
{
    IEnumerator Grow(float growtime);
}
