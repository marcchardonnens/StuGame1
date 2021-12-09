using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPlant : PlantBase
{
    public Collider outerCollider;
    public GameObject Bubble;
    public float Radius = 10f;
    public float PeriodicDamage = 10f;
    public float PulseTimer = 1f;
    public float ProjectileDamage = 40f;
    public float UpgradeSlowMultiplier = 0.5f;
    private List<Collider> affectedColliders = new List<Collider>();


    public override IEnumerator Grow(float growtime)
    {
        yield return base.Grow(growtime);
        StartCoroutine(PeriodicActions());
    }



    private IEnumerator PeriodicActions()
    {
        yield return new WaitForSeconds(GrowTime);
        while (true)
        {

            List<Collider> toRemove = new List<Collider>();
            foreach (Collider collider in affectedColliders)
            {
                if (collider == null || player == null)
                {
                    continue;
                }
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy)
                {
                    Debug.Log("pulse dmg");
                    bool lethal = enemy.TakeDamage(PeriodicDamage);
                    if (lethal)
                    {
                        toRemove.Add(collider);
                    }
                    StartCoroutine(Pulse());
                }
                else
                {
                    SimpleProjectile projectile = collider.GetComponent<SimpleProjectile>();
                    if (projectile)
                    {
                        if (projectile.gameObject.layer != GameConstants.PLAYERLAYER)
                        {
                            projectile.hp -= ProjectileDamage;
                            if (projectile.hp < 0)
                            {
                                toRemove.Add(collider);
                            }
                        }
                        StartCoroutine(Pulse());
                    }
                }
            }
            toRemove.ForEach(c => affectedColliders.Remove(c));

            yield return new WaitForSeconds(PulseTimer);
        }
    }


    public IEnumerator Pulse()
    {
        GameObject pulse = Instantiate(Bubble);
        pulse.transform.position = transform.position;
        pulse.transform.localScale *= 0;
        for (int i = 0; i < 50; i++)
        {
            pulse.transform.localScale = Vector3.one * (Bubble.transform.localScale.x / 50f) * i;

            yield return new WaitForSeconds(PulseTimer / 500f);
        }

        Destroy(pulse);
    }


    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy)
        {
            affectedColliders.Add(other);
            if (GameManager.ProfileData.HasShieldUpgrade)
            {
                enemy.CombatSpeed *= UpgradeSlowMultiplier;
                enemy.WanderSpeed *= UpgradeSlowMultiplier;
            }
        }
        else
        {
            SimpleProjectile projectile = other.GetComponent<SimpleProjectile>();
            if (projectile)
            {
                if (projectile.gameObject.layer != GameConstants.PLAYERLAYER)
                {
                    affectedColliders.Add(other);
                    if (GameManager.ProfileData.HasShieldUpgrade)
                    {
                        projectile.speed *= UpgradeSlowMultiplier;
                    }

                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        affectedColliders.Remove(other);
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy)
        {
            if (GameManager.ProfileData.HasShieldUpgrade)
            {
                enemy.CombatSpeed /= UpgradeSlowMultiplier;
                enemy.WanderSpeed /= UpgradeSlowMultiplier;
            }
        }
        else
        {
            SimpleProjectile projectile = other.GetComponent<SimpleProjectile>();
            if (projectile)
            {
                if (projectile.gameObject.layer != GameConstants.PLAYERLAYER)
                {
                    if (GameManager.ProfileData.HasShieldUpgrade)
                    {
                        projectile.speed /= UpgradeSlowMultiplier;
                    }

                }
            }
        }
    }
}
