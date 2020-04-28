using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyHand : Gun
{
    override
    public void Shoot(Transform origin)
    {
        PlayerManager.instance.smooch.Shoot(origin);
    }
}
