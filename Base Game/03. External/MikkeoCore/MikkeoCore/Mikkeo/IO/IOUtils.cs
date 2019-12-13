using UnityEngine;
using System.Collections;

namespace Mikkeo.IO {

	/// <summary>
	/// IO utils.
	/// </summary>
	public sealed class IOUtils : Singleton<IOUtils> {

		/// <summary>
		/// Loads the prefab from given path, parents it to gameobject, and resets position and rotation.
		/// </summary>
		/// <returns>The loaded prefab.</returns>
		/// <param name="pathToResource">Path to prefab.</param>
		/// <param name="parentPrefab">Parent prefab.</param>
		public GameObject LoadPrefabFromPath (string pathToResource, GameObject parentPrefab) {

			GameObject go = null;

			try {
				go = (GameObject)GameObject.Instantiate (Resources.Load<GameObject> (pathToResource));

				if (parentPrefab) {
					go.transform.parent = parentPrefab.transform;
					go.transform.localPosition = Vector3.zero;
					go.transform.rotation = Quaternion.identity;
				}
			} catch {
				Debug.Log ("Uh oh, problem loading: |" + pathToResource + "|");
			}

			return go;

		}
	}

}

