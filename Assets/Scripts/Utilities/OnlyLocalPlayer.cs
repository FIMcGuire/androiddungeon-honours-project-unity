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
        List<List<string>> allStats = networkManager.PlayerStats;
        Debug.Log(allStats.Count);
        List<string> stats = allStats[0];

        characterName = PlayerCanvasObject.GetChild(2).GetChild(0).GetChild(0).gameObject;

        this.GetComponent<DNDCombatUnit>().movementSpeed = int.Parse(stats[8]) / 5;
        Debug.Log(int.Parse(stats[8]) / 5);

        //Set charactersheet data
        characterName.GetComponent<TextMeshProUGUI>().SetText(player.GetDisplayName());
        int counter = 0;
        foreach (Transform child in PlayerCanvasObject.GetChild(2).GetChild(1))
        {
            child.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            counter++;
        }
        foreach (Transform child in PlayerCanvasObject.GetChild(2).GetChild(3))
        {
            child.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            counter++;
            if (counter == 9) { return; }
        }
    }
}
