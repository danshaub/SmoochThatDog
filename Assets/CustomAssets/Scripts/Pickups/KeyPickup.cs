using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : Pickup
{
    [System.Serializable]
    public struct Key{
        [Header("Key ID 0 used for scriptable doors")]
        public int keyID;
        public Sprite image;
    }

    public Key key;


    override
    public void TakePickup()
    {
        PlayerManager.instance.AddKey(key);
    }
}
