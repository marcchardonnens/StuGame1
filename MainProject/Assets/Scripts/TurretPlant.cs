using System.Numerics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

public class TurretPlant : PlantBase
{
    public float Damage = 50f;
    public float AttackCoolDown = 3f;
    public float ProjectileSpeed = 30f;
    public float Range = 30f;
    public bool Tracking = true;
    public bool SlowTracking = false;

    public GameObject ProjectilePrefab;
    public Transform[] ProjectileSpawnPosition;

    //public int AttackSalveSpreadAngle = 30;
    public int BaseAttackSalveAmount = 3;
    public int UpgradeAttackSalveAmount = 6;

    public override IEnumerator Grow()
    {
        yield return base.Grow();
        StartCoroutine(PeriodicActions(AttackCoolDown));
    }

    private IEnumerator PeriodicActions(float periodicDelay)
    {
        while (true)
        {
            int shots = GameManager.ProfileData.HasTurretUpgrade ? UpgradeAttackSalveAmount : BaseAttackSalveAmount;

            HashSet<Enemy> enemies = new HashSet<Enemy>();
            for (int i = 0; i < shots; i++)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, Range, 1 << GameConstants.ENEMYLAYER);

                //find closest target
                foreach (Collider hit in hits)
                {
                    Enemy enemy = hit.GetComponent<Enemy>();
                    if (enemy)
                    {
                        enemies.Add(enemy);
                    }
                }
            }

            enemies.OrderBy(x =>
            {
                float dist = Mathf.Abs(Vector3.Distance(transform.position, x.transform.position));
                return dist;
            });


            shots = System.Math.Min(shots, enemies.Count);
            for (int i = 0; i < shots; i++)
            {
                SimpleProjectile projectile = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity).GetComponent<SimpleProjectile>();
                projectile.SetPropertiesTracked(gameObject, enemies.ElementAt(i).transform.position - transform.position, ProjectileSpeed, Damage, 1000f, 10f, SlowTracking, 1000f, enemies.ElementAt(i).transform, true, Team);
                projectile.transform.position = ProjectileSpawnPosition[i%ProjectileSpawnPosition.Length].position;
            }

            yield return new WaitForSeconds(periodicDelay);
        }
    }
}
