using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerEnemy : RangedEnemy
{
    public enum HealingSubState
    {
        NOT_HEALING,
        MOVING_TO_TARGET,
        HEALING_TARGET,
        DONE
    }

    [Header("Healer Enemy Variables")]
    [Range(0f, 100f)]
    public float healSearchRadius = 25f;

    [HideInInspector] public HealingSubState healingSubState;
    [HideInInspector] public Enemy target;
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
                    if (SearchForHealTarget())
                    {
                        state = State.HEALING;
                        defaultSubState = DefaultSubState.NOT_DEFAULT;
                    }
                    else
                    {
                        PerformDefaultState();
                    }
                    break;

                case State.AGRO:
                    //Exit case: Player enters attack radius
                    if (inAttackRadius || shooting)
                    {
                        state = State.ATTACKING;
                        PerformAttackState();
                    }
                    //Exit case: Player breaks line of sight or leaves chase radius
                    else if (!hasLineOfSight)
                    {
                        state = State.DEFAULT;

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
                    if (!hasLineOfSight)
                    {
                        state = State.DEFAULT;
                        attackSubState = AttackSubState.NOT_ATTACKING;
                    }
                    else
                    {
                        PerformAttackState();
                    }
                    break;

                case State.HEALING:
                    if (inChaseRadius)
                    {
                        state = State.AGRO;
                        healingSubState = HealingSubState.NOT_HEALING;
                        target.targeted = false;
                        target = null;
                        PerformAgroState();
                    }
                    else if (!target.killed || healingSubState == HealingSubState.DONE)
                    {
                        state = State.DEFAULT;
                        healingSubState = HealingSubState.NOT_HEALING;
                        target.targeted = false;
                        target = null;
                        PerformDefaultState();
                    }
                    else
                    {
                        PerformHealState();
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



    protected void PerformHealState()
    {
        switch (healingSubState)
        {
            case HealingSubState.NOT_HEALING:
                agent.SetDestination(target.transform.position);
                healingSubState = HealingSubState.MOVING_TO_TARGET;
                break;

            case HealingSubState.MOVING_TO_TARGET:
                agent.SetDestination(target.transform.position);
                if (agent.remainingDistance < attackRaduis)
                {
                    healingSubState = HealingSubState.HEALING_TARGET;
                    StartCoroutine(HealTarget());
                }
                break;

            case HealingSubState.HEALING_TARGET:
                //EXIT CONDITION PROVIDED BY COROUTINE HealTarget()
                agent.SetDestination(transform.position);
                break;

            case HealingSubState.DONE:
                //No Logic Necessary
                break;

            default:
                Debug.LogError("Unrecognized Healing State");
                break;
        }
        
    }

    protected IEnumerator HealTarget()
    {
        yield return new WaitForSeconds(3f);

        if(target != null)
        {
            target.Revive();
            healingSubState = HealingSubState.DONE;
        }
    }

    protected bool SearchForHealTarget()
    {
        bool success = false;
        foreach(Enemy enemy in Enemy.curedEnemies)
        {
            float distance = Vector3.Distance(enemy.gameObject.transform.position, gameObject.transform.position);

            if(distance <= healSearchRadius && !enemy.targeted)
            {
                success = true;

                if(target == null || distance < Vector3.Distance(target.gameObject.transform.position, gameObject.transform.position))
                {
                    if(target != null)
                    {
                        target.targeted = false;
                    }
                    target = enemy;
                    target.targeted = true;
                }
            }
        }

        return success;
    }

    public override void Revive()
    {
        base.Revive();

        healingSubState = HealingSubState.NOT_HEALING;
        if(target != null)
        {
            target = null;
        }
        
    }

    public override void Kill()
    {
        base.Kill();

        

        if (target != null)
        {
            target.targeted = false;
            target = null;
        }

    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);

        Gizmos.DrawWireSphere(transform.position, healSearchRadius);

        if (target != null)
        {
            

            Gizmos.DrawWireCube(target.transform.position + Vector3.up, Vector3.one);
        }
    }
}
