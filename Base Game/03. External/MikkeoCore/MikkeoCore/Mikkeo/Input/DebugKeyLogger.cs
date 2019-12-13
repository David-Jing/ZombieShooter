using UnityEngine;
using System.Collections;

namespace Mikkeo.Input {

	//public class DebugKeyLogger : MonoBehaviour {
	public sealed class DebugKeyLogger : Singleton<DebugKeyLogger> {
		
		public int codeLength = 3;
		public string notificationName = "OnDebugCode";
		public string keyName = "debugCode";

		private string keySequence = "   ";
		private string allowedLetters = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		void OnGUI () {
				
			Event e = Event.current;
		
			foreach (char c in allowedLetters.ToCharArray()) {
				//Debug.Log (c);
				if (e.Equals (Event.KeyboardEvent (c.ToString ()))) {
					//Debug.Log (c.ToString() + " pressed");
					keySequence += c.ToString ();
					keySequence = keySequence.Substring (1, codeLength);	
					//Debug.Log (keySequence);
				
					PostNotification (notificationName, new Hashtable (){ { keyName , keySequence } });
				
					break;
				}
			}
		
		}

		void PostNotification (string notificationName, Hashtable parameters) {
			//Debug.Log ("Posting: "+notificationName);
			NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName, parameters));		
		}
	
	}
}
