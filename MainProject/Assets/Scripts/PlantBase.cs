using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlantBase : MonoBehaviour, IPlant
{
    public Team Team {get => Team.Player;}
    public float MaxHP = 500f;
    public float GrowTime = 5f;
    public float Duration = 0f; //0 = infinite
    private Vector3 finalScale;
    protected bool grown = false;
    protected PlayerController player;
    protected float currentHP;

    protected void Start()
    {
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
            StartCoroutine(Grow(GrowTime));
        }
        currentHP = MaxHP;
        player = GameManager.Instance.Player;
    }

    public virtual IEnumerator Grow(float growtime)
    {
        player = GameManager.Instance.Player;
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }

        if (transform.localScale.x < finalScale.x)
        {
            yield return null;
            transform.localScale += finalScale / growtime * Time.deltaTime;
        }
    }

    public virtual bool TakeDamage(float amount)
    {
        currentHP -= amount;
        if(currentHP <= 0)
        {
            Destroy(gameObject);
            //TODO raise plant died event
            return true;
        }
        //TODO raise plant take damage event
        return false;
    }
}
