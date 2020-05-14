using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    private void Awake()
    {
        instance = this;
    }
    [System.Serializable]
    public struct CheckpointData
    {
        #region Data Fields

        [System.Serializable]
        public struct PlayerData
        {
            public Vector3 worldPosition;
            public Vector3 localRotation;
            public int health;
            public int armor;
            public int maxArmor;
            public GunData[] guns;
            public int gunIndex;
            public int rageAmount;
            public List<KeyPickup.Key> keys;
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
            public Vector3 worldPosition;
            public Quaternion worldRotation;
            public int health;
            public bool cured;
            public Transform parent;
        }
        [System.Serializable]
        public struct PickupData
        {
            public bool active;
        }

        [System.Serializable]
        public struct DoorData
        {
            public bool locked;
            public bool open;
            public Material[][] materials;
        }

        #endregion

        public PlayerData player;
        public List<EnemyData> enemies;
        public List<PickupData> pickups;
        public List<DoorData> doors;
    }

    public CheckpointData currentCheckpoint;
    //TODO: keep track of statistics for player

    public GameObject[] rooms;
    public int[] defaultRooms;
    public GameObject[] enemyParents;
    public Pickup[] pickups;
    public Enemy[] enemies;
    public Door[] doors;

    private void Start()
    {
        LoadRooms(defaultRooms);
        MakeCheckpoint();
    }
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

        currentCheckpoint.doors.Clear();
        currentCheckpoint.pickups.Clear();
        currentCheckpoint.enemies.Clear();

        foreach (Door door in doors)
        {
            currentCheckpoint.doors.Add(door.MakeCheckpoint());
        }

        foreach (Pickup pickup in pickups)
        {
            currentCheckpoint.pickups.Add(pickup.MakeCheckpoint());
        }

        foreach (Enemy enemy in enemies)
        {
            currentCheckpoint.enemies.Add(enemy.MakeCheckpoint());
        }
    }

    public void LoadCheckpoint()
    {
        PlayerManager.instance.LoadCheckpoint(currentCheckpoint.player);

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].LoadCheckpoint(currentCheckpoint.doors[i]);
        }

        for (int i = 0; i < pickups.Length; i++)
        {
            pickups[i].LoadCheckpoint(currentCheckpoint.pickups[i]);
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].LoadCheckpoint(currentCheckpoint.enemies[i]);
        }
    }

    public void LoadRooms(int[] loadedRooms)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            bool loadRoom = false;
            for (int j = 0; j < loadedRooms.Length; j++)
            {
                if (i == loadedRooms[j])
                {
                    loadRoom = true;
                    break;
                }
            }

            if (loadRoom)
            {
                rooms[i].SetActive(true);
            }
            else
            {
                rooms[i].SetActive(false);
            }
        }
    }

}
