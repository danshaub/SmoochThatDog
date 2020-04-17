using UnityEngine;

//Reference : https://answers.unity.com/questions/986235/is-it-possible-to-check-collision-from-another-obj.html
public interface TriggerListener
{
    void OnTriggerEnter(Collider other);
    void OnTriggerExit(Collider other);

    void OnTriggerStay(Collider other);
}
