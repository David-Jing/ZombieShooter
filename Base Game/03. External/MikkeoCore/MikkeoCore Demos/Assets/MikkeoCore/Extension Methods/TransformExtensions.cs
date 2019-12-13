using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mikkeo.Extensions {
	
	public static class TransformExtensions {

		/*
		 * Transform modifiers
		 */
		public static void ResetTransform (this Transform t) {
			t.position = new Vector3 (0, 0, 0);
			t.rotation = Quaternion.identity;
			t.localScale = new Vector3 (1f, 1f, 1f);
		}
		
		// Finds the first child, grandchild etc with the specified name
		public static Transform FindChildByName (this Transform parent, string part) {
			foreach (Transform t in parent.GetComponentsInChildren<Transform>().Where(t => t.gameObject.name == part)) {
				return t;
			}
			Debug.LogWarning ("Component " + part + " not found as child of " + parent.name);
			return null;
		}
		
		
		// Finds the first child, grandchild etc with the specified tag
		public static Transform FindChildByTag (this Transform parent, string tag) {
			foreach (Transform t in parent.GetComponentsInChildren<Transform>()) {
				if (t != parent && t.tag == tag)
					return t;
			}
			Debug.LogWarning ("Tag " + tag + " not found in any children of " + parent.name);
			return null;
		}
		
		// Finds all the children, grandchildren with the specified tags
		public static Transform[] FindChildrenByTag (this Transform parent, string tag) {
			return parent.GetComponentsInChildren<Transform> ().Where (t => t != parent && t.tag == tag).ToArray ();
		}
		
		// Set the layer on this and any of its children, grandchildren etc
		public static void SetLayerOnAll (this Transform trans, string layerName, bool includeInactive) {
			foreach (Transform t in trans.GetComponentsInChildren<Transform>(includeInactive)) {
				t.gameObject.layer = LayerMask.NameToLayer (layerName);
			}
		}
		
		// Set the tag on this and any of its children, grandchildren etc
		public static void SetTagOnAll (this Transform trans, string tagName, bool includeInactive) {
			foreach (Transform t in trans.GetComponentsInChildren<Transform>(includeInactive)) {
				t.gameObject.tag = tagName;
			}
		}

		public static void SetPositionX (this Transform t, float value) {
			Vector3 v = t.position;
			v.x = value;
			t.position = v;
		}

		public static void SetPositionY (this Transform t, float value) {
			Vector3 v = t.position;
			v.y = value;
			t.position = v;
		}

		public static void SetPositionZ (this Transform t, float value) {
			Vector3 v = t.position;
			v.z = value;
			t.position = v;
		}

		public static void SetLocalPositionX (this Transform t, float value) {
			Vector3 v = t.localPosition;
			v.x = value;
			t.localPosition = v;
		}

		public static void SetLocalPositionY (this Transform t, float value) {
			Vector3 v = t.localPosition;
			v.y = value;
			t.localPosition = v;
		}

		public static void SetLocalPositionZ (this Transform t, float value) {
			Vector3 v = t.localPosition;
			v.z = value;
			t.localPosition = v;
		}

		public static void SetLocalScaleX (this Transform t, float value) {
			Vector3 v = t.localScale;
			v.x = value;
			t.localScale = v;
		}

		public static void SetLocalScaleY (this Transform t, float value) {
			Vector3 v = t.localScale;
			v.y = value;
			t.localScale = v;
		}

		public static void SetLocalScaleZ (this Transform t, float value) {
			Vector3 v = t.localScale;
			v.z = value;
			t.localScale = v;
		}
		
		



	}

}
