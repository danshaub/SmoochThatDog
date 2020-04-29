using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableObject
{
    void EnsureLayer();
    void Action();
}
