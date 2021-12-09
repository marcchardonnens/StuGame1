using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodResource : MonoBehaviour, ITakeDamage
{
    public Team Team { get => Team.Neutral; }
    [field: SerializeField]
    public float MaxHP { get; set; }
    [field: SerializeField]
    public float CurrentHP { get; protected set; }
    [SerializeField] private float DeathAnimationTime = 5f;
    public int WoodAmount = 10;
    private bool dead = false;

    public event Action<ITakeDamage, float> OnTakeDamage = delegate { };
    public event Action<ITakeDamage> OnDeath = delegate { };

    public bool TakeDamage(float amount)
    {
        OnTakeDamage?.Invoke(this, amount);
        CurrentHP -= amount;

        if (CurrentHP < 0 && !dead)
        {
            dead = true;
            OnDeath?.Invoke(this);
            GameManager.Instance.Player.GetWood(WoodAmount);

            GetComponent<Animation>().Play("tree003UpperPart|treeFallingCut");
            StartCoroutine(Death());
            return true;
        }
        return false;
    }


    public IEnumerator Death()
    {
        yield return new WaitForSeconds(DeathAnimationTime);
        Destroy(gameObject);
    }


}
