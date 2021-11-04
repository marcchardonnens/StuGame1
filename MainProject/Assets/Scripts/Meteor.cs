using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    //meteor properties
    private GameObject Indicator;
    private float Damage;
    private Vector3 TargetPos;
    private float speed;
    private float ExplotionRadius;

    private bool initialized = false;


    // Start is called before the first frame update
    void Start()
    {
    }

    public void Initialize(Vector3 target, float dmg, float explotion, GameObject indicator, float timeUntilDestinationReached)
    {
        TargetPos = target;
        Damage = dmg;
        Indicator = indicator;
        ExplotionRadius = explotion;

        float dist = Mathf.Abs(Vector3.Distance(gameObject.transform.position, target));

        speed = dist / timeUntilDestinationReached;

        initialized = true;
        indicator.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(initialized)
        {
            Vector3 dir = TargetPos - transform.position;
            transform.position += dir.normalized * speed * Time.deltaTime;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == GameConstants.GROUNDLAYER)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, ExplotionRadius);
            foreach (Collider collider in colliders)
            {
                PlayerController player = collider.GetComponent<PlayerController>();
                if(player)
                {
                    player.TakeDamage(Damage);
                }
                else
                {
                    Boss boss = collider.GetComponent<Boss>();
                    if(boss)
                    {
                        boss.TakeDamage(Damage);
                    }
                }

            }


            Destroy(Indicator);
            Destroy(gameObject);

        }
    }

}
