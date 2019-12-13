/// GimbalMouseLook rotates the transform based on the mouse delta.
/// Minimum and Maximum values can be used to constrain the possible rotation

/// To make an FPS style character:
/// - Create a capsule.
/// - Add the MouseLook script to the capsule.
///   -> Set the mouse look to use LookX. (You want to only turn character but not tilt it)
/// - Add FPSInputController script to the capsule
///   -> A CharacterMotor and a CharacterController component will be automatically added.

/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add a MouseLook script to the camera.
///   -> Set the mouse look to use LookY. (You want the camera to tilt up and down like a head. The character already turns.)

using UnityEngine;
using System.Collections;
using Mikkeo.Extensions;
using System.Collections.Specialized;

public class GimbalMouseLook : MonoBehaviour,ICameraController {

	public enum RotationAxes {
		MouseXAndY = 0,
		MouseX = 1,
		MouseY = 2
	}

	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 3F;
	public float sensitivityY = 2F;

	public float minimumX = -360F;
	public float maximumX = 360F;

	public float minimumY = -5F;
	public float maximumY = 20F;

	float lastRotationY = 0F;
	float rotationY = 0F;

	public bool isEnabled = false;

	public Vector3 cameraOffset;
	public Transform itemCameraIsMountedTo;

	GameObject cameraMount;
	GimbalMouseLookCameraMount cameraMountScript;


	void Start () {
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ())
			GetComponent<Rigidbody> ().freezeRotation = true;
		InvokeRepeating ("SendWhatWereLookingAt", 1, .5f);
	}


	void Update () {

		if ((isEnabled) && (cameraMount != null)) {

			if (axes == RotationAxes.MouseXAndY) {
				float rotationX = cameraMount.transform.localEulerAngles.y + Input.GetAxis ("Mouse X") * sensitivityX;

				rotationY += Input.GetAxis ("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

				cameraMount.transform.localEulerAngles = new Vector3 (-rotationY, rotationX, 0);

			} else if (axes == RotationAxes.MouseX) {
				cameraMount.transform.Rotate (0, Input.GetAxis ("Mouse X") * sensitivityX, 0);

			} else {
				rotationY += Input.GetAxis ("Mouse Y") * sensitivityY;
				rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);

				cameraMount.transform.localEulerAngles = new Vector3 (-rotationY, cameraMount.transform.localEulerAngles.y, 0);
			}
		}
	}


	void SendWhatWereLookingAt () {
		if (lastRotationY != rotationY) {

			//This is to tell any element of the game the vector were are looking at while the mouse is moving
			//PostNotification ("OnNewLookRotation", new Hashtable (){ { "yRotation",rotationY } });
			PostNotification ("OnNewLookRotation", new Hashtable (){ { "camRotation", transform.rotation } });
			lastRotationY = rotationY;
		}
	}


	#region ICameraController

	public CameraControllerType GetCameraControllerType () {
		return CameraControllerType.GIMBAL_MOUSE_LOOK;
	}

	public void Enable () {

		if (itemCameraIsMountedTo) {
			
			if (cameraMount == null) {
				
				cameraMount = new GameObject ();
				cameraMount.name = "Camera Mount";

				//Now attach the GimbalMouseLookCameraMount script to track the position of the item we are "mounted" on
				cameraMountScript = cameraMount.AddComponent <GimbalMouseLookCameraMount> ();
				cameraMountScript.itemCameraIsMountedTo = itemCameraIsMountedTo;
			}

			//Set mount to exact position of camera and parent it
			cameraMount.transform.position = transform.position;
			cameraMount.transform.rotation = transform.rotation;
			transform.parent = cameraMount.transform;

			//move away by cameraoffset, and reset camera to match the item we are mounted on
			transform.localPosition = cameraOffset;
			transform.localRotation = Quaternion.identity;

			cameraMountScript.isEnabled = true;
			isEnabled = true;

		} else {
			Debug.Log ("There is nothing to attach this camera to!");
		}


	}

	public void Disable () {
		isEnabled = false;
		cameraMountScript.isEnabled = false;
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
