using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour, INotify {
    public GameObject bulletPrefab;
    public Transform bulletContainer;
    public float velocity = 100f;

    void Start () {
        ListenFor ("FireGun");
        bulletPrefab.SetActive (false);
    }

    void FireGun (Notification n) {
        GameObject player = (GameObject) n.data["Char"];

        Vector3 playerPos = player.transform.Find ("GunPosition").transform.position;

        GameObject bullet = Instantiate (bulletPrefab, playerPos, player.transform.rotation);
        bullet.GetComponent<Rigidbody> ().velocity = player.transform.TransformDirection (Vector3.forward * velocity);
        bullet.SetActive (true);

        bullet.transform.SetParent (bulletContainer);
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