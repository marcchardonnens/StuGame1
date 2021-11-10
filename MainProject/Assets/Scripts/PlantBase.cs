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
        }
        currentHP = MaxHP;
        player = GameManager.Player;
    }

    public virtual IEnumerator Grow(float growtime)
    {
        player = GameManager.Player;
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
        throw new System.NotImplementedException();
    }
}
