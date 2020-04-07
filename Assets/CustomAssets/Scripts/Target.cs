using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int maxHealth = 10;
    public int health;
    private void Start()
    {
        health = maxHealth;
    }

    public void Hit(int damageHit)
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

    void Kill()
    {
        Destroy(gameObject);
    }
}
