using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Testing : MonoBehaviour
{
    [SerializeField] private PathfindingVisual pathfindingVisual;

    //create grid of any type
    private Grid<HeatMapGridObject> grid;
    private Grid<StringGridObject> stringGrid;

    //Pathfinding grid
    private Pathfinding pathfinding;

    //Dwarf
    GameObject dwarf;
    List<Vector3> movementPath = new List<Vector3>();

    bool buttonControl = false;

    //Runs at start
    private void Start()
    {
        pathfinding = new Pathfinding(10, 10);
        pathfindingVisual.SetGrid(pathfinding.GetGrid());
        dwarf = GameObject.Find("PlayerCharacter");
        dwarf.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);
        dwarf.transform.position = new Vector3(0, 0) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;

        //creates a grid object (width, height, square size, origin position, object type)
        //grid = new Grid<HeatMapGridObject>(16, 16, 10f, Vector3.zero, (Grid<HeatMapGridObject> g, int x, int y) => new HeatMapGridObject(g, x, y));
        //stringGrid = new Grid<StringGridObject>(16, 16, 10f, Vector3.zero, (Grid<StringGridObject> g, int x, int y) => new StringGridObject(g, x, y));

        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(grid);
        //heatMapGenericVisual.SetGrid(grid);
    }

    //Runs every frame
    private void Update()
    {

        //if left click draw path using A* between dwarf and mouse
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetGrid().GetXY(dwarf.transform.position, out int startX, out int startY);

            List<PathNode> path = pathfinding.FindPath(startX, startY, x, y);
            if (path != null)
            {
                for (int i = 0; i<path.Count - 1; i++)
                {
                    //Change float values depending on size of Grid, determined in Pathfinding.cs
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].x, path[i + 1].y) * 10f + Vector3.one * 5f, Color.green, 5f);
                }
            }
        }

        //if middle mouse click, add nearby cell to list of movement
        if (Input.GetMouseButtonDown(2))
        {
            //Get the coordinates of the mouse and the dwarf
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetGrid().GetXY(dwarf.transform.position, out int originX, out int originY);

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
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
        }
    }

    //This seems like a poor way to do this but it works for now
    //loops through a list of Vector3s and moves the dwarf between them every .5f, then clears the list
    IEnumerator tester()
    {
        foreach (Vector3 location in movementPath)
        {
            dwarf.transform.position = location;
            yield return new WaitForSeconds(.5f);
        }
        movementPath.Clear();
        Debug.Log("test");
    }

    public void toggleWall()
    {
        if (buttonControl)
        {
            buttonControl = false;
            StartCoroutine(tester());
            return;
        }
        else
        {
            buttonControl = true;
            return;
        }
    }

    //long boring code for getting mouseworldposition, Add to unique class later or clean up somewhere else
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 GetMouseWorldPositionWithZ()
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
    {
        return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
    }

    public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}

public class HeatMapGridObject
{
    public const int MIN = 0;
    public const int MAX = 100;

    private Grid<HeatMapGridObject> grid;
    private int x;
    private int y;
    private int value;

    public HeatMapGridObject(Grid<HeatMapGridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void AddValue(int addValue)
    {
        value += addValue;
        value = Mathf.Clamp(value, MIN, MAX);
        grid.TriggerGridObjectChanged(x, y);
    }

    public float GetValueNormalizsed()
    {
        return (float)value / MAX;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}

public class StringGridObject
{
    private Grid<StringGridObject> grid;
    private int x;
    private int y;

    public string letters;
    public string numbers;

    public StringGridObject(Grid<StringGridObject> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        letters = "";
        numbers = "";
    }

    public void AddLetter(string letter)
    {
        letters += letter;
        grid.TriggerGridObjectChanged(x, y);
    }

    public void AddNumber(string number)
    {
        numbers += number;
        grid.TriggerGridObjectChanged(x, y);
    }

    public override string ToString()
    {
        return letters + "\n" + numbers;
    }
}
