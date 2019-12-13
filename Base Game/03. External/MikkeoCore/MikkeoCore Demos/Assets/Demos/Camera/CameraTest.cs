using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mikkeo.IO;
using Mikkeo.Colour;
using Mikkeo.Input;

/*
 * CameraTest
 * 
 * A way to test a number of custom camera scripts:
 * 
 * SmoothFollowMany - Follows a list of gameobjects, fits them on-screen with a buffer
 * SmoothFollow2 - Follows behind an object
 * MouseLook
 * 
 */



public class CameraTest : MonoBehaviour,INotify {

	public Camera cam;

	Dictionary<string, Action> keyPressFunctionTable = new Dictionary<string, Action> ();

	void Start () {		

		SetupKeyLogger ();
		ListenFor ("OnDebugCode");

		SwitchCamera (CameraControllerType.GIMBAL_MOUSE_LOOK);

		StartCoroutine ("SwitchCamerasOverTime");
	}

	IEnumerator SwitchCamerasOverTime () {
		
		yield return new WaitForSeconds (6);
		SwitchCamera (CameraControllerType.SMOOTH_FOLLOW_2);
		yield return new WaitForSeconds (6);
		SwitchCamera (CameraControllerType.TRACKSIDE_CAMERAS);
		yield return new WaitForSeconds (6);
		SwitchCamera (CameraControllerType.SMOOTH_FOLLOW_MANY);
		yield return new WaitForSeconds (6);
		SwitchCamera (CameraControllerType.GIMBAL_MOUSE_LOOK);
		yield return new WaitForSeconds (6);

		StartCoroutine ("SwitchCamerasOverTime");
	}

	void OnDestroy () {
		StopListeningFor ("OnDebugCode");
	}

	void OnGUI () {
		
	}

	void CreateRedCube () {
		//Load a gameobject and track it
		GameObject go = IOUtils.Instance.LoadPrefabFromPath ("Cubes/Red Cube", null);
		PostNotification ("OnAddItemToTrackWithCamera", new Hashtable (){ { "item",go } });
		go.transform.position = Vector3.forward * 20f + Vector3.up;
	}

	void CreateMondrian () {
		//Load a gameobject and track it
		GameObject go = IOUtils.Instance.LoadPrefabFromPath ("Cubes/Mondrian", null);
		PostNotification ("OnAddItemToTrackWithCamera", new Hashtable (){ { "item",go } });
		go.transform.position = Vector3.left * -20f + Vector3.up;
	}


	void OnDebugCode (Notification n) {

		string debugCode = (string)n.data ["debugCode"];

		if (keyPressFunctionTable.ContainsKey (debugCode)) {
			keyPressFunctionTable [debugCode] ();
		}
	}


	void SwitchCamera (CameraControllerType controllerType) {
		
		ICameraController[] cameraController = cam.gameObject.GetComponents<ICameraController> ();

		for (int i = 0; i < cameraController.Length; i++) {

			CameraControllerType cct = cameraController [i].GetCameraControllerType ();

			//Debug.Log (cct);

			if (cct == controllerType) {
				cameraController [i].Enable ();
			} else {
				cameraController [i].Disable ();
			}
		}

	}


	#region Init

	void SetupKeyLogger () {
		
		keyPressFunctionTable.Add ("red", CreateRedCube);
		keyPressFunctionTable.Add ("mon", CreateMondrian);

		DebugKeyLogger.Instance.codeLength = 3;
		DebugKeyLogger.Instance.notificationName = "OnDebugCode";
		DebugKeyLogger.Instance.keyName = "debugCode";

	}

	#endregion

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

	public void PostNotification (string notificationName, Hashtable parameters) {
		NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName, parameters));		
	}

	#endregion


}
