using UnityEngine;
using System.Collections;

public class TracksideCameraPosition : MonoBehaviour, INotify {

	void OnTriggerEnter (Collider other) {
		//Debug.Log ("Trigger entered");
		PostNotification ("OnSetNewCameraPosition", new Hashtable () { { "newCameraPosition", transform.position }
			, { "collider",other }
		});
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
