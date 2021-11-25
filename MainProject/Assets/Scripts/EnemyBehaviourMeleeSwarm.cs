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
        float distToPlayer = Mathf.Abs(DistanceToPlayer());
        if (distToPlayer > Enemy.PlayerLoseRange)
        {
            outOfCombatTime = Time.time;
            currentState = EnemyState.ExitCombat;
            return;
        }
        else if (distToPlayer < Enemy.MeleeRange)
        {
            MeleeAttack();
        }
        else if (distToPlayer < Enemy.RangedAttackRangeMax && distToPlayer > Enemy.RangedAttackRangeMin)
        {
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
