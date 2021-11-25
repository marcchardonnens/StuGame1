using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//TODO Boss Behaviour
public class Boss : Enemy
{
    public int RangedAttackSalveAmount = 5;
    public float RangedAttackSalveSpreadAngle = 30f;
    public float MeteorDamage = 50f;
    public float MeteorDelay = 2f;
    public float MeteorImpactRadius = 3f;
    public float MeteorProjectileScale = 5f;
    public float MeteorAttackCooldown = 10f;
    public int MeteorSalveAmountMin = 5;
    public int MeteorSalveAmountLevel = 2;
    public float DelayBetweenMeteorsMin = 0.1f;
    public float DelayBetweenMeteorsMax = 0.5f;
    public float MeteorSpawnHeight = 100f;
    public GameObject MeteorPrefab;
    public GameObject MeteorIndicatorPrefab;


    public GameObject EnemyPrefab;
    public int AddSummonAmount = 3;
    public float AddSummonCoolDown = 20f;
    public float AddSummonRadius = 20f;
    

    private float meteorCd = 0f;
    private float summonCd = 0f;

    protected override IEnumerator PlayPeriodicSound()
    {
        AudioManager.Instance.PlayClip(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.EnemyBoss, Sounds));
        yield return new WaitForSeconds(Random.Range(2f,4f));
    }

    public override int RewardAmount()
    {
        float reward = BaseRewardAmount + currentLevel * EnemyLevelRewardMultiplier * BaseRewardAmount ;

        float ragebonus = reward * player.RageLevel * PlayerRageLevelRewardMultiplier;

        reward += ragebonus;

        float randomBonus = Random.Range(0f, reward * RandomRewardMultiplier);

        reward += randomBonus;

        return Mathf.RoundToInt(reward);
    }


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }
  
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        StartCoroutine(MeteorAttack());
        SummonAdds();

    }




    protected override void CalcRangedPos()
    {
        float dist = Mathf.Abs(Vector3.Distance(transform.position, player.transform.position));
        Vector3 pos = Vector3.MoveTowards(transform.position, player.transform.position, dist - AttackRange);

        //randomize pos slightly
        Vector3 randompos = Random.insideUnitSphere;
        randompos += pos;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randompos, out hit, wanderDistance, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }


    }

    // protected override void RangedAttack()
    // {
        // //cooldown check
        // if (nextRangedCd > Time.time)
        // {
        //     return;
        // }
        // nextRangedCd = Time.time + RangedAttackCooldown;


        // for (int i = 0; i < RangedAttackSalveAmount; i++)
        // {

        //     float angle;
        //     if (RangedAttackSalveAmount % 2 == 1)
        //     {
        //         angle = (RangedAttackSalveSpreadAngle * 2) / (RangedAttackSalveAmount - 1);
        //         angle = angle * (((RangedAttackSalveAmount - i) - RangedAttackSalveAmount / 2) - 1);

        //     }
        //     else
        //     {
        //         float spread = (RangedAttackSalveSpreadAngle * 2) / (RangedAttackSalveAmount - 1);
        //         angle = spread;
        //         angle = angle * (((RangedAttackSalveAmount - i) - RangedAttackSalveAmount / 2));
        //         angle -= spread / 2;

        //     }

        //     Vector3 direction = currentTarget.transform.position - transform.position;
        //     direction = direction.normalized;
        //     direction *= transform.localScale.z;
            

        //     SimpleProjectile projectile =
        //         Instantiate(ProjectilePrefab, direction + transform.position, Quaternion.identity).GetComponent<SimpleProjectile>();
        //     if (currentLevel < 2)
        //     {
        //         //simple projectile
        //         projectile.SetPropertiesSimple(gameObject, direction + transform.position, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, currentTarget, Team);
        //     }
        //     else
        //     {
        //         //slowtracking projectile
        //         projectile.SetPropertiesTracked(gameObject, direction + transform.position, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, true, ProjectileTurnSpeed, currentTarget.transform,false, Team);
        //     }

        //     projectile.transform.RotateAround(transform.position, Vector3.up, angle);

        // }
    // }

    // protected override void MeleeAttack()
    // {
    //     //cooldown check
    //     if (nextMeleeCd > Time.time)
    //     {
    //         return;
    //     }
    //     nextMeleeCd += Time.time + MeleeAttackCooldown;


    //     float meleeAttackHeight = 0.25f;
    //     Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, MeleeRange);
    //     Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2, MeleeRange);
    //     RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, MeleeRange, Vector3.forward);

    //     foreach (RaycastHit hit in hits)
    //     {

    //         //do damage to player
    //         PlayerController pc = hit.collider.GetComponent<PlayerController>();
    //         if (pc)
    //         {   
    //             pc.TakeDamage(MeleeDamage);
    //         }


    //         //TODO do damage to resources


    //     }


    // }

    protected override void OnDrawGizmosSelected()
    {
        float meleeAttackHeight = 0.25f;
        Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, MeleeRange);
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2f, MeleeRange);

        //Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(p1, MeleeRange);
        Gizmos.DrawWireSphere(p2, MeleeRange);
        
    }


    private IEnumerator MeteorAttack()
    {
        // if (outOfCombatTimer > Time.time && meteorCd < Time.time && !IsDead())
        // {
        //     outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
        //     meteorCd = Time.time + MeteorAttackCooldown;
        //     int meteors = MeteorSalveAmountMin + MeteorSalveAmountLevel * currentLevel;
        //     for (int i = 0; i < meteors; i++)
        //     {
        //         float nextDelay = Random.Range(DelayBetweenMeteorsMin, DelayBetweenMeteorsMax);


        //         SpawnMeteor(nextDelay);

                // yield return new WaitForSeconds(nextDelay);
        //     }
        // }

        yield return null;
    }

    private void SpawnMeteor(float delay)
    {

        //choose point near player
        int retries = 3;
        float accuracyOffset = 10f - 1f*currentLevel;

        Vector3 playerPos = player.transform.position;

        if(currentLevel > 5)
        {
            //moveprediction
        }

        Vector3 targetPos = Random.insideUnitSphere* accuracyOffset + playerPos;

        RaycastHit hit;
        for (int i = 0; i < retries; i++)
        {

            if(Physics.Raycast(new Vector3(targetPos.x, targetPos.y + 100f, targetPos.z), Vector3.down, out hit, 2000f, 1 << GameConstants.GROUNDLAYER ))
            {

                //make indicator
                GameObject indicator = Instantiate(MeteorIndicatorPrefab, hit.point, Quaternion.identity);

                GameObject meteorGO = Instantiate(MeteorPrefab, new Vector3(transform.position.x, hit.point.y + MeteorSpawnHeight, transform.position.z), Quaternion.LookRotation(hit.point));
                Meteor meteor = meteorGO.GetComponent<Meteor>();
                meteor.Initialize(hit.point, MeteorDamage, MeteorImpactRadius, indicator , MeteorDelay);


                break;

            }
        }


        //Meteor meteor = Instantiate(MeteorPrefab, )

    }


    public void SummonAdds()
    {
        // if (outOfCombatTimer > Time.time && summonCd < Time.time && !IsDead())
        // {
        //     summonCd = Time.time + AddSummonCoolDown;
        //     for (int i = 0; i < AddSummonAmount; i++)
        //     {
        //         Vector3 pos = Random.insideUnitSphere * AddSummonRadius + transform.position;

        //         NavMeshHit hit;
        //         if(NavMesh.SamplePosition(pos, out hit, AddSummonRadius, agent.areaMask))
        //         {
        //             Enemy enemy = Instantiate(EnemyPrefab, pos, Quaternion.identity).GetComponent<Enemy>();
        //             enemy.transform.position = hit.position;
        //         }
        //     }
        // }
    }



}
