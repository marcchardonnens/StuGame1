using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyBehaviourRangedKiting : EnemyBehaviourBase
{
    public float KitingPositionMargin = 1.5f; //choosing random pos within this range from target
    public EnemyBehaviourRangedKiting(Enemy enemy, GameObject model, Animation modelAnimation, NavMeshAgent agent, Vector3 spawnPoint) : base(enemy, model, modelAnimation, agent, spawnPoint)
    {
    }

    public override int DifficultyLevel => 1;


    protected override void CombatActions()
    {
        {
            float distToPlayer = DistanceToPlayer();
            if (distToPlayer > Enemy.PlayerLoseRange && outOfCombatTime <= Time.time)
            {
                currentState = EnemyState.ExitCombat;
                return;
            }
            else if (distToPlayer < Enemy.MeleeRange)
            {
                outOfCombatTime = Time.time + Enemy.TimeUntilOutOfCombat;
                MeleeAttack();
            }
            else if (distToPlayer < Enemy.RangedAttackRangeMax)
            {
                Enemy.StartCoroutine(RangedAttack());
            }
            CalcRangedPos();
        }
    }

    protected virtual void CalcRangedPos()
    {
        float dist = DistanceToPlayer();
        Vector3 destination = Vector3.MoveTowards(Enemy.transform.position, GameManager.Instance.Player.transform.position, dist - Enemy.RangedAttackRangeMax - KitingPositionMargin / 2f);

        //randomize pos slightly
        Vector3 randompos = Random.insideUnitSphere * KitingPositionMargin;
        randompos += destination;

        if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, KitingPositionMargin, agent.areaMask))
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(hit.position);
            }
        }
    }











    public override string Print(bool print = true)
    {
        return base.Print(print);
    }

}
