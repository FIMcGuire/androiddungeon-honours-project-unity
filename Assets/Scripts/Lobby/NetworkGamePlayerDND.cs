using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class NetworkGamePlayerDND : NetworkBehaviour
{

    [SyncVar]
    private string displayName = "Loading...";

    private NetworkManagerDND room;
    private NetworkManagerDND Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerDND;
        }
    }

    public override void OnStartClient()
    {
        //Likely not useful, unlikely to change scenes as App intended for combat ONLY
        DontDestroyOnLoad(gameObject);

        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }


}
