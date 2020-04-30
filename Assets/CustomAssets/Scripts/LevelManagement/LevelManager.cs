using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public struct CheckpointData
    {
        #region Data Fields

        [System.Serializable]
        public struct PlayerData
        {
            public Vector3 worldPosition;
            public Quaternion worldRotation;
            public int health;
            public int armor;
            public int maxArmor;
            public GunData[] guns;
            public int weaponIndex;

        }
        [System.Serializable]
        public struct EnemyData
        {

        }
        [System.Serializable]
        public struct GunData
        {
            public bool obtained;
            public int ammo;
        }
        [System.Serializable]
        public struct PickupData
        {

        }

        #endregion


    }
    //TODO: manage checkpoint system
    //TODO: keep track of statistics for player
    //TODO: keep track of enemies 


}
