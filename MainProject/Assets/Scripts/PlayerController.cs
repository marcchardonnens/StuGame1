using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Hand RightHand;
    //public Hand LeftHand;
    public float walkingSpeed = 7.5f;
    public float runningSpeed = 11.5f;
    public float blockingSpeed = 3.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

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
    public float RageIntoHPConversion = 0.1f;
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

    public int MaxSeeds = 5;
    public int SeedGrenadeCost = 1;
    public Transform SeedGrenadeRelease;
    public GameObject SeedGrenadePrefab;
    public int ShieldPlantCost = 1;
    public GameObject ShieldPlantPrefab;
    public int TurretPlantCost = 1;
    public GameObject TurretPlanPrefab;
    public int SeedPlantCost = 1;
    public GameObject SeedPlantPrefab;

    public float PlantPlaceMaxDistance = 5f;


    //BaseStats, will change throughout the game
    public float MaxHP = 100;
    public float BaseDamage = 5f;
    public float AttackSpeed = 0f;
    public float BaseBlockAmount = 0f;
    public float Armor = 0;


    CharacterController characterController; //unitys "improved rigidbody" for characters
    Vector3 moveDirection = Vector3.zero;
    float rotationX = 0;
    [HideInInspector] public bool canMove = true;
    [SerializeField] private float currentHP;
    [SerializeField] private int currentSeeds = 5;
    [SerializeField] private float currentShield = 0f;
    private StageManager stageManager;
    //private List<Enemy> chasingEnemies = new List<Enemy>(); // not sure i need this, but i might later



    private bool isBlocking = false;
    private float nextMeleeCD = 0f;
    private float rageTimer = 0f;


    private bool previewing = false;
    private int previewNumber = 0;
    //private bool preview1 = false; //grenade
    //private bool preview2 = false; //shield plant
    //private bool preview3 = false; //turret plant
    //private bool preview4 = false; //eating seed steroid



    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        characterController = GetComponent<CharacterController>();


        currentHP = MaxHP;

        LockCursor();
    }

    void Update()
    {

        //ragedissipation
        if (rageTimer < Time.time)
        {
            if (Rage > RageDissipationRatePerSecond * Time.deltaTime)
            {
                float rageLost = RageDissipationRatePerSecond * Time.deltaTime;
                Rage -= rageLost;

                float misshpPercent = Mathf.Abs(1 - (currentHP / MaxHP));

                float missinghpMultiplier = 1 + (RageHealingMissingHPMultiplierMax - 1) * misshpPercent;
                //
                Heal(rageLost * missinghpMultiplier);
            }
            else
            {
                Rage = 0f;
            }
        }

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
        else
        {
            if (Input.GetMouseButtonDown(1))
            {
                previewing = false;
            }
            else if (Input.GetMouseButtonDown(0))
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
            }
            else
            {
                if (previewNumber == 1)
                {
                    PreviewGrenades();
                }
                else if (previewNumber == 2)
                {
                    PreviewShieldPlant();
                }
                else if (previewNumber == 3)
                {
                    PreviewTurretPlant();
                }
                else if (previewNumber == 4)
                {
                    PreviewSeedPlant();
                }
            }
        }



        




        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX;
        if (canMove)
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
        if (canMove)
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
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpSpeed;
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
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

            
        }


    }

    private void ThrowGrenade()
    {
        if (currentSeeds < ShieldPlantCost)
        {
            Debug.Log("not enough seeds!");
            return;
        }

        SeedGrenade grenade = Instantiate(SeedGrenadePrefab, SeedGrenadeRelease.position, SeedGrenadeRelease.rotation).GetComponent<SeedGrenade>();
        grenade.Throw(this, transform.forward, BaseDamage);


    }
    private void PreviewGrenades()
    {

        //do the line thing
        //or maybe dont, its not the most important

    }

    private void PlaceShieldPlant()
    {
        throw new NotImplementedException();
    }

    private void PlaceTurretPlant()
    {
        throw new NotImplementedException();
    }

    private void PlaceSeedPlant()
    {
        throw new NotImplementedException();
    }

    private void PreviewSeedPlant()
    {
        throw new NotImplementedException();
    }

    private void PreviewTurretPlant()
    {
        throw new NotImplementedException();
    }

    private void PreviewShieldPlant()
    {
        throw new NotImplementedException();
    }



    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //later damage types
    public void TakeDamage(float amount)
    {
        rageTimer = Time.time + RageDissipationTime;
        float postMitigation = amount;

        //apply mitigation

        currentHP -= postMitigation;

        if (currentHP <= 0)
        {
            stageManager.EndStage(StageResult.Death);
        }

    }

    public void Heal(float amount)
    {
        //no overheal
        currentHP += Mathf.Clamp(amount, 0f, MaxHP - currentHP);
    }

    public void MeleeAttack()
    {
        if (nextMeleeCD > Time.time)
        {
            return;
        }

        rageTimer = Time.time + RageDissipationTime;   
        nextMeleeCD = Time.time + (RightHand.weapon.BaseAttackSpeed/ (1+(AttackSpeed/100)));
        RightHand.MeleeAttack();
        isBlocking = false;
    }

    public void RangedAttack()
    {

    }

    public void Block()
    {
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
     
        
        if (Rage + amount > RageLevelThreshholdCurrent && RageLevel < RageMaxLevel)
        {
            //conver full ragebar into hp

            Heal(RageLevelThreshholdCurrent * RageIntoHPConversion);
            RageLevelUp();
        }
        else
        {
            Rage += amount;
        }
    }

    public void RageLevelUp()
    {
        Rage = 0f;
        float expoGain = Mathf.Pow(LevelToThePowerOf, RageLevel);
        MaxHP += HPLevel * expoGain;
        Heal(currentHP += HPLevel * expoGain);
        BaseDamage += BaseDamageLevel * expoGain;
        AttackSpeed += AttackSpeedLevel * expoGain;
        Armor += ArmorLevel * expoGain;
        BaseBlockAmount += BaseBlockLevel * expoGain;
        walkingSpeed += WalkingSpeedLevel;
        runningSpeed += RunningSpeedLevel;
        blockingSpeed += BlockingSpeedLevel;



        
        RageLevel++;

        RageLevelThreshholdCurrent =
            RageLevelThreshholdCurrent * Mathf.Pow(RageLevelThreshholdIncreasePower, RageLevel);

        if (RageLevel == RageMaxLevel)
        {
            RageLevelThreshholdCurrent *= 100;
        }


        //character sais something, n stuff
    }

    public void GetMonsterXP(int amount)
    {
        stageManager.OnPlayerGetMonsterXP(amount);
    }

    public void ConsumeShroom()
    {

    }

}
