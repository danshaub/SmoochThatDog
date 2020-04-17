using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
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

        transform.LookAt(new Vector3(PlayerManager.instance.gameObject.transform.position.x, 
                                     transform.position.y,
                                     PlayerManager.instance.gameObject.transform.position.z));

    }
}
