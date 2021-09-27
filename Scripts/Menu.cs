using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>(); 

    // Start is called before the first frame update
    void Start()
    {
        // Disable the menu buttons at the start.
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        // Enable the cursor since we hide it when we play the game.
        Cursor.lockState = CursorLockMode.None;

        // Are we in a game?
        if (PhotonNetwork.InRoom)
        {
            // Go to the lobby
            SetScreen(lobbyScreen);
            UpdateLobbyUI();

            // Make the room visible.
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }


    // Changes the currently visible screen
    void SetScreen(GameObject screen)
    {
        // Disable all other screens.
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        // Activate the requested screen.
        screen.SetActive(true);

        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();
        if (screen == lobbyBrowserScreen)
            UpdateLobbyBrowserUI();

    }


    // ----------------------------------------------------------------------------
    // Main Screen Photon/Button Functions
    // ----------------------------------------------------------------------------


    // Called when player inputs Player Name
    public void OnPlayerNameValueChanged(TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }


    // Called when the program connects to Photon
    public override void OnConnectedToMaster()
    {
        // Enable the menu buttons once we connect to the server.
        createRoomButton.interactable = true;
        findRoomButton.interactable = true;
    }


    // Called when the "Create Room" button has been pressed.
    public void OnCreateRoomButton()
    {
        SetScreen(createRoomScreen);
    }


    // Called when the "Find Room" button has been pressed.
    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);
    }


    // ----------------------------------------------------------------------------
    // Create Room Screen Photon/Button Functions
    // ----------------------------------------------------------------------------


    // Called when the "Back" button gets pressed.
    public void OnBackButton()
    {
        SetScreen(mainScreen);
    }


    // Called when the "Create" button gets pressed/
    public void OnCreateButton(TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }


    // ----------------------------------------------------------------------------
    // Lobby Screen Photon/Button Functions
    // ----------------------------------------------------------------------------


    // Called when the player joins the lobby
    public override void OnJoinedRoom()
    {
        SetScreen(lobbyScreen);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }


    // Update player and room information for the player to see
    [PunRPC]
    void UpdateLobbyUI ()
    {
        // Enable or disable the start game button depending on if we're the host.
        startGameButton.interactable = PhotonNetwork.IsMasterClient;

        // Display all the players.
        playerListText.text = "";

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }

        // Set the room info text
        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;
    }


    // Called when a player leaves the lobby
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }


    // Called when the "Start" button gets pressed.
    public void OnStartGameButton()
    {
        // Hide the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // Tell everyone to load the game scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }


    // Called when the "Leave Lobby" button gets pressed.
    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }


    // ----------------------------------------------------------------------------
    // Lobby Browser Screen Photon/Button Functions
    // ----------------------------------------------------------------------------


    // Update available rooms information for the player to see
    void UpdateLobbyBrowserUI()
    {
        Debug.Log("In UpdateLobbyBrowserUI()");

        // disable all room buttons
        foreach (GameObject button in roomButtons)
        {
            Debug.Log("in foreach loop");
            button.SetActive(false);
        }

        Debug.Log("After Disable all room buttons");
        Debug.Log("Room count" + roomList.Count);

        // display all current rooms in the master server
        for (int x = 0; x < roomList.Count; ++x)
        {
            // get or create the button object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons[x];
            button.SetActive(true);
            
            // set the room name and player count texts
            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;

            // set the button OnClick event
            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }


    // Create a button for an available room.
    GameObject CreateRoomButton()
    {
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);
        return buttonObj;
    }


    // Called when "Join Room" button is clicked.
    public void OnJoinRoomButton(string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }


    // Called when "Refresh" button is clicked.
    public void OnRefreshButton()
    {
        UpdateLobbyBrowserUI();
    }
    

    // Called by Photon
    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        Debug.Log("OnRoomListUpdate is called from Photon");
        roomList = allRooms;
    }


    // ----------------------------------------------------------------------------
    // Exit Button
    // ----------------------------------------------------------------------------
    public void OnExitButton()
    {
        Debug.Log("Quit Button pressed (Menu.cs)");
        Application.Quit();
    }
}
