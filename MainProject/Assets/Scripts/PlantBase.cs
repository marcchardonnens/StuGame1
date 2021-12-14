using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlantBase : MonoBehaviour, IPlant
{
    public Team Team { get; } = Team.Player;
    [field: SerializeField]
    public float MaxHP { get; set; }
    [field: SerializeField]
    public float CurrentHP { get; protected set; }
    public float GrowTime = 5f;
    public float Duration = 0f; //0 = infinite
    private Vector3 finalScale;
    protected bool grown = false;
    public event Action<ITakeDamage, float> OnTakeDamage = delegate { };
    public event Action<ITakeDamage> OnDeath = delegate { };

    protected void Start()
    {
        if (GrowTime > 0)
        {
            StartCoroutine(Grow());
        }
        CurrentHP = MaxHP;
    }

    public virtual IEnumerator Grow()
    {
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }
        while (transform.localScale.x < finalScale.x)
        {
            transform.localScale += finalScale / GrowTime * Time.deltaTime;
            yield return null;
        }
        transform.localScale = finalScale;
        grown = true;
    }

    public virtual bool TakeDamage(float amount)
    {
        OnTakeDamage?.Invoke(this, amount);
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {   
            OnDeath?.Invoke(this);
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
