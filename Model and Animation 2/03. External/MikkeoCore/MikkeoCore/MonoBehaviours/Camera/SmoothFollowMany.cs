/*
 * 
 * SmoothFollowMany
 * 
 * Tracks a list of items with the camera (allItemsToTrack).
 * 
 * If no camera specified, looks inside current object, then finally uses smoothManyCam.main
 * It tracks the bounding box of all the objects, by cameraMovementSmoothing
 * It follows at a Vector3 cameraOffset
 * It adjusts the camera's FOV to get everything in view
 * The camera will go no lower than minFOV and will go no higher than maxFOV
 * 
 * There is a debugging bool you can check to show the bounding box
 * 
 * Notifications:
 * OnRemoveItemToTrackWithCamera -> "item": gameObject
 * OnAddItemToTrackWithCamera -> "item": gameObject
 * 
 * public API
 * RemoveItemToTrackWithCamera( GameObject )
 * AddItemToTrackWithCamera( GameObject )
 * 
 */ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SmoothFollowMany : MonoBehaviour, ICameraController {
	
	public Camera smoothManyCam;
	public Vector3 cameraOffset = Vector3.back * 10f + Vector3.up * 80f;
	public bool freezeYAxis = false;

	public float cameraMovementSmoothing = 5f;
	public float cameraFOVSmoothing = 2f;
	public float cameraRotationSmoothing = .1f;

	//Zoom up to 5x (why would you want more?
	[Range (.2f, 5f)]
	public float userZoom = 1f;
	public float maxFOV = 100f;
	public float minFOV = 25f;
	private float lastZoomUpdate;

	public float screenPadding = 100.0f;
	public bool usePercentageInsteadOfPixels = false;
	public float screenPaddingAsPercentageOfScreen = .1f;
		
	public bool showBoundingArea = false;
	public bool isEnabled = false;

	Bounds boundingArea = new Bounds ();
	public List<GameObject> allItemsToTrack;


	void Start () {
				
		if (!smoothManyCam)
			smoothManyCam = gameObject.GetComponent<Camera> ();
		if (!smoothManyCam)
			smoothManyCam = Camera.main;
		boundingArea = new Bounds ();

		ListenFor ("OnRemoveItemToTrackWithCamera");
		ListenFor ("OnAddItemToTrackWithCamera");
		ListenFor ("OnRemoveAllItemsToTrackWithCamera");
		ListenFor ("OnChangeCameraOffset");
		ListenFor ("OnChangeManualZoom");

	}

	void OnDestroy () {
		StopListeningFor ("OnRemoveItemToTrackWithCamera");
		StopListeningFor ("OnAddItemToTrackWithCamera");
	}

	void Update () {

		if (isEnabled) {
			
			float count = 0;
			foreach (GameObject go in allItemsToTrack) {
						
				if (count == 0) {
					boundingArea = new Bounds (go.transform.position, Vector3.zero);
					count++;
				} else {
					boundingArea.Encapsulate (go.transform.position);
				}

			}
				
			//Slerp camera to bounding centre
			Vector3 targetCamPos = boundingArea.center + cameraOffset;
			if (freezeYAxis)
				targetCamPos.y = cameraOffset.y;
			smoothManyCam.transform.position = Vector3.Slerp (smoothManyCam.transform.position, targetCamPos, Time.deltaTime * cameraMovementSmoothing);

			//Look at the bounding centre
			Quaternion targetRotation = Quaternion.LookRotation (boundingArea.center - smoothManyCam.transform.position);
			targetRotation.eulerAngles = new Vector3 (targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, 0); //smooth to only looking on x
			smoothManyCam.transform.rotation = Quaternion.Slerp (smoothManyCam.transform.rotation, targetRotation, cameraRotationSmoothing * Time.deltaTime);

			//Set field of view out or in depending on whether anything is outside of the camera's screen coords 
			float changeInFOV = 0f;
			float offCameraAmount = 0;
			float minOnCameraAmount = float.PositiveInfinity;

			// 1/userzoom means highZoom, zoom in
			float boundingZ = boundingArea.extents.z * 1 / userZoom;
			float boundingX = boundingArea.extents.x * 1 / userZoom;

			//Top left of bounding area
			Vector3 screenPoint = smoothManyCam.WorldToScreenPoint (boundingArea.center + Vector3.forward * boundingZ + Vector3.left * boundingX);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Bottom right of bounding area
			screenPoint = smoothManyCam.WorldToScreenPoint (boundingArea.center + Vector3.back * boundingZ + Vector3.right * boundingX);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Top right of bounding area
			screenPoint = smoothManyCam.WorldToScreenPoint (boundingArea.center + Vector3.forward * boundingZ + Vector3.right * boundingX);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			//Bottom left of bounding area
			screenPoint = smoothManyCam.WorldToScreenPoint (boundingArea.center + Vector3.back * boundingZ + Vector3.left * boundingX);
			offCameraAmount = Mathf.Max (offCameraAmount, LargestDistanceOffPaddedArea (screenPoint, screenPadding));
			minOnCameraAmount = Mathf.Min (minOnCameraAmount, SmallestDistanceInsidePaddedArea (screenPoint, screenPadding));

			if (offCameraAmount > 0) {
				changeInFOV = Mathf.Clamp (offCameraAmount / 1000, 0, 1);
			} else {
				//Everything in view, so let's find the smallest distance to the border and scale the zoom accordingly
				changeInFOV = Mathf.Clamp (minOnCameraAmount / 1000, 0, 1) * -1;
			}

			if (Time.realtimeSinceStartup - lastZoomUpdate < 1) {
				//fast zoom (pinching)
				smoothManyCam.fieldOfView += changeInFOV * 50;
			} else {
				//normal
				float smoothedFOV = smoothManyCam.fieldOfView + (changeInFOV * cameraFOVSmoothing * Time.deltaTime);
				smoothManyCam.fieldOfView = Mathf.Clamp (smoothedFOV, minFOV, maxFOV);
			}

		}
	}

	/// <summary>
	/// Returns the largest distance the gameobject is outside of the padded area.
	/// If everything is inside the padded zone, returns a zero
	/// </summary>
	/// <returns>The largest distance outside of the padded area covered by the smoothManyCam.</returns>
	/// <param name="screenPos">Screen position.</param>
	/// <param name="screenPadding">Screen padding.</param>
	float LargestDistanceOffPaddedArea (Vector3 screenPos, float screenPadding) {

		float left = 0;
		float right = 0;
		float top = 0;
		float bottom = 0;

		if (usePercentageInsteadOfPixels) {

			float pixelPaddingFromPercentage = screenPaddingAsPercentageOfScreen * Screen.width;
			if (screenPos.x < pixelPaddingFromPercentage)
				left = Mathf.Max (left, -screenPos.x + pixelPaddingFromPercentage); //left
			if (screenPos.x > Screen.width - pixelPaddingFromPercentage)
				right = Mathf.Max (right, screenPos.x - Screen.width + pixelPaddingFromPercentage); //right
			
			pixelPaddingFromPercentage = screenPaddingAsPercentageOfScreen * Screen.height;
			if (screenPos.y < pixelPaddingFromPercentage)
				bottom = Mathf.Max (bottom, -screenPos.y + pixelPaddingFromPercentage);  //bottom
			if (screenPos.y > Screen.height - pixelPaddingFromPercentage)
				top = Mathf.Max (top, screenPos.y - Screen.height + pixelPaddingFromPercentage); //top

		} else {

			if (screenPos.x < screenPadding)
				left = Mathf.Max (left, -screenPos.x + screenPadding); //too far left	
			if (screenPos.x > Screen.width - screenPadding)
				right = Mathf.Max (right, screenPos.x - Screen.width + screenPadding); //too far right
		
			if (screenPos.y < screenPadding)
				bottom = Mathf.Max (bottom, -screenPos.y + screenPadding); //bottom
			if (screenPos.y > Screen.height - screenPadding)
				top = Mathf.Max (top, screenPos.y - Screen.height + screenPadding); //top
		}

		float maxY = Mathf.Max (bottom, top);
		float maxX = Mathf.Max (left, right);

		if (maxY > maxX) {
			return maxY;
		} else {
			return maxX;
		}


	}


	/// <summary>
	/// Returns the smallest distance the gameobject is inside of the padded area.
	/// If anything is outside the padded zone, returns a zero
	/// </summary>
	/// <returns>The smallest distance inside of the padded area covered by the smoothManyCam.</returns>
	/// <param name="screenPos">Screen position.</param>
	/// <param name="screenPadding">Screen padding.</param>
	float SmallestDistanceInsidePaddedArea (Vector3 screenPos, float screenPadding) {

		//bottom left = 0,0

		float left = float.PositiveInfinity;
		float right = float.PositiveInfinity;
		float top = float.PositiveInfinity;
		float bottom = float.PositiveInfinity;

		if (usePercentageInsteadOfPixels) {

			float pixelPaddingFromPercentage = screenPaddingAsPercentageOfScreen * Screen.width;

			left = Mathf.Min (left, screenPos.x - pixelPaddingFromPercentage); //left
			right = Mathf.Min (right, Screen.width - pixelPaddingFromPercentage - screenPos.x); //right

			pixelPaddingFromPercentage = screenPaddingAsPercentageOfScreen * Screen.height;

			bottom = Mathf.Min (bottom, screenPos.y - pixelPaddingFromPercentage); //bottom
			top = Mathf.Min (top, Screen.height - pixelPaddingFromPercentage - screenPos.y); //top

		} else {
			left = Mathf.Min (left, screenPos.x - screenPadding); //left
			right = Mathf.Min (right, Screen.width - screenPadding - screenPos.x); //right

			bottom = Mathf.Min (bottom, screenPos.y - screenPadding); //bottom
			top = Mathf.Min (top, Screen.height - screenPadding - screenPos.y); //top
		}

		if ((left < 0) || (right < 0) || (top < 0) || (bottom < 0)) {
			//The bounds are actually outside the camera
			return 0;
		}

		float minY = Mathf.Min (bottom, top);
		float minX = Mathf.Min (left, right);

		if (minY < minX) {
			return minY;
		} else {
			return minX;
		}


	}

	public void AddItemToTrackWithCamera (GameObject go) {
		allItemsToTrack.Add (go);
	}

	void OnAddItemToTrackWithCamera (Notification n) {
		GameObject go = (GameObject)n.data ["item"];
		AddItemToTrackWithCamera (go);
		PostNotification ("OnNewBoundingArea", new Hashtable (){ { "position",boundingArea.center } });
	}


	public void RemoveItemToTrackWithCamera (GameObject item) {
		foreach (GameObject go in allItemsToTrack) {
			if (go == item) {
				allItemsToTrack.Remove (go);
			}
		}
	}

	void OnRemoveItemToTrackWithCamera (Notification n) {
		GameObject item = (GameObject)n.data ["item"];
		RemoveItemToTrackWithCamera (item);
	}

	void OnRemoveAllItemsToTrackWithCamera (Notification n) {
		allItemsToTrack.Clear ();
	}

	void OnDrawGizmos () {
		if (showBoundingArea) {
			Gizmos.color = Color.grey;
			Gizmos.DrawWireCube (boundingArea.center, boundingArea.size);
		}
	}

	void OnChangeCameraOffset (Notification n) {
		Vector3 newOffset = (Vector3)n.data ["cameraOffset"];
		cameraOffset = newOffset;
	}

	//Override zoom, allowing user to pinch or click a button for a set zoom level
	void OnChangeManualZoom (Notification n) {

		userZoom = (float)n.data ["manualZoom"];
		lastZoomUpdate = Time.realtimeSinceStartup;
	}

	#region ICameraController

	public CameraControllerType GetCameraControllerType () {
		return CameraControllerType.SMOOTH_FOLLOW_MANY;
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


