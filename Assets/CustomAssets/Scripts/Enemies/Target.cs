using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int maxHealth = 10;
    public int health;
    public int rageFill;
    private void Start()
    {
        health = maxHealth;
    }

    virtual public void Stun()
    {
        Debug.Log("Stunned");
    }

    virtual public void Hit(int damageHit)
    {
        health -= damageHit;

        if(health <= 0)
        {
            Kill();
        }
    }

    public void Heal(int healthHealed)
    {
        health = (int)Mathf.Clamp(health + healthHealed, 0f, maxHealth);
    }

    virtual public void Kill()
    {
        Destroy(gameObject);
    }
}
