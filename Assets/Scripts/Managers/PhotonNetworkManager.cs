using System;
using System.Collections;
using KnoxGameStudios;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PhotonNetworkManager : MonoBehaviourPunCallbacks
{
    public static PhotonNetworkManager Instance;
    private const int MaxPlayersPerRoom = 2;
    private bool isConnecting = false;
    [SerializeField] private int waitTimeForSecondPlayer;
    [SerializeField] private string nickName;
    public static Action GetPhotonPlayers = delegate { };
    public static Action OnLobbyJoined = delegate { };
    
    [Header("Connection Status")]
    [SerializeField] private GameObject topPanel;
    [SerializeField] private Text connectionStatusTxt;
    
    
    [Header("Login UI Panel")]
    [SerializeField] private InputField playerNameInputField;
    [SerializeField] private GameObject loginUIPanel;
    
    [Header("Game Options UI Panel")]
    [SerializeField] private GameObject gameOptionsUIPanel;
    
    [Header("Create Room UI Panel")]
    [SerializeField] private GameObject createRoomUIPanel;
    [SerializeField] private InputField roomNameInputField;
    [SerializeField] private InputField maxPlayersInputField;
    
    [Header("Join Random Room UI Panel")]
    [SerializeField] private GameObject joinRandomRoomUIPanel;
    
    [Header("Room List UI Panel")]
    [SerializeField] private GameObject roomListUIPanel;
    [SerializeField] private GameObject roomListEntryPrefab;
    [SerializeField] private Transform roomListContentTransform;
        
    [Header("Inside Room UI Panel")]
    [SerializeField] private GameObject insideRoomUIPanel;
    [SerializeField] private Text roomInfoTxt;
    [SerializeField] private GameObject playerListPrefab;
    [SerializeField] private Transform playerListContentTransform;
    [SerializeField] private Button startGameBtn;
    [SerializeField] private PhotonChatController photonChatController;


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        UIInvite.OnRoomInviteAccept += HandleRoomInviteAccept;
        // PhotonConnector.OnLobbyJoined += HandleLobbyJoined;
        // UIDisplayRoom.OnLeaveRoom += HandleLeaveRoom;
        // UIDisplayRoom.OnStartGame += HandleStartGame;
        // UIFriend.OnGetRoomStatus += HandleGetRoomStatus;
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void OnDestroy()
    {
           
        UIInvite.OnRoomInviteAccept -= HandleRoomInviteAccept;
        // PhotonConnector.OnLobbyJoined -= HandleLobbyJoined;
        // UIDisplayRoom.OnLeaveRoom -= HandleLeaveRoom;
        // UIDisplayRoom.OnStartGame -= HandleStartGame;
        // UIFriend.OnGetRoomStatus -= HandleGetRoomStatus;
           
    }
    private void Start()
    {
       
    }

    private void Update()
    {
        // if (connectionStatusTxt != null)
        // {
        //     connectionStatusTxt.text = PhotonNetwork.NetworkClientState.ToString();
        // }
    }

    #endregion
    #region UI Callbacks

    public void OnLoginBtnClicked(string userName)
    {
        nickName = userName;
        
        Debug.Log("Network Display name: " + nickName);
        if (!nickName.IsNullOrEmpty())
        {
            ConnectToPhoton();
        }
        else
        {
            Debug.Log("Invalid Name! Please enter player name again.");
        }
    }
    
    #endregion

    #region Photon Callbacks
    public void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnectedAndReady || PhotonNetwork.IsConnected) return;
        isConnecting = true;
        PhotonNetwork.AuthValues = new AuthenticationValues(nickName);
        PhotonNetwork.NickName = nickName;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnected()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " connected to the internet.");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to the photon master server.");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        photonChatController.gameObject.SetActive(true);
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("You have connected to a Photon Lobby");
        Debug.Log("Invoking get Playfab friends");
        GetPhotonPlayers?.Invoke();
        // OnLobbyJoined?.Invoke();
        
        string roomName = PlayerPrefs.GetString("PHOTONROOM");
            
        if (!string.IsNullOrEmpty(roomName))
        {
            JoinPlayerRoom();
        }
        else
        {
            CreatePhotonRoom($"{PhotonNetwork.LocalPlayer.UserId}'s Room");
        }
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room joining failed! \n" + message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Random room joining failed! \n" + message);
        string roomName = "Room:" + Random.Range(0, 10000);

        RoomOptions roomOptions = new RoomOptions {MaxPlayers = MaxPlayersPerRoom, IsVisible = true};
        roomOptions.CleanupCacheOnLeave = false;
        // ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
        // customRoomProperties["GameMode"] = "Multiplayer";
        // roomOptions.CustomRoomProperties = customRoomProperties;
        // roomOptions.CustomRoomPropertiesForLobby = new string[] { "GameMode" };
        PhotonNetwork.CreateRoom(roomName,  roomOptions);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " Created!");
    }

    public override void OnJoinedRoom()
    {
        // ActivatePanel(insideRoomUIPanel.name);
        // roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Players/Max Player: "
        //                    + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Joined " + PhotonNetwork.CurrentRoom.Name + " Players/Max Player: "
                             + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);

        // if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        // {
        //     StartCoroutine(WaitForSecondPlayer());
        // }
    }

    private IEnumerator WaitForSecondPlayer()
    {
        float elapsedTime = 0f;

        while (elapsedTime < waitTimeForSecondPlayer)
        {
            yield return null;

            if (PhotonNetwork.CurrentRoom.PlayerCount >= 2)
            {
                // Second player joined, exit the coroutine
                yield break;
            }

            elapsedTime += Time.deltaTime;
        }

        // Second player didn't join within the specified time, start the game with the bot
        StartGameWithBot();
    }

    private void StartGameWithBot()
    {
        CancelRoom();
        // GameData.gameMode = Frolicode.Enums.GameMode.PVC;
        SceneManager.LoadScene("Gameplay");
    }

    public void CancelRoom()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;     // Close the room
            PhotonNetwork.CurrentRoom.IsVisible = false;  // Hide the room

            // Leave the room to trigger OnLeftRoom callback
            //PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player player)
    {
        Debug.Log("Room Name: " + PhotonNetwork.CurrentRoom.Name + " Players/Max Player: "
                  + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("entered for gameplay start");
            if (PhotonNetwork.IsMasterClient)
            {
                SceneManager.LoadScene("Space Invaders");
            }
        }
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Debug.Log("Player left room");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // UIController.Instance.OpenGameWinScreen();
        }
    }

    #endregion
    
    #region Private Methods
    private void JoinPlayerRoom()
    {
        string roomName = PlayerPrefs.GetString("PHOTONROOM");
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
            PlayerPrefs.SetString("PHOTONROOM", "");
        }
    }

    private void HandleRoomInviteAccept(string roomName)
    {
        PlayerPrefs.SetString("PHOTONROOM", roomName);
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if (PhotonNetwork.InLobby)
            {
                JoinPlayerRoom();
            }
        }
    }
    #endregion
    
    #region Public Methods
    public void CreatePhotonRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions {IsOpen = true, MaxPlayers = MaxPlayersPerRoom, IsVisible = true};
        // roomOptions.CleanupCacheOnLeave = false;
        PhotonNetwork.JoinOrCreateRoom(roomName,  roomOptions, TypedLobby.Default);
    }
    
    public void ActivatePanel(string panelToActivate)
    {
        loginUIPanel.SetActive(panelToActivate.Equals(loginUIPanel.name));
        gameOptionsUIPanel.SetActive(panelToActivate.Equals(gameOptionsUIPanel.name));
        joinRandomRoomUIPanel.SetActive(panelToActivate.Equals(joinRandomRoomUIPanel.name));
        insideRoomUIPanel.SetActive(panelToActivate.Equals(insideRoomUIPanel.name));
        // createRoomUIPanel.SetActive(panelToActivate.Equals(createRoomUIPanel.name));
        // roomListUIPanel.SetActive(panelToActivate.Equals(roomListUIPanel.name));
    }
    
    #endregion
}
