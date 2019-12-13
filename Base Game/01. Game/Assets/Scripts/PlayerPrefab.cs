using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefab : MonoBehaviour, INotify {
    public float speed;
    public float fireRate;
    Rigidbody rb;
    Vector3 prev;
    bool canFire = true;

    // Start is called before the first frame update
    void Start () { }

    void FixedUpdate () {
        PlayerMovement ();

        if (Input.GetKeyDown (KeyCode.Space) && canFire == true) {
            canFire = false;
            Invoke ("ResetFire", fireRate);
            PostNotification ("FireGun", new Hashtable { { "Char", gameObject } });
        }
    }

    void ResetFire () {
        canFire = true;
    }

    void PlayerMovement () {
        float moveHorizontal = Input.GetAxisRaw ("Horizontal");
        float moveVertical = Input.GetAxisRaw ("Vertical");

        Vector3 playerMovement = new Vector3 (moveHorizontal, 0f, moveVertical);

        if (playerMovement != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation (playerMovement);
            prev = playerMovement;
        } else {
            transform.rotation = Quaternion.LookRotation (prev);
        }

        float magnitude = playerMovement.magnitude;

        if (playerMovement.magnitude < 1) {
            magnitude = 1;
        }

        transform.Translate (playerMovement / playerMovement.magnitude * speed * Time.deltaTime, Space.World);
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