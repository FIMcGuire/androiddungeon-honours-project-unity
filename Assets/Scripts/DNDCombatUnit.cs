using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DNDCombatUnit : NetworkBehaviour
{

    List<Vector3> movementPath = new List<Vector3>();

    //Pathfinding grid
    private Pathfinding pathfinding;

    public void Start()
    {
        pathfinding = Pathfinding.Instance;
        transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);
        transform.position = new Vector3(0, 0) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
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
            if (Input.GetMouseButtonDown(2))
            {
                //Get the coordinates of the mouse and the dwarf
                Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
                pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
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
                    movementPath.Add(new Vector3(originX, originY) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f);
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
                        movementPath.Add(cellPos);
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                toggleWalk();
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

    //This seems like a poor way to do this but it works for now
    //loops through a list of Vector3s and moves the dwarf between them every .5f, then clears the list
    IEnumerator tester()
    {
        for(int i=0;i<(movementPath.Count-1);++i)
        {
            var p0 = movementPath[i];
            var p1 = movementPath[i+1];
            int steps = 10;
            const float durationPerTile = 0.5f;
            for(int j=0;j< steps; ++j)
            {
                transform.position = Vector3.Lerp(p0, p1, j / (float)steps);
                yield return new WaitForSeconds(durationPerTile/steps);
            }
            
        }
        transform.position = movementPath[movementPath.Count - 1];

#if false
        foreach (Vector3 location in movementPath)
        {
            transform.position = location;
            yield return new WaitForSeconds(0.5f);
        }
#endif
        movementPath.Clear();
    }

    public void toggleWalk()
    {
        StartCoroutine(tester());
    }
}
