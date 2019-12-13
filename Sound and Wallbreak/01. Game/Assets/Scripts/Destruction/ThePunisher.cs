using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PicaVoxel;

public class ThePunisher : MonoBehaviour, INotify
{
    public float currentHealth = 100f;

    void Start()
    {
        ListenFor("OnBulletHit");
    }

    void OnBulletHit(Notification n)
    {
        Vector3 point = (Vector3)n.data["point"];
        GameObject go = (GameObject)n.data["object"];
        //Debug.Log(go.name + "|" + gameObject.name);
        if (go == gameObject)
        {
            Debug.Log(gameObject.name + " has been hit, will deal with health if necessary");
            currentHealth -= Random.RandomRange(15, 20);
            if (currentHealth < 20)
            {
                Debug.Log(gameObject.name + " is almost dead");
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

