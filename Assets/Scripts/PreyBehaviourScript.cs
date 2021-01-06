using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PreyBehaviourScript : MonoBehaviour
{
    #region Fields
    #region Component Variables
    public GameObject childPrefab;
    public NavMeshAgent nav; 
    private ParticleSystem ps;
    public ParticleSystem droplets;
    #endregion

    #region Mating Variables
    private GameObject mate;
    private bool pregnant = false;
    private bool seekMate = false;
    private bool foundMate = false;
    #endregion

    #region Saturation Variables
    private float saturation;
    public float startingSaturation = 0.0f;
    public float saturationLossRate;
    public float saturationFillRate;
    #endregion
    #region Lust Variables
    private float lust = 0.0f; 
    public float startingLust = 0.0f;
    public float maxLustRate;
    public float minLustRate;
    #endregion

    #region Fuzzy Variables
    #region Saturation
    public AnimationCurve hungry;
    public AnimationCurve full;
    private float hungryValue;
    private float fullValue;
    #endregion
    #region Lust
    public AnimationCurve lustless;
    public AnimationCurve lustful;
    private float lustlessValue;
    private float lustValue;
    #endregion
    #endregion

    #region Other Variables
    private bool handlePrio = true;
    #endregion
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        lust = startingLust;
        saturation = startingSaturation;
        nav = GetComponent<NavMeshAgent>();
        ps = GetComponent<ParticleSystem>();
        print("Prey " + gameObject.GetInstanceID() + " has been born.");
    }

    // Update is called once per frame
    void Update()
    {
        if (handlePrio)
            StartCoroutine(ChangePrio());
        
        UpdateValues();
        TakeAction();
    }

    private void TakeAction()
    {

        if (!foundMate)
        {
            if (seekMate)
            {
                Move();
            }
            else
            {
                #region Fuzzy Values
                hungryValue = hungry.Evaluate(saturation / 100);
                fullValue = full.Evaluate(saturation / 100);

                lustValue = lustful.Evaluate(lust / 100);
                lustlessValue = lustless.Evaluate(lust / 100);

                var walkingProbability = Math.Max(Math.Min(hungryValue, fullValue), 1 - hungryValue);

                var noMatingValue = Math.Max(hungryValue * lustValue, Math.Max(hungryValue * lustlessValue, lustlessValue * fullValue));
                var matingValue = lustValue * fullValue;
                #endregion

                if (Random.value <= walkingProbability)
                    Move();
                else
                    Eat();

                if (matingValue > noMatingValue)
                {
                    seekMate = true;
                    ps.Play();
                }
            }
        }
        else
        {
            GoToMate();
        }
    }

    private void UpdateValues()
    {
        saturation -= saturationLossRate * Time.deltaTime;
        lust += Random.Range(minLustRate, maxLustRate) * Time.deltaTime;

        if (saturation < 0)
        {
            saturation = 0;
        }

        if (lust < 0)
        {
            lust = 0;
        }
        else if (lust > 100)
        {
            lust = 100;
        }
    }

    private void GoToMate()
    {
        if (Vector3.Distance(gameObject.transform.position, mate.transform.position) < 2.2f)
        {
            mate = null;
            foundMate = false;
            lust = startingLust;
            saturation = startingSaturation;
            if (pregnant)
            {
                pregnant = false;
                var go = Instantiate(childPrefab, transform.position, Quaternion.identity, null);
                go.name = "Prey";
            }
        }
        else
            nav.SetDestination(mate.transform.position);
    }
    private void Move()
    {
        if (nav.isStopped)
        {
            nav.isStopped = false;
        }
        else if (!nav.hasPath)
        {
            print("Prey " + gameObject.GetInstanceID() + " has changed its destination.");
            nav.SetDestination(new Vector3(Random.Range(-50, 50), 0f, Random.Range(-50, 50)));
        }
    }
    private void Eat()
    {
        nav.isStopped = true;
        saturation += saturationFillRate * Time.deltaTime;

        if (saturation > 100)
        {
            saturation = 100;
        }
    }
    private IEnumerator ChangePrio()
    {
        handlePrio = false;
        nav.avoidancePriority = Random.Range(1, 10);
        yield return new WaitForSeconds(2f);
        handlePrio = true;
    }
    public bool LoveCall(GameObject caller)
    {
        if (seekMate)
        {
            seekMate = false;
            mate = caller;
            foundMate = true;
            pregnant = true;
            print("Prey " + gameObject.GetInstanceID() + " has found a mate of id " + mate.gameObject.GetInstanceID());
            return true;
        }
        return false;
    }
    public void OnTriggerStay(Collider other)
    {
        if (seekMate)
        {
            if (other.CompareTag("Prey"))
            {
                if (other.gameObject.GetComponent<PreyBehaviourScript>().LoveCall(gameObject))
                {
                    seekMate = false;
                    mate = other.gameObject;
                    foundMate = true;
                }
            }
        }
    }
    private void OnDestroy()
    {
        if (foundMate)
        {
            var ms = mate.GetComponent<PreyBehaviourScript>();
            ms.foundMate = false;
            ms.seekMate = true;
            ms.pregnant = false;
            ms.mate = null;
        }

        print("Prey " + gameObject.GetInstanceID() + " has died.");
    }

    private IEnumerator DieAnimation()
    {
        var drop = Instantiate(droplets, gameObject.transform.position, Quaternion.identity);
        drop.Play();
        yield return new WaitForSeconds(2f);
    }
}
