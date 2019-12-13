using System.Collections;
using System.Collections.Generic;
using Mikkeo.Extensions;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class GameMan : Singleton<GameMan>, INotify {
    public int playerCount;

    public int gameDuration;
    public float voteOneTime;
    public float voteTwoTime;
    public float voteDuration;
    int[] response;
    int numberOfResponse;
    // Start is called before the first frame update
    void Start () {
        Invoke ("Vote", voteOneTime);
        Invoke ("Vote", voteTwoTime);

        ListenFor ("OnVoteResponse");
    }

    IEnumerator Vote () {
        response = new int[playerCount];
        numberOfResponse = 0;
        for (int i = 0; i < playerCount; i++) {
            response[i] = 0;
        }

        PostSimpleNotification ("OnVote");

        yield return new WaitForSeconds (voteDuration);
    }
    void OnVoteResponse (Notification n) {
        int vote = (int) n.data["Vote"];
        response[vote]++;
    }

    #region iNotify

    public void ListenFor (string messageName) {
        NotificationCenter.DefaultCenter.AddObserver (this, messageName);
    }

    public void StopListeningFor (string messageName) {
        NotificationCenter.DefaultCenter.RemoveObserver (this, messageName);
    }

    public void PostSimpleNotification (string notificationName) {
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName));
    }

    public void PostNotification (string notificationName, Hashtable parameters, string debugMessage = "") {
        if (debugMessage != "")
            Debug.Log (debugMessage);
        NotificationCenter.DefaultCenter.PostNotification (new Notification (this, notificationName, parameters));
    }

    #endregion
}