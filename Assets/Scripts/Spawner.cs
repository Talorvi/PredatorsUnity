using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject predatorPrefab;
    public GameObject prayPrefab;
    public GameObject wardenPrefab;
    void Start()
    {
        #region Pray Spawn
        int rand = Random.Range(6, 12);
        for (int i = 0; i < rand; i++)
        {
            var ci = Random.insideUnitCircle * 20;
            var newPrey = Instantiate(prayPrefab,new Vector3(ci.x,1f, ci.y),Quaternion.identity,null);
            newPrey.name = "Prey";
        }
        #endregion
        #region Predator Spawn
        rand = Random.Range(2, 5);
        for (int i = 0; i < rand; i++)
        {
            var ci = (Random.insideUnitCircle * 20);
            ci += new Vector2(Mathf.Sign(ci.x) * 25, Mathf.Sign(ci.y) * 25);
            var newPredator = Instantiate(predatorPrefab, new Vector3(ci.x, 1f, ci.y), Quaternion.identity, null);
            newPredator.name = "Predator";
        }
        #endregion
        # region Warden Spawn
        var warden = Instantiate(wardenPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity, null);
        warden.name = "Warden";
        #endregion
    }
}
