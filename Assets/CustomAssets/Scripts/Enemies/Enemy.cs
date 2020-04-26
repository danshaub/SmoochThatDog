using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Target
{
    #region AnimationVariables
    public GameObject enemyGFX;
    public Animator enemyAnimations;
    public float angleToPlayer { get; protected set; }
    private float angleInRadians = 0f;
    private Vector2 horizPositionDifference;
    private Vector2 rotatedPositionDifference;
    private Vector2 forward = new Vector2(0f, 1f);
    private int activeLayerIndex = 2;
    private int previousLayerIndex = 2;
    #endregion

    #region AI Variables
    public float chaseLimitRadius = 10f;
    public float agroRadius = 7.5f;
    public float attackRaduis = 5f;
    Transform target;
    NavMeshAgent agent;

    private bool isAware;

    #endregion

    private void Start()
    {
        health = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        target = PlayerManager.instance.transform;
    }
    private void FixedUpdate()
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (!(isStunned || killed))
        {
            if (distance <= attackRaduis)
            {
                agent.SetDestination(target.position);
                AttackPlayer();
            }
            if (distance <= agroRadius)
            {
                isAware = true;
                agent.SetDestination(target.position);
            }
            else if (distance <= chaseLimitRadius && isAware)
            {
                agent.SetDestination(target.position);
            }
            else
            {
                isAware = false;
            }

            if (distance <= agent.stoppingDistance)
            {
                FaceTarget();
            }
        }
        else
        {
            isAware = false;
            agent.SetDestination(transform.position);
        }


        enemyAnimations.SetFloat("WalkSpeed", agent.desiredVelocity.magnitude);
    }

    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    virtual
    public void AttackPlayer()
    {

    }

    #region Overrides

    override
    public void Stun()
    {
        Debug.Log("In Stun()");
        if (!canStun)
        {
            return;
        }
        isStunned = true;
        canStun = false;
        enemyAnimations.SetBool("Stunned", true);
        StartCoroutine(StunnedCoroutine());
    }

    override
    public void Kill()
    {
        canStun = false;
        killed = true;

        enemyAnimations.SetBool("Cured", true);
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

    #region Sprite Renderer
    private void Update()
    {
        enemyGFX.transform.eulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.eulerAngles.y,
            z = 0
        };

        horizPositionDifference.x = CharacterActions.instance.transform.position.x - transform.position.x;
        horizPositionDifference.y = CharacterActions.instance.transform.position.z - transform.position.z;
        horizPositionDifference.Normalize();

        angleInRadians = (transform.eulerAngles.y * Mathf.PI) / 180f;

        rotatedPositionDifference.x = Mathf.Cos(angleInRadians) * horizPositionDifference.x - Mathf.Sin(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.y = Mathf.Sin(angleInRadians) * horizPositionDifference.x + Mathf.Cos(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.Normalize();

        angleToPlayer = Vector2.SignedAngle(forward, rotatedPositionDifference);

        if (angleToPlayer >= -150f && angleToPlayer < -90f)
        {
            activeLayerIndex = 0;
        }
        else if (angleToPlayer >= -90f && angleToPlayer < -30f)
        {
            activeLayerIndex = 1;
        }
        else if (angleToPlayer >= -30f && angleToPlayer < 30f)
        {
            activeLayerIndex = 2;
        }
        else if (angleToPlayer >= 30f && angleToPlayer < 90f)
        {
            activeLayerIndex = 3;
        }
        else if (angleToPlayer >= 90f && angleToPlayer < 150f)
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
            UpdateLayer();
        }
    }

    public void UpdateLayer()
    {
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

    #endregion
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseLimitRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, agroRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRaduis);

    }
}

