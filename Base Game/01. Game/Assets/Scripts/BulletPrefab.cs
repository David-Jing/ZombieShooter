using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPrefab : MonoBehaviour, INotify {

    public GameObject player;
    public float lifeTime = 3f;

    void Start () {
        Physics.IgnoreLayerCollision (9, 12);
        Invoke ("BulletDead", lifeTime);
    }

    void BulletDead () {
        Destroy (gameObject);
    }

    void OnCollisionEnter (Collision col) {
        if (col.gameObject.layer == 10 || col.gameObject.layer == 11) {
            if (col.gameObject.tag == "Zombie") {
                Debug.Log ("Hit Zombie");
                PostNotification ("OnBulletHitZombie", new Hashtable { { "Zombie", col.gameObject } });
            } else {
                //PostNotification ("OnBulletHit", new Hashtable { { "point", col.contacts[0].point }, { "object", col.gameObject } });
            }

            Destroy (gameObject);
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