using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    float Timer;
    List<Vector3> movementPath;

    //Runs at start
    private void Start()
    {
        pathfinding = new Pathfinding(10, 10);
        pathfindingVisual.SetGrid(pathfinding.GetGrid());
        dwarf = GameObject.Find("PlayerCharacter");

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
        Timer += Time.deltaTime * 1;

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
                    Debug.Log((path[i].x).ToString() + " | " + (path[i].y).ToString());
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].x, path[i + 1].y) * 10f + Vector3.one * 5f, Color.green, 5f);
                }
            }
        }

        //if right click mark cell as wall/not walkable
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
        }

        //if middle click move dwarf along path
        //Find a less jank way to do this
        if (Input.GetMouseButtonDown(2))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

            movementPath = pathfinding.FindPath(dwarf.transform.position, mouseWorldPosition);
            StartCoroutine(tester());
            
            
        }

        /*if (Input.GetMouseButtonDown(2))
        {
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            Vector3 cellPos = pathfinding.GetGrid().GetWorldPosition(x, y);
            dwarf.transform.position = cellPos;
        }*/
    }

    //This seems like a poor way to do this but it works for now
    IEnumerator tester()
    {
        foreach (Vector3 location in movementPath)
        {
            dwarf.transform.position = location;
            yield return new WaitForSeconds(.5f);
        }
        Debug.Log("test");
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
