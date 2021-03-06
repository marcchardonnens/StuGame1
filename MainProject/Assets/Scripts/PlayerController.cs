using System.Diagnostics;
using System.Threading;
using System;
using UnityEngine;
using Unity.AI.Navigation;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour, ITakeDamage
{
    public static event Action<PlayerController> OnPlayerCreated = delegate { };
    public static event Action<PlayerController> OnPlayerDestroyed = delegate { };
    public static event Action<int> OnPowerupComsume = delegate { };
    public static event Action<float, float> OnHealthChanged = delegate { };
    public static event Action<int, int> OnSeedCountChanged = delegate { };
    public static event Action<float, float> OnRageAmountChanged = delegate { };
    public static event Action<int, int> OnRageLevelUp = delegate { };
    public static event Action OnResourcesChanged = delegate { }; //maybe move that to hubmanager or elsewhere

    public event Action<ITakeDamage, float> OnTakeDamage = delegate { };
    public event Action<ITakeDamage, float> OnGetHealed = delegate { };
    public event Action<ITakeDamage> OnDeath = delegate { };

    [field: SerializeField]
    public Team Team { get; } = Team.Player;
    [field: SerializeField]
    public float MaxHP { get; set; }
    public float CurrentHP
    {
        get => currentHP; set
        {
            currentHP = value;
            OnHealthChanged?.Invoke(currentHP, MaxHP);
        }
    }



    public Hand RightHand;
    //public Hand LeftHand; 

    public ClipCollection<Sound>[] PlayerSounds;

    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float blockingSpeed = 3.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float FallDamageMultiplier = 10f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float InteractionRange = 5f;

    public float Rage = 0f;
    public int RageLevel = 0;
    public int RageMaxLevel = 10;
    public float RageLevelThreshholdCurrent = 100f;
    public float RageLevelThreshholdIncreasePower = 1.5f;
    public float MeleeHitRageAmount = 5f;
    public float BlockRageAmount = 1f;
    public float KillRageAmount = 15f;
    public float RageDissipationTime = 15f;
    public float RageDissipationRatePerSecond = 0.5f;
    public float RageIntoHPConversion = 1f;
    public float RageHealingMissingHPMultiplierMax = 2.5f;

    public float HPLevel = 10f;
    public float BaseDamageLevel = 2f;
    public float AttackSpeedLevel = 10f;
    public float BaseBlockLevel = 1f;
    public float ArmorLevel = 1f;
    public float WalkingSpeedLevel = 0.75f;
    public float RunningSpeedLevel = 1f;
    public float BlockingSpeedLevel = 0.5f;
    public float LevelToThePowerOf = 1.1f; //level up stats will be calced to whatever + upgrade * leveltothepowerof ** oldLevel

    public int MaxSeeds = 10;
    public int SeedGrenadeCost = 1;
    public Transform SeedGrenadeRelease;
    public float GrenadePreviewDistance = 15f;
    public float Radius = 0.8f;
    public GameObject SeedGrenadePrefab;
    public int ShieldPlantCost = 1;
    public GameObject ShieldPlantPrefab;
    public int TurretPlantCost = 1;
    public GameObject TurretPlantPrefab;
    public int SeedPlantCost = 1;
    public GameObject SeedPlantPrefab;


    public float PlantAnimationTime = 1f;
    public float PlantPlaceMaxDistance = 5f;
    public GameObject PreviewSphere;
    public LineRenderer PreviewLine;
    public Material PreviewGood;
    public Material PreviewBad;
    private MeshRenderer PreviewRenderer;

    //BaseStats, will change throughout the game
    // public float MaxHP = 100;
    public float BaseDamage = 5f;
    public float AttackSpeed = 0f;
    public float BaseBlockAmount = 0f;
    public float Armor = 0;


    public NavMeshSurface Surface;
    private CharacterController characterController; //unitys "improved rigidbody" for characters
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private int CurrentSeeds
    {
        get => currentSeeds; set
        {
            currentSeeds = value;
            OnSeedCountChanged?.Invoke(currentSeeds, MaxSeeds);
        }
    }

    [SerializeField] private float currentShield = 0f;
    // private StageManager stageManager;
    //private List<Enemy> chasingEnemies = new List<Enemy>(); // not sure i need this, but i might later



    private PlayerUIController playerUI = PlayerUIController.Instance;

    private int shroomCounter = 0;


    //TODO display swing timer

    private bool isBlocking = false;
    private float nextMeleeCD = 0f;
    private float rageTimer = 0f;


    private bool previewing = false;
    private int previewNumber = 0;
    private bool previewValid = false;

    private float plantAnimationTimer = 0f;
    private float MonsterXpMult = 0f;


    private bool playerWasGrounded = false;


    private IInteractable currentInteractable = null;
    private float nextFootStepTimer;
    private int currentSeeds;
    private float currentHP;

    void Awake()
    {
        // stageManager = FindObjectOfType<StageManager>();
        characterController = GetComponent<CharacterController>();
        PreviewSphere = Instantiate(PreviewSphere);
        PreviewRenderer = PreviewSphere.GetComponent<MeshRenderer>();
        PreviewLine = PreviewSphere.GetComponent<LineRenderer>();
        PreviewSphere.SetActive(false);
        OnPlayerCreated?.Invoke(this);

        HarvestableSeed.OnSeedHarvest += RefillSeeds;

    }

    void Start()
    {
        CurrentHP = MaxHP;
        CurrentSeeds = MaxSeeds;
        OnRageAmountChanged?.Invoke(Rage, RageLevelThreshholdCurrent);
        OnPowerupComsume?.Invoke(shroomCounter);
        OnSeedCountChanged?.Invoke(CurrentSeeds, MaxSeeds);
        OnResourcesChanged?.Invoke();
    }

    void OnDestroy()
    {
        OnPlayerDestroyed?.Invoke(this);
    }


    void Update()
    {
        if (!GameManager.Instance.GameUnpaused)
        {
            return;
        }



        ScanInteractable(InteractionRange);
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithObject();
        }



        //ragedissipation
        RageDisspation();




        if (Input.GetKeyDown("1"))
        {
            previewNumber = 1;
            previewing = true;
        }
        else if (Input.GetKeyDown("2"))
        {
            previewNumber = 2;
            previewing = true;
        }
        else if (Input.GetKeyDown("3"))
        {
            previewNumber = 3;
            previewing = true;
        }
        else if (Input.GetKeyDown("4"))
        {
            previewNumber = 4;
            previewing = true;
        }


        if (!previewing)
        {
            playerUI.UpdateSeedSelectionText("");
            if (plantAnimationTimer <= Time.time)
            {
                if (Input.GetMouseButton(0))
                {
                    if (RightHand.weapon.HasRangedAttack)
                    {
                        RangedAttack();
                    }
                    else
                    {
                        MeleeAttack();
                    }
                }
                if (Input.GetMouseButton(1) && !isBlocking)
                {
                    Block();
                }
                else if (!Input.GetMouseButton(1) && isBlocking)
                {
                    Unblock();
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                previewing = false;
                RightHand.gameObject.SetActive(true);
                PreviewSphere.SetActive(false);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                if (previewValid)
                {
                    //confirm Action
                    if (previewNumber == 1)
                    {
                        ThrowGrenade();
                    }
                    else if (previewNumber == 2)
                    {
                        PlaceShieldPlant();
                    }
                    else if (previewNumber == 3)
                    {
                        PlaceTurretPlant();
                    }
                    else if (previewNumber == 4)
                    {
                        PlaceSeedPlant();
                    }
                    previewing = false; //might be better to not go out of preview
                    plantAnimationTimer = Time.time + PlantAnimationTime;
                    RightHand.gameObject.SetActive(true);
                    PreviewSphere.SetActive(false);
                }
            }
            else
            {
                if (previewNumber == 1)
                {
                    previewValid = PreviewGrenades();
                    playerUI.UpdateSeedSelectionText("Seed Grenade");
                    playerUI.SetInteractText("Cost - " + SeedGrenadeCost + " Seed" + Util.Plural(SeedGrenadeCost) + "\nLeft Mouse - Throw\nRight Mouse - Cancel");
                }
                else if (previewNumber == 2)
                {
                    previewValid = PreviewShieldPlant();
                    playerUI.UpdateSeedSelectionText("Shield Plant");
                    playerUI.SetInteractText("Cost - " + SeedGrenadeCost + " Seed" + Util.Plural(ShieldPlantCost) + "\nLeft Mouse - Plant on Ground\nRight Mouse - Cancel");
                }
                else if (previewNumber == 3)
                {
                    previewValid = PreviewTurretPlant();
                    playerUI.UpdateSeedSelectionText("Turret Plant");
                    playerUI.SetInteractText("Cost - " + SeedGrenadeCost + " Seed" + Util.Plural(TurretPlantCost) + "\nLeft Mouse - Plant on Ground\nRight Mouse - Cancel");
                }
                else if (previewNumber == 4)
                {
                    previewValid = PreviewSeedPlant();
                    playerUI.UpdateSeedSelectionText("Seed Plant");
                    playerUI.SetInteractText("Cost - " + SeedGrenadeCost + " Seed" + Util.Plural(SeedPlantCost) + "\nLeft Mouse - Plant on Ground\nRight Mouse - Cancel");
                }
            }
        }








        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX;
        if (GameManager.Instance.GameUnpaused && !playerUI.PauseMenuOpen)
            if (isBlocking)
            {
                curSpeedX = blockingSpeed;
            }
            else if (isRunning)
            {
                curSpeedX = runningSpeed;
            }
            else
            {
                curSpeedX = walkingSpeed;
            }
        else
        {
            curSpeedX = 0;
        }
        curSpeedX *= Input.GetAxis("Vertical");


        float curSpeedY;
        if (GameManager.Instance.GameUnpaused && !playerUI.PauseMenuOpen)
            if (isBlocking)
            {
                curSpeedY = blockingSpeed;
            }
            else if (isRunning)
            {
                curSpeedY = runningSpeed;
            }
            else
            {
                curSpeedY = walkingSpeed;
            }
        else
        {
            curSpeedY = 0;
        }

        curSpeedY *= Input.GetAxis("Horizontal");


        float movementDirectionY = moveDirection.y;


        if (characterController.isGrounded && !playerWasGrounded)
        {
            TakeDamage((-movementDirectionY - jumpSpeed * 2) * FallDamageMultiplier);
        }


        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && GameManager.Instance.GameUnpaused && !playerUI.PauseMenuOpen && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
            AudioManager.Instance.PlayClip(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerJump, PlayerSounds), AudioManager.PLAYERACTIONCHANNEL);
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }





        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        if (!characterController.isGrounded)
        {
            playerWasGrounded = false;
            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            playerWasGrounded = true;
        }

        // Move the controller
        Physics.SyncTransforms();
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (GameManager.Instance.GameUnpaused && !playerUI.PauseMenuOpen)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }


        Vector3 xzMovement = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (characterController.isGrounded && nextFootStepTimer < Time.time && xzMovement.magnitude >= 1f)
        {
            Sound s = new Sound(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerStepSound, PlayerSounds));
            float soundWaitTime = 0.75f - xzMovement.magnitude / 50f;
            if (isRunning || isBlocking)
            {
                s.Volume *= 0.2f;
                s.VolumeRandomRange *= 0.2f;
                soundWaitTime *= 2;
            }
            AudioManager.Instance.PlayClip(s, AudioManager.STEPSOUNDCHANNEL);
            nextFootStepTimer = Time.time + soundWaitTime;
        }
    }



    private void InteractWithObject()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    public void ScanInteractable(float InteractionRange)
    {
        if (CrossHairLookPosition(out Collider collider, InteractionRange, 1 << GameConstants.INTERACTABLELAYER))
        {
            //give prio to interactables
            CheckColliderInteractable(collider);
            return;
        }
        else if (CrossHairLookPosition(out collider, InteractionRange))
        {
            CheckColliderInteractable(collider);
            return;
        }
        playerUI.SetInteractText("");
        currentInteractable = null;
    }

    private void CheckColliderInteractable(Collider collider)
    {
        if (collider != null)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable == null)
                interactable = collider.GetComponentInChildren<IInteractable>();
            if (interactable == null)
                interactable = collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                playerUI.SetInteractText(interactable.Name + "\n" + interactable.UiText());
                return;
            }
        }
        currentInteractable = null;
        playerUI.SetInteractText("");
    }

    public static bool CrossHairLookPositionStatic(out Collider collider, Camera cam, float maxDistance = float.MaxValue, int layermask = ~0)
    {
        //Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * maxDistance, Color.red, 0f, true);
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, maxDistance, layermask))
        {
            collider = hit.collider;
            return true;
        }
        collider = null;
        return false;
    }

    private void ThrowGrenade()
    {
        SeedGrenade grenade = Instantiate(SeedGrenadePrefab, SeedGrenadeRelease.position, SeedGrenadeRelease.rotation).GetComponent<SeedGrenade>();
        grenade.Throw(this, playerCamera.transform.forward, BaseDamage, true);
        CurrentSeeds -= SeedGrenadeCost;
    }

    private bool PreviewGrenades()
    {
        bool valid = true;
        PlacePreviewSphere(GrenadePreviewDistance);


        if (CurrentSeeds < SeedGrenadeCost)
        {
            valid = false;
        }

        PaintPreviewSphere(valid);
        return valid;
    }

    private void PlaceShieldPlant()
    {
        Instantiate(ShieldPlantPrefab, PreviewSphere.transform.position, Quaternion.identity);
        currentShield -= ShieldPlantCost;
    }

    private bool PreviewShieldPlant()
    {
        bool valid = PlacePreviewSphere(5f, 1 << GameConstants.GROUNDLAYER);

        if (Physics.CapsuleCast(PreviewSphere.transform.position,
                PreviewSphere.transform.position + new Vector3(0, 1f, 0), Radius, Vector3.up,
                ~(1 << GameConstants.GROUNDLAYER))
            || CurrentSeeds < ShieldPlantCost)
        {
            valid = false;
        }

        PaintPreviewSphere(valid);

        return valid;
    }

    private void PlaceTurretPlant()
    {
        Instantiate(TurretPlantPrefab, PreviewSphere.transform.position, Quaternion.identity);
        CurrentSeeds -= TurretPlantCost;
    }

    private bool PreviewTurretPlant()
    {
        bool valid = PlacePreviewSphere(5f, 1 << GameConstants.GROUNDLAYER);

        if (Physics.CapsuleCast(PreviewSphere.transform.position, PreviewSphere.transform.position + new Vector3(0, 1f, 0), Radius, Vector3.up, ~(1 << GameConstants.GROUNDLAYER))
            || CurrentSeeds < TurretPlantCost)
        {
            valid = false;
        }

        PaintPreviewSphere(valid);
        return valid;
    }

    private void PlaceSeedPlant()
    {
        Instantiate(SeedPlantPrefab, PreviewSphere.transform.position, Quaternion.identity);
        CurrentSeeds -= SeedPlantCost;
    }

    private bool PreviewSeedPlant()
    {
        bool valid = PlacePreviewSphere(5f, 1 << GameConstants.GROUNDLAYER);

        if (Physics.CapsuleCast(PreviewSphere.transform.position, PreviewSphere.transform.position + new Vector3(0, 1f, 0), Radius, Vector3.up, ~(1 << GameConstants.GROUNDLAYER))
            || CurrentSeeds < SeedPlantCost)
        {
            valid = false;
        }

        PaintPreviewSphere(valid);
        return valid;
    }


    private void PaintPreviewSphere(bool good)
    {
        if (good)
        {
            PreviewRenderer.sharedMaterial = PreviewGood;
            PreviewLine.material = PreviewGood;
        }
        else
        {
            PreviewRenderer.sharedMaterial = PreviewBad;
            PreviewLine.material = PreviewBad;
        }
    }







    //later damage types
    public bool TakeDamage(float amount)
    {
        OnTakeDamage?.Invoke(this, amount);
        if (amount <= 0)
        {
            return false;
        }
        rageTimer = Time.time + RageDissipationTime;
        float postMitigation = amount;

        if (postMitigation >= 0)
        {
            AudioManager.Instance.PlayClip(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerHurt, PlayerSounds), AudioManager.PLAYERACTIONCHANNEL);
            CurrentHP -= postMitigation;
            // OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        }

        if (CurrentHP <= 0)
        {
            OnDeath?.Invoke(this);
            return true;
        }

        return false;
    }

    public void Heal(float amount)
    {
        OnGetHealed?.Invoke(this, amount);

        //no overheal
        CurrentHP += Mathf.Clamp(amount, 0f, MaxHP - CurrentHP);
        // OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void RefillSeeds(int amount)
    {
        if (CurrentSeeds + amount <= MaxSeeds)
        {
            CurrentSeeds += amount;
        }
        else
        {
            CurrentSeeds = MaxSeeds;
        }
    }

    public void MeleeAttack()
    {
        if (nextMeleeCD > Time.time)
        {
            return;
        }
        AudioManager.Instance.PlayClip(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerAttack, PlayerSounds), AudioManager.PLAYERACTIONCHANNEL);
        nextMeleeCD = Time.time + (RightHand.weapon.BaseAttackSpeed / (1 + (AttackSpeed / 100)));
        RightHand.MeleeAttack();
        isBlocking = false;
    }

    public void RangedAttack()
    {

    }

    public void Block()
    {
        AudioManager.Instance.PlayClip(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerBlock, PlayerSounds), AudioManager.PLAYERACTIONCHANNEL);
        isBlocking = true;
        RightHand.Block();
    }

    public void Unblock()
    {
        isBlocking = false;
        RightHand.Unblock();
    }

    public void GenerateRage(float amount)
    {
        //amount mod here

        rageTimer = Time.time + RageDissipationTime;

        if (Rage + amount >= RageLevelThreshholdCurrent && RageLevel < RageMaxLevel)
        {
            //conver full ragebar into hp
            Heal(RageLevelThreshholdCurrent * RageIntoHPConversion);
            RageLevelUp();
        }
        else
        {
            if (Rage + amount > RageLevelThreshholdCurrent)
            {
                Rage = RageLevelThreshholdCurrent;
            }
            else
            {
                Rage += amount;
            }
        }

        OnRageAmountChanged?.Invoke(Rage, RageLevelThreshholdCurrent);
    }

    public void RageLevelUp()
    {
        Rage = 0f;
        float expoGain = Mathf.Pow(LevelToThePowerOf, RageLevel);
        MaxHP += HPLevel * expoGain;
        Heal(CurrentHP += HPLevel * expoGain);
        BaseDamage += BaseDamageLevel * expoGain;
        AttackSpeed += AttackSpeedLevel * expoGain;
        Armor += ArmorLevel * expoGain;
        BaseBlockAmount += BaseBlockLevel * expoGain;
        walkingSpeed += WalkingSpeedLevel;
        runningSpeed += RunningSpeedLevel;
        blockingSpeed += BlockingSpeedLevel;

        int rageLevelOld = RageLevel;
        RageLevel++;

        OnRageLevelUp?.Invoke(rageLevelOld, RageLevel);

        RageLevelThreshholdCurrent =
            RageLevelThreshholdCurrent * Mathf.Pow(RageLevelThreshholdIncreasePower, RageLevel);

        if (RageLevel == RageMaxLevel)
        {
            RageLevelThreshholdCurrent *= 100;
        }


        //character sais something, n stuff
    }

    private void RageDisspation()
    {
        if (rageTimer < Time.time)
        {
            if (Rage > RageDissipationRatePerSecond * Time.deltaTime)
            {
                float rageLost = RageDissipationRatePerSecond * Time.deltaTime;
                Rage -= rageLost;
                OnRageAmountChanged?.Invoke(Rage, RageLevelThreshholdCurrent);

                float misshpPercent = Mathf.Abs(1 - (CurrentHP / MaxHP));

                float missinghpMultiplier = 1 + (RageHealingMissingHPMultiplierMax - 1) * misshpPercent;


                Heal(rageLost * missinghpMultiplier);
            }
            else
            {
                Rage = 0f;
            }
        }
    }

    public void GetMonsterXP(int amount)
    {
        // stageManager.OnPlayerGetMonsterXP(Mathf.RoundToInt((float)amount * (1f + MonsterXpMult)));
    }

    public void GetWood(int amount)
    {
        // stageManager.OnPlayerGetWood(amount);
    }

    public void ConsumeShroom(Powerup powerup)
    {


        //TODO constants in powerup class
        //Shroom UI
        shroomCounter += 1;
        OnPowerupComsume?.Invoke(shroomCounter);

        AudioManager.Instance.PlayClip(ClipCollection<Sound>.ChooseClipFromType(SoundType.PlayerConsumeShroom, PlayerSounds), AudioManager.EFFECTCHANNEL);

        switch (powerup.Type)
        {
            case PowerupType.BlueShroom:
                {
                    walkingSpeed += 1f;
                    blockingSpeed += 0.5f;
                    runningSpeed += 1.5f;
                    break;
                }

            case PowerupType.GoldShroom:
                {
                    MonsterXpMult += 0.1f;
                    break;
                }

            case PowerupType.GreenShroom:
                {
                    RageIntoHPConversion += 0.25f;
                    break;
                }

            case PowerupType.IronShroom:
                {
                    Armor += 1;
                    break;
                }

            case PowerupType.RedShroom:
                {
                    AttackSpeed += 15;
                    break;
                }

            case PowerupType.StoneShroom:
                {
                    RightHand.weapon.StunDuration += 0.25f;
                    break;
                }

            case PowerupType.WoodShroom:
                {
                    BaseBlockAmount += 5f;
                    break;
                }

        }

    }

    public bool CrossHairLookPosition(out Vector3 pos, float maxDistance = float.MaxValue, int layermask = ~0, bool hitOnly = false)
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance, layermask))
        {
            pos = hit.point;
            return true;
        }
        else if (!hitOnly)
        {
            pos = playerCamera.transform.position + playerCamera.transform.forward * maxDistance;
            return false;
        }
        pos = Vector3.zero;
        return false;
    }

    public bool CrossHairLookPosition(out Collider collider, float maxDistance = float.MaxValue, int layermask = ~0)
    {
        //Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * maxDistance, Color.red, 0f, true);
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, maxDistance, layermask))
        {
            collider = hit.collider;
            return true;
        }
        collider = null;
        return false;
    }

    private bool PlacePreviewSphere(float maxDistance = 5f, int layerMask = ~(1 << GameConstants.PLAYERLAYER))
    {
        RightHand.StopAllAnimations();
        RightHand.gameObject.SetActive(false);
        PreviewSphere.SetActive(true);

        bool onGround = CrossHairLookPosition(out Vector3 lookPos, maxDistance, layerMask);
        PreviewSphere.transform.position = lookPos;

        PreviewLine.SetPosition(0, RightHand.transform.position);
        PreviewLine.SetPosition(1, PreviewSphere.transform.position);

        return onGround;
    }

}
