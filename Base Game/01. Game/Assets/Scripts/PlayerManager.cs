using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct ThePlayer {
    public int HP;
    public float movementSpeed;
    public float fireRate;
    public GameObject player;
    public bool isTraitor;
}

public class CharManager : MonoBehaviour, INotify {
    public Transform spawnCentre;
    public Transform playerContainer;
    public GameObject playerPrefab;
    public int maxPlayerHP;
    public float defaultMovementSpeed;
    public float defaultFireRate;
    public int numberOfPlayers;
    public float playerSpawnRange;
    public float minimumBuffRange;
    public float buffRate;
    Dictionary<string, ThePlayer> PlayerDict = new Dictionary<string, ThePlayer> ();

    void Start () { }

    void OnPlayerCreation (Notification n) {
        int spawnAngle = 0;
        int playerLabel = 0;

        int traitor = Random.Range (0, 4);

        for (int i = 0; i < numberOfPlayers; i++) {
            ThePlayer newPlayer = new ThePlayer ();
            newPlayer.HP = maxPlayerHP;
            newPlayer.fireRate = defaultFireRate;
            newPlayer.movementSpeed = defaultMovementSpeed;

            if (i == traitor) {
                newPlayer.isTraitor = true;
            } else {
                newPlayer.isTraitor = false;
            }

            newPlayer.player = Instantiate (playerPrefab, spawnCentre);

            newPlayer.player.transform.localPosition = new Vector3 (playerSpawnRange, 0, 0);
            spawnAngle += 90;
            spawnCentre.transform.Rotate (new Vector3 (0, spawnAngle, 0));

            newPlayer.player.transform.SetParent (playerContainer);
            newPlayer.player.transform.name = playerLabel.ToString ();

            PlayerDict.Add ((playerLabel++).ToString (), newPlayer);
        }
    }
    void OnPlayerProximity (Notification n) {

        float average = 0;
        GameObject thisPlayer;
        GameObject comparedPlayer;
        string thisName;
        string comparedName;

        for (int i = 0; i < PlayerDict.Count; i++) {
            thisName = "Player " + i;
            thisPlayer = PlayerDict[thisName].player;
            average = 0;
            for (int j = 0; j < PlayerDict.Count - 1; j++) {
                if (j != i) {
                    comparedName = "Player " + j;
                    comparedPlayer = PlayerDict[comparedName].player;
                    average += Vector3.Distance (thisPlayer.transform.position, comparedPlayer.transform.position);
                }
            }
            average = average / (PlayerDict.Count - 1);

            float buff = Mathf.Log (average / buffRate, 2);

            if (buff < 0) {
                buff = 1;
            }

            BuffStats (buff, PlayerDict[thisName]);
        }
    }
    void OnPlayerHit (Notification n) {
        GameObject player = (GameObject) n.data["playerHit"];
        ThePlayer selectPlayer;

        if (PlayerDict.TryGetValue (player.name, out selectPlayer)) {
            if (selectPlayer.HP < 1) {
                PlayerDict.Remove (player.name);
                // ---------Player Death Animation------
                //selectPlayer.player.GetComponent<PlayerPrefab> ().Die ();
                // -------------------------------------
            } else {
                selectPlayer.HP--;
                // ---------Player Hit Animation--------
                //selectPlayer.player.GetComponent<PlayerPrefab> ().Hit ();
                // -------------------------------------
            }
        };
    }

    public float movementSpeed;
    public float fireRate;
    public GameObject player;
    public bool isTraitor;

    void BuffStats (float buff, ThePlayer thisPlayer) {
        float ms = defaultMovementSpeed * buff;
        float fr = defaultFireRate * buff;
        GameObject player = PlayerDict[name].player;
        bool it = PlayerDict[name].isTraitor;

        PlayerDict.Remove (thisPlayer.player.name);

        ThePlayer newPlayer = new ThePlayer ();
        newPlayer.player = player;
        newPlayer.movementSpeed = ms;
        newPlayer.fireRate = fr;
        newPlayer.isTraitor = it;

        PlayerDict.Add (thisPlayer.player.name, newPlayer);
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