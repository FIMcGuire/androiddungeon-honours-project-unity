using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DNDCombatUnit : NetworkBehaviour
{
    [SerializeField] private GameObject testPrefab = null;
    [SerializeField] private GameObject pathPrefab = null;

    //List of Vector3 objects to move playercharacter
    List<Vector3> movementPath = new List<Vector3>();

    //Movement Speed
    [SerializeField] private int movementSpeed = 5;

    //Pathfinding grid
    private Pathfinding pathfinding = Pathfinding.Instance;

    private bool walking = false;

    List<GameObject> quadList = new List<GameObject>();

    public void Start()
    {
        transform.parent = GameObject.Find("PlayerObjects").transform;

        //Code to ensure scale of player game objects match scale of Grid
        //transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);

        //Code to move player game objects to 0,0 on Grid FIND A BETTER WAY SO THAT TWO OBJECTS CANNOT OCCUPY SAME SPACE
        //transform.position = new Vector3(0, 0) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
    }

    public override void OnStartAuthority()
    {
        enabled = true;

        base.OnStartAuthority();
    }

    //Runs every frame
    private void Update()
    {
        HandleMovement();
    }

    [Client]
    void HandleMovement()
    {
        if (hasAuthority)
        {
            /*
            //if left click draw path using A* between dwarf and mouse
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                pathfinding.GetGrid().GetXY(transform.position, out int startX, out int startY);

                List<PathNode> path = pathfinding.FindPath(startX, startY, x, y);
                if (path != null)
                {
                    for (int i = 0; i < path.Count - 1; i++)
                    {
                        //Change float values depending on size of Grid, determined in Pathfinding.cs
                        Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].x, path[i + 1].y) * 10f + Vector3.one * 5f, Color.green, 5f);
                    }
                }
            }*/

            //if middle mouse click, add nearby cell to list of movement
            if (Input.GetMouseButtonDown(2) && !walking && movementSpeed > 0)
            {
                //Get the coordinates of the mouse and the dwarf
                Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
                Grid<PathNode> test = pathfinding.GetGrid();
                test.GetXY(mouseWorldPosition, out int x, out int y);
                //pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                pathfinding.GetGrid().GetXY(transform.position, out int originX, out int originY);

                //if movementPath list contains any Vector3's, get Pathnode object from latest position in the list. Else, get Pathnode object from origin position of dwarf.
                PathNode cell;
                if (movementPath.Count > 0)
                {
                    Vector3 lastItem = movementPath[movementPath.Count - 1];
                    pathfinding.GetGrid().GetXY(lastItem, out int newX, out int newY);
                    cell = pathfinding.GetGrid().GetGridObject(newX, newY);
                }
                else
                {
                    cell = pathfinding.GetGrid().GetGridObject(originX, originY);
                    Vector3 originCell = new Vector3(originX, originY) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                    originCell.z = 0;
                    movementPath.Add(originCell);
                }

                //generate list of available neighbouring nodes/cells
                List<PathNode> availableMovement = pathfinding.GetNeighbourList(cell);

                //get Pathnode object from the mouse position
                PathNode movement = new PathNode(pathfinding.GetGrid(), x, y);

                //loop through the list of availableMovements to see if the selected cell is within the list
                //Couldnt use .Contains and had to loop through the list, this could be a problem if the list was LARGE. Should be okay.
                foreach (PathNode node in availableMovement)
                {
                    if ((node.x == movement.x && node.y == movement.y) && node.isWalkable)
                    {
                        //add the given movement to the list of Vector3 objects
                        Debug.Log("Okay!");
                        Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                        cellPos.z = 1;
                        movementPath.Add(cellPos);

                        cmdCreatePath(cellPos);
                        movementSpeed--;
                    }
                }
            }           
            
            if (Input.GetMouseButtonDown(1) && isServer)
            {
                cmdQuad();
            }

            //if middle click move dwarf along path
            //Find a less jank way to do this
            /*
            if (Input.GetMouseButtonDown(2) && buttonControl)
            {
                Vector3 mouseWorldPosition = GetMouseWorldPosition();
                //pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

                movementPath = pathfinding.FindPath(dwarf.transform.position, mouseWorldPosition);
                StartCoroutine(tester());

            }
            */
        }
    }

    [Command]
    void cmdCreatePath(Vector3 cellPos)
    {
        GameObject path = Instantiate(pathPrefab, cellPos, Quaternion.identity);
        path.tag = "Path";
        path.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 0);
        NetworkServer.Spawn(path, connectionToClient);
    }

    [Command]
    void cmdDestroyPath()
    {
        GameObject[] pathObjects = GameObject.FindGameObjectsWithTag("Path");
        for (int i = 0; i < pathObjects.Length; i++)
        {
            NetworkServer.Destroy(pathObjects[i]);
        }
    }

    [Command]
    void cmdQuad()
    {
        Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

        if (pathfinding.GetNode(x, y).isWalkable)
        {
            pathfinding.GetNode(x, y).SetIsWalkable(false);

            Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
            cellPos.z = 0;

            GameObject tester = Instantiate(testPrefab, cellPos, Quaternion.identity);
            tester.name = x.ToString() + "/" + y.ToString();
            //quadList.Add(tester);
            tester.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 1);
            NetworkServer.Spawn(tester);
        }
        else
        {
            Debug.Log("here");
            pathfinding.GetNode(x, y).SetIsWalkable(true);
            GameObject tester = GameObject.Find(x.ToString() + "/" + y.ToString());
            NetworkServer.Destroy(tester);
        }
        
    }

    //This seems like a poor way to do this but it works for now
    //loops through a list of Vector3s and moves the dwarf between them every .5f, then clears the list
    IEnumerator tester()
    {
        walking = true;
        //run through each position in movementPath
        for (int i=0; i<(movementPath.Count-1); i++)
        {
            //calculate start and end points for moving from one cell to the next
            var pointA = movementPath[i];
            var pointB = movementPath[i+1];
            
            //steps between each cell
            int steps = 10;

            //time to move from cell to cell
            const float durationPerTile = 0.5f;

            //for loop to move player from tile to tile
            for(int j=0;j< steps; ++j)
            {
                transform.position = Vector3.Lerp(pointA, pointB, j / (float)steps);
                yield return new WaitForSeconds(durationPerTile/steps);
            }
            
        }
        //move player to FINAL point, as without this player object will be just shy
        transform.position = movementPath[movementPath.Count - 1];

        movementPath.Clear();
        walking = false;
        movementSpeed = 5;
        cmdDestroyPath();
    }

    public void cmdtoggleWalk()
    {
        StartCoroutine(tester());
    }
}
