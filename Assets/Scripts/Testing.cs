using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Testing : NetworkBehaviour
{
    //create grid of any type
    private Grid<HeatMapGridObject> grid;
    private Grid<StringGridObject> stringGrid;

    //Pathfinding grid
    private Pathfinding pathfinding;

    //Runs at start
    private void Start()
    {
        pathfinding = new Pathfinding(10, 10);

        //heatMapVisual.SetGrid(grid);
        //heatMapBoolVisual.SetGrid(grid);
        //heatMapGenericVisual.SetGrid(grid);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && isServer)
        {
            Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            pathfinding.GetNode(x, y).SetIsWalkable(!pathfinding.GetNode(x, y).isWalkable);
        }
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
