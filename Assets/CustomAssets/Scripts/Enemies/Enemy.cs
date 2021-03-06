﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class Enemy : Target
{
    #region Enums
    [System.Serializable]
    public enum State
    {
        DEFAULT,
        AGRO,
        ATTACKING,
        SEARCHING,
        HEALING
    }
    [System.Serializable]
    public enum SearchSubState
    {
        NOT_SEARCHING,
        CHECK_LAST_POS,
        PICK_SEARCH_POINTS,
        MOVING_TO_POINT,
        ARRIVED_AT_POINT,
        STOPPED_AT_POINT,
        DONE
    }
    [System.Serializable]
    public enum DefaultSubState
    {
        NOT_DEFAULT,
        AT_POSITION,
        WAITING,
        MOVING_TO_NEXT
    }
    [System.Serializable]
    public enum AttackSubState
    {
        NOT_ATTACKING,
        WAITING,
        ATTACKING,
        DONE
    }

    [System.Serializable]
    public enum DefaultStateType
    {
        STATIONARY,
        PATROL,
        WANDER
    }

    [System.Serializable]
    public enum AttackTimeType
    {
        CONSTANT,
        RANDOM_BETWEEN_CONSTANTS
    }
    #endregion

    #region Variables

    public static List<Enemy> curedEnemies;

    #region AnimationVariables
    [Header("Animation Variables")]
    public GameObject enemyGFX;
    
    public GameObject damageDoneTextPrefab;
    [HideInInspector] public float damageTextVerticalOffset = 0.75f;
    public Color damageTextColor = Color.red;
    public Color finalHitTextColor = Color.green;


    //Hidden Variables
    [HideInInspector] public Animator enemyAnimations;
    [HideInInspector] public SpriteRenderer sprite;
    public float angleToPlayer { get; protected set; }
    protected float angleInRadians = 0f;
    protected Vector2 horizPositionDifference;
    protected Vector2 rotatedPositionDifference;
    protected Vector2 forward = new Vector2(0f, 1f);
    protected int activeLayerIndex = 2;
    protected int previousLayerIndex = 2;
    protected int damageDonePreviousFixedFrameFrame;
    protected int previousHealth;

    protected GameObject[] damageDoneTexts;
    protected int nextDamageText = 0;
    #endregion

    #region AI Variables
    [Header("Range (Ensure that Chase > Agro > Attack)")]
    [Header("AI Variables")]
    [Range(0f, 50f)]
    public float chaseLimitRadius = 10f;
    [Range(0f, 50f)]
    public float agroRadius = 7.5f;
    [Range(0f, 5f)]
    public float attackRaduis = 2f;
    [Header("Attacking")]
    public int attackDamage = 1;
    public AttackTimeType attackTimeType;
    public float attackTime = 1f;
    public float attackTimeMin = 1f;
    public float attackTimeMax = 2f;
    [Header("Movement")]
    [Range(0f, 60f)]
    public float timeAtPosition;
    public DefaultStateType defaultStateType;
    public List<Transform> patrolPoints;
    [Range(0f, 50f)]
    public float wanderRaduisMax = 10f;
    [Range(0f, 50f)]
    public float wanderRaduisMin = 0f;

    #region Internal Variables
    //States
    [HideInInspector] public State state;
    [HideInInspector] public DefaultSubState defaultSubState;
    [HideInInspector] public SearchSubState searchSubState;
    [HideInInspector] public AttackSubState attackSubState;
    //Location Flags
    protected bool hasLineOfSight = false;
    protected bool inChaseRadius = false;
    protected bool inAgroRadius = false;
    protected bool inAttackRadius = false;
    protected Vector3 nextPosition = Vector3.zero;
    protected Transform playerTransform;
    protected NavMeshAgent agent;
    protected bool destinationChanged;
    protected Vector3 previousDestination;
    protected List<Vector3> pointsToSearch = new List<Vector3>();
    protected Vector3 initialPosition;
    protected Quaternion initialRotation;
    protected int nextPatrolPoint = 0;
    protected float initialStoppingDistance;

    [HideInInspector] public bool targeted;

    #endregion
    #endregion

    #region Sounds

    [Header("Sounds")]
    public AudioClip ambientHostile;
    public AudioClip ambientCured;
    public AudioClip walkSounds;

    [HideInInspector] public AudioSource audioSource;
    protected bool playAmbientSounds;

    #endregion

    #endregion

    #region Methods
    private void Awake()
    {
        if (curedEnemies == null)
        {
            curedEnemies = new List<Enemy>();
        }
    }

    protected virtual void Start()
    {
        enemyAnimations = enemyGFX.GetComponent<Animator>();
        sprite = enemyGFX.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        health = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        previousDestination = agent.destination;
        playerTransform = PlayerManager.instance.transform;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialStoppingDistance = agent.stoppingDistance;

        damageDoneTexts = new GameObject[3];
        for (int i = 0; i < damageDoneTexts.Length; i++)
        {
            damageDoneTexts[i] = Instantiate(damageDoneTextPrefab, enemyAnimations.gameObject.transform);
            damageDoneTexts[i].SetActive(false);
        }

        audioSource.clip = walkSounds;
    }

    public LevelManager.CheckpointData.EnemyData MakeCheckpoint()
    {
        LevelManager.CheckpointData.EnemyData data;

        data.worldPosition = transform.position;
        data.worldRotation = transform.rotation;
        data.health = health;
        data.cured = killed;
        data.parent = transform.parent;

        return data;
    }
    public void LoadCheckpoint(LevelManager.CheckpointData.EnemyData data)
    {
        transform.position = data.worldPosition;
        transform.rotation = data.worldRotation;
        transform.parent = data.parent;

        if (!data.cured)
        {
            killed = false;
            GetComponent<Collider>().enabled = true;
            enemyAnimations.SetBool("Cured", false);
            health = data.health;
            state = State.DEFAULT;
            attackSubState = AttackSubState.NOT_ATTACKING;
            searchSubState = SearchSubState.NOT_SEARCHING;
            defaultSubState = DefaultSubState.NOT_DEFAULT;
            audioSource.clip = walkSounds;

            curedEnemies.Remove(this);
            targeted = false;
        }
        else
        {
            killed = true;
        }
    }
    protected IEnumerator DelayRespawn()
    {
        yield return new WaitForSeconds(delayBeforeRespawn);
        StartCoroutine(Respawn());
    }

    override protected IEnumerator Respawn()
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }

        health = maxHealth;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        agent.stoppingDistance = initialStoppingDistance;
        agent.SetDestination(transform.position);
        nextPatrolPoint = 0;
        state = State.DEFAULT;
        defaultSubState = DefaultSubState.NOT_DEFAULT;
        attackSubState = AttackSubState.NOT_ATTACKING;
        searchSubState = SearchSubState.NOT_SEARCHING;
        isStunned = false;
        sprite.color = Color.white;
        canStun = true;
        audioSource.clip = walkSounds;

        yield return new WaitForSeconds(respawnTime);

        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = true;
        }
        GetComponent<Collider>().enabled = true;


        killed = false;


        enemyAnimations.SetBool("Cured", false);
        StopAllCoroutines();
    }

    virtual protected void Update()
    {
        RotateSprite();
        UpdateLayer();
    }

    virtual protected void FixedUpdate()
    {
        previousHealth = health;
        destinationChanged = previousDestination != agent.destination;

        if (damageDonePreviousFixedFrameFrame != 0)
        {
            MakeDamageText();
        }

        if (!(isStunned || killed))
        {
            PerformAILogic();

            if (!playAmbientSounds)
            {
                playAmbientSounds = true;
                Invoke("PlaySoundRepeating", Random.Range(0.5f, 2f));
            }

            if (agent.desiredVelocity.magnitude > .01f)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            else
            {
                audioSource.Stop();
            }
        }
        else
        {
            if (killed)
            {
                //FaceTarget();

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            else
            {
                state = State.DEFAULT;
            }

            agent.SetDestination(transform.position);
        }

        enemyAnimations.SetFloat("WalkSpeed", agent.desiredVelocity.magnitude);

        damageDonePreviousFixedFrameFrame = 0;
        previousDestination = agent.destination;
    }

    virtual public void PlaySoundRepeating()
    {
        if (!playAmbientSounds)
        {
            return;
        }

        if (gameObject.activeInHierarchy)
        {
            audioSource.PlayOneShot(ambientHostile);
        }
        Invoke("PlaySoundRepeating", Random.Range(0.5f, 2f));
    }

    #region AI Methods
    virtual protected void PerformAILogic()
    {
        //Reset stopping distance if enemy is aware of player
        if (state != State.DEFAULT)
        {
            agent.stoppingDistance = initialStoppingDistance;
        }

        //Calculate distance to player
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        //reset location flags
        hasLineOfSight = false;
        inChaseRadius = false;
        inAgroRadius = false;
        inAttackRadius = false;

        //set location flags
        if (distance <= chaseLimitRadius)
        {
            inChaseRadius = true;
            hasLineOfSight = RaycastToPlayer();

            if (distance <= agroRadius && hasLineOfSight)
            {
                inAgroRadius = true;
                if (distance <= attackRaduis)
                {
                    inAttackRadius = true;
                }
            }
        }

        if (PlayerManager.instance.isAlive)
        {
            //Implementation of highest level state machine
            switch (state)
            {
                case State.DEFAULT:
                    //Exit case: Player enters agro radius
                    if (inAgroRadius)
                    {
                        state = State.AGRO;
                        defaultSubState = DefaultSubState.NOT_DEFAULT;
                        PerformAgroState();
                    }
                    else
                    {
                        PerformDefaultState();
                    }
                    break;

                case State.AGRO:
                    //Exit case: Player enters attack radius
                    if (inAttackRadius)
                    {
                        state = State.ATTACKING;
                        PerformAttackState();
                    }
                    //Exit case: Player breaks line of sight or leaves chase radius
                    else if (!hasLineOfSight)
                    {
                        state = State.SEARCHING;
                        PerformSearchingState();
                    }
                    else
                    {
                        PerformAgroState();
                    }
                    break;

                case State.ATTACKING:
                    //Exit Case: enemy finished attacking, return to agro
                    if (attackSubState == AttackSubState.DONE)
                    {
                        state = State.AGRO;
                        attackSubState = AttackSubState.NOT_ATTACKING;
                        PerformAgroState();
                    }
                    else
                    {
                        PerformAttackState();
                    }
                    break;

                case State.SEARCHING:
                    //Exit case: Searching concludes without finding player
                    if (searchSubState == SearchSubState.DONE)
                    {
                        state = State.DEFAULT;
                        searchSubState = SearchSubState.NOT_SEARCHING;
                        PerformDefaultState();
                    }
                    //Exit case: Enemy finds player
                    else if (hasLineOfSight)
                    {
                        state = State.AGRO;
                        searchSubState = SearchSubState.NOT_SEARCHING;
                    }
                    else
                    {
                        PerformSearchingState();
                    }
                    break;

                default:
                    Debug.LogError("Unrecognized State");
                    break;
            }
        }
        else
        {
            searchSubState = SearchSubState.NOT_SEARCHING;
            attackSubState = AttackSubState.NOT_ATTACKING;
            state = State.DEFAULT;
        }
    }

    #region Default State Logic
    virtual protected void PerformDefaultState()
    {
        agent.stoppingDistance = 0.5f;

        switch (defaultStateType)
        {
            //Return to 
            case Enemy.DefaultStateType.STATIONARY:
                PerformDefaultStationary();
                break;

            case Enemy.DefaultStateType.PATROL:
                PerformDefaultPatrol();
                break;

            case Enemy.DefaultStateType.WANDER:
                PerformDefaultWander();
                break;

            default:
                Debug.LogError("Unrecognized Default State Type");
                break;
        }
    }
    virtual protected void PerformDefaultStationary()
    {
        switch (defaultSubState)
        {
            case DefaultSubState.NOT_DEFAULT:
                //Upon entering sub state machine, move to initial position
                agent.SetDestination(initialPosition);
                defaultSubState = DefaultSubState.MOVING_TO_NEXT;
                break;

            case DefaultSubState.AT_POSITION:
                //upon arriving at initial position, begin waiting there
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    defaultSubState = DefaultSubState.WAITING;
                }
                break;

            case DefaultSubState.WAITING:
                //while waiting for player at initial position, face in initial direction
                transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * 5f);
                break;

            case DefaultSubState.MOVING_TO_NEXT:
                if (agent.destination != initialPosition)
                {
                    agent.SetDestination(initialPosition);
                }
                break;

            default:
                Debug.LogError("Unrecognized Default Substate");
                break;
        }
        // if (agent.remainingDistance < agent.stoppingDistance)
        // {
        //     agent.SetDestination(initialPosition);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * 5f);
        // }
    }
    virtual protected void PerformDefaultWander()
    {
        nextPosition = Vector3.zero;
        switch (defaultSubState)
        {
            case DefaultSubState.NOT_DEFAULT:
                //Upon entering sub state machine, move to new random position within wander radius
                nextPosition = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(wanderRaduisMin, wanderRaduisMax);
                agent.SetDestination(initialPosition + nextPosition);
                defaultSubState = DefaultSubState.MOVING_TO_NEXT;
                break;

            case DefaultSubState.AT_POSITION:
                //When upon arriving at position, begin waiting to move to next position
                nextPosition = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(wanderRaduisMin, wanderRaduisMax);
                StartCoroutine(SetDestinationAfter(initialPosition + nextPosition, timeAtPosition));
                defaultSubState = DefaultSubState.WAITING;
                break;

            case DefaultSubState.WAITING:
                //When the destination changes, change state to moving to next
                if (destinationChanged)
                {
                    defaultSubState = DefaultSubState.MOVING_TO_NEXT;
                }
                break;

            case DefaultSubState.MOVING_TO_NEXT:
                //When the enemy arrives, set state to at Position
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    defaultSubState = DefaultSubState.AT_POSITION;
                }
                break;

            default:
                Debug.LogError("Unrecognized Default Substate");
                break;
        }
    }
    virtual protected void PerformDefaultPatrol()
    {
        nextPosition = Vector3.zero;
        switch (defaultSubState)
        {
            case DefaultSubState.NOT_DEFAULT:
                //Upon entering sub state machine, move to new random position within wander radius
                agent.SetDestination(patrolPoints[nextPatrolPoint].position);
                nextPatrolPoint = (nextPatrolPoint + 1) % patrolPoints.Count;
                defaultSubState = DefaultSubState.MOVING_TO_NEXT;
                break;

            case DefaultSubState.AT_POSITION:
                //When upon arriving at position, begin waiting to move to next position
                StartCoroutine(SetDestinationAfter(patrolPoints[nextPatrolPoint].position, timeAtPosition));
                nextPatrolPoint = (nextPatrolPoint + 1) % patrolPoints.Count;
                defaultSubState = DefaultSubState.WAITING;
                break;

            case DefaultSubState.WAITING:
                //When the destination changes, change state to moving to next
                if (destinationChanged)
                {
                    defaultSubState = DefaultSubState.MOVING_TO_NEXT;
                }
                break;

            case DefaultSubState.MOVING_TO_NEXT:
                //When the enemy arrives, set state to at Position
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    defaultSubState = DefaultSubState.AT_POSITION;
                }
                break;

            default:
                Debug.LogError("Unrecognized Default Substate");
                break;
        }
    }
    #endregion

    virtual protected void PerformAgroState()
    {
        agent.SetDestination(playerTransform.position);
    }

    #region Attack State Logic
    virtual protected void PerformAttackState()
    {
        switch (attackSubState)
        {
            case AttackSubState.NOT_ATTACKING:
                float waitTime;

                if (attackTimeType == AttackTimeType.CONSTANT)
                {
                    waitTime = attackTime;
                }
                else
                {
                    waitTime = Random.Range(attackTimeMin, attackTimeMax);
                }

                StartCoroutine(WaitToAttack(waitTime));
                attackSubState = AttackSubState.WAITING;
                break;
            case AttackSubState.WAITING:
                agent.SetDestination(playerTransform.position);
                // **EXITED BY COROUTINE "WaitToAttack"**
                if (!inAttackRadius)
                {
                    StopCoroutine("WaitToAttack");
                    attackSubState = AttackSubState.DONE;
                }
                agent.SetDestination(transform.position);
                break;
            case AttackSubState.ATTACKING:

                // **EXITED BY FUNCTION CALL "HurtPlayer"
                break;
            case AttackSubState.DONE:
                // Currently Inaccessible
                break;
        }
    }

    virtual protected IEnumerator WaitToAttack(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        enemyAnimations.SetTrigger("Attack");
        attackSubState = AttackSubState.ATTACKING;
    }
    //Called by Attack Player animation behavior script
    virtual public bool HurtPlayer()
    {
        attackSubState = AttackSubState.DONE;
        if (isStunned || killed)
        {
            return false;
        }
        //If the player is in view of the enemy and close enough, hirt player
        if (RaycastToPlayer() && Vector3.Distance(playerTransform.position, transform.position) <= attackRaduis)
        {
            PlayerManager.instance.HurtPlayer(attackDamage);
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion
    #region Search State Logic
    virtual protected void PerformSearchingState()
    {
        switch (searchSubState)
        {
            case SearchSubState.NOT_SEARCHING:
                pointsToSearch.Clear();
                agent.SetDestination(playerTransform.position);
                searchSubState = SearchSubState.CHECK_LAST_POS;
                break;

            case SearchSubState.CHECK_LAST_POS:
                //Exit Case: enemy arrives at last known position of player
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    searchSubState = SearchSubState.PICK_SEARCH_POINTS;
                }
                break;

            case SearchSubState.PICK_SEARCH_POINTS:
                //pick three new search points around the enemy's location
                for (int i = 0; i < 3; i++)
                {
                    pointsToSearch.Add(transform.position + (new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * 2.5f));
                }
                searchSubState = SearchSubState.ARRIVED_AT_POINT;
                break;

            case SearchSubState.MOVING_TO_POINT:
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    searchSubState = SearchSubState.ARRIVED_AT_POINT;
                }
                break;

            case SearchSubState.ARRIVED_AT_POINT:
                if (pointsToSearch.Count > 0)
                {
                    StartCoroutine(SetDestinationAfter(pointsToSearch[0], timeAtPosition));
                    pointsToSearch.RemoveAt(0);
                    searchSubState = SearchSubState.STOPPED_AT_POINT;
                }
                else
                {
                    StopCoroutine("SetDestinationAfter");
                    searchSubState = SearchSubState.DONE;
                }
                break;

            case SearchSubState.STOPPED_AT_POINT:
                if (destinationChanged)
                {
                    searchSubState = SearchSubState.MOVING_TO_POINT;
                }
                break;

            case SearchSubState.DONE:
                //Currently Inaccessible
                break;
        }
    }
    #endregion
    virtual protected IEnumerator SetDestinationAfter(Vector3 position, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        agent.SetDestination(position);
    }

    virtual protected bool RaycastToPlayer()
    {
        Vector3 origin = transform.position + new Vector3(0f, agent.height, 0f);
        Vector3 castDestination = CharacterActions.instance.fpsTransform.position;
        RaycastHit hit;
        bool didHit = Physics.Raycast(origin, (castDestination - origin).normalized, out hit, chaseLimitRadius * 1.5f);
        if (didHit)
        {
            return hit.collider.CompareTag("Player");
        }
        else
        {
            return false;
        }
    }

    virtual protected void FaceTarget()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 7.5f);
    }

    #endregion

    #region Health Methods
    override public void Stun()
    {
        if (!canStun)
        {
            return;
        }
        isStunned = true;
        sprite.color = Color.blue;

        canStun = false;
        enemyAnimations.SetBool("Stunned", true);
        StartCoroutine(StunnedCoroutine());
    }

    override public void Hit(int damageHit)
    {
        if (killed)
        {
            return;
        }

        health -= damageHit;

        damageDonePreviousFixedFrameFrame = Mathf.Clamp(damageDonePreviousFixedFrameFrame + damageHit, 0, previousHealth);

        if (health <= 0)
        {

            Kill();
        }
        else if (RaycastToPlayer())
        {
            state = State.AGRO;
        }
    }

    override public void Kill()
    {
        curedEnemies.Add(this);
        targeted = false;

        canStun = false;
        killed = true;
        playAmbientSounds = false;
        new Color(0.7f, 1f, 0.7f);

        enemyAnimations.SetBool("Cured", true);

        GetComponent<Collider>().enabled = false;

        if (respawn)
        {
            StartCoroutine(DelayRespawn());
        }

        audioSource.clip = ambientCured;
    }

    virtual public void Revive()
    {
        curedEnemies.Remove(this);
        targeted = false;

        health = maxHealth;
        agent.stoppingDistance = initialStoppingDistance;
        agent.SetDestination(transform.position);
        nextPatrolPoint = 0;
        state = State.DEFAULT;
        defaultSubState = DefaultSubState.NOT_DEFAULT;
        attackSubState = AttackSubState.NOT_ATTACKING;
        searchSubState = SearchSubState.NOT_SEARCHING;
        isStunned = false;
        sprite.color = Color.white;
        canStun = true;
        audioSource.clip = walkSounds;

        GetComponent<Collider>().enabled = true;


        killed = false;


        enemyAnimations.SetBool("Cured", false);
        StopAllCoroutines();
    }

    public IEnumerator StunnedCoroutine()
    {
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
        sprite.color = new Color(1f, 1f, 1f, .5f);
        enemyAnimations.SetBool("Stunned", false);
        StartCoroutine(StunnedCooldownCoroutine());
    }

    public IEnumerator StunnedCooldownCoroutine()
    {
        yield return new WaitForSeconds(stunCooldown);
        canStun = true;
        sprite.color = Color.white;
    }
    #endregion

    #region Sprite Render Methods
    protected void RotateSprite()
    {
        enemyGFX.transform.eulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.eulerAngles.y,
            z = 0
        };
    }

    protected void UpdateLayer()
    {
        horizPositionDifference.x = CharacterActions.instance.transform.position.x - transform.position.x;
        horizPositionDifference.y = CharacterActions.instance.transform.position.z - transform.position.z;
        horizPositionDifference.Normalize();

        angleInRadians = (transform.eulerAngles.y * Mathf.PI) / 180f;

        rotatedPositionDifference.x = Mathf.Cos(angleInRadians) * horizPositionDifference.x - Mathf.Sin(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.y = Mathf.Sin(angleInRadians) * horizPositionDifference.x + Mathf.Cos(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.Normalize();

        angleToPlayer = Vector2.SignedAngle(forward, rotatedPositionDifference);

        if (angleToPlayer >= -157.5f && angleToPlayer < -112.5f)
        {
            activeLayerIndex = 0;
        }
        else if (angleToPlayer >= -112.5f && angleToPlayer < -67.5f)
        {
            activeLayerIndex = 1;
        }
        else if (angleToPlayer >= -67.5f && angleToPlayer < -22.5f)
        {
            activeLayerIndex = 2;
        }
        else if (angleToPlayer >= -22.5f && angleToPlayer < 22.5f)
        {
            activeLayerIndex = 3;
        }
        else if (angleToPlayer >= 22.5f && angleToPlayer < 67.5f)
        {
            activeLayerIndex = 4;
        }
        else if (angleToPlayer >= 67.5f && angleToPlayer < 112.5f)
        {
            activeLayerIndex = 5;
        }
        else if (angleToPlayer >= 112.5f && angleToPlayer < 157.5f)
        {
            activeLayerIndex = 6;
        }
        else
        {
            activeLayerIndex = 7;
        }

        if (previousLayerIndex != activeLayerIndex)
        {
            previousLayerIndex = activeLayerIndex;

            for (int i = 0; i < enemyAnimations.layerCount; i++)
            {
                if (i == activeLayerIndex)
                {
                    enemyAnimations.SetLayerWeight(i, 1);
                }
                else
                {
                    enemyAnimations.SetLayerWeight(i, 0);
                }
            }
        }

    }

    protected void MakeDamageText()
    {
        DamageText dt = damageDoneTexts[nextDamageText].GetComponent<DamageText>();

        if (damageDoneTexts[nextDamageText].activeInHierarchy == true)
        {
            dt.EndAscent();
        }

        damageDoneTexts[nextDamageText].transform.localPosition = new Vector3(0f, damageTextVerticalOffset, 0f);

        string txt = "-" + damageDonePreviousFixedFrameFrame.ToString();

        dt.text.text = txt;

        if (health == 0)
        {
            dt.text.color = finalHitTextColor;
        }
        else
        {
            dt.text.color = damageTextColor;
        }

        damageDoneTexts[nextDamageText].SetActive(true);
        dt.BeginAscent();

        nextDamageText = (nextDamageText + 1) % damageDoneTexts.Length;
    }

    #endregion
    virtual protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseLimitRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRaduis);

        Gizmos.color = Color.magenta;
        switch (defaultStateType)
        {
            case Enemy.DefaultStateType.STATIONARY:
                if (Application.isPlaying)
                {
                    Gizmos.DrawSphere(initialPosition, 0.2f);
                }
                else
                {
                    Gizmos.DrawSphere(transform.position, 0.2f);
                }

                break;
            case Enemy.DefaultStateType.PATROL:
                Vector3 cubeSize = new Vector3(0.2f, 0.2f, 0.2f);
                for (int i = 0; i < patrolPoints.Count; i++)
                {
                    if (patrolPoints[i] == null)
                    {
                        continue;
                    }
                    Gizmos.DrawCube(patrolPoints[i].position, cubeSize);
                    if (patrolPoints[(i + 1) % patrolPoints.Count] == null)
                    {
                        continue;
                    }
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[(i + 1) % patrolPoints.Count].position);
                }

                break;

            case Enemy.DefaultStateType.WANDER:
                if (Application.isPlaying)
                {
                    Gizmos.DrawWireSphere(initialPosition, wanderRaduisMax);
                    Gizmos.DrawWireSphere(initialPosition, wanderRaduisMin);
                }
                else
                {
                    Gizmos.DrawWireSphere(transform.position, wanderRaduisMax);
                    Gizmos.DrawWireSphere(transform.position, wanderRaduisMin);
                }
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }

        if (agent != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(agent.destination, 0.3f);
        }
    }

    #endregion
}

