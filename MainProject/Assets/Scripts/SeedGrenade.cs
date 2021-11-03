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

    private float totalDamage;
    private PlayerController player;

    public void Throw(PlayerController pc, Vector3 direction, float bonusDamage)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(direction * ThrowForce, ForceMode.VelocityChange);
        totalDamage = BaseDamage + bonusDamage;
        player = pc;

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
                if (lethal)
                {
                    player.GetMonsterXP(enemy.RewardAmount());
                    player.GenerateRage(player.KillRageAmount);
                }
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

        yield return new WaitForSeconds(effectDuration);


        //visuals here


        Destroy(gameObject);
    }


    void OnCollisionEnter(Collision collision)
    {
        if (Sticky)
        {
            Destroy(GetComponent<Rigidbody>());
        }
    }
    
}
