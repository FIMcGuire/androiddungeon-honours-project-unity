using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DNDHost : NetworkBehaviour
{
    [SerializeField] private GameObject quadPrefab = null;
    [SerializeField] private GameObject roughPrefab = null;
    [SerializeField] private GameObject pathPrefab = null;
    [SerializeField] private GameObject monsterPrefab = null;
    private List<Transform> monsterButtons = null;

    private Transform HostCanvasObject;
    private Sprite monsterSprite = null;
    private bool monsterSelected = false;

    private GameObject selectedMonster = null;

    //List of Vector3 objects to move playercharacter
    List<Vector3> movementPath = new List<Vector3>();

    //List of GameObjects to remove player Path
    List<GameObject> movementPathGameObjects = new List<GameObject>();

    //Movement Speed
    [SerializeField] public int movementSpeed = 5;
    public int maxSpeed = 5;

    //Pathfinding grid
    private Pathfinding pathfinding = Pathfinding.Instance;

    //NetworkManager
    private NetworkManagerDND networkManager = NetworkManager.singleton as NetworkManagerDND;

    private bool walking = false;

    private enum Mode
    {
        Walls,
        RoughTerrain,
        Spawn,
        Movement,
        Idle
    }

    private Mode state = Mode.Idle;

    public void Start()
    {
        //Code to ensure scale of player game objects match scale of Grid
        //transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);

        //Code to move player game objects to 0,0 on Grid FIND A BETTER WAY SO THAT TWO OBJECTS CANNOT OCCUPY SAME SPACE
        //transform.position = new Vector3(0, 0) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
    }

    public override void OnStartAuthority()
    {
        enabled = true;

        GameObject.Find("FOV").SetActive(false);

        HostCanvasObject = this.transform.Find("Canvas");

        HostCanvasObject.gameObject.SetActive(true);

        base.OnStartAuthority();
    }

    //Runs every frame
    private void Update()
    {
        HandleHostControls();
    }

    public void SpawnButton(GameObject button)
    {
        if (movementPath != null)
        {
            movementPath.Clear();
            cmdDestroyPath();
            movementSpeed = maxSpeed;
        }
        monsterSprite = button.GetComponent<Image>().sprite;
        state = Mode.Spawn;
    }

    public void ToggleWalls()
    {
        if (state != Mode.Walls)
        {
            state = Mode.Walls;
        }
        else
        {
            state = Mode.Idle;
        }
    }

    public void ToggleRoughTerrain()
    {
        if (state != Mode.RoughTerrain)
        {
            state = Mode.RoughTerrain;
        }
        else
        {
            state = Mode.Idle;
        }
    }

    [Client]
    void HandleHostControls()
    {
        if (hasAuthority)
        {
            if (Input.GetMouseButtonDown(0) && state == Mode.Walls && isServer)
            {
                cmdQuad();
            }

            if (Input.GetMouseButtonDown(0) && state == Mode.RoughTerrain && isServer)
            {
                cmdRoughTerrain();
            }

            if (Input.GetMouseButtonDown(0) && isServer && !walking)
            {
                var hit = Physics2D.Raycast(new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y), Vector2.zero, 0f);

                if (hit)
                {
                    if (hit.transform.gameObject.GetComponent<NetworkIdentity>().connectionToClient != connectionToClient) { return; }
                    selectedMonster = hit.transform.gameObject;
                    state = Mode.Movement;
                    if (movementPath != null)
                    {
                        movementPath.Clear();
                        cmdDestroyPath();
                        movementSpeed = maxSpeed;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && state == Mode.Movement && selectedMonster != null && !walking && movementSpeed > 0)
            {
                //Get the coordinates of the mouse and the dwarf
                Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
                Grid<PathNode> test = pathfinding.GetGrid();
                test.GetXY(mouseWorldPosition, out int x, out int y);
                //pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
                pathfinding.GetGrid().GetXY(selectedMonster.transform.position, out int originX, out int originY);

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
                        Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                        cellPos.z = -1;
                        movementPath.Add(cellPos);

                        cmdCreatePath(cellPos);
                        movementSpeed--;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && state == Mode.Spawn)
            {
                cmdSpawnNPC();
                state = Mode.Idle;
            }
        }
    }

    [Command]
    private void cmdSpawnNPC()
    {
        Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

        if (x >= 0 && y >= 0)
        {
            Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
            cellPos.z = 0;

            //instantiate player prefab at current spawnpoint location and then tie it to client connection
            var monsterInstance = Instantiate(monsterPrefab, cellPos, Quaternion.identity);
            monsterInstance.GetComponent<SpriteRenderer>().sprite = monsterSprite;
            monsterInstance.name = monsterInstance.GetComponent<SpriteRenderer>().sprite.ToString() + " " + x.ToString() + "/" + y.ToString();
            monsterInstance.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);
            NetworkServer.Spawn(monsterInstance, connectionToClient);
        }
    }

    [Command]
    public void cmdDisconnect()
    {
        networkManager.OnServerDisconnect(connectionToClient);
    }

    [Command]
    void cmdCreatePath(Vector3 cellPos)
    {
        GameObject path = Instantiate(pathPrefab, cellPos, Quaternion.identity);
        path.tag = "Path";
        path.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 0);
        NetworkServer.Spawn(path, connectionToClient);
        movementPathGameObjects.Add(path);
    }

    [Command]
    void cmdDestroyPath()
    {
        if (movementPathGameObjects.Count > 0)
        {
            foreach (var pathObject in movementPathGameObjects)
            {
                NetworkServer.Destroy(pathObject);
            }
        }
    }

    [Command]
    void cmdQuad()
    {
        Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

        if (x >= 0 && y >= 0)
        {
            if (pathfinding.GetNode(x, y).isWalkable)
            {
                //Not working across clients, spawning but not toggling
                pathfinding.GetNode(x, y).SetIsWalkable(false);
                Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                cellPos.z = 0;

                GameObject tester = Instantiate(quadPrefab, cellPos, Quaternion.identity);
                tester.name = "Wall: " + x.ToString() + "/" + y.ToString();
                //quadList.Add(tester);
                tester.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 1);
                NetworkServer.Spawn(tester);
            }
            else
            {
                pathfinding.GetNode(x, y).SetIsWalkable(true);
                GameObject tester = GameObject.Find("Wall: " + x.ToString() + "/" + y.ToString());
                NetworkServer.Destroy(tester);
            }
        }
    }

    [Command]
    void cmdRoughTerrain()
    {
        Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

        if (x >= 0 && y >= 0)
        {
            if (pathfinding.GetNode(x, y).isWalkable)
            {
                //Not working across clients, spawning but not toggling
                pathfinding.GetNode(x, y).SetIsWalkable(false);
                Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                cellPos.z = 0;

                GameObject tester = Instantiate(roughPrefab, cellPos, Quaternion.identity);
                tester.name = "Rough: " + x.ToString() + "/" + y.ToString();
                //quadList.Add(tester);
                tester.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 1);
                NetworkServer.Spawn(tester);
            }
            else
            {
                pathfinding.GetNode(x, y).SetIsWalkable(true);
                GameObject tester = GameObject.Find("Rough: " + x.ToString() + "/" + y.ToString());
                NetworkServer.Destroy(tester);
            }
        }
    }

    //This seems like a poor way to do this but it works for now
    //loops through a list of Vector3s and moves the dwarf between them every .5f, then clears the list
    IEnumerator tester()
    {
        walking = true;
        //run through each position in movementPath
        for (int i = 0; i < (movementPath.Count - 1); i++)
        {
            //calculate start and end points for moving from one cell to the next
            var pointA = movementPath[i];
            var pointB = movementPath[i + 1];

            //steps between each cell
            int steps = 10;

            //time to move from cell to cell
            const float durationPerTile = 0.5f;

            //for loop to move player from tile to tile
            for (int j = 0; j < steps; ++j)
            {
                selectedMonster.transform.position = Vector3.Lerp(pointA, pointB, j / (float)steps);
                yield return new WaitForSeconds(durationPerTile / steps);
            }

        }
        //move player to FINAL point, as without this player object will be just shy
        selectedMonster.transform.position = movementPath[movementPath.Count - 1];

        movementPath.Clear();
        walking = false;
        movementSpeed = maxSpeed;
        cmdDestroyPath();
        Debug.Log(walking + movementSpeed.ToString() + "/" + maxSpeed.ToString());
    }

    public void cmdtoggleWalk()
    {
        if (movementPath.Count > 0)
        {
            StartCoroutine(tester());
            state = Mode.Idle;
        }
    }
}
