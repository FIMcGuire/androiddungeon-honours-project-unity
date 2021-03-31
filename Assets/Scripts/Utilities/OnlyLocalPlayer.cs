using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class OnlyLocalPlayer : NetworkBehaviour
{
    private Transform PlayerCanvasObject;
    private List<NetworkGamePlayerDND> gamePlayers;
    private GameObject characterName;
    private NetworkManagerDND networkManager;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = NetworkManager.singleton as NetworkManagerDND;
        gamePlayers = networkManager.GamePlayers;

        transform.parent = GameObject.Find("PlayerObjects").transform;

        if (hasAuthority)
        {
            PlayerCanvasObject = this.transform.Find("Canvas");

            PlayerCanvasObject.gameObject.SetActive(true);
            foreach (var player in gamePlayers)
            {
                if (player.hasAuthority)
                {
                    setCharacterSheet(player);
                }
                else
                {
                    Debug.Log("No Authority");
                }
            }
        }
        else
        {
            enabled = false;
        }
    }

    void setCharacterSheet(NetworkGamePlayerDND player)
    {
        List<string> stats = player.playerStats;

        characterName = PlayerCanvasObject.Find("Character_Sheet").Find("Character_Name").GetChild(0).gameObject;
        Transform mainStats = PlayerCanvasObject.Find("Character_Sheet").Find("Stat_PanelHolder");
        Transform currentStats = PlayerCanvasObject.Find("Character_Sheet").Find("CurrentStat_PanelHolder");

        this.GetComponent<DNDCombatUnit>().movementSpeed = int.Parse(stats[8]) / 5;
        this.GetComponent<DNDCombatUnit>().maxSpeed = int.Parse(stats[8]) / 5;

        //Set charactersheet data
        characterName.GetComponent<TextMeshProUGUI>().SetText(player.GetDisplayName());
        int counter = 0;
        foreach (Transform child in mainStats)
        {
            child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            child.GetChild(2).GetComponent<TextMeshProUGUI>().SetText(getModifier(int.Parse(stats[counter])));
            counter++;
        }
        foreach (Transform child in currentStats)
        {
            child.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            counter++;
            if (counter == 9) { return; }
        }
    }

    string getModifier(int stat)
    {
        switch (stat)
        {
            case 1:
                return "-5";
            case 2:
            case 3:
                return "-4";
            case 4:
            case 5:
                return "-3";
            case 6:
            case 7:
                return "-2";
            case 8:
            case 9:
                return "-1";
            case 10:
            case 11:
                return "+0";
            case 12:
            case 13:
                return "+1";
            case 14:
            case 15:
                return "+2";
            case 16:
            case 17:
                return "+3";
            case 18:
            case 19:
                return "+4";
            case 20:
                return "+5";
            default:
                return "+0";
        }
    }
}
