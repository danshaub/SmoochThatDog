using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DamageText : MonoBehaviour
{
    public TMPro.TextMeshPro text;
    public float lifeTime = 0.5f;
    public float riseSpeed = 0.5f;

    private void Update()
    {
        transform.localPosition += new Vector3(0f, riseSpeed * Time.deltaTime, 0f);

        transform.eulerAngles = new Vector3
        {
            x = 0,
            y = CharacterActions.instance.transform.eulerAngles.y,
            z = 0
        };
    }

    public void BeginAscent()
    {
        StartCoroutine(Lifetime());
    }

    public void EndAscent()
    {
        StopCoroutine(Lifetime());
        gameObject.SetActive(false);
    }

    private IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}
