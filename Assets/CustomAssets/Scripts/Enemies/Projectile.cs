using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject particles;
    Rigidbody rb;
    [HideInInspector] public float speed = 1f;
    [HideInInspector] public int damage = 0;

    bool flying = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (flying)
        {
            rb.MovePosition(transform.position + transform.forward * speed * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name + "  " + other.gameObject.layer.ToString());
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.HurtPlayer(damage);
        }

        StartCoroutine(OnHit());
    }

    public IEnumerator OnHit()
    {
        flying = false;

        GetComponent<MeshRenderer>().enabled = false;

        particles.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(particles.GetComponent<ParticleSystem>().main.startLifetime.constant);

        GetComponent<MeshRenderer>().enabled = true;

        flying = true;

        gameObject.SetActive(false);
    }
}
