using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Target
{
    [System.Serializable]
    public enum DefaultStateType
    {
        Stationary,
        Partol,
        Wander
    }

    [System.Serializable]
    public enum AttackTimeType
    {
        Constant,
        RandomBetweenTwoConstants
    }

    #region AnimationVariables
    [Header("Animation Variables")]
    public GameObject enemyGFX;
    public Animator enemyAnimations;
    public GameObject damageDoneTextPrefab;
    public float damageTextVerticalOffset;
    public Color damageTextColor;
    public Color finalHitTextColor;

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
    public DefaultStateType defaultState;
    public List<Transform> patrolPoints;
    public float timeAtPosition;
    public float wanderRaduisMax;
    public float wanderRaduisMin;

    //Hidden Variables
    Transform target;
    NavMeshAgent agent;
    private bool isAware;
    private bool lostSight;
    private bool lookingForTarget;
    private bool nextLookingPlaceSet;
    private int lookingPlacesRemaining;
    private List<Vector3> lookingPlaces;
    [HideInInspector] public bool attacking = false;
    Vector3 initialPosition;
    Quaternion initialRotation;
    int nextPatrolPoint = 0;
    float initialStoppingDistance;
    bool settingPosition;
    bool canSetPosition = true;
    #endregion

    #region Methods
    private void Start()
    {
        health = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.transform;
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
        isAware = false;
        canSetPosition = true;
        settingPosition = false;
        attacking = false;
        isStunned = false;
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
            isAware = false;
            agent.SetDestination(transform.position);
        }

        enemyAnimations.SetFloat("WalkSpeed", agent.desiredVelocity.magnitude);
        damageDonePreviousFixedFrameFrame = 0;
    }

    #region AI Methods
    virtual protected void PerformAILogic()
    {
        //Reset stopping distance if enemy is aware of player
        if (isAware)
        {
            agent.stoppingDistance = initialStoppingDistance;
        }

        //Calculate distance to player
        float distance = Vector3.Distance(target.position, transform.position);
        bool hasLineOfSight = RaycastToPlayer();
        if (PlayerManager.instance.isAlive)
        {
            //Attack if close enough to attack
            if (distance <= attackRaduis && (isAware || hasLineOfSight))
            {
                //Debug.Log("Attacking");
                agent.SetDestination(target.position);
                if (!attacking)
                {
                    float attackTime = 0;
                    switch (attackTimeType)
                    {
                        case AttackTimeType.Constant:
                            attackTime = this.attackTime;
                            break;
                        case AttackTimeType.RandomBetweenTwoConstants:
                            attackTime = Random.Range(attackTimeMin, attackTimeMin);
                            break;
                        default:
                            Debug.LogError("Unknown condition");
                            break;
                    }
                    StartCoroutine(AttackPlayer(attackTime));
                }
            }
            //Chase player if close enough to gain agro
            else if (distance <= agroRadius && hasLineOfSight)
            {
                //Debug.Log("Agro");
                isAware = true;
                agent.SetDestination(target.position);
            }
            //Chase player if close enough to chase and is aware
            else if (distance <= chaseLimitRadius && isAware && hasLineOfSight)
            {
                //Debug.Log("Chasing");
                agent.SetDestination(target.position);
            }
            //If the enemy was aware the previous frame, it has now lost sight
            else if (isAware)
            {
                //Debug.Log("Lost Sight");
                lostSight = true;
                isAware = false;
            }
            else if (lostSight)
            {
                //Debug.Log("Picking positions to look");
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    lookingForTarget = true;
                    lostSight = false;
                    lookingPlacesRemaining = 3;
                    lookingPlaces = new List<Vector3>();
                    for (int i = 0; i < lookingPlacesRemaining; i++)
                    {
                        if (i == 0)
                        {
                            lookingPlaces.Add(transform.position + (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(2, 4)));
                        }
                        else
                        {
                            lookingPlaces.Add(lookingPlaces[i - 1] + (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized * Random.Range(2, 4)));
                        }
                    }
                    nextLookingPlaceSet = false;
                }
            }
            else if (lookingForTarget)
            {
                //Debug.Log("Looking for target");
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    if (!nextLookingPlaceSet && lookingPlacesRemaining > 0)
                    {
                        StartCoroutine(LookForTarget());
                    }
                    else if (lookingPlacesRemaining == 0)
                    {
                        StartCoroutine(FinishLooking());
                    }
                }
            }
            //otherwise, return to default state
            else
            {
                PerformDefaultState();
            }

            //Ensure facing target even if too close to move towards player
            if (distance <= agent.stoppingDistance)
            {
                FaceTarget();

            }
        }
        else
        {
            PerformDefaultState();
        }
    }

    protected void PerformDefaultState()
    {
        //Debug.Log("Default State");
        agent.stoppingDistance = 0.5f;

        switch (defaultState)
        {
            //Return to 
            case Enemy.DefaultStateType.Stationary:

                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    agent.SetDestination(initialPosition);
                    transform.rotation = Quaternion.Slerp(transform.rotation, initialRotation, Time.deltaTime * 5f);
                }
                break;
            case Enemy.DefaultStateType.Partol:
                if (canSetPosition && agent.remainingDistance < agent.stoppingDistance)
                {
                    if (!settingPosition)
                    {
                        //Debug.Log("Setting position");
                        settingPosition = true;
                        canSetPosition = false;
                        StartCoroutine(NextPatrolPosition());
                    }
                }
                break;

            case Enemy.DefaultStateType.Wander:
                if (agent.remainingDistance < agent.stoppingDistance)
                {
                    if (!settingPosition)
                    {

                        settingPosition = true;
                        StartCoroutine(NextWanderPosition());
                    }
                }
                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
    }

    protected IEnumerator LookForTarget()
    {
        //Debug.LogWarning("In set coroutine");
        nextLookingPlaceSet = true;
        yield return new WaitForSeconds(timeAtPosition);
        //Debug.LogWarning("Exiting set coroutine");
        agent.SetDestination(lookingPlaces[--lookingPlacesRemaining]);
        StartCoroutine(DoneSettingLookingPlace());

    }

    protected IEnumerator DoneSettingLookingPlace()
    {
        yield return new WaitForSeconds(.5f);
        nextLookingPlaceSet = false;
    }

    protected IEnumerator FinishLooking()
    {
        yield return new WaitForSeconds(timeAtPosition);
        lookingForTarget = false;
    }

    protected IEnumerator PositionSetCooldown()
    {
        yield return new WaitForSeconds(1);
        canSetPosition = true;
    }

    protected IEnumerator NextPatrolPosition()
    {
        yield return new WaitForSeconds(timeAtPosition);

        agent.SetDestination(patrolPoints[nextPatrolPoint].position);
        nextPatrolPoint = (nextPatrolPoint + 1) % patrolPoints.Count;

        settingPosition = false;
        StartCoroutine(PositionSetCooldown());
    }

    protected IEnumerator NextWanderPosition()
    {
        yield return new WaitForSeconds(timeAtPosition);

        Vector3 newPosition = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized * Random.Range(wanderRaduisMin, wanderRaduisMax);
        agent.SetDestination(initialPosition + newPosition);

        settingPosition = false;
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
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 7.5f);
    }

    virtual protected IEnumerator AttackPlayer(float attackInSeconds)
    {
        attacking = true;
        yield return new WaitForSeconds(attackInSeconds);
        enemyAnimations.SetTrigger("Attack");
    }

    //Called by Attack Player animation behavior script
    virtual public bool HurtPlayer()
    {
        if (Vector3.Distance(target.position, transform.position) <= attackRaduis)
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

    #region Health Methods

    override public void Stun()
    {
        if (!canStun)
        {
            return;
        }
        isStunned = true;
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
            killed = true;
            Kill();
        }
        else
        {
            isAware = true;
        }
    }

    override public void Kill()
    {
        canStun = false;
        killed = true;

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
        enemyAnimations.SetBool("Stunned", false);
        StartCoroutine(StunnedCooldownCoroutine());
    }

    public IEnumerator StunnedCooldownCoroutine()
    {
        yield return new WaitForSeconds(stunCooldown);
        canStun = true;
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
        switch (defaultState)
        {
            case Enemy.DefaultStateType.Stationary:
                if (Application.isPlaying)
                {
                    Gizmos.DrawSphere(initialPosition, 0.2f);
                }
                else
                {
                    Gizmos.DrawSphere(transform.position, 0.2f);
                }

                break;
            case Enemy.DefaultStateType.Partol:
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

            case Enemy.DefaultStateType.Wander:
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

