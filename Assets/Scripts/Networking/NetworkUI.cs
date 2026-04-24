using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField ipInputField;
    public Button hostButton;
    public Button joinButton;
    public GameObject uiPanel;

    void Start()
    {
        // Set default IP
        ipInputField.text = "127.0.0.1";

        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        SetIP();
        NetworkManager.Singleton.StartHost();
        HideUI();
    }

    private void StartClient()
    {
        SetIP();
        NetworkManager.Singleton.StartClient();
        HideUI();
    }

    private void SetIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipInputField.text;
    }

    private void HideUI()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
}