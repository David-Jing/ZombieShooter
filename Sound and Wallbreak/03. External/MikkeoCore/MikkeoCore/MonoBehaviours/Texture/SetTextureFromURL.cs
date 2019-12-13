using UnityEngine;
using System.Collections;

public class SetTextureFromURL : MonoBehaviour
{

	public int mapSize = 30;
	public float yPosition = 4f;
	public string imageRatioXxY = "1280x777";
	public string url = "http://www.sfu.ca/content/sfu/students/recreation/facility/location/_jcr_content/main_content/image.img.jpg/1381340313917.rendition-large.jpg";
	public float scaleOverride = 1f;

	public bool reload = true;


	void Update ()
	{

		if (reload) {
			reload = false;
			StartCoroutine ("LoadMapOverlay");
		}

	}

	IEnumerator LoadMapOverlay ()
	{
		WWW www = new WWW (url);
		yield return www;
		MeshRenderer mr = gameObject.GetComponent<MeshRenderer> ();
		mr.sharedMaterial.mainTexture = www.texture;
		string[] xAndY = imageRatioXxY.Split ('x');
		int xSize = int.Parse (xAndY [0]);
		int ySize = int.Parse (xAndY [1]);
		float yRatio = ySize * 1f / xSize * 1f;
		Debug.Log (xSize + "|" + ySize + "|" + yRatio);
		transform.localScale = new Vector3 (mapSize * 5f, 1f, mapSize * 5f * yRatio) * scaleOverride;
		transform.localPosition = Vector3.up * yPosition;
		transform.RotateAround (transform.position, Vector3.up, 180f);
	}
}