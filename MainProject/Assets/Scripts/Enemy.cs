using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum EnemyState
{
    Spawning,
    Idle,
    Stunned,
    Wandering,
    EnteringCombat,
    RangedAttacking,
    MeleeAttacking,
    ReturnToSpawn,
    Dying,
    Dead,
}

public class Enemy : MonoBehaviour, ITakeDamage
{
    public float MaxHP = 1000f;
    public float MeleeDamage = 15f;
    public float RangedDamage = 5f;
    public float Armor = 0f;
    public float FrontBlock = 0f;
    public float PlayerDetectRange = 15f;
    public float PlayerLoseRange = 25f;
    public float TimeUntilOutOfCombat = 10f;
    public float spawnReturnDistance = 5f;
    public float spawnTime = 2f;
    public float deathTime = 2f;
    public float meleeRange = 3f;
    public float attackRange = 15f;
    public float MeleeAttackCooldown = 2f;
    public float RangedAttackCooldown = 4f;
    public float MeleeAnimationTime = 0.5f;
    public float RangedAnimationTime = 0.5f;

    public int currentLevel = 0;
    public int BaseRewardAmount = 500;
    public float PlayerRageLevelRewardMultiplier = 0.50f;
    public float EnemyLevelRewardMultiplier = 1.75f;
    public float RandomRewardMultiplier = 0.2f;

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
    public float NavMeshPosCorrectionMax = 25f;

    protected Vector3 spawnPoint;

    //AI stuff
    public const int MELEEMASK = 1 << 0 | 1 << 3;
    public const int RANGEDMASK = 1 << 0 | 1 << 4;
    public const int WALKABLEMASK = 1 << 0;
    public const int MELEEONLYMASK = 1 << 3;
    public const int RANGEDONLYMASK = 1 << 4;

    protected const float SLOWUPDATETIME = .1f;

    public float AreaDefaultCost = 1f;
    public float nonPrioAreaCost = 10f;

    public float aiCirclingMargin = 3f; //choosing random pos within this range from target
    protected NavMeshAgent agent;
    protected EnemyState currentState;
    protected EnemyState? queuedState = null;
    protected PlayerController player;
    protected Transform currentTarget = null;

    protected float slowupdate = SLOWUPDATETIME;
    protected float nextMeleeCd = 0f;
    protected float nextRangedCd = 0f;
    protected float stunnedTimer = 0f;
    [SerializeField] protected float currentHP;
    public Slider HealthSlider;
    protected float outOfCombatTimer = 0f;

    protected GameObject child;
    protected Animation childAnim;

    public Team Team {get => Team.Enemy;}

    // Start is called before the first frame update
    protected virtual void Start()
    {
        child = transform.GetChild(0).gameObject;
        childAnim = child.GetComponent<Animation>();
        currentHP = MaxHP;
        agent = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerController>();

        Vector3 sourcePostion = transform.position;
        if (NavMesh.SamplePosition(sourcePostion, out NavMeshHit closestHit, NavMeshPosCorrectionMax, agent.areaMask))
        {
            transform.position = closestHit.position;
            spawnPoint = transform.position;
            StartCoroutine(CheckDistance());
        }
        else
        {
            Debug.Log("Enemy Bad Spawn");
            Destroy(gameObject);
        }
        currentState = EnemyState.Spawning;
        //agent.areaMask = WALKABLEMASK;
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        HealthSlider.minValue = 0;
        HealthSlider.maxValue = MaxHP;
        HealthSlider.value = currentHP;
        HealthSlider.gameObject.transform.LookAt(player.playerCamera.transform.position);
        HealthSlider.gameObject.transform.localEulerAngles += new Vector3(0,180f,0);

        slowupdate -= Time.deltaTime;
        stunnedTimer -= Time.deltaTime;

        //if killing blow from something else
        CheckDeathCondition();

        if (slowupdate <= 0)
        {
            EnemyActions(true);
        }
        else
        {
            EnemyActions(false);
        }

    }


    public virtual bool TakeDamage(float amount)
    {
        amount -= Armor;

        //TODO mittigation


        if (amount > 0)
        {
            currentHP -= amount;
        }


        bool killingBlow = CheckDeathCondition();
        if (killingBlow)
        {
            player.GetMonsterXP(RewardAmount());
            player.GenerateRage(player.KillRageAmount);
        }

        return killingBlow;

    }

