using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Generic Grid script, can be used to create a dynamic grid that contains any kind of object
public class Grid<TGridObject>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    //create variables
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;
    private TextMesh[,] debugTextArray;

    //constructor, also takes in a Func parameter to create any manner of custom object
    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<TGridObject> createGridObject)
    {

        //set variables
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        //instantiate gridArray
        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject();
            }
        }

        bool showDebug = true;
        if (showDebug)
        {
            //Array that contains text for each grid square
            debugTextArray = new TextMesh[width, height];

            //create said text for grid and draw lines around each square (find a better way to draw these)
            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugTextArray[x, y] = CreateWorldText(null, gridArray[x, y]?.ToString(), GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);

                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
        //populate gridArray
        /*for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject();
            }
        }*/
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetWidth()
    {
        return width;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    //gets the world position using x and y values
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    //gets the X and Y values using the world position
    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    //sets the value of a given grid square using X and Y values
    public void SetValue(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridValueChanged != null) OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

    //sets the value of a given grid square using world position
    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetValue(x, y, value);
    }

    //gets the value of a grid square using x and y values
    public TGridObject GetValue(int x, int y)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    //gets the value of a grid square using world position
    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValue(x, y);
    }

    //code to generate text inside a grid square (long and boring) Add to different class maybe
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.text = text;
        textMesh.fontSize = 40;
        textMesh.color = color;
        return textMesh;
    }
}
