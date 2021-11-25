using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;


//TODO propper state machine
public enum EnemyState
{
    Spawning,
    Idle,
    Wandering,
    EnteringCombat,
    Combat,
    ExitCombat,
    Dying,
}


[RequireComponent(typeof(SpacialAudioSource))]
public class Enemy : MonoBehaviour, ITakeDamage
{
    public static readonly List<Enemy> All = new List<Enemy>();
    [field: SerializeField]
    public float MaxHP { get; set; }
    [field: SerializeField]
    public float CurrentHP { get; protected set; }
    public float MeleeDamage = 15f;
    public float RangedDamage = 5f;
    public float Armor = 0f;
    public float FrontBlock = 0f;
    public float PlayerDetectRange = 15f;
    public float PlayerLoseRange = 25f;
    public float TimeUntilOutOfCombat = 10f;
    public float SpawnReturnDistance = 5f;
    public float SpawnTime = 2f;
    public float DeathTime = 2f;
    public float MeleeRange = 3f;
    public float AttackRange = 15f;
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

    public ClipCollection<SpacialSound>[] Sounds;
    public SpacialAudioSource SpacialAudio;

    //AI stuff
    public EnemyBehaviourBase Behaviour;
    public float NavMeshPosCorrectionMax = 25f;
    protected Vector3 spawnPoint;
    public const int MELEEMASK = 1 << 0 | 1 << 3;
    public const int RANGEDMASK = 1 << 0 | 1 << 4;
    public const int WALKABLEMASK = 1 << 0;
    public const int MELEEONLYMASK = 1 << 3;
    public const int RANGEDONLYMASK = 1 << 4;

    protected NavMeshAgent agent;
    protected PlayerController player;
    // protected Transform currentTarget = null;

    [SerializeField] protected float currentHP;
    public Slider HealthSlider;
    protected GameObject model; //TODO remove later
    protected Animation modelAnimation; //TODO remove later
    public event Action<ITakeDamage, float> OnTakeDamage = delegate { };
    public event Action<ITakeDamage> OnDeath = delegate { };
    public Team Team { get => Team.Enemy; }



    protected virtual void OnEnable()
    {
        All.Add(this);
        ChangeBehaviour(currentLevel);
        PlayerController.OnRageLevelUp += OnRageLevelUp;
    }

    private void OnRageLevelUp(int oldLevel, int newLevel)
    {
        LevelUp();
        currentLevel = newLevel;
        ChangeBehaviour(currentLevel);
    }

    protected virtual void OnDisable()
    {
        All.Remove(this);
        PlayerController.OnRageLevelUp -= OnRageLevelUp;
    }

    protected virtual void Awake()
    {
        model = transform.GetChild(0).gameObject;
        modelAnimation = model.GetComponent<Animation>();
        currentHP = MaxHP;
        agent = GetComponent<NavMeshAgent>();
        SpacialAudio = GetComponent<SpacialAudioSource>();

        currentLevel = player.RageLevel;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {

        player = GameManager.Instance.Player;

        Vector3 sourcePostion = transform.position;
        if (NavMesh.SamplePosition(sourcePostion, out NavMeshHit closestHit, NavMeshPosCorrectionMax, agent.areaMask))
        {
            transform.position = closestHit.position;
            spawnPoint = transform.position;
            StartCoroutine(Behaviour.CheckDistance());
        }
        else
        {
            Debug.Log("Enemy Bad Spawn");
            Destroy(gameObject);
        }
        StartCoroutine(PlayPeriodicSound());
    }



    // Update is called once per frame
    protected virtual void Update()
    {

        UpdateHealthbar();

        Behaviour.Tick();

    }

    public void ChangeBehaviour(int level)
    {
        if(level == 0)
        {
            Behaviour = new EnemyBehaviourMeleeSwarm(this, model, modelAnimation, agent, spawnPoint);
        }
        else if(level == 1)
        {
            Behaviour = new EnemyBehaviourRangedKiting(this, model, modelAnimation, agent, spawnPoint);
        }
    }

    private void UpdateHealthbar()
    {
        HealthSlider.minValue = 0;
        HealthSlider.maxValue = MaxHP;
        HealthSlider.value = currentHP;
        HealthSlider.gameObject.transform.LookAt(player.playerCamera.transform.position);
        HealthSlider.gameObject.transform.localEulerAngles += new Vector3(0, 180f, 0);
    }

    protected virtual IEnumerator PlayPeriodicSound()
    {
        // AudioManager.Instance.PlayClip(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Enemy, Sounds));
        SpacialAudio.Play(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Enemy, Sounds));
        yield return new WaitForSeconds(Random.Range(2f, 4f));
    }

