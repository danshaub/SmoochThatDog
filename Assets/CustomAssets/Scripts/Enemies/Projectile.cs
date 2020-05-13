using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed = 0;
    [HideInInspector] public int damage = 0;
    public GameObject particles;
    Rigidbody rb;
    public Animator animator;
    public GameObject laserSprite;
    public GameObject minimapLaser;

    protected Vector2 horizPositionDifference;
    protected Vector2 rotatedPositionDifference;
    protected Vector2 forward = new Vector2(0f, 1f);

    public float angleToPlayer { get; protected set; }
    protected float angleInRadians = 0f;
    private int activeLayerIndex;
    private int previousLayerIndex;



    bool flying = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        RotateSprite();
        UpdateLayer();
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
        if (other.CompareTag("Player"))
        {
            PlayerManager.instance.HurtPlayer(damage);
        }

        StartCoroutine(OnHit());
    }

    public IEnumerator OnHit()
    {
        flying = false;

        laserSprite.SetActive(false);
        minimapLaser.SetActive(false);
        GetComponent<Collider>().enabled = false;

        particles.GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(particles.GetComponent<ParticleSystem>().main.startLifetime.constant);

        laserSprite.SetActive(true);
        minimapLaser.SetActive(true);
        GetComponent<Collider>().enabled = true;

        flying = true;

        gameObject.SetActive(false);
    }

    protected void RotateSprite()
    {
        // laserSprite.transform.eulerAngles = new Vector3
        // {
        //     x = 0,
        //     y = CharacterActions.instance.transform.eulerAngles.y,
        //     z = 0
        // };

        laserSprite.transform.LookAt(CharacterActions.instance.transform);
    }

    protected void UpdateLayer()
    {
        horizPositionDifference.x = CharacterActions.instance.transform.position.x - transform.position.x;
        horizPositionDifference.y = CharacterActions.instance.transform.position.z - transform.position.z;
        horizPositionDifference.Normalize();

        angleInRadians = (transform.eulerAngles.y * Mathf.PI) / 180f;

        rotatedPositionDifference.x = Mathf.Cos(angleInRadians) * horizPositionDifference.x - Mathf.Sin(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.y = Mathf.Sin(angleInRadians) * horizPositionDifference.x + Mathf.Cos(angleInRadians) * horizPositionDifference.y;
        rotatedPositionDifference.Normalize();

        angleToPlayer = Vector2.SignedAngle(forward, rotatedPositionDifference);

        if (angleToPlayer >= -157.5f && angleToPlayer < -112.5f)
        {
            activeLayerIndex = 0;
        }
        else if (angleToPlayer >= -112.5f && angleToPlayer < -67.5f)
        {
            activeLayerIndex = 1;
        }
        else if (angleToPlayer >= -67.5f && angleToPlayer < -22.5f)
        {
            activeLayerIndex = 2;
        }
        else if (angleToPlayer >= -22.5f && angleToPlayer < 22.5f)
        {
            activeLayerIndex = 3;
        }
        else if (angleToPlayer >= 22.5f && angleToPlayer < 67.5f)
        {
            activeLayerIndex = 4;
        }
        else if (angleToPlayer >= 67.5f && angleToPlayer < 112.5f)
        {
            activeLayerIndex = 5;
        }
        else if (angleToPlayer >= 112.5f && angleToPlayer < 157.5f)
        {
            activeLayerIndex = 6;
        }
        else
        {
            activeLayerIndex = 7;
        }

        if (previousLayerIndex != activeLayerIndex)
        {
            previousLayerIndex = activeLayerIndex;

            for (int i = 0; i < animator.layerCount; i++)
            {
                if (i == activeLayerIndex)
                {
                    animator.SetLayerWeight(i, 1);
                }
                else
                {
                    animator.SetLayerWeight(i, 0);
                }
            }
        }

    }
}
