/*
 * 
 * TracksideCameras
 * 
 * Designed specifically for racing games, but can work anywhere
 * 
 * REQUIRES at least a couple of GameObjects with TracksideCameraPosition on them, as well as
 * the tracked item should have a rigidbody (typical, but you still need to know this)
 * 
 * Waits for notification "OnSetNewCameraPosition" (sent from TracksideCameraPosition script) then 
 * moves to that position.  TracksideCameraPosition simply sends its co-ordinate on OnTriggerEnter(),
 * as well as the gameObject so it can be compared
 * 
 * When the item to track enters any of the colliders of the trackside camera locations,
 * the camera moves to that location, then changes the FOV to set the object to fit the screen
 * 
 * The camera tracks the item
 * The camera fits screen to maxDistanceSize when far away
 * The camera fits screen to minDistanceSize when close
 * The camera will go no lower than minFOV and will go no higher than maxFOV
 * 
 * 
 * If no camera specified, looks inside current object, then finally uses trackSideCamera.main
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Specialized;


public class TracksideCameras : MonoBehaviour, ICameraController {

	public Camera trackSideCamera;
	public GameObject target;
	public float nearPadding = .2f;
	public float farPadding = .4f;
	public float distanceAtMinFOV = 40f;
	public float cameraSmoothing = .5f;
	public bool isEnabled = false;

	Collider targetCollider;
	float maxDistance;
	float screenPaddingPercent = .1f;

	void Start () {

		if (!trackSideCamera)
			trackSideCamera = gameObject.GetComponent<Camera> ();
		if (!trackSideCamera)
			trackSideCamera = Camera.main;
		ListenFor ("OnSetNewCameraPosition");
	}

	void OnDestroy () {
	}


	void OnDrawGizmos () {
		if (targetCollider != null) {
			Gizmos.color = Color.red;
			Gizmos.DrawWireCube (targetCollider.bounds.center, targetCollider.bounds.size);
		}
	}


	void Update () {

		if ((isEnabled) && (targetCollider != null)) {

			//Set field of view out or in depending on whether anything is outside of the camera's screen coords 
			float changeInFOV = 0f;
			float offCameraAmount = 0f;
			float minOnCameraAmount = float.PositiveInfinity;

			FaceTargetOnXYAxis ();

			//hmm
			float distance = (trackSideCamera.gameObject.transform.position - target.transform.position).magnitude;
			screenPaddingPercent = Mathf.Clamp01 ((distance - distanceAtMinFOV) / maxDistance) * (farPadding - nearPadding) + nearPadding;
			//Plotter.Instance.Plot ("screenPadding", screenPaddingPercent);

			float screenPadding = screenPaddingPercent * Screen.height;
			if (Screen.width < Screen.height)
				screenPadding = screenPaddingPercent * Screen.width;
			Bounds boundingArea = targetCollider.bounds;



			//Top left of bounding area
			Vector3 screenPoint = trackSideCamera.WorldToScreenPoint (boundingArea.center + Vector3.forward * boundingArea.extents.z + Vector3.left * boundingArea.extents.x);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Bottom right of bounding area
			screenPoint = trackSideCamera.WorldToScreenPoint (boundingArea.center + Vector3.back * boundingArea.extents.z + Vector3.right * boundingArea.extents.x);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Top right of bounding area
			screenPoint = trackSideCamera.WorldToScreenPoint (boundingArea.center + Vector3.forward * boundingArea.extents.z + Vector3.right * boundingArea.extents.x);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Bottom left of bounding area
			screenPoint = trackSideCamera.WorldToScreenPoint (boundingArea.center + Vector3.back * boundingArea.extents.z + Vector3.left * boundingArea.extents.x);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			if (offCameraAmount > 0) {
				changeInFOV = Mathf.Clamp (offCameraAmount / 1000, 0, 1);
			} else {
				//Everything in view, so let's find the smallest distance to the border and scale the zoom accordingly
				changeInFOV = Mathf.Clamp (minOnCameraAmount / 1000, 0, 1) * -1;
			}

			trackSideCamera.fieldOfView = Mathf.Clamp (trackSideCamera.fieldOfView + (changeInFOV * cameraSmoothing), 0, 100);

			//trackSideCamera.fieldOfView = Mathf.Clamp01 ((distance - distanceAtMinFOV) / maxDistance) * (maxFOV - minFOV) + minFOV;


		}
	}

	void OnSetNewCameraPosition (Notification n) {

		if (isEnabled) {
			Vector3 position = (Vector3)n.data ["newCameraPosition"];
			Collider c = (Collider)n.data ["collider"];
			GameObject go = c.gameObject;

			if (go == target) {
				//Debug.Log ("Switching to see: " + go.name);

				targetCollider = c;
				trackSideCamera.gameObject.transform.position = position;
				maxDistance = (trackSideCamera.gameObject.transform.position - target.transform.position).magnitude;


			} else {
				//Debug.Log ("Ignoring: " + go.name);
			}
		}
	}

	/// <summary>
	/// Returns the largest distance the gameobject is outside of the padded area.
	/// If everything is inside the padded zone, returns a zero
	/// </summary>
	/// <returns>The largest distance outside of the padded area covered by the trackSideCamera.</returns>
	/// <param name="screenPos">Screen position.</param>
	/// <param name="screenPadding">Screen padding.</param>
	float LargestDistanceOffPaddedArea (Vector3 screenPos, float screenPadding) {

		float returnValue = 0;

		if (screenPos.x < screenPadding)
			returnValue = Mathf.Max (returnValue, -screenPos.x + screenPadding);

		if (screenPos.x > Screen.width - screenPadding)
			returnValue = Mathf.Max (returnValue, screenPos.x - Screen.width + screenPadding);

		if (screenPos.y < screenPadding)
			returnValue = Mathf.Max (returnValue, -screenPos.y + screenPadding);

		if (screenPos.y > Screen.height - screenPadding)
			returnValue = Mathf.Max (returnValue, screenPos.y - Screen.height + screenPadding);

		return returnValue;
	}


	/// <summary>
	/// Returns the smallest distance the gameobject is inside of the padded area.
	/// If anything is outside the padded zone, returns a zero
	/// </summary>
	/// <returns>The smallest distance inside of the padded area covered by the trackSideCamera.</returns>
	/// <param name="screenPos">Screen position.</param>
	/// <param name="screenPadding">Screen padding.</param>
	float SmallestDistanceInsidePaddedArea (Vector3 screenPos, float screenPadding) {

		float returnValue = float.PositiveInfinity;

		returnValue = Mathf.Min (returnValue, screenPos.x - screenPadding);
		returnValue = Mathf.Min (returnValue, Screen.width - screenPadding - screenPos.x);
		returnValue = Mathf.Min (returnValue, screenPos.y - screenPadding);
		returnValue = Mathf.Min (returnValue, Screen.height - screenPadding - screenPos.y);

		if (returnValue < 0) {
			//The bounds are actually outside the camera
			returnValue = 0;
		}

		return returnValue;

	}


	void FaceTargetOnXYAxis () {
		trackSideCamera.gameObject.transform.LookAt (target.transform);
		Quaternion q = trackSideCamera.gameObject.transform.rotation;
		q.eulerAngles = (Vector3.up * q.eulerAngles.y + Vector3.right * q.eulerAngles.x);
		trackSideCamera.gameObject.transform.rotation = q;
	}



	#region ICameraController

	public CameraControllerType GetCameraControllerType () {
		return CameraControllerType.TRACKSIDE_CAMERAS;
	}

	public void Enable () {
		isEnabled = true;
	}

	public void Disable () {
		isEnabled = false;
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


