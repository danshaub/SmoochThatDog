using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLevelScreen : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LevelLoader.instance.LoadLevel(0);
        }
    }
}
