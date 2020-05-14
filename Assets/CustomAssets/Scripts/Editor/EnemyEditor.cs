using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(Enemy))]
public class EnemyEditor : TargetEditor
{
    bool showPatrolPositions = true;
    int patrolPositionCount = 0;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Target Variables", GUI.skin.label);

        base.OnInspectorGUI();

        Enemy enemy = target as Enemy;

        EditorGUILayout.LabelField("Animation Variables", GUI.skin.label);
        enemy.enemyGFX = (GameObject)EditorGUILayout.ObjectField("Enemy GFX", enemy.enemyGFX, typeof(GameObject), true);
        enemy.enemyAnimations = (Animator)EditorGUILayout.ObjectField("Enemy Animations", enemy.enemyAnimations, typeof(Animator), true);
        enemy.damageDoneTextPrefab = (GameObject)EditorGUILayout.ObjectField("Damage Done Text Prefab", enemy.damageDoneTextPrefab, typeof(GameObject), true);
        enemy.damageTextVerticalOffset = EditorGUILayout.FloatField("Damage Text Vertical Offset", enemy.damageTextVerticalOffset);
        enemy.damageTextColor = EditorGUILayout.ColorField("Damage Text Color", enemy.damageTextColor);
        enemy.finalHitTextColor = EditorGUILayout.ColorField("Final Hit Color", enemy.finalHitTextColor);

        EditorGUILayout.LabelField("AI Variables", GUI.skin.label);
        enemy.chaseLimitRadius = EditorGUILayout.FloatField("Chase Limit Radius", enemy.chaseLimitRadius);
        enemy.agroRadius = EditorGUILayout.FloatField("Agro Radius", enemy.agroRadius);
        enemy.attackRaduis = EditorGUILayout.FloatField("Attack Radius", enemy.attackRaduis);
        enemy.attackDamage = EditorGUILayout.IntField("Attack Damage", enemy.attackDamage);

        enemy.attackTimeType = (Enemy.AttackTimeType)EditorGUILayout.EnumPopup("Attack Time Type", enemy.attackTimeType);
        EditorGUI.indentLevel = 1;

        switch (enemy.attackTimeType)
        {
            case Enemy.AttackTimeType.CONSTANT:
                enemy.attackTime = EditorGUILayout.FloatField("Attack Time", enemy.attackTime);
                break;
            case Enemy.AttackTimeType.RANDOM_BETWEEN_CONSTANTS:
                enemy.attackTimeMin = EditorGUILayout.FloatField("Min Attack Time", enemy.attackTimeMin);
                enemy.attackTimeMax = EditorGUILayout.FloatField("Max Attack Time", enemy.attackTimeMax);
                break;
            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
        EditorGUI.indentLevel = 0;

        enemy.defaultStateType = (Enemy.DefaultStateType)EditorGUILayout.EnumPopup("Default State", enemy.defaultStateType);

        EditorGUI.indentLevel = 1;
        switch (enemy.defaultStateType)
        {
            case Enemy.DefaultStateType.STATIONARY:
                break;
            case Enemy.DefaultStateType.PATROL:
                showPatrolPositions = EditorGUILayout.Foldout(showPatrolPositions, "Patrol Points");
                EditorGUI.indentLevel = 2;

                if (showPatrolPositions)
                {
                    patrolPositionCount = enemy.patrolPoints.Count;
                    patrolPositionCount = EditorGUILayout.IntField("Size", patrolPositionCount);
                    if (patrolPositionCount >= 0)
                    {
                        List<Transform> tempList = new List<Transform>();
                        for (int i = 0; i < patrolPositionCount; i++)
                        {
                            if (i < enemy.patrolPoints.Count)
                            {
                                tempList.Add(enemy.patrolPoints[i]);
                            }
                            else
                            {
                                tempList.Add(null);
                            }
                        }

                        enemy.patrolPoints = tempList;

                        for (int i = 0; i < enemy.patrolPoints.Count; i++)
                        {
                            enemy.patrolPoints[i] = (Transform)EditorGUILayout.ObjectField("Element " + i.ToString(), enemy.patrolPoints[i], typeof(Transform), true);
                        }

                    }
                    else
                    {
                        Debug.LogError("Size must be non-negative");
                        patrolPositionCount = enemy.patrolPoints.Count;
                    }
                }
                EditorGUI.indentLevel = 1;



                break;

            case Enemy.DefaultStateType.WANDER:
                enemy.wanderRaduisMax = EditorGUILayout.FloatField("Max Wander Radius", enemy.wanderRaduisMax);
                enemy.wanderRaduisMin = EditorGUILayout.Slider("Min Wander Radius", enemy.wanderRaduisMin, 0f, enemy.wanderRaduisMax);

                break;

            default:
                Debug.LogError("Unrecognized Option");
                break;
        }
        enemy.timeAtPosition = EditorGUILayout.FloatField("Time At Position", enemy.timeAtPosition);
        EditorGUI.indentLevel = 0;
    }
}
