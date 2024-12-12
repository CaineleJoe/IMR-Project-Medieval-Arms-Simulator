using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class CrusaderAI : MonoBehaviour
{
    private SphereCollider AICollider;
    private Animator AIAnimator;
    private Transform AITransform;

    private Random rand = new Random();

    [SerializeField] private float dodgeChance = 30.0f;
    [SerializeField] private float attackShieldChance = 25.0f;
    [SerializeField] private float rethinkChance = 35.5f;
    [SerializeField] private float seeRadius = 7f;
    [SerializeField] private float movingSpeed = .6f;
    [SerializeField] private float engageDistance = 1.5f;

    [SerializeField] private float health = 100f;
    [SerializeField] private float runDistance = 2.5f;

    [SerializeField] private float runawayDistance = 5f;


    [SerializeField] private bool isWalking = false;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool isEngaged = false;
    [SerializeField] private bool hasDied = false;
    [SerializeField] private bool hasBeenHit = false;

    private string currentState;


    private const string AI_WALK = "walk";
    private const string AI_RUN = "run";
    private string[] AI_ATTACKS = {
        "atack1", "atack2", "atack3"
    };

    private const string AI_SHIELD_ATTACK = "atack shield";
    private const string AI_IDLE = "idle1 0";



    private Dictionary<int, List<GameObject>> _closeTriggers = new Dictionary<int, List<GameObject>>();

    private void Awake()
    {
        if (AICollider == null)
        {
            AICollider = GetComponent<SphereCollider>();
        }

        if (AIAnimator == null)
        {
            AIAnimator = GetComponent<Animator>();
        }

        if (AITransform == null)
        {
            AITransform = GetComponent<Transform>();
        }
        
        _closeTriggers.Add(0, new List<GameObject>());
        _closeTriggers.Add(1, new List<GameObject>());
        _closeTriggers.Add(2, new List<GameObject>());
    }

    private void Start()
    {
        if (AICollider != null)
        {
            AICollider.radius = seeRadius;
        }
    }
    

    private void FixedUpdate()
    {
        Debug.Log("Thinking...");
        if (_closeTriggers.ContainsKey(0) && _closeTriggers[0].Count > 0) // enemy near
        {
            Debug.Log("I am seeing an enemy...");
            isWalking = true;
            GameObject closestEnemy = null;
            float closestDistance = float.MaxValue;
            foreach (var enemy in _closeTriggers[0])
            {
                var currentDistance =
                    Vector3.Distance(this.gameObject.transform.position, enemy.transform.position);
                if (currentDistance < closestDistance)
                {
                    closestEnemy = enemy;
                    closestDistance = currentDistance;
                }
            }

            if (closestEnemy is null)
                return;

            isRunning = Vector3.Distance(closestEnemy.transform.position, AITransform.position) >= runDistance;
            var closestEnemySameY = new Vector3(closestEnemy.transform.position.x, AITransform.position.y,
                closestEnemy.transform.position.z);
                
            AITransform.LookAt(Vector3.Lerp(AITransform.position + AITransform.forward, closestEnemySameY, 0.05f));

            AITransform.position =
                Vector3.MoveTowards(AITransform.position,
                    closestEnemySameY - AITransform.forward * engageDistance,
                    movingSpeed * (isRunning ? 2.2f : 1f) * Time.fixedDeltaTime);

            isEngaged = Vector3.Distance(AITransform.position, closestEnemySameY) < 1.0f;
        }
        else
        {
            isWalking = false;
        }

        if (_closeTriggers.ContainsKey(1) && _closeTriggers[1].Count > 0) // ally near
        {
            Debug.Log("Moving away from ally!");
            List<GameObject> closestAllies = new List<GameObject>();
            foreach (var ally in _closeTriggers[1])
            {
                var currentDistance =
                    Vector3.Distance(this.gameObject.transform.position, ally.transform.position);
                if (currentDistance < runawayDistance)
                {
                    var closestAllySameY = new Vector3(ally.transform.position.x, AITransform.position.y,
                        ally.transform.position.z);
                    var direction = transform.position - ally.transform.position; 
                    direction.Normalize();
                    direction.y = 0;
                    AITransform.position = Vector3.MoveTowards(AITransform.position, closestAllySameY + direction,
                        movingSpeed * Time.fixedDeltaTime);
                }
            }

            
        }
    }

    private void Update()
    {
        AIAnimationLogic();
    }

    void AIAnimationLogic()
    {
        if(isWalking && !isRunning && !isEngaged)
            ChangeAnimationState(AI_WALK);
        if(isRunning && !isEngaged)
            ChangeAnimationState(AI_RUN);
        if(!isWalking && !isRunning && !isEngaged)
            ChangeAnimationState(AI_IDLE);
        if (isEngaged && AIAnimator.GetCurrentAnimatorStateInfo(0).length <= AIAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime)
        {
            var attackWithShield = rand.Next(0, 100) < attackShieldChance;
            if(attackWithShield)
                ChangeAnimationState(AI_SHIELD_ATTACK);
            else
                ChangeAnimationState(AI_ATTACKS[rand.Next(0, AI_ATTACKS.Length-1)]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);

        switch (other.tag)
        {
            case "MainCamera":
                _closeTriggers[0].Add(other.gameObject);
                break;
            case "Enemy":
                _closeTriggers[1].Add(other.gameObject);
                break;
            case "InterestPoints":
                _closeTriggers[2].Add(other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _closeTriggers.
            Where(kvp => kvp.Value.Contains(other.gameObject)).
            Select(kvp => kvp.Value).ToList().
            ForEach(list => list.Remove(other.gameObject));
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        AIAnimator.Play(newState);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (AICollider == null) AICollider = GetComponent<SphereCollider>();
        if (AICollider != null)
        {
            AICollider.radius = seeRadius;
        }
    }
#endif
}