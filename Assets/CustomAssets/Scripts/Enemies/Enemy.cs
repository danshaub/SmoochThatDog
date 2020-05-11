using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Target
{
    #region Enums
    [System.Serializable]
    public enum State
    {
        DEFAULT,
        AGRO,
        ATTACKING,
        SEARCHING
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
        Constant,
        RandomBetweenTwoConstants
    }
    #endregion

    #region AnimationVariables
    [Header("Animation Variables")]
    public GameObject enemyGFX;
    public Animator enemyAnimations;
    public GameObject damageDoneTextPrefab;
    public float damageTextVerticalOffset;
    public Color damageTextColor;
    public Color finalHitTextColor;
    public SpriteRenderer sprite;

    //Hidden Variables
    public float angleToPlayer { get; protected set; }
    private float angleInRadians = 0f;
    private Vector2 horizPositionDifference;
    private Vector2 rotatedPositionDifference;
    private Vector2 forward = new Vector2(0f, 1f);
    private int activeLayerIndex = 2;
    private int previousLayerIndex = 2;
    private int damageDonePreviousFixedFrameFrame;
    private int previousHealth;

    private GameObject[] damageDoneTexts;
    private int nextDamageText = 0;
    #endregion

    #region AI Variables
    [Header("AI Variables")]
    public float chaseLimitRadius = 10f;
    public float agroRadius = 7.5f;
    public float attackRaduis = 5f;
    public int attackDamage = 1;
    public AttackTimeType attackTimeType;
    public float attackTime = 1f;
    public float attackTimeMin = 1f;
    public float attackTimeMax = 2f;
    public DefaultStateType defaultStateType;
    public List<Transform> patrolPoints;
    public float timeAtPosition;
    public float wanderRaduisMax;
    public float wanderRaduisMin;

    #region Internal Variables
    //States
    public State state;
    public DefaultSubState defaultSubState;
    public SearchSubState searchSubState;
    public AttackSubState attackSubState;
    //Location Flags
    bool hasLineOfSight = false;
    bool inChaseRadius = false;
    bool inAgroRadius = false;
    bool inAttackRadius = false;
    Vector3 nextPosition = Vector3.zero;
    Transform playerTransform;
    NavMeshAgent agent;
    bool destinationChanged;
    Vector3 previousDestination;
    private List<Vector3> pointsToSearch = new List<Vector3>();
    Vector3 initialPosition;
    Quaternion initialRotation;
    int nextPatrolPoint = 0;
    float initialStoppingDistance;

    #endregion
    #endregion
    #region Sounds

    public AudioSource audioSource;
    public AudioClip ambientHostile;
    public AudioClip ambientCured;
    public AudioClip attackSound;

    #endregion
    #region Methods
    private void Start()
    {
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

    private void Update()
    {
        RotateSprite();
        UpdateLayer();
    }

    private void FixedUpdate()
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
        }
        else
        {
            if (killed)
            {
                FaceTarget();
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

            if (distance <= agroRadius)
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
    private void PerformDefaultState()
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
    private void PerformDefaultStationary()
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
    private void PerformDefaultWander()
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
    private void PerformDefaultPatrol()
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

    protected void PerformAgroState()
    {
        agent.SetDestination(playerTransform.position);
    }

    #region Attack State Logic
    protected void PerformAttackState()
    {
        switch (attackSubState)
        {
            case AttackSubState.NOT_ATTACKING:
                float waitTime;

                if (attackTimeType == AttackTimeType.Constant)
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
    protected void PerformSearchingState()
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
    protected IEnumerator SetDestinationAfter(Vector3 position, float seconds)
    {
        Debug.Log("In set position after");
        yield return new WaitForSeconds(seconds);
        agent.SetDestination(position);
    }

    protected bool RaycastToPlayer()
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

    protected void FaceTarget()
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
        canStun = false;
        killed = true;
        new Color(0.7f, 1f, 0.7f);

        enemyAnimations.SetBool("Cured", true);

        GetComponent<Collider>().enabled = false;

        if (respawn)
        {
            StartCoroutine(DelayRespawn());
        }
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

        if (angleToPlayer >= -180f && angleToPlayer < -120f)
        {
            activeLayerIndex = 0;
        }
        else if (angleToPlayer >= -120f && angleToPlayer < -60f)
        {
            activeLayerIndex = 1;
        }
        else if (angleToPlayer >= -60f && angleToPlayer < 0f)
        {
            activeLayerIndex = 2;
        }
        else if (angleToPlayer >= 0f && angleToPlayer < 60f)
        {
            activeLayerIndex = 3;
        }
        else if (angleToPlayer >= 60f && angleToPlayer < 120f)
        {
            activeLayerIndex = 4;
        }
        else
        {
            activeLayerIndex = 5;
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

