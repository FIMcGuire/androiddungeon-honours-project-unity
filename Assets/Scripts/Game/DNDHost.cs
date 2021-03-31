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
    [SerializeField] private List<Sprite> monsterSprites = null;

    private Transform HostCanvasObject;
    private Sprite monsterSprite = null;
    private string monsterName = null;

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
        GameObject.Find("FOV").SetActive(false);
        GameObject.Find("BlackCanvas").SetActive(false);

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
        if (state == Mode.Idle)
        {
            if (movementPath != null)
            {
                movementPath.Clear();
                cmdDestroyPath();
                movementSpeed = maxSpeed;
            }
            monsterName = button.name;
            Debug.Log(button.name);
            switch (monsterName)
            {
                case "Goblin":
                    monsterSprite = monsterSprites[0];
                    break;
                case "Bugbear":
                    monsterSprite = monsterSprites[1];
                    break;
                case "Bandit":
                    monsterSprite = monsterSprites[2];
                    break;
                case "Bandit2":
                    monsterSprite = monsterSprites[3];
                    break;
                case "Wolf":
                    monsterSprite = monsterSprites[4];
                    break;
                case "Mimic":
                    monsterSprite = monsterSprites[5];
                    break;
            }
            state = Mode.Spawn;
        }   
    }

    public void ToggleWalls()
    {
        if (state != Mode.Walls)
        {
            HostCanvasObject.Find("Button_Panel").Find("WallsButton").GetComponent<Image>().color = Color.green;
            HostCanvasObject.Find("Button_Panel").Find("RoughButton").GetComponent<Image>().color = Color.white;
            state = Mode.Walls;
        }
        else
        {
            HostCanvasObject.Find("Button_Panel").Find("WallsButton").GetComponent<Image>().color = Color.white;
            state = Mode.Idle;
        }
    }

    public void ToggleRoughTerrain()
    {
        if (state != Mode.RoughTerrain)
        {
            HostCanvasObject.Find("Button_Panel").Find("WallsButton").GetComponent<Image>().color = Color.white;
            HostCanvasObject.Find("Button_Panel").Find("RoughButton").GetComponent<Image>().color = Color.green;
            state = Mode.RoughTerrain;
        }
        else
        {
            HostCanvasObject.Find("Button_Panel").Find("RoughButton").GetComponent<Image>().color = Color.white;
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

                    if (selectedMonster != null)
                        selectedMonster.GetComponent<SpriteRenderer>().color = Color.white;

                    selectedMonster = hit.transform.gameObject;
                    selectedMonster.GetComponent<SpriteRenderer>().color = Color.green;
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
                        if (node.isRoughTerrain)
                        {
                            movementSpeed-=2;
                        }
                        else
                        {
                            movementSpeed--;
                        }
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

    #region SERVER

    [Command]
    public void cmdDisconnect()
    {
        networkManager.OnServerDisconnect(connectionToClient);
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
            monsterInstance.name = monsterName + " " + x.ToString() + "/" + y.ToString();
            monsterInstance.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize() / 5, pathfinding.GetGrid().GetCellSize() / 5, 1);
            NetworkServer.Spawn(monsterInstance, connectionToClient);
            monsterInstance.layer = 9;

            RpcUpdateMonster(monsterInstance, monsterName, x.ToString() + "/" + y.ToString());
        }
    }

    [ClientRpc]
    void RpcUpdateMonster(GameObject monster, string monsterName, string position)
    {
        switch (monsterName)
        {
            case "Goblin":
                monsterSprite = monsterSprites[0];
                break;
            case "Bugbear":
                monsterSprite = monsterSprites[1];
                break;
            case "Bandit":
                monsterSprite = monsterSprites[2];
                break;
            case "Bandit2":
                monsterSprite = monsterSprites[3];
                break;
            case "Wolf":
                monsterSprite = monsterSprites[4];
                break;
            case "Mimic":
                monsterSprite = monsterSprites[5];
                break;
        }
        monster.name = monsterName + " " + position;
        monster.GetComponent<SpriteRenderer>().sprite = monsterSprite;
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
            if (pathfinding.GetNode(x, y).isWalkable && !pathfinding.GetNode(x, y).isRoughTerrain)
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
                RpcQuad(x, y, false);
            }
            else
            {
                pathfinding.GetNode(x, y).SetIsWalkable(true);
                GameObject tester = GameObject.Find("Wall: " + x.ToString() + "/" + y.ToString());
                NetworkServer.Destroy(tester);
                RpcQuad(x, y, true);
            }
        }
    }

    [ClientRpc]
    void RpcQuad(int x, int y, bool createDestroy)
    {
        if (!createDestroy)
        {
            pathfinding.GetNode(x, y).SetIsWalkable(false);
        }
        else
        {
            pathfinding.GetNode(x, y).SetIsWalkable(true);
        }
    }

    [Command]
    void cmdRoughTerrain()
    {
        Vector3 mouseWorldPosition = Utils.GetMouseWorldPosition();
        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);

        if (x >= 0 && y >= 0)
        {
            if (pathfinding.GetNode(x, y).isWalkable && !pathfinding.GetNode(x, y).isRoughTerrain)
            {
                //Not working across clients, spawning but not toggling
                pathfinding.GetNode(x, y).SetIsRoughTerrain(true);
                Vector3 cellPos = new Vector3(x, y) * pathfinding.GetGrid().GetCellSize() + Vector3.one * pathfinding.GetGrid().GetCellSize() * .5f;
                cellPos.z = 0;

                GameObject tester = Instantiate(roughPrefab, cellPos, Quaternion.identity);
                tester.name = "Rough: " + x.ToString() + "/" + y.ToString();
                //quadList.Add(tester);
                tester.transform.localScale = new Vector3(pathfinding.GetGrid().GetCellSize(), pathfinding.GetGrid().GetCellSize(), 1);
                NetworkServer.Spawn(tester);
                RpcRoughTerrain(x,y,true);
            }
            else
            {
                pathfinding.GetNode(x, y).SetIsRoughTerrain(false);
                GameObject tester = GameObject.Find("Rough: " + x.ToString() + "/" + y.ToString());
                NetworkServer.Destroy(tester);
                RpcRoughTerrain(x, y, false);
            }
        }
    }

    [ClientRpc]
    void RpcRoughTerrain(int x, int y, bool createDestroy)
    {
        if (!createDestroy)
        {
            pathfinding.GetNode(x, y).SetIsRoughTerrain(false);
        }
        else
        {
            pathfinding.GetNode(x, y).SetIsRoughTerrain(true);
        }
    }

    #endregion

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
        selectedMonster.GetComponent<SpriteRenderer>().color = Color.white;
        state = Mode.Idle;
    }

    public void cmdtoggleWalk()
    {
        if (movementPath.Count > 0)
        {
            StartCoroutine(tester());
        }
    }
}
