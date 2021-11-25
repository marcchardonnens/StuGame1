using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviourRangedKiting : EnemyBehaviourBase
{
    public EnemyBehaviourRangedKiting(Enemy enemy, GameObject model, Animation modelAnimation, NavMeshAgent agent, Vector3 spawnPoint) : base(enemy, model, modelAnimation, agent, spawnPoint)
    {
    }

    public override int DifficultyLevel => 0;
    protected override void CombatActions()
    {
        throw new System.NotImplementedException();
    }
}
