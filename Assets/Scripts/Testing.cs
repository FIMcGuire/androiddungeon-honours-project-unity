using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Testing : NetworkBehaviour
{

    //Pathfinding grid
    private Pathfinding pathfinding;

    //Runs at start
    private void Start()
    {
        createGrid();
    }

    void createGrid()
    {
        pathfinding = new Pathfinding(10, 10);
    }
}
