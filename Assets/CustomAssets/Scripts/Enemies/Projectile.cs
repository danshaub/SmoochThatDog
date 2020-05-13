using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject particlePrefab;
    private GameObject particles;
    [HideInInspector] public float speed;
    [HideInInspector] public int damage;

    private void Start()
    {
        particles = Instantiate(particlePrefab, transform);
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.HurtPlayer(damage);
        }

        gameObject.SetActive(false);
    }
}
