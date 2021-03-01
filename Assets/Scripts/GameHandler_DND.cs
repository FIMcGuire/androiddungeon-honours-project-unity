using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameHandler_DND : NetworkBehaviour
{
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    private Pathfinding pathfinding;
    private GameObject networkMan;

    private void Start()
    {
        networkMan = GameObject.Find("NetworkManager");
        createGrid();
    }

    void createGrid()
    {
        pathfinding = new Pathfinding(mapWidth, mapHeight);
        List<NetworkGamePlayerDND> test = networkMan.GetComponent<NetworkManagerDND>().GamePlayers;
        Debug.Log(test[0].ToString());
    }
}
