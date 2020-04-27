using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public TMPro.TextMeshPro text;
    public float lifeTime = 0.5f;
    public float riseSpeed = 0.5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.localPosition += new Vector3(0f, riseSpeed * Time.fixedDeltaTime, 0f);

        transform.eulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.eulerAngles.y,
            z = 0
        };
    }
}
