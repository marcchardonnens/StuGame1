using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public const string SWINGANIM = "SwingHammerAxe";
    public const string BLOCKANIM = "BlockHammerAxe";
    public const string UNBLOCKANIM = "UnBlockHammerAxe";

    public const float SWINGTIME = 0.50f;
    public const float BLOCKTIME = 0.15f;

    public PlayerController Player;
    public Weapon weapon;


    private Animation anim;
    private bool isBlocking = false;

    private Vector3 resetPos;
    private Quaternion resetRot;

    

    // Start is called before the first frame update
    void Start()
    {
        resetPos = transform.localPosition;
        resetRot = transform.localRotation;
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
            float animationSpeed = 1f + (Player.AttackSpeed/100f);
            anim[SWINGANIM].speed = animationSpeed;
            anim.Play(SWINGANIM);
            Debug.Log("animation speed " + animationSpeed);
            weapon.MeleeAttack(SWINGTIME / animationSpeed);
        }
    }

    public void RangedAttack()
    {
        //throw stone
    }

    public void Block()
    {
        isBlocking = true;
        float animationSpeed = 1f + (Player.AttackSpeed / 100f);
        anim[BLOCKANIM].speed = animationSpeed;
        anim.Play(BLOCKANIM);
        weapon.Block();
    }

    public void Unblock()
    {
        isBlocking = false;
        float animationSpeed = 1f + (Player.AttackSpeed / 100f);
        anim[UNBLOCKANIM].speed = animationSpeed;
        anim.Play(UNBLOCKANIM);
        weapon.Block();
    }

    public void StopAllAnimations()
    {
        anim.Stop();
        transform.localPosition = resetPos;
        transform.localRotation = resetRot;
    }

}

