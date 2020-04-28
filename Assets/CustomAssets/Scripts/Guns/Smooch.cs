using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smooch : Gun, TriggerListener
{
    private BoxCollider hitBox;
    [HideInInspector] public List<GameObject> targetsInRange;
    private List<GameObject> targetsInRangeTemp;


    private void Start()
    {
        enabled = true;
        targetsInRange.Clear();
        hitBox = CharacterActions.instance.GetComponentInChildren<BoxCollider>();
        TriggerBridge tb = hitBox.gameObject.AddComponent<TriggerBridge>();
        tb.Initialize(this);
        hitBox.size = new Vector3(maxBulletSpread, 3, range);
        hitBox.center = new Vector3(0f, -.25f, (range + 1f) / 2f);

    }

    public void CallStart()
    {
        Start();
    }

    override
    public void Shoot(Transform origin)
    {
        PlayerManager.instance.smoochAnimations.SetTrigger("Shoot");
        PlayerManager.instance.GetComponent<AudioSource>().PlayOneShot(shootSound);
        targetsInRangeTemp = targetsInRange;
        for (int i = targetsInRange.Count - 1; i >= 0; i--)
        {
            Hit(targetsInRange[i]);
        }
        targetsInRangeTemp.Clear();
    }


    public void Hit(GameObject hit)
    {
        Target hitTarget;

        try
        {
            hitTarget = hit.GetComponent<Target>();
        }
        catch
        {
            return;
        }
        if (!PlayerManager.instance.isRaged && hitTarget.canStun && !hitTarget.killed)
        {
            hitTarget.Stun();
            PlayerManager.instance.AddRage(hitTarget.rageFill);
        }
        else if (PlayerManager.instance.isRaged)
        {
            hitTarget.Hit(damagePerBullet);
        }
    }

    public void OnTriggerEnter(Collider other)
    {

    }

    public void OnTriggerExit(Collider other)
    {
        targetsInRange.Remove(other.gameObject);
    }

    public void OnTriggerStay(Collider other)
    {
        if (!targetsInRange.Contains(other.gameObject) && other.GetComponent<Target>() != null)
        {
            targetsInRange.Add(other.gameObject);
        }

    }

    public void RemoveFromTargetList(GameObject go)
    {
        targetsInRange.Remove(go);
    }
}
