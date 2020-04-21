using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRandomly : MonoBehaviour
{
    [Range(0f, 1f)] public float speed = .01f;
    [Range(0f, 180f)] public float tolerance = .05f;

    private float angleX;
    private float angleY;
    private float angleZ;

    private float targetX;
    private float targetY;
    private float targetZ;

    // Update is called once per frame
    void Update()
    {
        angleX = Mathf.Lerp(angleX, targetX, speed);
        angleY = Mathf.Lerp(angleY, targetY, speed);
        angleZ = Mathf.Lerp(angleZ, targetZ, speed);

        transform.localEulerAngles = new Vector3(angleX, angleY, angleZ);

        if(Mathf.Abs(targetX - angleX) < tolerance)
        {
            PickNewLookAtX();
        }
        if(Mathf.Abs(targetY - angleY) < tolerance)
        {
            PickNewLookAtY();
        }
        if(Mathf.Abs(targetZ - angleZ) < tolerance)
        {
            PickNewLookAtZ();
        }
    }

    void PickNewLookAtX()
    {
        targetX = Random.Range(0f, 360f);
    }
    void PickNewLookAtY()
    {
        targetY = Random.Range(0f, 360f);
    }
    void PickNewLookAtZ()
    {
        targetZ = Random.Range(0f, 360f);
    }
}
