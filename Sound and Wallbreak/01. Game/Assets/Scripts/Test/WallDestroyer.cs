using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PicaVoxel;


public class WallDestroyer : MonoBehaviour, INotify
{



    public GameObject wallSegmentPrefab;

    GameObject wallSegment;
    void Start()
    {
        wallSegment = Instantiate(wallSegmentPrefab, transform.localPosition, Quaternion.identity);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TakeAHit();
        }
    }
    void TakeAHit()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000.0f))
        {
            Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object

            DestroyVoxels dv = hit.transform.gameObject.GetComponent<DestroyVoxels>();
            if (dv)
                dv.TakeAHit();
        }

    }

    #region iNotify

    public void ListenFor(string messageName)
    {
        NotificationCenter.DefaultCenter.AddObserver(this, messageName);
    }

    public void StopListeningFor(string messageName)
    {
        NotificationCenter.DefaultCenter.RemoveObserver(this, messageName);
    }

    public void PostSimpleNotification(string notificationName)
    {
        NotificationCenter.DefaultCenter.PostNotification(new Notification(this, notificationName));
    }

    public void PostNotification(string notificationName, Hashtable parameters, string debugMessage = "")
    {
        if (debugMessage != "")
            Debug.Log(debugMessage);
        NotificationCenter.DefaultCenter.PostNotification(new Notification(this, notificationName, parameters));
    }

    #endregion



}
