using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler_DND : MonoBehaviour
{
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    public static GameHandler_DND Instance { get; private set; }

    private Grid<DNDCombatSystem.GridObject> grid;
    private Pathfinding pathfinding;

    private void Awake()
    {
        Instance = this;

        int mapWidth = this.mapWidth;
        int mapHeight = this.mapHeight;
        float cellSize = 10f;
        Vector3 origin = new Vector3(0, 0);

        grid = new Grid<DNDCombatSystem.GridObject>(mapWidth, mapHeight, cellSize, origin, (Grid<DNDCombatSystem.GridObject> g, int x, int y) => new DNDCombatSystem.GridObject(g, x, y));

        //pathfinding = new Pathfinding(mapWidth, mapHeight);
    }

    public Grid<DNDCombatSystem.GridObject> GetGrid()
    {
        return grid;
    }

    public class EmptyGridObject
    {

        private Grid<EmptyGridObject> grid;
        private int x;
        private int y;

        public EmptyGridObject(Grid<EmptyGridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;

            Vector3 worldPos00 = grid.GetWorldPosition(x, y);
            Vector3 worldPos10 = grid.GetWorldPosition(x + 1, y);
            Vector3 worldPos01 = grid.GetWorldPosition(x, y + 1);
            Vector3 worldPos11 = grid.GetWorldPosition(x + 1, y + 1);

            Debug.DrawLine(worldPos00, worldPos01, Color.white, 999f);
            Debug.DrawLine(worldPos00, worldPos10, Color.white, 999f);
            Debug.DrawLine(worldPos01, worldPos11, Color.white, 999f);
            Debug.DrawLine(worldPos10, worldPos11, Color.white, 999f);
        }

        public override string ToString()
        {
            return "";
        }
    }
}
