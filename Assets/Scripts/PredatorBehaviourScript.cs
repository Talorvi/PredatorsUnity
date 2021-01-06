using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PredatorBehaviourScript : MonoBehaviour
{
    #region Fields
    #region Component Variables
    public GameObject childPrefab;
    private NavMeshAgent nav;
    private ParticleSystem ps;
    public ParticleSystem droplets;
    #endregion region
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
    #region Saturation Variables
    private float saturation;
    public float startingSaturation;
    public float saturationLossRate = 0.1f;
    public float pregnancySaturationCost = 50f;
    public float saturationHuntReward = 60;
    #endregion
    #region Lust Variables
    private float lust;
    public float startingLust;
    public float maxLustRate;
    public float minLustRate;
    #endregion
    #region Hunting Variables
    private bool isHunting = false;
    public bool hasPrey = false;
    public GameObject prey;
    #endregion
    #region Mating Variables
    private bool seekMate = false;
    private GameObject mate = null;
    private bool foundMate = false;
    private bool pregnant = false;
    private Vector3 nestPosition;
    #endregion
    #region Other Variables
    private bool handlePrio = false;
    private bool running = false;
    public float runDistance = 4.0f;
    #endregion
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        lust = startingLust;
        saturation = startingSaturation;
        nav = GetComponent<NavMeshAgent>();
        ps = GetComponent<ParticleSystem>();
        print("Predator " + gameObject.GetInstanceID() + " has been born.");
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
        if (running)
        {
            if (!nav.hasPath)
            {
                running = false;
            }
        }
        else if (foundMate)
        {
            GoToMate();
        }
        else if (hasPrey)
        {
            Hunt();
        }
        else
        {
            if (!seekMate)
            {
                #region Fuzzy Values
                hungryValue = hungry.Evaluate(saturation / 100);
                fullValue = full.Evaluate(saturation / 100);

                lustValue = lustful.Evaluate(lust / 100);
                lustlessValue = lustless.Evaluate(lust / 100);

                var noMatingValue = Math.Max(hungryValue * lustValue, Math.Max(hungryValue * lustlessValue, lustlessValue * fullValue));
                var matingValue = lustValue * fullValue;
                #endregion

                if (matingValue > noMatingValue)
                {
                    seekMate = true;
                    FindLove();
                    ps.Play();
                }

                if (hungryValue > fullValue)
                {
                    isHunting = true;
                }
            }
            Move();
        }
    }

    private void UpdateValues()
    {
        saturation -= saturationLossRate * Time.deltaTime;
        lust += Random.Range(minLustRate, maxLustRate) * Time.deltaTime;

        if (saturation < 0)
        {
            StartCoroutine(DieAnimation());
            Destroy(gameObject);
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

    private void Move()
    {
        if (nav.isStopped)
        {
            nav.isStopped = false;
        }
        else if (!nav.hasPath)
        {
            nav.SetDestination(new Vector3(Random.Range(-50, 50), 0f, Random.Range(-50, 50)));
            print("Predator " + gameObject.GetInstanceID() + " has changed its destination.");
        }
    }

    private void Hunt()
    {
        nav.SetDestination(prey.transform.position);
        if (Vector3.Distance(gameObject.transform.position, prey.transform.position) < 2.5)
        {
            Eat();
        }
    }

    private void Eat()
    {
        isHunting = false;
        hasPrey = false;
        saturation += saturationHuntReward;
        if (saturation > 100)
        {
            saturation = 100;
        }
        print("Predator " + gameObject.GetInstanceID() + " has eaten Prey " + prey.gameObject.GetInstanceID());
        StartCoroutine(DieAnimation());
        Destroy(prey);
        prey = null;
    }

    private void FindLove()
    {
        var predators = GameObject.FindGameObjectsWithTag("Predator");
        foreach (var predator in predators)
        {
            if (predator != gameObject)
            {
                var pbs = predator.gameObject.GetComponent<PredatorBehaviourScript>();
                if (pbs.seekMate)
                {
                    var c = (Random.insideUnitCircle * 20);
                    c += new Vector2(Mathf.Sign(c.x) * 25, Mathf.Sign(c.y) * 25);
                    nestPosition = new Vector3(c.x, 1f, c.y);
                    pbs.nestPosition = nestPosition;
                    mate = predator.gameObject;
                    pbs.mate = gameObject;
                    seekMate = false;
                    pbs.seekMate = false;
                    foundMate = true;
                    pbs.foundMate = true;
                    pbs.pregnant = true;
                    print("Predator " + gameObject.GetInstanceID() + " has found a mate in Predator " + pbs.gameObject.GetInstanceID());
                }
            }
        }
    }

    private void GoToMate()
    {
        if (Vector3.Distance(gameObject.transform.position, mate.transform.position) < 2.2f)
        {
            nav.ResetPath();
            mate = null;
            foundMate = false;
            lust = startingLust;
            if (pregnant)
            {
                saturation -= pregnancySaturationCost;
                pregnant = false;
                var go = Instantiate(childPrefab, transform.position, Quaternion.identity, null);
                go.name = "Predator";
            }
        }
        else
            nav.SetDestination(nestPosition);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Warden"))
        {
            RunFromWarden(other.gameObject);
        }
        else if (!hasPrey && isHunting && other.CompareTag("Prey") && Vector3.Distance(other.gameObject.transform.position, gameObject.transform.position) <= 6)
        {
            NegotiatePrey(other.gameObject);
        }
    }
    private void NegotiatePrey(GameObject prey)
    {
        var predators = GameObject.FindGameObjectsWithTag("Predator");
        bool canAttack = true;
        foreach (var predator in predators)
        {
            var pbs = predator.gameObject.GetComponent<PredatorBehaviourScript>();
            if (pbs.prey == prey)
            {
                if (pbs.saturation < saturation)
                {
                    canAttack = false;
                    break;
                }
                pbs.prey = null;
                pbs.hasPrey = false;
                print("Predator " + gameObject.GetInstanceID() + " has has taken over a Prey " + prey.gameObject.GetInstanceID() + " with " + pbs.gameObject.GetInstanceID());
            }
        }
        if (canAttack)
        {
            this.prey = prey;
            hasPrey = true;
            print("Predator " + gameObject.GetInstanceID() + " has begun hunting a Prey " + prey.gameObject.GetInstanceID());
        }
    }
    private void RunFromWarden(GameObject warden)
    {
        running = true;
        hasPrey = false;
        prey = null;
        var runVector = gameObject.transform.position - (runDistance * Vector3.Normalize(warden.transform.position - gameObject.transform.position));
        nav.SetDestination(runVector);
    }
    private IEnumerator ChangePrio()
    {
        handlePrio = false;
        nav.avoidancePriority = Random.Range(1, 10);
        yield return new WaitForSeconds(2f);
        handlePrio = true;
    }
    
    private void OnDestroy()
    {
        if (foundMate)
        {
            var ms = mate.GetComponent<PredatorBehaviourScript>();
            ms.foundMate = false;
            ms.seekMate = true;
            ms.pregnant = false;
            ms.mate = null;
        }
        
        print("Predator " + gameObject.GetInstanceID() + " has died.");
    }

    private IEnumerator DieAnimation()
    {
        var drop = Instantiate(droplets, gameObject.transform.position, Quaternion.identity);
        drop.Play();
        yield return new WaitForSeconds(2f);
    }
}
