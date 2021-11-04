using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPlant : MonoBehaviour
{
    public float Health = 200f;
    public float Duration = 0f; // 0 = infinite
    public float GrowTime = 60f;
    public float UpgradeGrowtimeMultiplier = 0.5f;

    public Light light;
    public GameObject bloom;

    private bool grown = false;
    private Vector3 finalScale;
    private PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.ProfileData.HasSeedUpgrade)
        {
            GrowTime *= UpgradeGrowtimeMultiplier;
        }
        player = StageManager.Player;
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }
    }


    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Grow());
    }

    public void Interact()
    {

        Debug.Log("Seedplant Interact");

        if (grown)
        {
            //refill seeds



            Destroy(gameObject);
        }
    }

    private IEnumerator Grow()
    {
        if (transform.localScale.x < finalScale.x)
        {
            yield return null;
            transform.localScale += (finalScale / GrowTime) * Time.deltaTime;
        }
        else
        {
            grown = true;
            light.enabled = true;
            bloom.SetActive(true);
        }
    }

    public bool TakeDamage(float amount)
    {
        bool lethal = false;



        return lethal;
    }


}
