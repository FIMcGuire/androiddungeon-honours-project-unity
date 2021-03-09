using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCurrentStatInput : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField Health = null;
    [SerializeField] private TMP_InputField ArmorClass = null;
    [SerializeField] private TMP_InputField Speed = null;
    [SerializeField] private Button continueButton = null;

    public static string HealthStat { get; private set; }
    private const string PlayerPrefsHealthKey = "Health";
    public static string ArmorClassStat { get; private set; }
    private const string PlayerPrefsArmorClassKey = "ArmorClass";
    public static string SpeedStat { get; private set; }
    private const string PlayerPrefsSpeedKey = "Speed";

    private void Start() => SetUpInputField();

    private void Update()
    {
        if (!string.IsNullOrEmpty(Health.text) && !string.IsNullOrEmpty(ArmorClass.text) && !string.IsNullOrEmpty(Speed.text))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsHealthKey) && !PlayerPrefs.HasKey(PlayerPrefsArmorClassKey) && !PlayerPrefs.HasKey(PlayerPrefsSpeedKey)) { return; }

        string defaultHealth = PlayerPrefs.GetString(PlayerPrefsHealthKey);
        string defaultArmorClass = PlayerPrefs.GetString(PlayerPrefsArmorClassKey);
        string defaultSpeed = PlayerPrefs.GetString(PlayerPrefsSpeedKey);

        Health.text = defaultHealth;
        ArmorClass.text = defaultArmorClass;
        Speed.text = defaultSpeed;

        string[] values = { defaultHealth, defaultArmorClass, defaultSpeed };
        int counter = 0;
        foreach (Transform text in this.gameObject.transform)
        {
            text.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().SetText(values[counter].ToString());
            counter++;
            if (counter == 3) { return; }
        }
    }

    public void SaveStats()
    {
        HealthStat = Health.text;
        ArmorClassStat = ArmorClass.text;
        SpeedStat = Speed.text;

        PlayerPrefs.SetString(PlayerPrefsHealthKey, HealthStat);
        PlayerPrefs.SetString(PlayerPrefsArmorClassKey, ArmorClassStat);
        PlayerPrefs.SetString(PlayerPrefsSpeedKey, SpeedStat);
    }
}
