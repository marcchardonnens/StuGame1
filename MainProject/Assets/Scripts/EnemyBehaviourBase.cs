using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public abstract class EnemyBehaviourBase
{
    protected Enemy Enemy;
    protected EnemyState state = EnemyState.Spawning;
    protected GameObject model;
    protected Animation modelAnimation;
    protected EnemyState currentState;
    protected Vector3 spawnPoint;
    protected NavMeshAgent agent;

    public void Tick()
    {
        EvaluateActions();
    }

    protected void EvaluateActions()
    {
        switch (state)
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
            case EnemyState.ReturnToSpawn:
                ReturnToSpawn();
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
        throw new NotImplementedException();
    }

    private void Dying()
    {
        throw new NotImplementedException();
    }

    private void ReturnToSpawn()
    {
        throw new NotImplementedException();
    }

    protected virtual void ExitCombat()
    {
        throw new NotImplementedException();
    }

    protected virtual void Combat()
    {
        throw new NotImplementedException();
    }

    protected virtual void EnteringCombat()
    {
        throw new NotImplementedException();
    }

    private void Wandering()
    {
        modelAnimation.Play("Walk");
        CheckPlayerNearby();

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
        CheckPlayerNearby();

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

    protected void Spawning()
    {
        throw new NotImplementedException();
    }

    protected bool CheckPlayerNearby()
    {
        if (Vector3.Distance(GameManager.Instance.Player.gameObject.transform.position, Enemy.transform.position) < Enemy.PlayerDetectRange)
        {
            currentState = EnemyState.EnteringCombat;
            return true;
        }
        return false;
    }
}
