using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPlant : MonoBehaviour
{
    public Collider outerCollider;
    public float Radius = 10f;
    public float PeriodicDamage = 10f;
    public float PulseTimer = 1f;
    public float ProjectileDamage = 40f;
    public float Duration = 0f;
    public float GrowTime = 0f;


    //private bool grown = false;
    private Vector3 finalScale;
    private List<Collider> affectedColliders = new List<Collider>();
    private PlayerController player;

    void Awake()
    {
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }
    }
    void Start()
    {
        player = StageManager.Player;

        StartCoroutine(PeriodicActions());

    }

    void Update()
    {
        StartCoroutine(Grow());
    }


    private IEnumerator Grow()
    {
        if (transform.localScale.x < finalScale.x)
        {
            yield return null;
            transform.localScale += (finalScale / GrowTime) * Time.deltaTime;
        }
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
                        player.GetMonsterXP(enemy.RewardAmount());
                        toRemove.Add(collider);
                    }
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
                    }
                }
            }
            toRemove.ForEach(c => affectedColliders.Remove(c));

            yield return new WaitForSeconds(PulseTimer);
        }
    }


    private void Pulse()
    {

    }
    

    void OnTriggerEnter(Collider other)
    {
        affectedColliders.Add(other);
    }

    void OnTriggerExit(Collider other)
    {
        affectedColliders.Remove(other);
    }


}
