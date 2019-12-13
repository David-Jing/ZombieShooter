using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SendNotificationOnClick : MonoBehaviour, INotify {

	public bool isEnabled = true;

	public string onMessage;
	public string offMessage;
	public bool isAToggleButton = false;
	
	//private float timeMouseWasPressed;
	private bool isButtonToggledOn = false;
	private string messageSent;

	private Hashtable extraParams;

	//private int pressCount=0;

	//private Button button;

	void Start () {
		//button = gameObject.GetComponent<Button> ();
	}

	public void SendNotification () {

		if (isEnabled) {

			if (isAToggleButton) {
				if (!isButtonToggledOn) {
					//timeMouseWasPressed = Time.time;
					messageSent = onMessage;
					isButtonToggledOn = true;
				} else {
					messageSent = offMessage;			
					isButtonToggledOn = false;
				}
			} else {
				messageSent = onMessage;
			}
		
		
			if (messageSent != "") {
				//Send half of the messages if we're a toggle button?
				/*
			if (isAToggleButton == true){
				pressCount++;
				if (pressCount % 2 == 0) 
					Debug.Log ("Skipping one notification because I'm a toggle button: "+pressCount);
					return;
			}
			*/
				//Debug.Log ("Sending |" + messageSent + "|");
				PostNotification (messageSent, extraParams);
			}
		}
	}


	public void AddExtraParameters (Hashtable ht) {
		extraParams = ht;
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
