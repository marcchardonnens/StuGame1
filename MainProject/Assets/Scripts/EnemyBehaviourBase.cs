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
    protected Enemy Enemy;
    protected GameObject model;
    protected Animation modelAnimation;
    protected NavMeshAgent agent;
    protected Vector3 spawnPoint;




    protected EnemyState currentState = EnemyState.Spawning;
    protected float outOfCombatTimer = 0f;
    protected float nextMeleeCd = 0f;
    protected float nextRangedCd = 0f;
    protected float stunnedTimer = 0f;

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

    private void EvaluateActions()
    {
        switch (currentState)
        {
            case EnemyState.Spawning:
                Spawning();
                break;
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Stunned:
                Stunned();
                break;
            case EnemyState.Wandering:
                Wandering();
                break;
            case EnemyState.EnteringCombat:
                EnteringCombat();
                break;
            case EnemyState.Combat:
                Combat();
                break;
            case EnemyState.ExitCombat:
                ExitCombat();
                break;
            case EnemyState.Dying:
                Dying();
                break;
            case EnemyState.Dead:
                Dead();
                break;
        }
    }

    private void Dead()
    {
        UnityEngine.Object.Destroy(Enemy.gameObject);
    }

    private void Dying()
    {
        model.transform.eulerAngles += new Vector3((90f / Enemy.DeathTime) * Time.deltaTime, 0, 0);
        model.transform.position += new Vector3(0, (-2f / Enemy.DeathTime) * Time.deltaTime, 0);

        if (stunnedTimer <= 0)
        {
            currentState = EnemyState.Dead;
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

    protected virtual void Combat()
    {
        throw new NotImplementedException();
    }

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

    private void Stunned()
    {
        throw new NotImplementedException();
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
            await Task.Delay(SecondsToMillis(Enemy.SpawnTime/2f) / 100);
        }
        await Task.Delay(SecondsToMillis(Enemy.SpawnTime/2f));
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
}
