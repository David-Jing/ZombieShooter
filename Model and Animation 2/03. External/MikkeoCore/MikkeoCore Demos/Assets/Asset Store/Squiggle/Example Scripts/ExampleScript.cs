using UnityEngine;
using System.Collections;

public class ExampleScript : MonoBehaviour {
	public bool BooleanValue;
	public float FloatValue;
	public int IntValue;
	public Vector2 Vector2Value;
	public Vector3 Vector3Value;
	public Color ColorValue;
	public Color32 Color32Value;
	public string StringValue;

	//int iSinTest = 0;
	//int iCosTest = 0;

	void Update () {
		BooleanValue = Mathf.Repeat (Time.time, 1.0f) > 0.5f;

		FloatValue = Mathf.PingPong (Time.realtimeSinceStartup, 1.0f);

		IntValue = Mathf.FloorToInt (FloatValue * 10.0f);

		Vector2Value = GetComponent<Rigidbody2D> ().velocity;

		Vector3Value = transform.position;

		ColorValue = Color.Lerp (DebugGraph.DefaultBlue, new Color (1.0f, 0.75f, 0.25f), Mathf.PingPong (Time.realtimeSinceStartup, 1.0f));

		Color32Value = ColorValue;

		StringValue = "Hello World! The Current Frame Number Is: " + Time.frameCount;

		float sin = Mathf.Sin (Mathf.Repeat (Time.time, 6.28f));
		float cos = Mathf.Cos (Mathf.Repeat (Time.time, 6.28f));

		DebugGraph.Log ("Color Gradient", ColorValue);

		DebugGraph.Write ("String", StringValue);

		DebugGraph.Log ("Vector3", Input.mousePosition);

		DebugGraph.Draw (new Vector2 (sin * Time.time, cos * Time.time));

		DebugGraph.MultiLog ("Related Variables", DebugGraph.DefaultRed, sin);
		DebugGraph.MultiLog ("Related Variables", DebugGraph.DefaultGreen, cos);
	}
}
