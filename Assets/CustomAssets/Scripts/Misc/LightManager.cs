using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightManager : MonoBehaviour
{
    [System.Serializable]
    public struct LightLevel
    {
        public GameObject[] lights;
    }
    public LightLevel[] lightLevels;
    public int currentLightLevel;
    public bool includeBaseLevelInDetail = false;
    private bool previouslyIncluded = false;
    private int previousLightLevel;
    private void OnValidate()
    {
        currentLightLevel = Mathf.Clamp(currentLightLevel, 0, lightLevels.Length - 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Application.isPlaying)
        {
            if ((currentLightLevel != previousLightLevel) || (includeBaseLevelInDetail != previouslyIncluded))
            {
                SetLightLevel(currentLightLevel);
            }

            previouslyIncluded = includeBaseLevelInDetail;
            previousLightLevel = currentLightLevel;
        }
    }

    public void SetLightLevel(int newLightLevel)
    {
        currentLightLevel = Mathf.Clamp(newLightLevel, 0, lightLevels.Length - 1);

        for (int i = 0; i < lightLevels.Length; i++)
        {
            //Set base light detail inactive if not included in higher detail levels
            if (i == 0)
            {
                if (includeBaseLevelInDetail || currentLightLevel == 0)
                {
                    foreach (GameObject light in lightLevels[i].lights)
                    {
                        light.SetActive(true);
                    }
                }
                else
                {
                    foreach (GameObject light in lightLevels[i].lights)
                    {
                        light.SetActive(false);
                    }
                }

            }
            //Set lights to active if they are within the current light level
            else if (i <= currentLightLevel)
            {
                foreach (GameObject light in lightLevels[i].lights)
                {
                    try
                    {
                        light.SetActive(true);
                    }
                    catch
                    {

                    }

                }
            }
            //Otherwise turn them off
            else
            {
                foreach (GameObject light in lightLevels[i].lights)
                {
                    try
                    {
                        light.SetActive(false);
                    }
                    catch
                    {

                    }
                }
            }
        }
    }
}
