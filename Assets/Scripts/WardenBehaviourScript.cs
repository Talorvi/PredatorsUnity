using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WardenBehaviourScript : MonoBehaviour
{
    #region Fields
    private Rigidbody rBody;
    private NavMeshAgent nav;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (nav.isStopped)
        {
            nav.isStopped = false;
        }
        else if (!nav.hasPath)
        {
            nav.SetDestination(new Vector3(Random.Range(-50, 50), 0f, Random.Range(-50, 50)));
        }
    }
}
