/*
 * 
 * Atlas Manager
 * 
 * remaps the UVs when it hears a notification with data "atlasManagerName" matches atlasManagerName
 * this allows multiple atlasmanagers to be controlled individually by Notifications
 * 
 * atlasManagerName (Notifications send this to remap)
 * atlasName - atlas to load from resources the atlas file and remap any matching meshes' to the atlas
 * 
 * */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

public class AtlasManager : MonoBehaviour
{
	
	public string atlasManagerName;  //ATLAS IDENTIFIER - Identifies this atlas so when a notification comes in, we swap the uvs on this atlas, not a different one
	public string gameObjectNameToSwapAtlasIn; // MESHFILTER - Name of MeshFilter in hierarchy to swap?  Local search?
	public string atlasName; // TEXTURE - The path to the atlas for loading, eg: "Billboards/1x4_billboard_atlas"
	public string spriteFileName; //Filename INSIDE TEXTURE PACKER - Filename of the sprite INSIDE the teture to use - saved by Texture Packer
	public bool saveUVsManually = false;

	Dictionary<string,object> textureInfo = new Dictionary<string, object> ();
	Dictionary<string,object> metaInfo = new Dictionary<string, object> ();
	Dictionary<string,object> resolutionInfo = new Dictionary<string, object> ();
	Dictionary<string,object> spriteData = new Dictionary<string, object> ();
	Dictionary<string,object> spriteScalingInfo = new Dictionary<string, object> ();
	long atlasWidth;
	long atlasHeight;
	Vector2[] originalUVs;
	Mesh mesh;
	
	public bool isReloading = false;

	void Awake ()
	{
		//Listen to whether we move the uvs or not by changing the sprite filename, and remapping
		NotificationCenter.DefaultCenter.AddObserver (this, "OnSpriteChanged");
	}

	IEnumerator Start ()
	{				
		yield return new WaitForSeconds (.01f);
		
		LoadAtlasInfo ();
		
		//Debug.Log ("W: " + atlasWidth + ", H:" + atlasHeight);
		
		/*
		meta": {
		"app": "http://www.texturepacker.com",
		"version": "1.0",
		"image": "atlas.png",
		"format": "RGBA8888",
		"size": {"w":1024,"h":1024},
		...
		*/
		if (!saveUVsManually) {

			SaveOriginalUVs ();				
			RemapUVs (spriteFileName);
		}

	}
	
	//NotificationCenter
	void OnSpriteChanged (Notification notification)
	{	
		Hashtable spriteInfo = notification.data;
		if (spriteInfo ["atlasManagerName"] as string == atlasManagerName) {
			RemapUVs (spriteInfo ["spriteFileName"] as string);
		}
	}
	
	
	
	void Update ()
	{
		if (isReloading) {
			isReloading = false;
			RemapUVs (spriteFileName);
		}
	}
	
	
	public void SaveOriginalUVs ()
	{
		
		//Get the FIRST mesh filter named appropriately
		foreach (MeshFilter mf in transform.GetComponentsInChildren<MeshFilter>()) {
			if (mf.name == gameObjectNameToSwapAtlasIn) {
				mesh = mf.mesh;
				break;
			}
		}
		//Renderer renderer = gameObjectToSwapToAtlas.GetComponent<MeshRenderer> ().renderer;
		
		originalUVs = (Vector2[])mesh.uv;
		
	}
	
	public void RemapUVs (string fileName)
	{
				
		if (textureInfo.Count == 0)
			LoadAtlasInfo ();
		
		//Debug.Log ("Filename is: "+fileName);
		
		//Get the sprite data
		try {
			
			spriteData = textureInfo [fileName] as Dictionary<string,object>;
			spriteScalingInfo = spriteData ["frame"] as Dictionary<string,object>;
			
			long spriteWidth = (long)spriteScalingInfo ["w"];
			long spriteHeight = (long)spriteScalingInfo ["h"];
			long spriteX = (long)spriteScalingInfo ["x"];
			long spriteY = (long)spriteScalingInfo ["y"];
			
			//			Debug.Log (spriteWidth + "|" + spriteHeight + "|" + spriteX + "|" + spriteY);
			
			//			foreach (var item in spriteScalingInfo as Dictionary<string,object>) {
			//				Debug.Log (item.Key);
			//			}
			
			
			Vector2[] newUVs = new Vector2[originalUVs.Length];
			int i = 0;
			
			float spriteWidthAsPercentage = (float)spriteWidth / (float)atlasWidth;
			float spriteHeightAsPercentage = (float)spriteHeight / (float)atlasHeight;
			//Debug.Log ("sx: " + spriteX + " sw: " + spriteWidth + " aw: " + atlasWidth + " ssp: " + spriteSizeAsPercentage);
			
			while (i < originalUVs.Length) {
				
				newUVs [i].x = (originalUVs [i].x * spriteWidthAsPercentage) + ((float)spriteX / (float)atlasWidth);
				newUVs [i].y = 1 - ((1 - originalUVs [i].y) * spriteHeightAsPercentage) - ((float)spriteY / (float)atlasHeight);
				i++;
				
				//Debug.Log (originalUVs [i] + " -->" + newUVs [i]);
			}
			mesh.uv = newUVs;
			
		} catch (System.Exception e) {

			if (e.ToString () == "Mooo") {
				Debug.Log ("This is to prevent an editor warning.");
			}

			// A texture name incorrectly assigned here
			//Debug.Log (gameObject.name + ": We can't find the key: " + fileName + " in the atlas " + atlasName + " [" + e.ToString () + "]");
			//Debug.Log (e);
		}
		
		
	}
	
	void LoadAtlasInfo ()
	{
	
		if (string.IsNullOrEmpty (atlasName)) {
			Debug.Log ("Atlas name not set for " + gameObject.name);
			return;
		}
		
		//Debug.Log ("Loading " + atlasName); 
	
	
		TextAsset textAsset = (TextAsset)Resources.Load (atlasName, typeof(TextAsset));
		//Debug.Log (textAsset);
		string jsonString = textAsset.ToString ();
		Dictionary<string,object> dict = Json.Deserialize (jsonString) as Dictionary<string,object>;

		/*
		 * Texture Packer data:
		 * 
		"frame": {"x":134,"y":2,"w":64,"h":64},
		"rotated": false,
		"trimmed": false,
		"spriteSourceSize": {"x":0,"y":0,"w":64,"h":64},
		"sourceSize": {"w":64,"h":64}
		*/
		
		textureInfo = dict ["frames"] as Dictionary<string,object>;
		metaInfo = dict ["meta"] as Dictionary<string,object>;
		resolutionInfo = metaInfo ["size"] as Dictionary<string,object>;
		atlasWidth = (long)resolutionInfo ["w"];
		atlasHeight = (long)resolutionInfo ["h"];
	
		
	}
	
}