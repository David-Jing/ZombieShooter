using UnityEngine;
using System.Collections;

public enum CameraControllerType {
	SMOOTH_FOLLOW_2,
	SMOOTH_FOLLOW_MANY,
	TRACKSIDE_CAMERAS,
	GIMBAL_MOUSE_LOOK
}


public interface ICameraController:IEnableable {
	
	CameraControllerType GetCameraControllerType ();

}


