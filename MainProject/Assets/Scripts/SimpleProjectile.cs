using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    public bool IsPlayerProjectile = false;
    public Vector3 direction;
    public float speed;
    public float hp;
    public float damage;
    public float turnspeed;
    public bool tracking;
    public bool slowtracking;
    public Transform tracked;
    public float lifetime;

    public GameObject source;

    private Vector3 initialDirection;

    private bool destroy = false;

    private bool initialized = false;

    private bool hitTrackedOnly = false;

    public void SetPropertiesSimple(GameObject source, Vector3 direction, float speed, float damage, float hp, float lifetime, Transform tracked, bool fromPlayer = false)
    {
        this.source = source;
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.hp = hp;
        this.lifetime = lifetime;
        this.turnspeed = 0;
        this.tracking = false;
        this.slowtracking = false;
        this.tracked = tracked;
        transform.rotation = Quaternion.LookRotation((tracked.position + Vector3.up ) - transform.position);
        IsPlayerProjectile = fromPlayer;
        initialized = true;
        gameObject.SetActive(true);
        Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), source.GetComponent<Collider>());
    }

    public void SetPropertiesTracked(GameObject source, Vector3 direction, float speed, float damage, float hp, float lifetime, bool slowtracking, float turnspeed, Transform tracked, bool HitTrackedOnly, bool fromPlayer = false)
    {
        this.source = source;
        this.direction = direction;
        this.speed = speed;
        this.damage = damage;
        this.hp = hp;
        this.lifetime = lifetime;
        this.turnspeed = turnspeed;
        this.tracking = true;
        this.slowtracking = slowtracking;
        this.tracked = tracked;
        transform.rotation = Quaternion.LookRotation(tracked.position - transform.position);
        this.hitTrackedOnly = HitTrackedOnly;
        IsPlayerProjectile = fromPlayer;
        initialized = true;
        gameObject.SetActive(true);

        if (source.GetComponent<Collider>() != null)
        {
            Physics.IgnoreCollision(gameObject.GetComponent<Collider>(), source.GetComponent<Collider>());
        }
    }

    void Awake()
    {
        gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (tracked == null)
        {
            SelfDestruct();
            return;
        }
        
        if (tracking)
        {
            Vector3 targetDir = tracked.position - transform.position;
            if (slowtracking)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, Mathf.Deg2Rad * turnspeed * Time.deltaTime, 0f));
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(targetDir);
            }
        }


        transform.position += transform.forward * speed * Time.deltaTime;

        lifetime -= Time.deltaTime;

        if (lifetime <= 0 || hp <= 0)
        {
            SelfDestruct();
        }

    }


    void OnTriggerEnter(Collider collider)
     {
        if (hitTrackedOnly)
        {
            if (collider.transform == tracked)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy)
                {
                    bool lethal = enemy.TakeDamage(damage);
                }
                SelfDestruct();
            }
            return;
        }

        PlayerController pc = collider.transform.GetComponent<PlayerController>();
        if (pc)
        {
            pc.TakeDamage(damage);
        }

        if (collider.gameObject.layer == GameConstants.SCENERYLAYER)
        {
            SelfDestruct();
        }

    }


    public void SelfDestruct()
    {
        Destroy(gameObject);
    }


}
