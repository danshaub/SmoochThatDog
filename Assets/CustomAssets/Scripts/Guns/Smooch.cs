using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smooch : Gun, TriggerListener
{ 
    private BoxCollider hitBox;
    [HideInInspector] public List<GameObject> targetsInRange;

    private void Start()
    {
        hitBox = CharacterActions.instance.fpsCamera.GetComponent<BoxCollider>();
        TriggerBridge tb = hitBox.gameObject.AddComponent<TriggerBridge>();
        tb.Initialize(this);
        hitBox.size = new Vector3(maxBulletSpread, 2f, range);
        hitBox.center = new Vector3(0f, -1f, (range + 1f) / 2f);
    }
    override
    public void Shoot(Transform origin)
    {
        PlayerManager.instance.playerAnimation.SetTrigger("Shoot");
        PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(shootSound);
        foreach(GameObject gm in targetsInRange)
        {
            Hit(gm);
        }
    }


    public void Hit(GameObject hit)
    {
        Target hitTarget;
        Debug.Log("Smooch Hit!");
        try
        {
            hitTarget = hit.GetComponent<Target>();
        }
        catch
        {
            return;
        }
        if (!PlayerManager.instance.isRaged)
        {
            hitTarget.Stun();
            PlayerManager.instance.AddRage(hitTarget.rageFill);
        }
        else
        {
            hitTarget.Hit(damagePerBullet);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        targetsInRange.Add(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        targetsInRange.Remove(other.gameObject);
    }
}
