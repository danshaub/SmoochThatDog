using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public AudioClip pickupSound;
    public bool respawns;
    public float respawnTime;
    public float bobSpeed;
    public float bobHeight;

    private float currentTime;
    private float startYPos;

    //public GameObject particles;
    private void Start()
    {
        startYPos = transform.position.y;
        //particles.GetComponent<ParticleSystem>().Play();
    }
    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3
        {
            x = transform.position.x,
            y = startYPos + Mathf.Cos(currentTime) * bobHeight,
            z = transform.position.z
        };

        currentTime = (currentTime + (Time.deltaTime * bobSpeed)) % (2 * Mathf.PI);

        transform.eulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.eulerAngles.y,
            z = 0
        };

    }

    virtual
    public void TakePickup()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupSound != null)
            {
                PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(pickupSound);
            }
            TakePickup();
            if (respawns)
            {
                StartCoroutine(Respawn());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public IEnumerator Respawn()
    {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = false;
        }
        GetComponent<BoxCollider>().enabled = false;
        yield return new WaitForSeconds(respawnTime);
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            sr.enabled = true;
        }
        GetComponent<BoxCollider>().enabled = true;
    }
}
