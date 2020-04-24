﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int maxHealth = 10;
    public int health;
    public int rageFill;
    public bool respawn = false;
    public float respawnTime = 0f;
    public bool isStunned { get; protected set; }
    public bool killed { get; protected set; }
    private void Start()
    {
        health = maxHealth;
    }

    virtual public void Stun()
    {

    }

    virtual public void Hit(int damageHit)
    {
        if (killed)
        {
            return;
        }

        health -= damageHit;

        if(health <= 0)
        {
            killed = true;
            Kill();
        }
    }

    public void Heal(int healthHealed)
    {
        health = (int)Mathf.Clamp(health + healthHealed, 0f, maxHealth);
    }

    virtual public void Kill()
    {
        if(PlayerManager.instance.currentGunIndex == 0)
        {

            ((Smooch)PlayerManager.instance.CurrentGun()).RemoveFromTargetList(gameObject);
        }
        if (respawn)
        {
            StartCoroutine(Respawn());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Respawn()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(respawnTime);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        health = maxHealth;
    }
}
