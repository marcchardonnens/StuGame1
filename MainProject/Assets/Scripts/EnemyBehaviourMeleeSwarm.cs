using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviourMeleeSwarm : EnemyBehaviourBase
{

    public override int DifficultyLevel { get => 0; }

    public EnemyBehaviourMeleeSwarm(Enemy enemy, GameObject model, Animation modelAnimation, NavMeshAgent agent, Vector3 spawnPoint) : base(enemy, model, modelAnimation, agent, spawnPoint)
    {
        
    }

    protected override void CombatActions()
    {
        float distToPlayer = Mathf.Abs(Vector3.Distance(GameManager.Instance.Player.transform.position, Enemy.transform.position));
        if (distToPlayer > Enemy.PlayerLoseRange)
        {
            currentState = EnemyState.ExitCombat;
        }
        else if (distToPlayer < Enemy.MeleeRange)
        {
            outOfCombatTime = Time.time + Enemy.TimeUntilOutOfCombat;
            MeleeAttack();
            agent.SetDestination(GameManager.Instance.Player.transform.position);
        }
        else if (distToPlayer < Enemy.AttackRange)
        {
            if (outOfCombatTime < (Time.time + Enemy.TimeUntilOutOfCombat / 2f))
            {
                outOfCombatTime = Time.time + Enemy.TimeUntilOutOfCombat / 2f;
            }
            RangedAttack();
        }

        if (agent.isOnNavMesh)
        {
            agent.SetDestination(GameManager.Instance.Player.transform.position);
        }
    }

    protected override void ExitCombat()
    {
        base.ExitCombat();
    }

    protected override void EnteringCombat()
    {
        base.EnteringCombat();
    }

    internal override IEnumerator CheckDistance()
    {
        return base.CheckDistance();
    }
}
