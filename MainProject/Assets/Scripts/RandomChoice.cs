using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomChoice
{
    public GameObject prefab;
    public float SpawnWeight;

    public static GameObject Choose(RandomChoice[] choices, System.Random RNG)
    {
        float totalWeight = 0f;
        foreach (RandomChoice choice in choices)
        {
            totalWeight += choice.SpawnWeight;
        }

        double value = RNG.NextDouble() * totalWeight;
        float curWeight = 0f;
        foreach (RandomChoice choice in choices)
        {
            curWeight += choice.SpawnWeight;
            if (curWeight >= value)
            {
                return choice.prefab;
            }
        }

        return null;
    }

}
