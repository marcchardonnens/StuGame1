using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Survivor : MonoBehaviour
{

    public float followInterval = 3f;


    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(FollowPlayer());
    }

    private IEnumerator FollowPlayer()
    {
        while(true)
        {

            agent.SetDestination(GameManager.Instance.Player.transform.position);
            yield return new WaitForSeconds(followInterval);     
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
