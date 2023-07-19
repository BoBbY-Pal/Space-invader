using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeHandler : MonoBehaviour
{
    // How often to check for challenges in seconds
    public float pollInterval = 1f;

    [SerializeField]private string playFabId; // Set this to your PlayFab ID
    [SerializeField]private string roomId; 
    [SerializeField] private GameObject challengePopUpPrefab;
    [SerializeField] private Button acceptChallengeBtn;
    [SerializeField] private Button declineChallengeBtn;
    
    // This flag indicates whether a challenge is currently being processed
    private bool isProcessingChallenge = false;

    private void Start()
    {
        // playFabId = PlayFabManager.Instance.playerFabID;
        acceptChallengeBtn.onClick.AddListener(AcceptChallengeRequest);
        declineChallengeBtn.onClick.AddListener(DeclineChallengeRequest);
        // DeleteChallenge();
        
        // Start the polling coroutine
        StartCoroutine(PollForChallenge());
    }

    private IEnumerator PollForChallenge()
    {
        while (true)
        {
            if (!isProcessingChallenge)
            {
                Debug.Log("Checking for received challenges");

                // Create the request
                var request = new GetUserDataRequest
                {
                    PlayFabId = playFabId,
                    Keys = new List<string> {"Challenge"}
                };

                // Send the request
                PlayFabClientAPI.GetUserData(request, OnDataReceived, OnError);
            }

            // Wait for the specified interval before the next check
            yield return new WaitForSeconds(pollInterval);
        }
    }

    private void OnDataReceived(GetUserDataResult result)
    {
        Debug.Log("Data received");
        if (result.Data != null && result.Data.ContainsKey("Challenge"))
        {
            // Set the flag to indicate that a challenge is being processed
            isProcessingChallenge = true;
            var challenge = JsonUtility.FromJson<ChallengeData>(result.Data["Challenge"].Value);
            roomId = challenge.RoomID;
            Debug.Log("Room ID: " + roomId);
            // Instantiate(challengePopUpPrefab, transform);
            challengePopUpPrefab.SetActive(true);
            // You've received a challenge! Handle it as needed.
            Debug.Log("You've received a challenge!");
            DeleteChallenge();
        }
    }

    private void DeleteChallenge()
    {
        // Create the request
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "deleteChallenge",
            // GeneratePlayStreamEvent = true,
        };

        // Send the request
        PlayFabClientAPI.ExecuteCloudScript(request, success =>
        {
            Debug.Log("Challenge key deleted from server. ");
            
            // Clear the flag to allow new challenges to be processed
            isProcessingChallenge = false;
        }, OnError);
    }
    private void OnError(PlayFabError error)
    {
        Debug.LogError($"Error getting user data: {error.GenerateErrorReport()}");
        
        // Clear the flag to allow new challenges to be processed
        isProcessingChallenge = false;
    }

    private void AcceptChallengeRequest()
    {
        PhotonNetwork.JoinRoom(roomId);
        Debug.Log("Challenge accepted");
    }
    private void DeclineChallengeRequest()
    {
        challengePopUpPrefab.SetActive(false);
        Debug.Log("Challenge declined ");
    }
    
    [System.Serializable]
    public class ChallengeData
    {
        public string ChallengingPlayer;
        public string Timestamp;
        public string RoomID;
    }
}