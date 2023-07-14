using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;
    private const int MaxPlayersPerRoom = 2;
    private bool isConnecting = false;
    [SerializeField] private int waitTimeForSecondPlayer;

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


    #region Unity Methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    
    private void Start()
    {
    //     if (!topPanel.activeSelf)
    //     {
    //         topPanel.SetActive(true);
    //     }
    //     ActivatePanel(loginUIPanel.name);
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
        string playerName = userName;
        Debug.Log("Network Display name: " + playerName);
        if (!playerName.IsNullOrEmpty())
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            ConnectToPhoton();
        }
        else
        {
            Debug.Log("Invalid Name! Please enter player name again.");
        }
    }

    public void OnJoinRandomRoomBtnClicked()
    {
        Debug.Log("Random room joining.");
        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
        customRoomProperties["GameMode"] = "Multiplayer";
        PhotonNetwork.JoinRandomRoom(customRoomProperties, 0);
        ActivatePanel(joinRandomRoomUIPanel.name);
        // GameData.gameMode = Frolicode.Enums.GameMode.MULTIPLAYER;
    }

    public IEnumerator OnStartGameBtnClicked()
    {
        // ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
        // customRoomProperties["GameMode"] = "Multiplayer";
        // PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        yield return new WaitForSeconds(2f);
        ActivatePanel("");
        
        topPanel.SetActive(false);
        PhotonNetwork.LoadLevel("Gameplay");
        Debug.Log("Loading new scene");
    }
    #endregion

    #region Photon Callbacks
    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            isConnecting = true;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnected()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " connected to the internet.");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to the photon master server.");
        if (isConnecting)
        {
            // ActivatePanel(gameOptionsUIPanel.name);
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

    public string CreateRoom()
    {
        string roomName = "Room:" + Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions {MaxPlayers = MaxPlayersPerRoom, IsVisible = false};
        roomOptions.CleanupCacheOnLeave = false;
        PhotonNetwork.CreateRoom(roomName,  roomOptions);
        return roomName;
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
        // roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Players/Max Player: "
                           // + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
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

    public override void OnPlayerLeftRoom(Photon.Realtime.Player player)
    {
        // roomInfoTxt.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + " Players/Max Player: "
        //                    + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Debug.Log("Player left room");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // UIController.Instance.OpenGameWinScreen();
        }
    }

    #endregion
    
    #region Public Methods

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
