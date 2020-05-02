using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLevel : MonoBehaviour, IInteractableObject
{
    public GameObject winLevelCanvas;
    public void EnsureLayer()
    {

    }

    public void Action()
    {
        winLevelCanvas.SetActive(true);
        CharacterActions.instance.lockControl = true;
    }
}
