using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RandomChoice<T>
{
    public T prefab;
    public float SpawnWeight = 1f;

    public static T Choose(RandomChoice<T>[] choices, System.Random RNG)
    {
        float totalWeight = 0f;
        foreach (RandomChoice<T> choice in choices)
        {
            totalWeight += choice.SpawnWeight;
        }

        double value = RNG.NextDouble() * totalWeight;
        float curWeight = 0f;
        foreach (RandomChoice<T> choice in choices)
        {
            curWeight += choice.SpawnWeight;
            if (curWeight >= value)
            {
                return choice.prefab;
            }
        }

        return default;
    }

        public static T Choose(RandomChoice<T>[] choices)
    {
        float totalWeight = 0f;
        foreach (RandomChoice<T> choice in choices)
        {
            totalWeight += choice.SpawnWeight;
        }

        float value = UnityEngine.Random.Range(0,1f) * totalWeight;
        float curWeight = 0f;
        foreach (RandomChoice<T> choice in choices)
        {
            curWeight += choice.SpawnWeight;
            if (curWeight >= value)
            {
                return choice.prefab;
            }
        }

        return default;
    }

}
