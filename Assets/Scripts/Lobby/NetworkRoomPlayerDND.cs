using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class NetworkRoomPlayerDND : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private Button startGameButton = null;

    Pathfinding pathfinding;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    [Header("MapSelection")]
    [SerializeField] private TMP_InputField mapWidth = null;
    [SerializeField] private TMP_InputField mapHeight = null;
    [SerializeField] private Image mapPreview = null;
    [SerializeField] private Sprite castleMap = null;
    [SerializeField] private Sprite forestMap = null;
    [SerializeField] private TextMeshProUGUI mapTitle = null;

    [SyncVar(hook = nameof(HandleMapChanged))]
    public int mapCounter = 0;
    [SyncVar(hook = nameof(HandleWidthChanged))]
    public int width;
    [SyncVar(hook = nameof(HandleHeightChanged))]
    public int height;

    //Stats & Sprites
    [SyncVar]
    public string playerSprite;
    public List<List<string>> allPlayerStats;
    [SyncVar]
    public List<string> playerStats;

    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
        get
        {
            return isLeader;
        }
    }

    private NetworkManagerDND room;
    private NetworkManagerDND Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerDND;
        }
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);

        List<string> stats = new List<string>() { PlayerStatInput.StrengthStat.ToString(), PlayerStatInput.DexterityStat.ToString(), PlayerStatInput.ConstitutionStat.ToString(),
            PlayerStatInput.WisdomStat.ToString(), PlayerStatInput.IntelligenceStat.ToString(), PlayerStatInput.CharismaStat.ToString(), PlayerCurrentStatInput.HealthStat,
            PlayerCurrentStatInput.ArmorClassStat, PlayerCurrentStatInput.SpeedStat, PlayerCurrentStatInput.InitStat};

        string icon = PlayerStatInput.Icon;

        CmdSetStatsAndIcon(stats, icon);

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);

        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);

        UpdateDisplay();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();
    public void HandleWidthChanged(int oldValue, int newValue)
    {
        Debug.Log(DisplayName + " VALUE CHANGED");
        foreach (var player in Room.RoomPlayers)
        {
            player.width = newValue;
        }
    }

    public void HandleHeightChanged(int oldValue, int newValue)
    {
        Debug.Log(DisplayName + " VALUE CHANGED");
        foreach (var player in Room.RoomPlayers)
        {
            player.height = newValue;
        }
    }
    public void HandleMapChanged(int oldValue, int newValue)
    {
        foreach (var player in Room.RoomPlayers)
        {
            player.mapCounter = newValue;
        }
    }

    public void HandleStatsChanged(List<string> oldValue, List<string> newValue) => UpdateStats();

    private void UpdateStats()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateStats();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            allPlayerStats.Add(Room.RoomPlayers[i].playerStats);
        }
    }

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        startGameButton.interactable = readyToStart;
    }

    public void mapCycle()
    {
        if (mapCounter == 0)
        {
            mapCounter = 1;
        }
        else
        {
            mapCounter = 0;
        }
    }

    private void Update()
    {
        if (mapCounter == 0)
        {
            mapPreview.sprite = castleMap;
            mapTitle.SetText("Castle");
        }
        else
        {
            mapPreview.sprite = forestMap;
            mapTitle.SetText("Forest");
        }
    }

    #region SERVER

    [Command]
    public void CmdCreateGrid()
    {
        if (!isLeader) { return; }

        //int width = int.Parse(mapWidth.text);
        //int height = int.Parse(mapHeight.text);

        if (mapCounter == 0)
        {
            width = 26;
            height = 27;
        }
        else
        {
            width = 22;
            height = 26;
        }

        //Room.mapCounter = this.mapCounter;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName; 
    }

    [Command]
    private void CmdSetStatsAndIcon(List<string> stats, string icon)
    {
        playerStats = stats;
        playerSprite = icon;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        //If this instance is not the first player in RoomPlayers (lobby leader/host) then return
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        Room.StartGame();
    }
    #endregion
}
