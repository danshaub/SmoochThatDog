﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncFaceMovement : StateMachineBehaviour
{
    public static float animationTime = -1;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(SyncIdleGun.animationTime != -1)
        {
            animator.Play(0, layerIndex, SyncIdleGun.animationTime);
        }
        else if (SyncIdleSmooch.animationTime != -1)
        {
            animator.Play(0, layerIndex, SyncIdleSmooch.animationTime);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animationTime = stateInfo.normalizedTime - (int)stateInfo.normalizedTime;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animationTime = -1;
    }
}
