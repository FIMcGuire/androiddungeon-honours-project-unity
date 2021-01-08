using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private HeatMapVisual heatMapVisual;
    [SerializeField] private HeatMapBoolVisual heatMapBoolVisual;

    //create grid of any type
    private Grid<bool> grid;

    //Runs at start
    private void Start()
    {
        //creates a grid object (width, height, square size, origin position, object type)
        grid = new Grid<bool>(16, 16, 10f, Vector3.zero, () => new bool());

        //heatMapVisual.SetGrid(grid);
        heatMapBoolVisual.SetGrid(grid);
    }

    //Runs every frame
    private void Update()
    {
        //if left click toggle values inside grid square
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 position = GetMouseWorldPosition();
            grid.SetValue(position, true);
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

    public int value;

    public void AddValue(int addValue)
    {
        value += addValue;
        value = Mathf.Clamp(value, MIN, MAX);
    }
}
