using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviourMeleeSwarm : EnemyBehaviourBase
{

    public override int DifficultyLevel {get => 1;}

    public EnemyBehaviourMeleeSwarm(Enemy enemy, GameObject model, Animation modelAnimation, NavMeshAgent agent, Vector3 spawnPoint) : base(enemy, model, modelAnimation, agent, spawnPoint)
    {
        
    }
    
    protected override void CombatActions()
    {
        throw new System.NotImplementedException();
    }
}
