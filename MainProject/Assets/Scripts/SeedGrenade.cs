using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedGrenade : MonoBehaviour
{
    public float ThrowForce = 20f;
    public float ExplosionForce = 50f;
    public float Delay = 3f;
    public float ExplodeRadius = 10f;
    public float BaseDamage = 30f;
    public bool Sticky = true;
    public bool upgraded = false;
    public GameObject LingerField;
    public int LingerCicles = 10;
    public float LingerDamage = 10f;
    public float LingerPulseDelay = 1f;

    private float totalDamage;
    private PlayerController player;

    public void Throw(PlayerController pc, Vector3 direction, float bonusDamage, bool upgraded)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(direction * ThrowForce, ForceMode.VelocityChange);
        totalDamage = BaseDamage + bonusDamage;
        player = pc;
        this.upgraded = upgraded;

        StartCoroutine(Explode());
    }


    public IEnumerator Explode()
    {

        yield return new WaitForSeconds(Delay);


        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplodeRadius);

        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy)
            {
                bool lethal = enemy.TakeDamage(totalDamage);

            }


            Rigidbody orb = collider.GetComponent<Rigidbody>();
            if (orb)
            {
                orb.AddExplosionForce(ExplosionForce, transform.position, ExplodeRadius);
            }

            //TODO damage to resources

        }


        StartCoroutine(PlayEffect());


    }

    public IEnumerator PlayEffect(float effectDuration = 0.1f)
    {


        StartCoroutine(Pulse());

        if (GameManager.ProfileData.HasGrenadeUpgrade)
        {
            StartCoroutine(LingeringField());
        }
        else
        {
            yield return new WaitForSeconds(effectDuration);
            Destroy(gameObject);
        }

        //visuals here


    }

    public IEnumerator LingeringField()
    {

        GameObject field = Instantiate(LingerField);
        field.transform.position = transform.position;
        field.transform.localScale *= ExplodeRadius;
        for (int i = 0; i < LingerCicles; i++)
        {

            StartCoroutine(Pulse());

            Collider[] colliders = Physics.OverlapSphere(transform.position, ExplodeRadius);

            foreach (Collider collider in colliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if(enemy)
                {
                    enemy.TakeDamage(LingerDamage);
                }
            }

            yield return new WaitForSeconds(LingerPulseDelay);
        }

        Destroy(field);
        Destroy(gameObject);

    }

    public IEnumerator Pulse()
    {
        GameObject pulse = Instantiate(LingerField);
        pulse.transform.position = transform.position;
        pulse.transform.localScale *= 0;
        for (int i = 0; i < 50; i++)
        {
            pulse.transform.localScale = Vector3.one * (ExplodeRadius / 50f) * i;

            yield return new WaitForSeconds(LingerPulseDelay / 500f);
        }

        Destroy(pulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Sticky)
        {
            Destroy(GetComponent<Rigidbody>());
        }
    }
    
}
