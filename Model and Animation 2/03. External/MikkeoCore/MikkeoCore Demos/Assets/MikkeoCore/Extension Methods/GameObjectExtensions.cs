using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mikkeo.Extensions {
	
	public static class GameObjectExtensions {

		
		/* 
		 * GameObject modifiers
		 */
		public static bool HasRigidbody (this GameObject gobj) {
			return (gobj.GetComponent<Rigidbody> () != null);
		}

		public static bool HasAnimation (this GameObject gobj) {
			return (gobj.GetComponent<Animation> () != null);
		}

		public static void SetLayerRecursively (this GameObject gameObject, int layer) {
			gameObject.layer = layer;
			foreach (Transform t in gameObject.transform)
				t.gameObject.SetLayerRecursively (layer);
		}

		public static void SetTagRecursively (this GameObject gameObject, string tag) {
			gameObject.tag = tag;
			foreach (Transform t in gameObject.transform)
				t.gameObject.SetTagRecursively (tag);
		}

		public static void SetLayerRecursivelyByName (this GameObject gameObject, string layerName) {
			int layer = LayerMask.NameToLayer (layerName);
			gameObject.layer = layer;
			foreach (Transform t in gameObject.transform)
				t.gameObject.SetLayerRecursively (layer);
		}

		public static void SetSpriteLayerRecursively (this GameObject gameObject, string layerName) {
			//Change sprite renderers to new level
			SpriteRenderer[] srs = (SpriteRenderer[])gameObject.GetComponentsInChildren<SpriteRenderer> ();
			foreach (SpriteRenderer sr in srs) {
				sr.sortingLayerName = layerName;
			}		

		}

		public static void SetCollisionRecursively (this GameObject gameObject, bool tf) {
			Collider[] colliders = gameObject.GetComponentsInChildren<Collider> ();
			foreach (Collider collider in colliders)
				collider.enabled = tf;
		}

		public static void SetVisualRecursively (this GameObject gameObject, bool tf) {
			Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer> ();
			foreach (Renderer renderer in renderers)
				renderer.enabled = tf;
		}

		public static int GetCollisionMask (this GameObject gameObject, int layer = -1) {
			if (layer == -1)
				layer = gameObject.layer;
			
			int mask = 0;
			for (int i = 0; i < 32; i++)
				mask |= (Physics.GetIgnoreLayerCollision (layer, i) ? 0 : 1) << i;
			
			return mask;
		}



	}

}
