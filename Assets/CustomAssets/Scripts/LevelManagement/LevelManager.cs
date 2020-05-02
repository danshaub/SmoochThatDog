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
            public int gunIndex;

        }
        [System.Serializable]
        public struct GunData
        {
            public bool collected;
            public int ammoRemaining;
        }
        [System.Serializable]
        public struct EnemyData
        {

        }
        [System.Serializable]
        public struct PickupData
        {
            public bool pickedUp;
        }

        #endregion

        public PlayerData player;
    }

    public CheckpointData currentCheckpoint;
    //TODO: manage checkpoint system
    //TODO: keep track of statistics for player
    //TODO: keep track of enemies 

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            MakeCheckpoint();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            LoadCheckpoint();
        }
    }
    public void MakeCheckpoint()
    {
        currentCheckpoint.player = PlayerManager.instance.MakeCheckpoint();
    }

    public void LoadCheckpoint()
    {
        PlayerManager.instance.LoadCheckpoint(currentCheckpoint.player);
    }

}