    public virtual void LevelUp()
    {
        MaxHP *= MaxHP * 1.1f;
        currentHP += MaxHP * 0.1f;
        ProjectileSpeed *= 1.05f;
        wanderSpeed *= 1.1f;
        combatSpeed *= 1.1f;


    }

    public virtual int RewardAmount()
    {
        float reward = BaseRewardAmount + currentLevel * EnemyLevelRewardMultiplier * BaseRewardAmount;

        float ragebonus = reward * player.RageLevel * PlayerRageLevelRewardMultiplier;

        reward += ragebonus;

        float randomBonus = Random.Range(0f, reward * RandomRewardMultiplier);

        reward += randomBonus;

        return Mathf.RoundToInt(reward);
    }


    public virtual void Stun(float duration, EnemyState? nextState = null)
    {
        stunnedTimer += duration;
        queuedState = nextState;
        if (currentState != EnemyState.Dying && currentState != EnemyState.Dead)
        {
            currentState = EnemyState.Stunned;
        }
    }





    protected virtual IEnumerator CheckDistance()
    {

        while (true)
        {

            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance > 300f)
            {
                Destroy(gameObject);
            }

            yield return new WaitForSeconds(5f);
        }


    }

    protected virtual IEnumerator Spawn()
    {
        childAnim.Play("Walk");
        float posy = child.transform.position.y - 1.5f;

        for (int i = 0; i < 100; i++)
        {
            float newy = posy + ((1.5f / 100f) * (float)i);
            child.transform.position = new Vector3(transform.position.x, newy, transform.position.z);
            yield return new WaitForSeconds(1f / 100);
        }
        yield return new WaitForSeconds(1f);
        currentState = EnemyState.Idle;
    }




    protected virtual void EnemyActions(bool isSlowUpdate = false)
    {

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
                    childAnim.Stop();
                    if (Vector3.Distance(player.gameObject.transform.position, transform.position) < PlayerDetectRange)
                    {
                        currentState = EnemyState.EnteringCombat;
                        break;
                    }

                    Vector3 randompos = Random.insideUnitSphere * wanderDistance;
                    randompos += spawnPoint;
                    if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, wanderDistance, agent.areaMask))
                    {
                        if (agent.isOnNavMesh)
                        {
                            agent.SetDestination(hit.position);
                        }
                        currentState = EnemyState.Wandering;
                    }

                    break;
                }

            case EnemyState.Wandering:
                {
                    childAnim.Play("Walk");
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

            case EnemyState.Stunned:
                {
                    childAnim.Stop();
                    //not stunned anymore
                    if (stunnedTimer <= 0)
                    {
                        if (queuedState.HasValue)
                        {
                            currentState = queuedState.Value;
                            queuedState = null;
                        }
                        else
                        {
                            //no state given, figure it out again
                            currentState = EnemyState.Idle;
                        }
                    }
                    break;
                }

            case EnemyState.EnteringCombat:
                {
                    outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
                    currentTarget = player.transform;
                    //currentState = (Random.Range(0, 1) > 0)
                    //    ? EnemyState.MeleeAttacking
                    //    : EnemyState.RangedAttacking;

                    if (currentLevel < 2)
                    {
                        currentState = EnemyState.MeleeAttacking;
                    }
                    else
                    {
                        if (Random.Range(0, currentLevel) < 2)
                        {
                            currentState = EnemyState.MeleeAttacking;
                        }
                        else
                        {
                            currentState = EnemyState.RangedAttacking;
                        }
                    }

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
                    if (distToPlayer > PlayerLoseRange && outOfCombatTimer <= Time.time)
                    {
                        currentState = EnemyState.ReturnToSpawn;
                        break;
                    }
                    else if (distToPlayer < meleeRange)
                    {
                        outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
                        MeleeAttack();
                        break;
                    }
                    else if (distToPlayer < attackRange)
                    {
                        if (outOfCombatTimer < (Time.time + TimeUntilOutOfCombat / 2f))
                        {
                            outOfCombatTimer = Time.time + TimeUntilOutOfCombat / 2f;
                        }
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
                        outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
                        MeleeAttack();
                        agent.SetDestination(player.transform.position);
                    }
                    else if (distToPlayer < attackRange)
                    {
                        if (outOfCombatTimer < (Time.time + TimeUntilOutOfCombat / 2f))
                        {
                            outOfCombatTimer = Time.time + TimeUntilOutOfCombat / 2f;
                        }
                        RangedAttack();
                    }

                    if (isSlowUpdate && agent.isOnNavMesh)
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

                    if (isSlowUpdate && agent.isOnNavMesh)
                    {
                        agent.SetDestination(spawnPoint);
                    }

                    break;
                }

            case EnemyState.Dying:
                {

                    //play death animation



                    child.transform.eulerAngles += new Vector3((90f / deathTime) * Time.deltaTime, 0, 0);
                    child.transform.position += new Vector3(0, (-2f / deathTime) * Time.deltaTime, 0);

                    if (stunnedTimer <= 0)
                    {
                        currentState = EnemyState.Dead;
                    }


                    break;
                }

            case EnemyState.Dead:
                {


                    Destroy(gameObject);
                    break;
                }

        }
    }

    protected virtual void CalcRangedPos()
    {
        float dist = Mathf.Abs(Vector3.Distance(transform.position, player.transform.position));
        Vector3 pos = Vector3.MoveTowards(transform.position, player.transform.position, dist - attackRange);

        //randomize pos slightly
        Vector3 randompos = Random.insideUnitSphere * aiCirclingMargin;
        randompos += pos;

        if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, wanderDistance, agent.areaMask))
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            //become melee
            SetMeleeAgent();
            currentState = EnemyState.MeleeAttacking;
        }
    }

    protected virtual void RangedAttack()
    {


        //cooldown check
        if (nextRangedCd > Time.time)
        {
            return;
        }
        StartCoroutine(PlayAnimation(1f, "Shoot", "Walk"));

        nextRangedCd = Time.time + RangedAttackCooldown;

        SimpleProjectile projectile =
            Instantiate(ProjectilePrefab, transform.position + Vector3.up + transform.forward * 1.5f, Quaternion.identity).GetComponent<SimpleProjectile>();
        if (Random.Range(0f, 1f) <= ProjectileTrackingChance)
        {
            //simple projectile
            projectile.SetPropertiesSimple(gameObject, Vector3.up + currentTarget.transform.position - transform.position, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, currentTarget, Team);
        }
        else
        {
            //slowtracking projectile
            projectile.SetPropertiesTracked(gameObject, Vector3.up + transform.position + transform.forward * 0.5f, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, true, ProjectileTurnSpeed, currentTarget.transform, false, Team);
        }


    }

    protected virtual void MeleeAttack()
    {
        //cooldown check
        if (nextMeleeCd > Time.time)
        {
            return;
        }
        StartCoroutine(PlayAnimation(2f, "Melee", "Walk"));
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

    protected virtual IEnumerator PlayAnimation(float duration, string anmin, string queued)
    {

        childAnim.Play(anmin);
        agent.isStopped = true;

        yield return new WaitForSeconds(duration);
        childAnim.Play(queued);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

    }

    protected virtual void OnDrawGizmosSelected()
    {
        float meleeAttackHeight = 0.25f;
        Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, meleeRange);
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2f, meleeRange);

        //Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(p1, meleeRange);
        Gizmos.DrawWireSphere(p2, meleeRange);

    }

    protected virtual void SetMeleeAgent()
    {
        agent.areaMask = MELEEMASK;
        agent.SetAreaCost(MELEEONLYMASK, AreaDefaultCost);
        agent.SetAreaCost(RANGEDONLYMASK, nonPrioAreaCost);

        //could set angular speed/radius or other agent settings here
    }

    protected virtual void SetRangedAgent()
    {
        agent.areaMask = RANGEDMASK;
        agent.SetAreaCost(MELEEONLYMASK, nonPrioAreaCost);
        agent.SetAreaCost(RANGEDONLYMASK, AreaDefaultCost);
    }


    protected virtual bool CheckDeathCondition()
    {
        bool dead = false;

        //initialize death if not already dying or dead
        if (currentHP <= 0 && currentState != EnemyState.Dead && currentState != EnemyState.Dying)
        {
            dead = true;
            InitializeDeath();
        }

        return dead;
    }

    protected virtual void InitializeDeath()
    {
        childAnim.Play("Death");
        //initiate death
        currentState = EnemyState.Dying;
        stunnedTimer = deathTime;
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }


    protected virtual bool IsDead()
    {
        if (currentState == EnemyState.Dying ||
            currentState == EnemyState.Dead ||
            currentHP <= 0)
        {
            return true;
        }

        return false;
    }




    protected IEnumerator ExecuteIn(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action();
    }

}
