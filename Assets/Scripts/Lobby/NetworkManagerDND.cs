using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Net.Sockets;
using System.Net;

public class NetworkManagerDND : NetworkManager
{
    //min number of players in order to start game
    [SerializeField] private int minPlayers = 2;
    //scene for the menuScene (i.e. scene game starts on)
    [Scene] [SerializeField] private string menuScene = string.Empty;

    GameObject spawnSystem;

    Pathfinding pathfinding;

    //prefab for lobby client object
    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerDND roomPlayerPrefab = null;

    //prefab for game client object
    [Header("Game")]
    [SerializeField] private NetworkGamePlayerDND gamePlayerPrefab = null;
    [SerializeField] public GameObject playerSpawnSystem = null;

    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;

    public List<NetworkRoomPlayerDND> RoomPlayers { get; } = new List<NetworkRoomPlayerDND>();
    public List<NetworkGamePlayerDND> GamePlayers { get; } = new List<NetworkGamePlayerDND>();

    public override void OnStartServer() => spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().path != menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    //Method called when a client connects to the server, including the host's client instance
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //Determine if the active scene is the menu scene
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            //Set bool true if there is only one client connection
            bool isLeader = RoomPlayers.Count == 0;

            //if isLeader is true, determine devices IP Address
            if (isLeader)
            {
                //Open a temporary socket to determine device IPv4 or IPv6 address
                string localIP;
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                //Set IP text to IP Address
                GameObject.Find("IPAddress").GetComponent<TextMeshProUGUI>().SetText("IP Address: " + "\n" + localIP);
            }

            //Create NetworkRoomPlayerDND instance equal to instantiated prefab
            NetworkRoomPlayerDND roomPlayerInstance = Instantiate(roomPlayerPrefab);
            //Set IsLeader value of object equal to isLeader variable
            roomPlayerInstance.IsLeader = isLeader;

            //Tie gameobject to client connection and spawn object
            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    //Method called on Quit, disconnects all players
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkBehaviour>();

            if (player is NetworkRoomPlayerDND roomPlayer)
            {
                RoomPlayers.Remove(roomPlayer);

                NotifyPlayersOfReadyState();
            }
            else if (player is NetworkGamePlayerDND gamePlayer)
            {
                GamePlayers.Remove(gamePlayer);
                StopHost();
            }
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();

        RoomPlayers.Clear();
        GamePlayers.Clear();

        ServerChangeScene(menuScene);
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach(var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }
        return true;
    }

    public void StartGame()
    {
        if (SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene("Scene_DND_01");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        //from menu to game
        if (SceneManager.GetActiveScene().path == menuScene && newSceneName.StartsWith("Scene_DND"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(gamePlayerPrefab);
                gamePlayerInstance.SetPlayerName(RoomPlayers[i].DisplayName);
                gamePlayerInstance.SetStats(RoomPlayers[i].playerStats);
                gamePlayerInstance.SetIcon(RoomPlayers[i].playerSprite);
                gamePlayerInstance.SetValues(RoomPlayers[i].width, RoomPlayers[i].height, RoomPlayers[i].mapCounter);
                gamePlayerInstance.IsLeader = RoomPlayers[i].IsLeader;

                NetworkServer.Destroy(conn.identity.gameObject);

                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject, true);
            }
        }
        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith("Scene_DND"))
        {
            GameObject playerSpawnSystemInstance = Instantiate(playerSpawnSystem);
            spawnSystem = playerSpawnSystemInstance;

            NetworkServer.Spawn(playerSpawnSystemInstance);
        }
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }
}
