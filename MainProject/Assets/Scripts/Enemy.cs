using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;
// using UnityEditor;


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

[SelectionBase]
[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour, ITakeDamage
{
    public static readonly List<Enemy> All = new List<Enemy>();
    [field: SerializeField]
    public float MaxHP { get; set; }
    [field: SerializeField]
    public float CurrentHP { get; protected set; }
    public float MeleeDamage = 30f;
    public float RangedDamage = 12f;
    public float Armor = 0f;
    public float FrontBlock = 0f;
    public float PlayerDetectRange = 15f;
    public float PlayerLoseRange = 50f;
    public float TimeUntilOutOfCombat = 10f;
    public float SpawnReturnDistance = 5f;
    public float SpawnTime = 2f;
    public float DeathTime = 2f;
    public float MeleeRadius = 0.75f;
    public float MeleeRange { get => 2 * MeleeRadius; }
    public float RangedAttackRangeMin = 10f;
    public float RangedAttackRangeMax = 35f;
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

    public float WanderSpeed = 2f;
    public float WanderDistanceMin = 5f;
    public float WanderDistanceMax = 15f;
    public float CombatSpeed = 4f;
    public float StoppingDistance = 2.5f;
    public float TurnSpeed = 120f;

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

    protected virtual void Awake()
    {
        model = transform.GetChild(0).gameObject;
        // SceneVisibilityManager.instance.DisablePicking(model, true);
        modelAnimation = model.GetComponent<Animation>();
        currentHP = MaxHP;
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = StoppingDistance;
        agent.updateRotation = false;
        SpacialAudio = new SpacialAudioSource(GetComponent<AudioSource>());

        HealthSlider.gameObject.SetActive(false);

    }

    protected virtual void OnEnable()
    {
        player = GameManager.Instance.Player;
        // currentLevel = player.RageLevel; //TODO

        Vector3 sourcePostion = transform.position;
        if (NavMesh.SamplePosition(sourcePostion, out NavMeshHit closestHit, NavMeshPosCorrectionMax, agent.areaMask))
        {
            transform.position = closestHit.position;
            spawnPoint = transform.position;
        }
        else
        {
            Debug.Log("Enemy Bad Spawn");
            Destroy(gameObject);
        }
        ChangeBehaviour(currentLevel);
        StartCoroutine(Behaviour.CheckDistance());

        All.Add(this);
        PlayerController.OnRageLevelUp += OnRageLevelUp;

    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        StartCoroutine(PlayPeriodicSound());
        StartCoroutine(SpawnAnimation());
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            SpacialAudio.Play(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Enemy, Sounds));
        }
        Behaviour.Tick();
        // UpdateHealthbar();

    }
    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        All.Remove(this);
        PlayerController.OnRageLevelUp -= OnRageLevelUp;
    }

    protected IEnumerator SpawnAnimation()
    {
        modelAnimation.Stop();
        float posy = model.transform.position.y - 1.5f;

        for (int i = 0; i < 100; i++)
        {
            float newy = posy + (1.5f / 100f * i);
            model.transform.position = new Vector3(transform.position.x, newy, transform.position.z);
            yield return new WaitForSeconds(SpawnTime / 2f / 100);
        }
        yield return new WaitForSeconds(SpawnTime / 2f);
        Behaviour.UpdateState(EnemyState.Idle);
    }

    public void ChangeBehaviour(int level)
    {
        if (level == 0)
        {
            Behaviour = new EnemyBehaviourMeleeSwarm(this, model, modelAnimation, agent, spawnPoint);
        }
        else if (level == 1)
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
        while(currentHP > 0)
        {
            // AudioManager.Instance.PlayClip(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Enemy, Sounds));
            SpacialAudio.Play(ClipCollection<SpacialSound>.ChooseClipFromType(SoundType.Enemy, Sounds));
            // Debug.Log("enemy making sound");
            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }
    private void OnRageLevelUp(int oldLevel, int newLevel)
    {
        // LevelUp();
        currentLevel = newLevel;
        ChangeBehaviour(currentLevel);
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
        WanderSpeed *= 1.1f;
        CombatSpeed *= 1.1f;


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


    protected virtual void CalcRangedPos()
    {

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




    #region DebugCode
    protected virtual void OnDrawGizmosSelected()
    {
        float meleeAttackHeight = 1.25f;
        Vector3 p1 = transform.position + new Vector3(0, meleeAttackHeight, 0) + transform.forward * MeleeRadius;
        Vector3 p2 = transform.position + new Vector3(0, meleeAttackHeight / 2f, 0) + transform.forward * MeleeRadius;
        Vector3 p3 = transform.position + transform.forward * MeleeRadius;

        //Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(p1, MeleeRadius);
        Gizmos.DrawWireSphere(p2, MeleeRadius);
        Gizmos.DrawWireSphere(p3, MeleeRadius);
    }

    public virtual string Print(bool print = true)
    {
        string s = this.GetType().ToString();
        s += "enemy\n";
        s += Behaviour.Print(false);

        if (print)
        {
            Debug.Log(s);
        }

        return s;
    }


    //on mouse down is for gameview only
    // void OnMouseDown()
    // {
    //     Print();
    // }

    #endregion

}
