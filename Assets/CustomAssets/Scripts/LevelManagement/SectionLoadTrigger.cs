using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionLoadTrigger : MonoBehaviour
{
    public int[] roomsToLoad;
    public int currentRoom;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.instance.LoadRooms(roomsToLoad);
        }
        else if (other.CompareTag("Enemy"))
        {
            other.gameObject.transform.parent = LevelManager.instance.enemyParents[currentRoom].transform;
        }
    }
}
