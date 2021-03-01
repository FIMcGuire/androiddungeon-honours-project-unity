using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNDCombatSystem : MonoBehaviour
{
    [SerializeField] private DNDCombatUnit dndCombatUnit;

    private void Start()
    {
        //This will change based on PC movement speed
        int maxMoveDistance = 5;
        //grid.GetXY(dndCombatUnit.GetPosition(), out int unitX, out int unitY);
    }

    public class GridObject
    {

        private Grid<GridObject> grid;
        private int x;
        private int y;
        private bool isValidMovePosition;

        public GridObject(Grid<GridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }

        public void SetIsValidMovePosition(bool set)
        {
            isValidMovePosition = set;
        }

        public bool GetIsValidMovePosition()
        {
            return isValidMovePosition;
        }

    }
}
