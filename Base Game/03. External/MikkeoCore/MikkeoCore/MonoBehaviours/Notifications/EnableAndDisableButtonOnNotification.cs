using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnableAndDisableButtonOnNotification : MonoBehaviour {
	
	private Button button;

	public string identifier;
	
	// Use this for initialization
	void Start () {	
		NotificationCenter.DefaultCenter.AddObserver (this, "OnEnableButton");	
		NotificationCenter.DefaultCenter.AddObserver (this, "OnDisableButton");	
		button = gameObject.GetComponent<Button> ();
	}

	void OnEnableButton (Notification n) {
		string id = (string)n.data ["identifier"];
		if (id == identifier)
			button.interactable = true;
	}

	void OnDisableButton (Notification n) {
		string id = (string)n.data ["identifier"];

		if (id == identifier)
			button.interactable = false;
	}
	


}
