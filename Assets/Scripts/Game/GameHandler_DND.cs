using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class GameHandler_DND : NetworkBehaviour
{
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private GameObject backgroundImage;
    [SerializeField] private Sprite castleMap;
    [SerializeField] private Sprite forestMap;

    private Pathfinding pathfinding;
    private NetworkManagerDND networkMan;

    private void Start()
    {
        networkMan = NetworkManager.singleton as NetworkManagerDND;
        cmdCreateGrid();
    }

    //Make the image change on all clients
    //[Command]
    
    void cmdCreateGrid()
    {
        pathfinding = new Pathfinding(networkMan.GamePlayers[0].width, networkMan.GamePlayers[0].height);
        backgroundImage.transform.localScale = new Vector3(pathfinding.GetGrid().GetWidth() / 2f, pathfinding.GetGrid().GetHeight() / 2f);

        if (networkMan.GamePlayers[0].mapCounter == 0)
        {
            backgroundImage.GetComponent<Image>().sprite = castleMap;
        }
        else
        {
            backgroundImage.GetComponent<Image>().sprite = forestMap;
        }
    }
}
