using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Target
{
    public Material angry;
    public Material happy;
    public Material stunned;

    private bool canStun = true;

    private void Update()
    {
        transform.LookAt(new Vector3(PlayerManager.instance.gameObject.transform.position.x,
                                     transform.position.y,
                                     PlayerManager.instance.gameObject.transform.position.z));
    }
    public void Start()
    {
        health = maxHealth;
        GetComponent<MeshRenderer>().material = angry;
        canStun = true;
        isStunned = false;
    }

    override
    public void Stun()
    {
        if (!canStun)
        {
            return;
        }
        isStunned = true;
        canStun = false;
        GetComponent<MeshRenderer>().material = stunned;
        StartCoroutine(StunnedCoroutine());
    }

    override
    public void Kill()
    {
        GetComponent<MeshRenderer>().material = happy;
        canStun = false;
        StopCoroutine(StunnedCoroutine());
        StartCoroutine(Dead());
    }

    public IEnumerator StunnedCoroutine()
    {
        yield return new WaitForSeconds(2f);
        isStunned = false;
        canStun = true;
        GetComponent<MeshRenderer>().material = angry;
    }

    public IEnumerator Dead()
    {
        yield return new WaitForSeconds(3f);

        Start();
    }
}
