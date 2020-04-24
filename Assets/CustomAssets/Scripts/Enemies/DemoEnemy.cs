using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoEnemy : Target
{
    public Material angry;
    public Material happy;
    public Material stunned;
    public TMPro.TextMeshPro text;

    private bool canStun = true;

    private void Update()
    {
        transform.localEulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.localEulerAngles.y,
            z = 0
        };
    }
    public void Start()
    {
        killed = false;
        health = maxHealth;
        GetComponent<MeshRenderer>().material = angry;
        canStun = true;
        isStunned = false;
        text.text = "HOSTILE";
        text.color = Color.red;
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
        text.text = "STUNNED";
        text.color = Color.blue;
        GetComponent<MeshRenderer>().material = stunned;
        StartCoroutine(StunnedCoroutine());
    }

    override
    public void Kill()
    {
        GetComponent<MeshRenderer>().material = happy;
        canStun = false;
        text.text = "CURED";
        text.color = Color.green;
        StopCoroutine(StunnedCoroutine());
        StartCoroutine(Dead());
    }

    public IEnumerator StunnedCoroutine()
    {
        yield return new WaitForSeconds(2f);
        isStunned = false;
        canStun = true;
        text.text = "HOSTILE";
        text.color = Color.red;
        GetComponent<MeshRenderer>().material = angry;
    }

    public IEnumerator Dead()
    {
        yield return new WaitForSeconds(3f);

        Start();
    }
}
