using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, InteractableObject
{
    public List<GameObject> doors;
    public List<Transform> closedTransforms;
    public List<Transform> openTransforms;

    private List<Vector3> moveSpeeds = new List<Vector3>();
    private List<Vector3> rotateSpeeds = new List<Vector3>();
    private List<Vector3> scaleSpeeds = new List<Vector3>();

    public int sequenceFrames = 1;
    public bool closeAutomatically = false;
    public float closeTime = 1f;
    public bool locked = false;
    public int lockID = 0;
    public bool interactable = true;

    public bool open { get; private set; } = false;
    public int sequenceFrame { get; private set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
        EnsureLayer();

        if(doors.Count != closedTransforms.Count || doors.Count != openTransforms.Count)
        {
            Debug.LogError("Unequal number of doors and transforms");

            foreach(GameObject door in doors)
            {
                door.SetActive(false);
            }
            return;
        }

        open = false;

        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].transform.position = closedTransforms[i].position;
            doors[i].transform.localScale = closedTransforms[i].localScale;
            doors[i].transform.rotation = closedTransforms[i].rotation;

            moveSpeeds.Add((openTransforms[i].position - closedTransforms[i].position) / (float)sequenceFrames);
            rotateSpeeds.Add((openTransforms[i].localEulerAngles - closedTransforms[i].localEulerAngles) / (float)sequenceFrames);
            scaleSpeeds.Add((openTransforms[i].localScale - closedTransforms[i].localScale) / (float)sequenceFrames);            
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (open && sequenceFrame != sequenceFrames)
        {
            sequenceFrame = Mathf.Clamp(sequenceFrame + 1, 0, sequenceFrames);
            UpdateTransform();
        }
        else if (!open && sequenceFrame != 0)
        {
            sequenceFrame = Mathf.Clamp(sequenceFrame - 1, 0, sequenceFrames);
            UpdateTransform();
        }
    }

    private void UpdateTransform()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].transform.position = closedTransforms[i].position + moveSpeeds[i] * sequenceFrame;
            doors[i].transform.localScale = closedTransforms[i].localScale + scaleSpeeds[i] * sequenceFrame;
            doors[i].transform.localEulerAngles = closedTransforms[i].localEulerAngles + rotateSpeeds[i] * sequenceFrame;
        }
    }

    public void EnsureLayer()
    {
        foreach(Transform tran in GetComponentsInChildren<Transform>())
        {
            tran.gameObject.layer = 10;
        }
    }

    virtual public void Action()
    {
        if (!interactable)
        {
            return;
        }

        if (open && !closeAutomatically)
        {
            Close();
        }
        else if (!locked || (locked && PlayerManager.instance.HasKey(lockID)))
        {
            Open();
        }
    }

    virtual public void Close()
    {
        open = false;
    }

    virtual public void Open()
    {
        open = true;

        if (closeAutomatically)
        {
            Invoke("Close", closeTime);
        }
    }
}
