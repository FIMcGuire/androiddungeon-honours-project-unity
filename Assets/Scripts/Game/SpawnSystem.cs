using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class SpawnSystem : NetworkBehaviour
{
    //gameobjects to hold player and spawnpoint prefabs
    [SerializeField] private GameObject hostPrefab = null;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject monsterPrefab = null;
    [SerializeField] private GameObject spawnPoint = null;

    [SerializeField] private List<Sprite> allSprites = null;

    //list of coords for spawnpoints
    private static List<Transform> spawnPoints = new List<Transform>();

    //simple counter to increment as each player connects
    public int nextIndex = 0;

    //instance of pathfinding
    Pathfinding pathfinding = Pathfinding.Instance;

    //instance of NetworkManagerDND
    private NetworkManagerDND networkManager = NetworkManager.singleton as NetworkManagerDND;

    //method called from PlayerSpawnPoint adds coords to list
    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);

        //Order by position in heriarchy
        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }

    //method called from PlayerSpawnPoint removes coords from list
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    //when server starts and player calls OnServerReadied event, call SpawnPlayer method
    public override void OnStartServer() => NetworkManagerDND.OnServerReadied += SpawnPlayer;

    //when players leaves, call event and remove player
    [ServerCallback]
    private void OnDestroy() => NetworkManagerDND.OnServerReadied -= SpawnPlayer;

    //Called before Start() when script instance being loaded
    void Awake()
    {
        //get coords of grid 0,0 and instantiate spawnpoint prefab there
        pathfinding.GetGrid().GetXY(new Vector3(0, 0), out int x, out int y);

        int counter = 0;
        foreach (var gamer in networkManager.GamePlayers)
        {
            if (!gamer.IsLeader)
            {
                Vector3 cell = new Vector3(x + counter, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                cell.z = 0;
                Instantiate(spawnPoint, cell, Quaternion.identity);
                counter++;
            }
        }
    }

    //Server code to spawn player prefabs on every client instance
    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        //coords of current spawnPoint (from nextIndex counter)
        Transform listSpawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

        //if null, return
        if(listSpawnPoint == null)
        {
            return;
        }

        //Determine who the host is
        NetworkGamePlayerDND player = null;
        foreach (var gamer in networkManager.GamePlayers)
        {
            if (gamer.IsLeader)
            {
                player = gamer;
                break;
            }
        }

        //If conn is the HOST client spawn Host prefab, else spawn player prefab
        if (conn == player.connectionToClient)
        {
            //instantiate host prefab at 0,0,0 and then tie it to the host's client connection
            var playerInstance = Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
            playerInstance.name = "Host";
            NetworkServer.Spawn(playerInstance, conn);
        }
        else
        {
            //instantiate player prefab at current spawnpoint location and then tie it to client connection
            var playerInstance = Instantiate(playerPrefab, spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);

            string playerName = null;
            foreach(var gamer in networkManager.GamePlayers)
            {
                if (conn == gamer.connectionToClient)
                {
                    playerName = gamer.GetDisplayName();
                    foreach (var sprite in allSprites)
                    {
                        if (gamer.icon == sprite.name)
                        {
                            playerInstance.GetComponent<SpriteRenderer>().sprite = sprite;
                            break;
                        }
                    }
                }
            }
            
            playerInstance.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);
            NetworkServer.Spawn(playerInstance, conn);

            RpcUpdatePlayer(playerInstance, playerInstance.GetComponent<SpriteRenderer>().sprite.name, playerName);

            //increment counter
            nextIndex++;
        }
    }

    [ClientRpc]
    void RpcUpdatePlayer(GameObject player, string spriteName, string playerName)
    {
        player.name = playerName;
        foreach (var sprite in allSprites)
        {
            if (spriteName == sprite.name)
            {
                player.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }
    }
}
