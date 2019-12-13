using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct TheWall {
    public int HP;
    public GameObject wall;
}

public class WallManager : MonoBehaviour, INotify {
    public Transform wallContainer;
    public int maxWallHP;
    int wallLabel = 0;
    Dictionary<string, TheWall> WallDict = new Dictionary<string, TheWall> ();
    void Start () {

        foreach (Transform thisWall in wallContainer) {
            TheWall newWall = new TheWall ();
            newWall.HP = maxWallHP;
            newWall.wall = thisWall.gameObject;
            thisWall.name = "Wall " + wallLabel;

            WallDict.Add ("Wall " + wallLabel++, newWall);
        }

        ListenFor ("onZombieHitWall");
    }

    void onZombieHitWall (Notification n) {
        GameObject wall = (GameObject) n.data["Wall"];
        TheWall selectWall = WallDict[wall.name];

        Debug.Log (selectWall.HP);

        if (selectWall.HP < 1) {
            WallDict.Remove (wall.name);
            //Destroy (selectWall.wall);
        } else {
            selectWall.HP--;
        }
    }

    #region iNotify

    public void ListenFor (string messageName) {
        NotificationCenter.DefaultCenter.AddObserver (this, messageName);
    }

    public void StopListeningFor (string messageName) {
        NotificationCenter.DefaultCenter.RemoveObserver (this, messageName);
    }

    public void PostSimpleNotification (string notificationName) {
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName));
    }

    public void PostNotification (string notificationName, Hashtable parameters, string debugMessage = "") {
        if (debugMessage != "")
            Debug.Log (debugMessage);
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName, parameters));
    }

    #endregion
}