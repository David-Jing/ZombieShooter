using UnityEngine;
using System.Collections;

// The Notification class is the object that is send to receiving objects of a notification type.
// This class contains the sending GameObject, the name of the notification, and optionally a hashtable containing data.
public class Notification
{

	//public Notification (GameObject aSender, string aName, Hashtable aData)
	//{
	//  throw new System.NotImplementedException ();
	//}

	public Component sender;
	public string name;
	public Hashtable data;

	public Notification (Component aSender, string aName)
	{
		sender = aSender;
		name = aName;
		data = null;
	}

	public Notification (Component aSender, string aName, Hashtable aData)
	{
		sender = aSender;
		name = aName;
		data = aData;
	}


}
