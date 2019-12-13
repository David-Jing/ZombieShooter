using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentleBreeze : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward, ForceMode.Acceleration);
    }

    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);

            PostNotification("OnBulletHit", new Hashtable {
                    { "point", contact.point},
                    {"object", contact.otherCollider.gameObject }
                    });
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

