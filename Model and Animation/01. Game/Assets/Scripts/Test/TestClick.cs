using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClick : MonoBehaviour, INotify
{

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

        ListenFor("OnSayHi");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSayHi(Notification n)
    {

        Debug.Log("Hello, I am " + gameObject.name);
        ///        Debug.Log("Hello, I am "+n.)


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
