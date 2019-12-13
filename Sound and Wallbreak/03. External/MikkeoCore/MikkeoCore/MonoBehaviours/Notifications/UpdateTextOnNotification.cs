using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UpdateTextOnNotification : MonoBehaviour
{

	private Text textLabel;
	public string identifier;
	
	// Use this for initialization
	void Start ()
	{	
		NotificationCenter.DefaultCenter.AddObserver (this, "OnChangeText");	
		textLabel = gameObject.GetComponent<Text> ();
	}
	
	void OnChangeText (Notification n)
	{
		//Debug.Log ("Changing text to: "+ (string) n.data["text"]);
		string id = (string)n.data ["identifier"];
		if (id == identifier)
			textLabel.text = (string)n.data ["text"];
	}
	
	
}
