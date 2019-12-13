using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PicaVoxel;

public class MikeSpawner : MonoBehaviour, INotify
{

    public Dictionary<string, GameObject> inventory;
    public GameObject exploderPrefab;

    string code;

    void Start()
    {
        ListenFor("OnDebugCode");
    }

    void OnDebugCode(Notification n)
    {
        code = (string)n.data["debugCode"];
        Debug.Log("Got code: " + code);

    }

    void Update()
    {
        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            BlowUpVoxelThing();
        }
    }

    void BlowUpVoxelThing()
    {


        // Cast a ray from the camera position outward

        //Ray r = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(r.origin, r.direction * 1000f, Color.red, 10f);

        RaycastHit hitInfo;
        if (Physics.Raycast(r, out hitInfo))
        {

            Debug.Log("I just clicked on " + hitInfo.transform.name);

            // first, cast the ray using normal Unity physics. Don't forget to include a layer mask if needed

            Volume pvo = hitInfo.collider.GetComponentInParent<Volume>();
            if (pvo != null) // check to see if we have hit a PicaVoxel Volume. because the Hitbox is a child object on the Volume, we use GetComponentInParent
            {
                PostNotification("OnBulletHit", new Hashtable {
                    { "point", hitInfo.point},
                    {"object",hitInfo.transform.gameObject }
                    });
            }

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
