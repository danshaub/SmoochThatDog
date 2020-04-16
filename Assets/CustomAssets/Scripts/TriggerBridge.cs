using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Reference : https://answers.unity.com/questions/986235/is-it-possible-to-check-collision-from-another-obj.html
public class TriggerBridge : MonoBehaviour
{
    TriggerListener listener;
    public void Initialize(TriggerListener l)
    {
        listener = l;
    }
    private void OnTriggerEnter(Collider other)
    {
        listener.OnTriggerEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        listener.OnTriggerExit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        listener.OnTriggerStay(other);
    }
}
