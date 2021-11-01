using System;using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum EnemyState
{
    Spawning,
    Idle,
    Wandering,
    EnteringCombat,
    RangedAttacking,
    MeleeAttacking,
    ReturnToSpawn,
    Dying,
    Dead,
}





public class Enemy : MonoBehaviour
{
    public float Health = 100f;
    public float MeleeDamage = 15f;
    public float RangedDamage = 5f;
    public float PlayerDetectRange = 15f;
    public float PlayerLoseRange = 25f;
    public float spawnReturnDistance = 5f;
    public float spawnTime = 2f;
    public float deathTime = 2f;
    public float meleeRange = 3f;
    public float attackRange = 15f;
    public float MeleeAttackCooldown = 2f;
    public float RangedAttackCooldown = 4f;

    public GameObject ProjectilePrefab;
    public float ProjectileLifetime = 10f;
    public float ProjectileSpeed = 5f;
    public float ProjectileTurnSpeed = 5f;
    public float ProjectileHP = 50f; //relevant for aura plant for example
    public float ProjectileTrackingChance = 0.5f; //slowtracking or simple

    public float wanderSpeed = 2f;
    public float wanderDistance = 5f;
    public float combatSpeed = 4f;

    public float Gravity = 20f;

    private Vector3 spawnPoint;

    //AI stuff
    private const int MELEEMASK = 1 << 0  | 1 << 3;
    private const int RANGEDMASK = 1 << 0 | 1 << 4;
    private const int WALKABLEMASK = 1 << 0;
    private const int MELEEONLYMASK = 1 << 3;
    private const int RANGEDONLYMASK = 1 << 4;

    private const float SLOWUPDATETIME = 1f;

    public float AreaDefaultCost = 1f;
    public float nonPrioAreaCost = 10f;

    public float aiCirclingMargin = 3f; //choosing random pos within this range from target
    private NavMeshAgent agent;
    private EnemyState currentState;
    private PlayerController player;
    private Transform currentTarget = null;

