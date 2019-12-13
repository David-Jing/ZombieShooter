using UnityEngine;
using System.Collections;


//Can notify others, AND listen
public interface INotify {
	void PostSimpleNotification (string notificationName);

	void PostNotification (string notificationName, Hashtable parameters, string debugMessage);

	void ListenFor (string messageName);

	void StopListeningFor (string messageName);
}

