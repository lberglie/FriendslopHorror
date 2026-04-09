using UnityEngine;
using Unity.Netcode;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;

public class SteamworksLogic : MonoBehaviour
{

    private FacepunchTransport transport;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Initialize Steam Client using Spacewar App ID (480)
        try
        {
            // 1. Check if Steam is ALREADY initialized (by the Transport or Unity Editor state)
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(480, true);
                Debug.Log("Steam initialized manually. User: " + SteamClient.Name);
            }
            else
            {
                Debug.Log("Steam was already initialized. Bypassing Init. User: " + SteamClient.Name);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error initializing Steam: " + e.Message);
            return;
        }

        transport = GetComponent<FacepunchTransport>();

        // Subscribe to Steam callbacks
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }

    // Update is called once per frame
    private void Update()
    {
        SteamClient.RunCallbacks();
    }

    private void OnDisable()
    {
        // Cleanup callbacks and close the Steam connection when shutting down
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
        SteamClient.Shutdown();
    }

    // --- HOSTING ---
    public async void HostLobby()
    {
        // Create a lobby for up to 4 players (perfect for a co-op horror MVP)
        await SteamMatchmaking.CreateLobbyAsync(4);
    }

    private void OnLobbyCreated(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.LogError("Failed to create lobby: " + result);
            return;
        }

        // Make lobby public and joinable via the Steam friends list
        lobby.SetPublic();
        lobby.SetJoinable(true);

        // Start the server
        NetworkManager.Singleton.StartHost();

        Debug.Log("Lobby created successfully! Lobby ID: " + lobby.Id);
    }

    // --- JOINING ---
    private async void OnGameLobbyJoinRequested(Lobby lobby, SteamId friendId)
    {
        // Triggered when a player accepts an invite through the Steam Overlay (Shift+Tab)
        await lobby.Join();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        // Prevent the host from running client code when they enter their own lobby
        if (NetworkManager.Singleton.IsHost) return;

        // Tell the Facepunch Transport the SteamID of the lobby owner so it knows where to connect
        transport.targetSteamId = lobby.Owner.Id;

        // Tell Netcode to start as Client
        NetworkManager.Singleton.StartClient();
        Debug.Log("Joined lobby and Client started.");
    }

    // --- MVP TESTING UI ---
    // Simple on-screen button to trigger hosting
    private void OnGUI()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host Game", GUILayout.Width(200), GUILayout.Height(50)))
            {
                HostLobby();
            }
        }
    }
}