    private float slowupdate = SLOWUPDATETIME;
    private float nextMeleeCd = 0f;
    private float nextRangedCd = 0f;



    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();
        spawnPoint = transform.position;
        currentState = EnemyState.Spawning;
        //agent.areaMask = WALKABLEMASK;
        StartCoroutine(ExecuteIn(2, () => currentState = EnemyState.Idle));
    }

    // Update is called once per frame
    void Update()
    {
        slowupdate -= Time.deltaTime;

        if (slowupdate <= 0)
        {
            EnemyActions(true);
        }
        else
        {
            EnemyActions(false);
        }




    }


    private void EnemyActions(bool isSlowUpdate = false)
    {
        if (Health <= 0 && currentState != EnemyState.Dead)
        {
            //set death state regardless of current state
            currentState = EnemyState.Dying;
        }


        switch (currentState)
        {
            case EnemyState.Spawning:
            {
                //play animation


                //state changed in start method
                break;
            }

            case EnemyState.Idle:
            {

                if (Vector3.Distance(player.gameObject.transform.position, transform.position) < PlayerDetectRange)
                {
                    currentState = EnemyState.EnteringCombat;
                    break;
                }

                Vector3 randompos = Random.insideUnitSphere * wanderDistance;
                randompos += spawnPoint;
                NavMeshHit hit;
                    
                NavMesh.SamplePosition(randompos, out hit, wanderDistance, agent.areaMask);
                agent.SetDestination(hit.position);

                currentState = EnemyState.Wandering;
                break;
            }

            case EnemyState.Wandering:
            {
                if (Mathf.Abs(Vector3.Distance(player.gameObject.transform.position, transform.position)) < PlayerDetectRange)
                {
                    currentState = EnemyState.EnteringCombat;
                    break;
                }

                if (!agent.hasPath)
                {
                    currentState = EnemyState.Idle;
                }

                break;
            }

            case EnemyState.EnteringCombat:
            {
                currentTarget = player.transform;
                currentState = (Random.Range(0, 1) > 0)
                    ? EnemyState.MeleeAttacking
                    : EnemyState.RangedAttacking;

                if (currentState == EnemyState.MeleeAttacking)
                {
                    SetMeleeAgent();
                }
                else
                {
                    SetRangedAgent();
                }
                
                break;
            }

            case EnemyState.RangedAttacking:
            {
                float distToPlayer = Mathf.Abs(Vector3.Distance(player.transform.position, transform.position));
                if (distToPlayer > PlayerLoseRange)
                {
                    currentState = EnemyState.ReturnToSpawn;
                    break;
                }
                else if (distToPlayer < meleeRange)
                {
                    MeleeAttack();
                    agent.SetDestination(player.transform.position);
                    break;  
                }
                else if (distToPlayer < attackRange)
                {
                    RangedAttack();
                }

                //movement
                if (isSlowUpdate)
                {
                    CalcRangedPos();
                }
                
                break;
            }

            case EnemyState.MeleeAttacking:
            {
                float distToPlayer = Mathf.Abs(Vector3.Distance(player.transform.position, transform.position));
                if (distToPlayer > PlayerLoseRange)
                {
                    currentState = EnemyState.ReturnToSpawn;
                }
                else if (distToPlayer < meleeRange)
                {
                    MeleeAttack();
                }
                else if (distToPlayer < attackRange)
                {
                    RangedAttack();
                }

                if (isSlowUpdate)
                {
                    agent.SetDestination(player.transform.position);
                }


                break;
            }

            case EnemyState.ReturnToSpawn:
            {
                if (Mathf.Abs(Vector3.Distance(spawnPoint, transform.position)) < spawnReturnDistance)
                {
                    currentState = EnemyState.Idle;
                }

                if(isSlowUpdate)
                {
                    agent.SetDestination(spawnPoint);
                }

                break;
            }

            case EnemyState.Dying:
            {

                //play death animation

                GetComponent<CharacterController>().enabled = false;
                GetComponent<NavMeshAgent>().enabled = false;
                GetComponent<Collider>().enabled = false;

                transform.eulerAngles += new Vector3((90f / deathTime) * Time.deltaTime, 0, 0);
                transform.position += new Vector3(0, (2f / deathTime) * Time.deltaTime, 0);
                    
                break;
            }

            case EnemyState.Dead:
            {

                //delete object;

                break;
            }

        }
    }

    private void CalcRangedPos()
    {
        float dist = Mathf.Abs(Vector3.Distance(transform.position, player.transform.position));
        Vector3 pos = Vector3.MoveTowards(transform.position, player.transform.position, dist - attackRange);

        //randomize pos slightly
        Vector3 randompos = Random.insideUnitSphere * aiCirclingMargin;
        randompos += pos;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randompos, out hit, wanderDistance, agent.areaMask))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            //become melee
            SetMeleeAgent();
            currentState = EnemyState.MeleeAttacking;
        }

    }

    private void RangedAttack()
    {
        //cooldown check
        if (nextRangedCd > Time.time)
        {
            return;
        }
        nextRangedCd = Time.time + RangedAttackCooldown;

        SimpleProjectile projectile =
            Instantiate(ProjectilePrefab, transform.position + transform.forward*1.5f, Quaternion.identity).GetComponent<SimpleProjectile>();
        if (Random.Range(0f, 1f) <= ProjectileTrackingChance)
        {
            //simple projectile
            projectile.SetPropertiesSimple(gameObject, currentTarget.transform.position - transform.position, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, currentTarget);
        }
        else
        {
            //slowtracking projectile
            projectile.SetPropertiesTracked(gameObject, transform.position + transform.forward*0.5f, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, true, ProjectileTurnSpeed, currentTarget.transform);
        }






    }

    private void MeleeAttack()
    {
        //cooldown check
        if (nextMeleeCd > Time.time)
        {
            return;
        }
        nextMeleeCd += Time.time + MeleeAttackCooldown;


        float meleeAttackHeight = 0.25f;
        Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, meleeRange);
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2, meleeRange);
        RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, meleeRange, Vector3.forward);

        foreach (RaycastHit hit in hits)
        {

            //do damage to player
            PlayerController pc = hit.collider.GetComponent<PlayerController>();
            if (pc)
            {   
                pc.TakeDamage(MeleeDamage);
            }


            //TODO do damage to resources


        }


    }

    void OnDrawGizmosSelected()
    {
        float meleeAttackHeight = 0.25f;
        Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, meleeRange);
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2f, meleeRange);

        //Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(p1, meleeRange);
        Gizmos.DrawWireSphere(p2, meleeRange);
        
    }

    private void SetMeleeAgent()
    {
        agent.areaMask = MELEEMASK;
        agent.SetAreaCost(MELEEONLYMASK, AreaDefaultCost);
        agent.SetAreaCost(RANGEDONLYMASK, nonPrioAreaCost);
        
        //could set angular speed/radius or other agent settings here
    }

    private void SetRangedAgent()
    {
        agent.areaMask = RANGEDMASK;
        agent.SetAreaCost(MELEEONLYMASK, nonPrioAreaCost);
        agent.SetAreaCost(RANGEDONLYMASK, AreaDefaultCost);
    }

    private IEnumerator ExecuteIn(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

}
