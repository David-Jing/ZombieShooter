using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SendNotificationOnTMInputChange : MonoBehaviour, INotify
{

    public string changeTextMessage;
    public string messageKey = "text";

    public TextMeshProUGUI inputTextBox;

    void Update()
    {

        if (Input.anyKeyDown)
        {

            Debug.Log("Text is: " + inputTextBox.text);
            PostNotification(changeTextMessage, new Hashtable { { messageKey, inputTextBox.text } });

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
