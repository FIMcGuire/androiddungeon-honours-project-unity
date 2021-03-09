using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider Strength = null;
    [SerializeField] private Slider Dexterity = null;
    [SerializeField] private Slider Constitution = null;
    [SerializeField] private Slider Wisdom = null;
    [SerializeField] private Slider Intelligence = null;
    [SerializeField] private Slider Charisma = null;

    public static float StrengthStat { get; private set; }
    private const string PlayerPrefsSTRKey = "STR";
    public static float DexterityStat { get; private set; }
    private const string PlayerPrefsDEXKey = "DEX";
    public static float ConstitutionStat { get; private set; }
    private const string PlayerPrefsCONKey = "CON";
    public static float WisdomStat { get; private set; }
    private const string PlayerPrefsWISKey = "WIS";
    public static float IntelligenceStat { get; private set; }
    private const string PlayerPrefsINTKey = "INT";
    public static float CharismaStat { get; private set; }
    private const string PlayerPrefsCHAKey = "CHA";

    private void Start() => SetUpInputField();

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsSTRKey) && !PlayerPrefs.HasKey(PlayerPrefsDEXKey) && !PlayerPrefs.HasKey(PlayerPrefsCONKey)
            && !PlayerPrefs.HasKey(PlayerPrefsWISKey) && !PlayerPrefs.HasKey(PlayerPrefsINTKey) && !PlayerPrefs.HasKey(PlayerPrefsCHAKey)) { return; }

        float defaultSTR = PlayerPrefs.GetFloat(PlayerPrefsSTRKey);
        float defaultDEX = PlayerPrefs.GetFloat(PlayerPrefsDEXKey);
        float defaultCON = PlayerPrefs.GetFloat(PlayerPrefsCONKey);
        float defaultWIS = PlayerPrefs.GetFloat(PlayerPrefsWISKey);
        float defaultINT = PlayerPrefs.GetFloat(PlayerPrefsINTKey);
        float defaultCHA = PlayerPrefs.GetFloat(PlayerPrefsCHAKey);

        Strength.value = defaultSTR;
        Dexterity.value = defaultDEX;
        Constitution.value = defaultCON;
        Wisdom.value = defaultWIS;
        Intelligence.value = defaultINT;
        Charisma.value = defaultCHA;

        float[] values = { defaultSTR, defaultDEX, defaultCON, defaultWIS, defaultINT, defaultCHA };
        Transform StatPanel = GameObject.Find("Panel_StatInput").transform;
        int counter = 0;
        foreach (Transform text in StatPanel)
        {
            text.GetChild(3).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().SetText(values[counter].ToString());
            counter++;
            if (counter == 6) { return; }
        }
    }

    public void SaveStats()
    {
        StrengthStat = Strength.value;
        DexterityStat = Dexterity.value;
        ConstitutionStat = Constitution.value;
        WisdomStat = Wisdom.value;
        IntelligenceStat = Intelligence.value;
        CharismaStat = Charisma.value;

        PlayerPrefs.SetFloat(PlayerPrefsSTRKey, StrengthStat);
        PlayerPrefs.SetFloat(PlayerPrefsDEXKey, DexterityStat);
        PlayerPrefs.SetFloat(PlayerPrefsCONKey, ConstitutionStat);
        PlayerPrefs.SetFloat(PlayerPrefsWISKey, WisdomStat);
        PlayerPrefs.SetFloat(PlayerPrefsINTKey, IntelligenceStat);
        PlayerPrefs.SetFloat(PlayerPrefsCHAKey, CharismaStat);
    }
}
