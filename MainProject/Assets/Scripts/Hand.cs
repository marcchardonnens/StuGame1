using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public const string SWINGANIM = "Swing";
    public const string BLOCKANIM = "Block";
    public const string UNBLOCKANIM = "UnBlock";

    public const float SWINGTIME = 1f;
    public const float BLOCKTIME = 0.75f;

    public PlayerController Player;
    public Weapon weapon;


    private Animation anim;
    private bool isBlocking = false;

    

    // Start is called before the first frame update
    void Start()
    {
        weapon.Pickup(Player);
        anim = GetComponent<Animation>();
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MeleeAttack()
    {
        isBlocking = false;
        if (weapon != null)
        {
            float animationSpeed = 1f/(weapon.BaseAttackSpeed * SWINGTIME / (1f + (Player.AttackSpeed/100f)));
            anim[SWINGANIM].speed = animationSpeed;
            anim.Play(SWINGANIM);
            weapon.MeleeAttack(animationSpeed);
        }

    }

    public void RangedAttack()
    {
        //throw stone
    }

    public void Block()
    {
        isBlocking = true;
        float animationSpeed = 1f / (weapon.BaseAttackSpeed * BLOCKTIME / (1f + (Player.AttackSpeed / 100f)));
        anim[BLOCKANIM].speed = animationSpeed;
        anim.Play(BLOCKANIM);
        weapon.Block();
    }

    public void Unblock()
    {
        isBlocking = false;
        float animationSpeed = 1f / (weapon.BaseAttackSpeed * BLOCKTIME / (1f + (Player.AttackSpeed / 100f)));
        anim[UNBLOCKANIM].speed = animationSpeed;
        anim.Play(UNBLOCKANIM);
        weapon.Block();
    }

}

