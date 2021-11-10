using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodResource : MonoBehaviour, ITakeDamage
{
    public Team Team {get => Team.Neutral;}
    [SerializeField] private float DeathAnimationTime = 5f;
    public float Health = 300f;
    public int WoodAmount = 10;
    private bool dead = false;

    public bool TakeDamage(float amount)
    {
        Health -= amount;
        
        if (Health < 0 && !dead)
        {
            dead = true;
            StageManager.Player.GetWood(WoodAmount);

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
