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

    [SyncVar]
    public int width;

    [SyncVar]
    public int height;

    [SyncVar]
    public int mapCounter;

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
        if (Room.dontDestroyOnLoad) { DontDestroyOnLoad(gameObject); }

        Room.GamePlayers.Add(this);
    }

    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);

        base.OnStopClient();
    }

    [Server]
    public void SetPlayerName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetValues(int width, int height, int mapCounter)
    {
        this.width = width;
        this.height = height;
        this.mapCounter = mapCounter;
    }

    public string GetDisplayName()
    {
        return displayName;
    }    
}