    public virtual bool TakeDamage(float amount)
    {
        OnTakeDamage?.Invoke(this, amount);

        amount -= Armor;

        //TODO mittigation


        if (amount > 0)
        {
            currentHP -= amount;
        }


        bool killingBlow = CheckDeathCondition();
        if (killingBlow)
        {
            OnDeath?.Invoke(this);
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


    public virtual void Stun(float duration)
    {
        Behaviour.Stun(duration);
    }





    // protected virtual void EnemyActions(bool isSlowUpdate = false)
    // {

    //     switch (currentState)
    //     {
    //         case EnemyState.Spawning:
    //             {
    //                 //play animation


    //                 //state changed in start method
    //                 break;
    //             }

    //         case EnemyState.Idle:
    //             {
    //                 modelAnimation.Stop();
    //                 if (Vector3.Distance(player.gameObject.transform.position, transform.position) < PlayerDetectRange)
    //                 {
    //                     currentState = EnemyState.EnteringCombat;
    //                     break;
    //                 }

    //                 Vector3 randompos = Random.insideUnitSphere * wanderDistance;
    //                 randompos += spawnPoint;
    //                 if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, wanderDistance, agent.areaMask))
    //                 {
    //                     if (agent.isOnNavMesh)
    //                     {
    //                         agent.SetDestination(hit.position);
    //                     }
    //                     currentState = EnemyState.Wandering;
    //                 }

    //                 break;
    //             }

    //         case EnemyState.Wandering:
    //             {
    //                 modelAnimation.Play("Walk");
    //                 if (Mathf.Abs(Vector3.Distance(player.gameObject.transform.position, transform.position)) < PlayerDetectRange)
    //                 {
    //                     currentState = EnemyState.EnteringCombat;
    //                     break;
    //                 }

    //                 if (!agent.hasPath)
    //                 {
    //                     currentState = EnemyState.Idle;
    //                 }

    //                 break;
    //             }

    //         case EnemyState.EnteringCombat:
    //             {
    //                 outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
    //                 currentTarget = player.transform;
    //                 //currentState = (Random.Range(0, 1) > 0)
    //                 //    ? EnemyState.MeleeAttacking
    //                 //    : EnemyState.RangedAttacking;

    //                 if (currentLevel < 2)
    //                 {
    //                     currentState = EnemyState.MeleeAttacking;
    //                 }
    //                 else
    //                 {
    //                     if (Random.Range(0, currentLevel) < 2)
    //                     {
    //                         currentState = EnemyState.MeleeAttacking;
    //                     }
    //                     else
    //                     {
    //                         currentState = EnemyState.RangedAttacking;
    //                     }
    //                 }

    //                 if (currentState == EnemyState.MeleeAttacking)
    //                 {
    //                     SetMeleeAgent();
    //                 }
    //                 else
    //                 {
    //                     SetRangedAgent();
    //                 }

    //                 break;
    //             }

    //         case EnemyState.RangedAttacking:
    //             {
    //                 float distToPlayer = Mathf.Abs(Vector3.Distance(player.transform.position, transform.position));
    //                 if (distToPlayer > PlayerLoseRange && outOfCombatTimer <= Time.time)
    //                 {
    //                     currentState = EnemyState.ReturnToSpawn;
    //                     break;
    //                 }
    //                 else if (distToPlayer < MeleeRange)
    //                 {
    //                     outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
    //                     MeleeAttack();
    //                     break;
    //                 }
    //                 else if (distToPlayer < AttackRange)
    //                 {
    //                     if (outOfCombatTimer < (Time.time + TimeUntilOutOfCombat / 2f))
    //                     {
    //                         outOfCombatTimer = Time.time + TimeUntilOutOfCombat / 2f;
    //                     }
    //                     RangedAttack();
    //                 }

    //                 //movement
    //                 if (isSlowUpdate)
    //                 {
    //                     CalcRangedPos();
    //                 }

    //                 break;
    //             }

    //         case EnemyState.MeleeAttacking:
    //             {
    //                 float distToPlayer = Mathf.Abs(Vector3.Distance(player.transform.position, transform.position));
    //                 if (distToPlayer > PlayerLoseRange)
    //                 {
    //                     currentState = EnemyState.ReturnToSpawn;
    //                 }
    //                 else if (distToPlayer < MeleeRange)
    //                 {
    //                     outOfCombatTimer = Time.time + TimeUntilOutOfCombat;
    //                     MeleeAttack();
    //                     agent.SetDestination(player.transform.position);
    //                 }
    //                 else if (distToPlayer < AttackRange)
    //                 {
    //                     if (outOfCombatTimer < (Time.time + TimeUntilOutOfCombat / 2f))
    //                     {
    //                         outOfCombatTimer = Time.time + TimeUntilOutOfCombat / 2f;
    //                     }
    //                     RangedAttack();
    //                 }

    //                 if (isSlowUpdate && agent.isOnNavMesh)
    //                 {
    //                     agent.SetDestination(player.transform.position);
    //                 }


    //                 break;
    //             }

    //         case EnemyState.ReturnToSpawn:
    //             {
    //                 if (Mathf.Abs(Vector3.Distance(spawnPoint, transform.position)) < SpawnReturnDistance)
    //                 {
    //                     currentState = EnemyState.Idle;
    //                 }

    //                 if (isSlowUpdate && agent.isOnNavMesh)
    //                 {
    //                     agent.SetDestination(spawnPoint);
    //                 }

    //                 break;
    //             }

    //         case EnemyState.Dying:
    //             {

    //                 //play death animation



    //                 model.transform.eulerAngles += new Vector3((90f / DeathTime) * Time.deltaTime, 0, 0);
    //                 model.transform.position += new Vector3(0, (-2f / DeathTime) * Time.deltaTime, 0);

    //                 if (stunnedTimer <= 0)
    //                 {
    //                     currentState = EnemyState.Dead;
    //                 }


    //                 break;
    //             }

    //         case EnemyState.Dead:
    //             {


    //                 Destroy(gameObject);
    //                 break;
    //             }

    //     }
    // }

    protected virtual void CalcRangedPos()
    {
        // float dist = Mathf.Abs(Vector3.Distance(transform.position, player.transform.position));
        // Vector3 pos = Vector3.MoveTowards(transform.position, player.transform.position, dist - AttackRange);

        // //randomize pos slightly
        // Vector3 randompos = Random.insideUnitSphere * aiCirclingMargin;
        // randompos += pos;

        // if (NavMesh.SamplePosition(randompos, out NavMeshHit hit, wanderDistance, agent.areaMask))
        // {
        //     if (agent.isOnNavMesh)
        //     {
        //         agent.SetDestination(hit.position);
        //     }
        // }
    }

    // protected virtual void RangedAttack()
    // {


    //     //cooldown check
    //     if (nextRangedCd > Time.time)
    //     {
    //         return;
    //     }
    //     StartCoroutine(PlayAnimation(1f, "Shoot", "Walk"));

    //     nextRangedCd = Time.time + RangedAttackCooldown;

    //     SimpleProjectile projectile =
    //         Instantiate(ProjectilePrefab, transform.position + Vector3.up + transform.forward * 1.5f, Quaternion.identity).GetComponent<SimpleProjectile>();
    //     if (Random.Range(0f, 1f) <= ProjectileTrackingChance)
    //     {
    //         //simple projectile
    //         projectile.SetPropertiesSimple(gameObject, Vector3.up + currentTarget.transform.position - transform.position, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, currentTarget, Team);
    //     }
    //     else
    //     {
    //         //slowtracking projectile
    //         projectile.SetPropertiesTracked(gameObject, Vector3.up + transform.position + transform.forward * 0.5f, ProjectileSpeed, RangedDamage, ProjectileHP, ProjectileLifetime, true, ProjectileTurnSpeed, currentTarget.transform, false, Team);
    //     }
    // }

    // protected virtual void MeleeAttack()
    // {
    //     //cooldown check
    //     if (nextMeleeCd > Time.time)
    //     {
    //         return;
    //     }
    //     StartCoroutine(PlayAnimation(2f, "Melee", "Walk"));
    //     nextMeleeCd += Time.time + MeleeAttackCooldown;


    //     float meleeAttackHeight = 0.25f;
    //     Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, MeleeRange);
    //     Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2, MeleeRange);
    //     RaycastHit[] hits = Physics.CapsuleCastAll(p1, p2, MeleeRange, Vector3.forward);

    //     foreach (RaycastHit hit in hits)
    //     {

    //         //do damage to player
    //         PlayerController pc = hit.collider.GetComponent<PlayerController>();
    //         if (pc)
    //         {
    //             pc.TakeDamage(MeleeDamage);
    //         }


    //         //TODO do damage to resources


    //     }


    // }



    protected virtual void OnDrawGizmosSelected()
    {
        float meleeAttackHeight = 0.25f;
        Vector3 p1 = transform.position + new Vector3(0, -meleeAttackHeight / 2f, MeleeRange);
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2f, MeleeRange);

        //Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(p1, MeleeRange);
        Gizmos.DrawWireSphere(p2, MeleeRange);

    }

    protected virtual bool CheckDeathCondition()
    {
        bool dead = false;

        //initialize death if not already dying or dead
        if (currentHP <= 0)
        {
            dead = true;
            InitializeDeath();
        }

        return dead;
    }

    protected virtual void InitializeDeath()
    {
        Behaviour.UpdateState(EnemyState.Dying);
    }


    protected virtual bool IsDead()
    {
        if (currentHP <= 0)
        {
            return true;
        }

        return false;
    }

}
