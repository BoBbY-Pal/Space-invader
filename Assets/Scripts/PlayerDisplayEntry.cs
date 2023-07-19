using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDisplayEntry : MonoBehaviour
{
    [SerializeField] TMP_Text displayNameTxt;
    [SerializeField] Button inviteBtn;
    private string playfabID;
    public PlayerLeaderboardEntry inviteePlayer;
    private FriendInfo frnd;
    
    public void Setup(PlayerLeaderboardEntry player)
    {
        displayNameTxt?.SetText($"{player.DisplayName}");
        playfabID = player.PlayFabId;
        
    }

    private void Start()
    {
        inviteBtn.onClick.AddListener(ChallengePlayer);
    }

    private void ChallengePlayer()
    {
        // string roomID = PhotonNetworkManager.Instance.CreateRoom();
        string roomID = "";
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "sendChallenge",
            FunctionParameter = new
            {
                ChallengedPlayerId = playfabID,
                RoomID = roomID
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            Debug.Log("Challenge request successfully sent to " + playfabID);
        }, error => 
        {
            Debug.Log("Failed to send Challenge Request ");
        });
    }
}
