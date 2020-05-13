﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemy : Enemy
{
    public enum AgroSubState
    {
        CHASING,
        SHOOTING,
        NOT_AGRO
    }

    public AgroSubState agroSubState;
    public GameObject projectilePrefab;

    public float projectileSpeed;
    public int projectileDamage;
    protected GameObject[] projectiles;
    protected int nextProjectile = 0;
    protected override void Start()
    {
        base.Start();
        projectiles = new GameObject[5];
        for (int i = 0; i < projectiles.Length; i++)
        {
            projectiles[i] = Instantiate(projectilePrefab);
            projectiles[i].GetComponent<Projectile>().speed = projectileSpeed;
            projectiles[i].GetComponent<Projectile>().damage = projectileDamage;
            projectiles[i].SetActive(false);
        }
    }

    override protected void PerformAILogic()
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
                    if (inChaseRadius)
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

    override protected void PerformAgroState()
    {
        switch (agroSubState)
        {
            case AgroSubState.NOT_AGRO:

                break;
            case AgroSubState.CHASING:

                break;

            case AgroSubState.SHOOTING:

                break;
            default:
                Debug.LogError("Unrecognized Agro State");
                break;
        }
    }

    override protected void PerformAttackState()
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

    override public bool HurtPlayer()
    {
        attackSubState = AttackSubState.DONE;
        if (isStunned || killed)
        {
            return false;
        }
        //If the player is in view of the enemy and close enough, hurt player
        if (RaycastToPlayer())
        {
            if (Vector3.Distance(playerTransform.position, transform.position) <= attackRaduis && false)
            {
                PlayerManager.instance.HurtPlayer(attackDamage);
                return true;
            }
            else
            {
                projectiles[nextProjectile].SetActive(true);
                projectiles[nextProjectile].transform.position = transform.position + new Vector3(0f, 0.75f, 0f);
                projectiles[nextProjectile].transform.rotation = transform.rotation;
                return true;
            }
        }
        else
        {
            return false;
        }
    }
}
