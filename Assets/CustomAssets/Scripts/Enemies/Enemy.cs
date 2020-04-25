using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Target
{
    public GameObject enemyQuad;
    public Animator enemyAnimations;
    public float rotationSpeed;
    public float walkSpeed;

    public float angleToPlayer { get; protected set; }
    private float angleInRadians = 0f;
    private Vector2 horizPositionDifference;
    private Vector2 rotatedPositionDifference;
    private Vector2 forward = new Vector2(1f, 0f);
    private int activeLayerIndex = 2;
    private int previousLayerIndex = 2;

    private void FixedUpdate()
    {
        transform.localEulerAngles += new Vector3(0f, rotationSpeed * Time.deltaTime, 0f);
        transform.position += transform.forward * walkSpeed * Time.deltaTime;
    }
    private void Update()
    {
        enemyQuad.transform.eulerAngles = new Vector3
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
}
