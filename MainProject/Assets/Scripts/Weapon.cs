using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private PlayerController player;
    private bool isHeld = false;

    //stats
    public float Damage = 5f;
    public float BaseAttackSpeed = 5;
    public float AnimationTimeScale = 1f;
    public float KnockbackStrength = 0f;
    public float StunDuration = 0f;
    public bool StunAttack = false;
    public float SelfLockAnimationTime = 0f;
    public float Range = 1f;
    public bool BlockIsPercent = false;
    public float Blockvalue = 5f;

    public bool HasRangedAttack = false;
    public GameObject ProjectilePrefab;
    public float ProjectileLifetime = 5f;
    public float ProjectileSpeed = 10f;


    //private Collider ownCollider;



    private float swingTimer = 0f;
    private bool blocking = false;

    void Start()
    {
        //dont update while not picked up
        //needs to change if weapons can be thrown
        enabled = false;
    }

    public void Pickup(PlayerController pc)
    {
        player = pc;
        isHeld = true;
        //set components/values to when it is held
        enabled = true;
        

    }

    public void MeleeAttack(float swingTime)
    {
        blocking = false;
        swingTimer = swingTime * AnimationTimeScale;
    }

    public void RangedAttack()
    {
        if (!HasRangedAttack)
        {
            //should ideally never get here
            return;
        }






    }

    public void Block()
    {
        blocking = true;

        //prevent exploit
        swingTimer = 0f;
    }

    public void Unblock()
    {
        blocking = false;
    }

    void Update()
    {
        swingTimer -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("stick collision");
        if (player != null && swingTimer > 0 && !blocking)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy)
            {
                Debug.Log("enemy damage");
                float totalDamage = Damage + player.BaseDamage;
                bool lethal = enemy.TakeDamage(totalDamage);
                player.GenerateRage(player.MeleeHitRageAmount);

                if (lethal)
                {
                    player.GetMonsterXP(enemy.RewardAmount());
                    player.GenerateRage(player.KillRageAmount);
                }

            }

            //deal dmg to enemies and resources
            //mutiply weapon damage with player basedmg, etc
        }
        if (player != null && blocking)
        {
            SimpleProjectile projectile = collider.GetComponent<SimpleProjectile>();
            if (projectile)
            {
                Debug.Log("projectile block");
                player.GenerateRage(player.BlockRageAmount);

                if (BlockIsPercent)
                {
                    projectile.damage *= Blockvalue;
                }
                else
                {
                    projectile.damage -= Blockvalue;
                    
                    //no healing projectiles
                    if (projectile.damage <= 0f)
                    {
                        projectile.SelfDestruct();
                    }
                }
            }

            //blocking
            //reduce damage of projectile
        }

    }
}
