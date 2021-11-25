using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using static Util;



public abstract class EnemyBehaviourBase
{
    public abstract int DifficultyLevel { get; }
    public bool IsInCombat { get => outOfCombatTime > Time.time; }
    public bool IsStunned { get => stunnedTime > Time.time; }

    protected bool MeleeCDReady { get => nextMeleeCd <= Time.time; }
    protected bool RangedCDReady { get => nextRangedCd <= Time.time; }

    private const string AnimationWalk = "Walk";
    private const string AnimationShoot = "Shoot";
    private const string AnimationMelee = "Melee";
    private const string AnimationDeath = "Death";
    public float aiCirclingMargin = 3f; //choosing random pos within this range from target

    protected Enemy Enemy;
    protected GameObject model;
    protected Animation modelAnimation;
    protected NavMeshAgent agent;
    protected Vector3 spawnPoint;




    protected EnemyState currentState = EnemyState.Spawning;
    protected Transform currentTarget;
    protected float outOfCombatTime = 0f;
    protected float stunnedTime = 0f;
    protected float nextMeleeCd = 0f;
    protected float nextRangedCd = 0f;


    protected EnemyBehaviourBase(Enemy enemy, GameObject model, Animation modelAnimation, NavMeshAgent agent, Vector3 spawnPoint)
    {
        Enemy = enemy;
        this.model = model;
        this.modelAnimation = modelAnimation;
        this.agent = agent;
        this.spawnPoint = spawnPoint;
    }
    public void Tick()
    {
        EvaluateActions();
    }

    public void UpdateState(EnemyState newState)
    {
        currentState = newState;
    }

    private void EvaluateActions()
    {
        if(AnimationLock())
        {
            return;
        }
        StunnedActions();
        switch (currentState)
        {
            case EnemyState.Spawning:
                break;
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Wandering:
                Wandering();
                break;
            case EnemyState.EnteringCombat:
                EnteringCombat();
                break;
            case EnemyState.Combat:
                CombatActions();
                break;
            case EnemyState.ExitCombat:
                ExitCombat();
                break;
            case EnemyState.Dying:
                Dying();
                break;
        }
    }

    private bool AnimationLock()
    {
        return modelAnimation.IsPlaying(AnimationShoot) || modelAnimation.IsPlaying(AnimationMelee);
    }

    private bool StunnedActions()
    {
        if (stunnedTime > Time.time)
        {


            agent.isStopped = true;
            return true; //still stunned
        }

        agent.isStopped = false;
        return false; //not stunned
    }

    private void Dying()
    {
        modelAnimation.Play(AnimationDeath);
        model.transform.eulerAngles += new Vector3((90f / Enemy.DeathTime) * Time.deltaTime, 0, 0);
        model.transform.position += new Vector3(0, (-2f / Enemy.DeathTime) * Time.deltaTime, 0);

        if (stunnedTime <= Time.time)
        {
            UnityEngine.Object.Destroy(Enemy.gameObject);
        }
    }

    protected virtual void ExitCombat()
    {
        currentTarget = null;
        //return to spawnpoint
        if (Mathf.Abs(Vector3.Distance(spawnPoint, Enemy.transform.position)) < Enemy.SpawnReturnDistance)
        {
            currentState = EnemyState.Idle;
        }
        agent.SetDestination(spawnPoint);
    }

    protected abstract void CombatActions();

    protected virtual void EnteringCombat()
    {
        outOfCombatTime = Time.time + Enemy.TimeUntilOutOfCombat;
        currentTarget = GameManager.Instance.Player.transform;
        currentState = EnemyState.Combat;
    }

