using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodResource : MonoBehaviour
{

    public float Health = 300f;
    public int WoodAmount = 10;

    private bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TakeDamage(float amount)
    {
        Health -= amount;
        
        if (Health < 0 && !dead)
        {
            dead = true;
            StageManager.Player.GetWood(WoodAmount);

            GetComponent<Animation>().Play("tree003UpperPart|treeFallingCut");
            StartCoroutine(Death());
        }
    }


    public IEnumerator Death()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }


}
