using UnityEngine;
using System.Collections;
using Mikkeo.Extensions;

public class ColliderSleeper : MonoBehaviour, INotify {

	public float colliderSleepRadius = 10f;
	string[] objectsOfInterestSubstrings;

	bool areChildrenSleeping;
	bool lastTimeWereChildrenSleeping;
	SphereCollider detector;

	void Awake () {
		objectsOfInterestSubstrings = new string[2]{ "Track", "Tank" };
		detector = gameObject.AddComponent<SphereCollider> ();
		detector.isTrigger = true;
		detector.radius = colliderSleepRadius;
		detector.enabled = false;

		NotificationCenter.DefaultCenter.AddObserver (this, "OnSetColliderSleepRadius");
	}


	void OnSetColliderSleepRadius (Notification n) {
		colliderSleepRadius = (float)n.data ["radius"];
	}


	void Start () {
		float repeatRate = .5f + Random.Range (0f, 0.1f);
		InvokeRepeating ("CheckForLocalActivity", 1f, repeatRate);
	}

	void CheckForLocalActivity () {
		StartCoroutine ("ICheckForLocalActivity");
	}

	IEnumerator ICheckForLocalActivity () {
		lastTimeWereChildrenSleeping = areChildrenSleeping;
		areChildrenSleeping = true;
		detector.enabled = true;
		//Debug.Log ("Looking for local intruders.");
		yield return new WaitForSeconds (.05f);
		detector.enabled = false;
		if ((areChildrenSleeping) && (areChildrenSleeping != lastTimeWereChildrenSleeping)) {
			SleepChildColliders ();
		}
		if ((areChildrenSleeping == false) && (areChildrenSleeping != lastTimeWereChildrenSleeping)) {
			WakeChildColliders ();
		}
	}

	void OnTriggerStay (Collider other) {
		if (IsItemOfInterest (other)) {
			areChildrenSleeping = false;
		}

	}

	bool IsItemOfInterest (Collider other) {
		foreach (string s in objectsOfInterestSubstrings) {
			if (other.name.IndexOf (s) > -1)
				return true;
		}
		return false;
	}


	void SleepChildColliders () {
		//Debug.Log ("Putting kids to bed");
		gameObject.SetCollisionRecursively (false);
	}

	void WakeChildColliders () {
		//Debug.Log ("Waking the kids");
		gameObject.SetCollisionRecursively (true);
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