    private void Wandering()
    {
        modelAnimation.Play(AnimationWalk);
        if(CheckPlayerNearby(Enemy.PlayerDetectRange))
        {
            currentState = EnemyState.EnteringCombat;
            return;
        }

        if (!agent.hasPath)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void Idle()
    {
        modelAnimation.Stop();
        if(CheckPlayerNearby(Enemy.PlayerDetectRange))
        {
            currentState = EnemyState.EnteringCombat;
            return;
        }

        Vector3 randompos = Random.insideUnitSphere * Enemy.wanderDistance;
        randompos += spawnPoint;
        if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, Enemy.wanderDistance/2f, agent.areaMask))
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
            currentState = EnemyState.Wandering;
        }
    }



    protected bool CheckPlayerNearby(float range)
    {
        if (Vector3.Distance(GameManager.Instance.Player.gameObject.transform.position, Enemy.transform.position) < range)
        {
            return true;
        }
        return false;
    }

    internal void Stun(float duration)
    {
        if (stunnedTime < duration + Time.time)
        {
            stunnedTime = duration + Time.time;
        }
        // UpdateState(EnemyState.Stunned); //TODO fix
    }
    internal virtual IEnumerator CheckDistance()
    {
        while (true)
        {
            float distance = Vector3.Distance(Enemy.transform.position, GameManager.Instance.Player.transform.position);
            if (distance > 300f)
            {
                UnityEngine.Object.Destroy(Enemy.gameObject);
            }

            yield return new WaitForSeconds(5f);
        }
    }

    protected void MeleeAttack()
    {
        if (nextMeleeCd > Time.time)
        {
            return;
        }
        Enemy.StartCoroutine(PlayAnimation(2f, AnimationMelee, AnimationWalk));
        nextMeleeCd += Time.time + Enemy.MeleeAttackCooldown;

        float meleeAttackHeight = 0.25f;
        Vector3 p1 = Enemy.transform.position + new Vector3(0, -meleeAttackHeight / 2f, Enemy.MeleeRange);
        Vector3 p2 = Enemy.transform.position + new Vector3(0, meleeAttackHeight / 2, Enemy.MeleeRange);
        RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, Enemy.MeleeRange, Vector3.forward);

        foreach (RaycastHit hit in hits)
        {
            //do damage to player
            ITakeDamage damageable = hit.collider.GetComponent<ITakeDamage>();
            if (damageable != null)
            {
                damageable.TakeDamage(Enemy.MeleeDamage);
            }
        }
    }

    protected void RangedAttack()
    {
        //cooldown check
        if (nextRangedCd > Time.time)
        {
            return;
        }
        Enemy.StartCoroutine(PlayAnimation(1f, AnimationShoot, AnimationWalk));

        nextRangedCd = Time.time + Enemy.RangedAttackCooldown;

        SimpleProjectile projectile =
            Enemy.Instantiate(Enemy.ProjectilePrefab, Enemy.transform.position + Vector3.up + Enemy.transform.forward * 1.5f, Quaternion.identity).GetComponent<SimpleProjectile>();
        if (Random.Range(0f, 1f) <= Enemy.ProjectileTrackingChance)
        {
            //simple projectile
            projectile.SetPropertiesSimple(Enemy.gameObject,
                                           Vector3.up + currentTarget.transform.position - Enemy.transform.position,
                                           Enemy.ProjectileSpeed,
                                           Enemy.RangedDamage,
                                           Enemy.ProjectileHP,
                                           Enemy.ProjectileLifetime,
                                           currentTarget,
                                           Enemy.Team);
        }
        else
        {
            //slowtracking projectile
            projectile.SetPropertiesTracked(Enemy.gameObject,
                                            Vector3.up + Enemy.transform.position + Enemy.transform.forward * 0.5f,
                                            Enemy.ProjectileSpeed,
                                            Enemy.RangedDamage,
                                            Enemy.ProjectileHP,
                                            Enemy.ProjectileLifetime,
                                            true,
                                            Enemy.ProjectileTurnSpeed,
                                            currentTarget.transform,
                                            false,
                                            Enemy.Team);
        }
    }


    public virtual IEnumerator PlayAnimation(float duration, string anmin, string queued)
    {
        modelAnimation.wrapMode = WrapMode.Once;
        modelAnimation.Play(anmin);
        agent.isStopped = true;

        while(modelAnimation.isPlaying)
        {
            yield return null;
        }

        // yield return new WaitForSeconds(duration);
        modelAnimation.Play(queued);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

    }

}
