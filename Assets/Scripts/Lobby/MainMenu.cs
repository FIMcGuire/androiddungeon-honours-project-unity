using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private NetworkManagerDND networkManager;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    private void Start()
    {
        networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManagerDND>();
    }

    public void HostLobby()
    {
        networkManager.StartHost();

        landingPagePanel.SetActive(false);
    }
}
