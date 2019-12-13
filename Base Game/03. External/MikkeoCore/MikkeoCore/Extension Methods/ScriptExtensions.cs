using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mikkeo.Extensions {
	
	public static class ScriptExtensions {

		
		public static void PostNotification (this Component script, string notificationName, Hashtable parameters) {
			NotificationCenter.DefaultCenter.PostNotification (new Notification (script, notificationName, parameters));
		}

	}

}
