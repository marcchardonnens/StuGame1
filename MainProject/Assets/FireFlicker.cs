using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    private const float Time = 0.05f;
    private const float TimeLarge = 0.1f;
    private const float FlickerIntensityRange = 0.15f;
    private const float FlickerIntensityRangeLarge = 0.33f;
    Light Fire;
    private float baseIntensity;
    // Start is called before the first frame update
    void Start()
    {
        if(Fire == null)
            Fire = GetComponentInChildren<Light>();
        baseIntensity = Fire.intensity;
        StartCoroutine(SmallFlicker());
        StartCoroutine(SlowBigFlicker());
    }

    // Update is called once per frame
    private IEnumerator SmallFlicker()
    {
        while(true)
        {
            Fire.intensity += Util.HalfRandomRange(baseIntensity * FlickerIntensityRange);
            yield return new WaitForSecondsRealtime(Time);
        }
    }
    private IEnumerator SlowBigFlicker()
    {
        while(true)
        {
            Fire.intensity = baseIntensity + Util.HalfRandomRange(baseIntensity * FlickerIntensityRangeLarge);
            yield return new WaitForSecondsRealtime(TimeLarge);
        }
    }
}
