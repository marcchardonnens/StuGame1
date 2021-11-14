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
    protected PlayerController player;
    public event Action<float> OnTakeDamage = delegate { };
    public event Func<IPlant> OnDeath = delegate { return null; };

    protected void Start()
    {
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
            StartCoroutine(Grow(GrowTime));
        }
        CurrentHP = MaxHP;
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
        OnTakeDamage?.Invoke(amount);
        CurrentHP -= amount;
        if (CurrentHP <= 0)
        {   
            OnDeath?.Invoke();
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
