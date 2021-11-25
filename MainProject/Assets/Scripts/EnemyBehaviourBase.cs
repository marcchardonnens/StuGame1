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

    public float aiCirclingMargin = 3f; //choosing random pos within this range from target

    protected Enemy Enemy;
    protected GameObject model;
    protected Animation modelAnimation;
    protected NavMeshAgent agent;
    protected Vector3 spawnPoint;




    protected EnemyState currentState = EnemyState.Spawning;
    protected Transform target;
    protected float outOfCombatTimer = 0f;
    protected float nextMeleeCd = 0f;
    protected float nextRangedCd = 0f;
    protected float stunnedTime = 0f;

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
        StunnedActions();
        switch (currentState)
        {
            case EnemyState.Spawning:
                Spawning();
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
        modelAnimation.Play("Death");
        model.transform.eulerAngles += new Vector3((90f / Enemy.DeathTime) * Time.deltaTime, 0, 0);
        model.transform.position += new Vector3(0, (-2f / Enemy.DeathTime) * Time.deltaTime, 0);

        if (stunnedTime <= Time.time)
        {
            UnityEngine.Object.Destroy(Enemy.gameObject);
        }
    }

    protected virtual void ExitCombat()
    {
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
        outOfCombatTimer = Time.time + Enemy.TimeUntilOutOfCombat;
    }

    private void Wandering()
    {
        modelAnimation.Play("Walk");
        CheckPlayerNearby(Enemy.PlayerDetectRange);

        if (!agent.hasPath)
        {
            currentState = EnemyState.Idle;
        }
    }

    private void Idle()
    {
        modelAnimation.Stop();
        CheckPlayerNearby(Enemy.PlayerDetectRange);

        Vector3 randompos = Random.insideUnitSphere * Enemy.wanderDistance;
        randompos += spawnPoint;
        if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, Enemy.wanderDistance, agent.areaMask))
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
            currentState = EnemyState.Wandering;
        }
    }

    protected async void Spawning()
    {
        modelAnimation.Stop();
        float posy = model.transform.position.y - 1.5f;

        for (int i = 0; i < 100; i++)
        {
            float newy = posy + ((1.5f / 100f) * (float)i);
            model.transform.position = new Vector3(Enemy.transform.position.x, newy, Enemy.transform.position.z);
            await Task.Delay(SecondsToMillis(Enemy.SpawnTime / 2f) / 100);
        }
        await Task.Delay(SecondsToMillis(Enemy.SpawnTime / 2f));
        currentState = EnemyState.Idle;
    }

    protected bool CheckPlayerNearby(float range)
    {
        if (Vector3.Distance(GameManager.Instance.Player.gameObject.transform.position, Enemy.transform.position) < range)
        {
            currentState = EnemyState.EnteringCombat;
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
        // UpdateState(EnemyState.Stunned);
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
}
