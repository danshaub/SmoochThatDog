﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Door))]
public class EnemyGate : MonoBehaviour, ITriggerableObject
{
    private Door door;
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public GameObject enemyParent;

    private bool triggered = false;
    private Enemy[] enemies;

    private void Start()
    {
        door = GetComponent<Door>();
    }
    private void Update()
    {
        if (triggered)
        {
            foreach (Enemy enemy in enemies)
            {
                if (!enemy.killed)
                {
                    return;
                }
            }

            Unlock();
        }
    }

    public void Trigger()
    {
        enemies = enemyParent.GetComponentsInChildren<Enemy>();
        triggered = true;

        door.Close();
        door.locked = true;

        foreach (GameObject go in door.doors)
        {
            Material[] mats = go.GetComponent<Renderer>().materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].color.Equals(unlockedMaterial.color))
                {
                    mats[i] = lockedMaterial;
                }
            }

            go.GetComponent<Renderer>().materials = mats;
        }
    }

    public void Unlock()
    {
        triggered = false;
        door.locked = false;

        if (!door.interactable)
        {
            door.Open();
        }
        

        foreach (GameObject go in door.doors)
        {
            Material[] mats = go.GetComponent<Renderer>().materials;
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].color.Equals(lockedMaterial.color))
                {
                    mats[i] = unlockedMaterial;
                }
            }

            go.GetComponent<Renderer>().materials = mats;
        }
    }
}
