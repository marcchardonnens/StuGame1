using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPlant : MonoBehaviour
{
    public float Health = 200f;
    public float Damage = 50f;
    public float CoolDown = 3f;
    public float ProjectileSpeed = 30f;
    public float Range = 30f;
    public bool Tracking = true;
    public bool SlowTracking = false;

    public GameObject ProjectilePrefab;
    
    public float Duration = 0f; // 0 = infinite
    public float GrowTime = 10f;



    private bool grown = false;
    private Vector3 finalScale;
    private PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        player = StageManager.Player;
        if (GrowTime > 0)
        {
            finalScale = transform.localScale;
            transform.localScale = Vector3.zero;
        }
        StartCoroutine(PeriodicActions());
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(Grow());
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
        }
    }

    private IEnumerator PeriodicActions()
    {
        yield return new WaitForSeconds(GrowTime);
        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, Range, 1 << GameConstants.ENEMYLAYER);

            float closestDistance = float.MaxValue;
            Enemy closestEnemy = null;

            //find closest target
            foreach (Collider hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy)
                {
                    float dist = Mathf.Abs(Vector3.Distance(transform.position, enemy.transform.position));
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestEnemy = enemy;
                    }
                }
            }

            if (closestEnemy != null)
            {
                SimpleProjectile projectile = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity).GetComponent<SimpleProjectile>();
                projectile.SetPropertiesTracked(gameObject, (closestEnemy.transform.position - transform.position) + new Vector3(0,3f,0), ProjectileSpeed, Damage, 1000f, 10f, SlowTracking, 1000f, closestEnemy.transform, true);
            }


            yield return new WaitForSeconds(CoolDown);
        }
    }

}
