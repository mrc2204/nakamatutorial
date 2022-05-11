/*
Copyright 2021 Heroic Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Nakama;
using Nakama.TinyJson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A singleton class that handles all game level logic.
/// </summary>
public class GameManager : MonoBehaviour
{
    public NakamaConnection NakamaConnection;
    public GameObject NetworkLocalPlayerPrefab;
    public GameObject NetworkRemotePlayerPrefab;
    public Text WinningPlayerText;
    public GameObject SpawnPoints;

    private IDictionary<string, GameObject> players;
    private string localUser;
    private GameObject localPlayer;
    private IMatch currentMatch;

    private Transform[] spawnPoints;

    private string localDisplayName;
    public static GameManager Instance;

    /// <summary>
    /// Called by Unity when the GameObject starts.
    /// </summary>
    /// 
    public static GameManager GetInstance()
    {
        Application.runInBackground = true; // dam bao viec chay game duoi background khi test desktop
        return Instance;
    }
    private void Awake()
    {
        Debug.Log("Awake NetworkClient");
        Instance = this;
    }
    private async void Start()
    {
        // Create an empty dictionary to hold references to the currently connected players.
        players = new Dictionary<string, GameObject>();

        // Get a reference to the UnityMainThreadDispatcher.
        // We use this to queue event handler callbacks on the main thread.
        // If we did not do this, we would not be able to instantiate objects or manipulate things like UI.
        var mainThread = UnityMainThreadDispatcher.Instance();

        // Connect to the Nakama server.
        await NakamaConnection.Connect();
        localUser = NakamaConnection.Session.UserId;
        // Enable the Find A Match button on the main menu.
        //MainMenu.GetComponent<MainMenu>().EnableFindMatchButton();

        // Setup network event handlers.
        NakamaConnection.Socket.ReceivedMatchmakerMatched += m => mainThread.Enqueue(() => OnReceivedMatchmakerMatched(m));
        NakamaConnection.Socket.ReceivedMatchPresence += m => mainThread.Enqueue(() => OnReceivedMatchPresence(m));
        NakamaConnection.Socket.ReceivedMatchState += m => mainThread.Enqueue(async () => await OnReceivedMatchState(m));

        // Setup in-game menu event handler.
        //InGameMenu.GetComponent<InGameMenu>().OnRequestQuitMatch.AddListener(async () => await QuitMatch());
        currentMatch = await NakamaConnection.Socket.JoinMatchAsync("b6c4c321-378a-4a18-9cd6-1e1352e3a931.nakama");
        foreach (var user in currentMatch.Presences)
        {
            Debug.Log("ss: "+user.UserId);
            Debug.Log("ssss: "+ localUser);
            if (user.UserId == localUser)
                continue;
            SpawnPlayer(currentMatch.Id, user);
        }
        Debug.Log("size : " + currentMatch.Size);
       
    }


    private async void OnReceivedMatchmakerMatched(IMatchmakerMatched matched)
    {
        //Debug.Log("=========> OnReceivedMatchmakerMatched " + matched.ToJson());
        // Cache a reference to the local user.
     
        // Join the match.
        var match = await NakamaConnection.Socket.JoinMatchAsync(matched);

        // Disable the main menu UI and enable the in-game UI.
        // In a larger game we would probably transition to a totally different scene.
        //MainMenu.GetComponent<MainMenu>().DeactivateMenu();
        //InGameMenu.SetActive(true);

        //// Play the match music.
        //AudioManager.PlayMatchTheme();

        // Spawn a player instance for each connected user.
        foreach (var user in match.Presences)
        {
            SpawnPlayer(match.Id, user);
            Debug.Log("1" + match.Id);
       
        }
      

        // Cache a reference to the current match.
        currentMatch = match;
    }

    /// <summary>
    /// Called when a player/s joins or leaves the match.
    /// </summary>
    /// <param name="matchPresenceEvent">The MatchPresenceEvent data.</param>
    private void OnReceivedMatchPresence(IMatchPresenceEvent matchPresenceEvent)
    {
        Debug.Log(matchPresenceEvent.ToJson());
        // For each new user that joins, spawn a player for them.
        foreach (var user in matchPresenceEvent.Joins)
        {
            Debug.Log("1:" + user.UserId);
            Debug.Log("22:" + NakamaConnection.Session.UserId);
            if (user != null) {
              
                SpawnPlayer(matchPresenceEvent.MatchId, user);
            }
        }

        // For each player that leaves, despawn their player.
        foreach (var user in matchPresenceEvent.Leaves)
        {        
            if (players.ContainsKey(user.UserId))
            {
                Debug.Log("daxoa : " + user.UserId);

                Destroy(players[user.UserId]);
                players.Remove(user.UserId);
            }
        }
    }

    /// <summary>
    /// Called when new match state is received.
    /// </summary>
    /// <param name="matchState">The MatchState data.</param>
    private async Task OnReceivedMatchState(IMatchState matchState)
    {
        // Get the local user's session ID.
        var userSessionId = matchState.UserPresence.SessionId;

        // If the matchState object has any state length, decode it as a Dictionary.
        var state = matchState.State.Length > 0 ? System.Text.Encoding.UTF8.GetString(matchState.State).FromJson<Dictionary<string, string>>() : null;

        // Decide what to do based on the Operation Code as defined in OpCodes.
        /*switch(matchState.OpCode)
        {
            case OpCodes.Died:
                // Get a reference to the player who died and destroy their GameObject after 0.5 seconds and remove them from our players array.
                var playerToDestroy = players[userSessionId];
                Destroy(playerToDestroy, 0.5f);
                players.Remove(userSessionId);

                // If there is only one player left and that us, announce the winner and start a new round.
                if (players.Count == 1 && players.First().Key == localUser.SessionId) {
                    AnnounceWinnerAndStartNewRound();
                }
                break;
            case OpCodes.Respawned:
                // Spawn the player at the chosen spawn index.
                SpawnPlayer(currentMatch.Id, matchState.UserPresence, int.Parse(state["spawnIndex"]));
                break;
            case OpCodes.NewRound:
                // Display the winning player's name and begin a new round.
                await AnnounceWinnerAndRespawn(state["winningPlayerName"]);
                break;
            default:
                break;
        }*/
    }


    private void SpawnPlayer(string matchId, IUserPresence user, int spawnIndex = -1)
    {
       
        // If the player has already been spawned, return early.
        if (players.ContainsKey(user.UserId))
        {
            return;
        }

        // If the spawnIndex is -1 then pick a spawn point at random, otherwise spawn the player at the specified spawn point.
        var spawnPoint = spawnIndex == -1 ?
            SpawnPoints.transform.GetChild(Random.Range(0, SpawnPoints.transform.childCount - 1)) :
            SpawnPoints.transform.GetChild(spawnIndex);

        // Set a variable to check if the player is the local player or not based on session ID.
        var isLocal = user.UserId == localUser;
        Debug.Log("UserID: "+ user.UserId);
        Debug.Log("LocalID: "+ localUser);
        // Choose the appropriate player prefab based on if it's the local player or not.
        var playerPrefab = isLocal ? NetworkLocalPlayerPrefab : NetworkRemotePlayerPrefab;

        // Spawn the new player.
        var player = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity);

        // Setup the appropriate network data values if this is a remote player.
        if (!isLocal)
        {
            player.GetComponent<PlayerNetworkRemoteSync>().NetworkData = new RemotePlayerNetworkData
            {
                MatchId = matchId,
                User = user
            };
        }

        // Add the player to the players array.
        players.Add(user.UserId, player);

        // If this is our local player, add a listener for the PlayerDied event.
        if (isLocal)
        {
            localPlayer = player;
        

            //player.GetComponent<PlayerHealthController>().PlayerDied.AddListener(OnLocalPlayerDied);
        }

        // Give the player a color based on their index in the players array.
        //player.GetComponentInChildren<PlayerColorController>().SetColor(System.Array.IndexOf(players.Keys.ToArray(), user.SessionId));
    }

   
   /* public async void AnnounceWinnerAndStartNewRound()
    {
        // Get the winning player name, this will only ever be called if we are the winner so it's fine to grab our name from here.
        // We could get this from Nakama using the following code:
        // var account = await NakamaConnection.Client.GetAccountAsync(NakamaConnection.Session);
        // var winningPlayerName = account.User.DisplayName;
        // However, as explained below in the SetDisplayName method, when testing/debugging locally we would get the same name for all clients.
        // So instead we will just use a private variable.
        var winningPlayerName = localDisplayName;

        // Send a network message telling everyone else that we won.
        //await SendMatchStateAsync(OpCodes.NewRound, MatchDataJson.StartNewRound(winningPlayerName));

        // Display the winning player message and respawn our player.
        await AnnounceWinnerAndRespawn(winningPlayerName);
    }*/

    /// <summary>
    /// Displays the winning player message and respawns the player.
    /// </summary>
    /// <param name="winningPlayerName">The name of the winning player.</param>
   /* private async Task AnnounceWinnerAndRespawn(string winningPlayerName)
    {
        
        // Set the winning player text label.
        WinningPlayerText.text = string.Format("{0} won this round!", winningPlayerName);

        // Wait for 2 seconds.
        await Task.Delay(2000);

        // Reset the winner player text label.
        WinningPlayerText.text = "";

        // Remove ourself from the players array and destroy our player.
        players.Remove(localUser.SessionId);
        Destroy(localPlayer);

        // Choose a new spawn position and spawn our local player.
        var spawnIndex = Random.Range(0, SpawnPoints.transform.childCount - 1);
        Debug.Log("AnnounceWinnerAndRespawn");
        SpawnPlayer(currentMatch.Id, localUser, spawnIndex);

        // Tell everyone where we respawned.
        //SendMatchState(OpCodes.Respawned, MatchDataJson.Respawned(spawnIndex));
    }*/

    /// <summary>
    /// Quits the current match.
    /// </summary>
    public async Task QuitMatch()
    {
        // Ask Nakama to leave the match.
        await NakamaConnection.Socket.LeaveMatchAsync(currentMatch);

        // Reset the currentMatch and localUser variables.
        currentMatch = null;
        localUser = null;

        // Destroy all existing player GameObjects.
        foreach (var player in players.Values)
        {
            Destroy(player);
        }

        // Clear the players array.
        players.Clear();

        // Show the main menu, hide the in-game menu.

        // Play the main menu theme.
        //AudioManager.PlayMenuTheme();
    }

    /// <summary>
    /// Sends a match state message across the network.
    /// </summary>
    /// <param name="opCode">The operation code.</param>
    /// <param name="state">The stringified JSON state data.</param>
    public async Task SendMatchStateAsync(long opCode, string state)
    {
        if(currentMatch != null)
            await NakamaConnection.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    /// <summary>
    /// Sends a match state message across the network.
    /// </summary>
    /// <param name="opCode">The operation code.</param>
    /// <param name="state">The stringified JSON state data.</param>
    public void SendMatchState(long opCode, string state)
    {
        if(currentMatch !=null)
            NakamaConnection.Socket.SendMatchStateAsync(currentMatch.Id, opCode, state);
    }

    /// <summary>
    /// Sets the local user's display name.
    /// </summary>
    /// <param name="displayName">The new display name for the local user</param>
    public void SetDisplayName(string displayName)
    {
        // We could set this on our Nakama Client using the below code:
        // await NakamaConnection.Client.UpdateAccountAsync(NakamaConnection.Session, null, displayName);
        // However, since we're using Device Id authentication, when running 2 or more clients locally they would both display the same name when testing/debugging.
        // So in this instance we will just set a local variable instead.
        localDisplayName = displayName;
    }
}
