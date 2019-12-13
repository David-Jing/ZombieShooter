using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mikkeo.Extensions;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class GameManager : Singleton<GameManager>, INotify
{


    void Start()
    {
        ListenForMessages();
        Invoke("Init", 3f);
    }

    #region INIT and LISTEN

    void Init()
    {


        ListenForMessages();
        PostSimpleNotification("OnSayHi");

    }


    void ListenForMessages()
    {


    }

    #endregion



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
