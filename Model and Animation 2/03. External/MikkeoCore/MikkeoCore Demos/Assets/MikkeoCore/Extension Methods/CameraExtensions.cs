using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mikkeo.Extensions {
	
	public static class CameraExtensions {
		
		public static Vector3 GetWorldPositionAtScreenTopCentre (this Camera camera) {
			int ScreenHeight = Screen.height;
			int ScreenWidth = Screen.width;
			Vector3 pos = Camera.main.ScreenToWorldPoint (new Vector3 (.5f * ScreenWidth, 1f * ScreenHeight, 5f));
			return pos;
		}

	}

}
