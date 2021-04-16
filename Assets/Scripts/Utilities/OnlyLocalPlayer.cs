using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;

public class OnlyLocalPlayer : NetworkBehaviour
{
    private Transform PlayerCanvasObject;
    private List<NetworkGamePlayerDND> gamePlayers;
    private GameObject characterName;
    private NetworkManagerDND networkManager;

    [SerializeField] private Sprite toggleOn;
    [SerializeField] private Sprite toggleOff;

    private List<string> stats;
    private int[] statsMain;
    private int proficiency;

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
        stats = player.playerStats;
        statsMain = new int[] { int.Parse(stats[0]), int.Parse(stats[1]), int.Parse(stats[2]),
                                        int.Parse(stats[3]), int.Parse(stats[4]), int.Parse(stats[5]) };

        proficiency = (int)Mathf.Ceil((float.Parse(stats[10]) / 4) + 1);

        characterName = PlayerCanvasObject.Find("Character_Sheet").Find("Character_Name").GetChild(0).gameObject;
        Transform mainStats = PlayerCanvasObject.Find("Character_Sheet").Find("Stat_PanelHolder");
        Transform currentStats = PlayerCanvasObject.Find("Character_Sheet").Find("CurrentStat_PanelHolder");
        Transform savingThrows = PlayerCanvasObject.Find("Character_Sheet").Find("SavingThrow_Panel");
        Transform abilities = PlayerCanvasObject.Find("Character_Sheet").Find("Ability_Panel");

        this.GetComponent<DNDCombatUnit>().movementSpeed = int.Parse(stats[8]) / 5;
        this.GetComponent<DNDCombatUnit>().maxSpeed = int.Parse(stats[8]) / 5;

        //Set charactersheet data
        characterName.GetComponent<TextMeshProUGUI>().SetText(player.GetDisplayName());
        int counter = 0;
        foreach (Transform child in mainStats)
        {
            child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            child.GetChild(2).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(int.Parse(stats[counter])));
            savingThrows.GetChild(counter).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(int.Parse(stats[counter])));
            counter++;
        }
        foreach (Transform child in currentStats)
        {
            child.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(stats[counter]);
            counter++;
            if (counter == 9) { break; }
        }
        foreach (Transform child in abilities)
        {
            //Dont need one for "2" as there are no abilities that coincide with constitution
            if (child.name.StartsWith("0"))
            {
                child.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(statsMain[0]));
            }
            else if (child.name.StartsWith("1"))
            {
                child.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(statsMain[1]));
            }
            else if (child.name.StartsWith("3"))
            {
                child.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(statsMain[3]));
            }
            else if (child.name.StartsWith("4"))
            {
                child.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(statsMain[4]));
            }
            else if (child.name.StartsWith("5"))
            {
                child.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(Utils.getModifier(statsMain[5]));
            }
        }
    }

    public void addProficiency(GameObject profButton)
    {
        int current = int.Parse(profButton.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text);

        if (profButton.GetComponent<Toggle>().isOn)
        {
            profButton.GetComponent<Image>().sprite = toggleOn;
            if ((current + proficiency) >= 0)
                profButton.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().SetText("+" + (current + proficiency).ToString());
            else
                profButton.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().SetText((current + proficiency).ToString());
        }
        else
        {
            profButton.GetComponent<Image>().sprite = toggleOff;
            if ((current - proficiency) >= 0)
                profButton.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().SetText("+" + (current - proficiency).ToString());
            else
                profButton.transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().SetText((current - proficiency).ToString());
        }
    }
}
