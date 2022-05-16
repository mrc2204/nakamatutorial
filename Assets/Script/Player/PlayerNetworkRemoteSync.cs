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
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Syncs a remotely connected player's character using received network data.
/// </summary>
public class PlayerNetworkRemoteSync : MonoBehaviour
{
    public RemotePlayerNetworkData NetworkData;

    /// <summary>
    /// The speed (in seconds) in which to smoothly interpolate to the player's actual position when receiving corrected data.
    /// </summary>
    public float LerpTime = 0.05f;

    private GameManager gameManager;
    private PlayerMovementController playerMovementController;
    private Rigidbody playerRigidbody;
    private Transform playerTransform;
    private float lerpTimer;
    private Vector3 lerpFromPosition;
    private Vector3 lerpToPosition;
    private bool lerpPosition;

    /// <summary>
    /// Called by Unity when this GameObject starts.
    /// </summary>
    private void Start()
    {
        // Cache a reference to the required components.
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        playerMovementController = GetComponentInChildren<PlayerMovementController>();
        playerRigidbody = GetComponentInChildren<Rigidbody>();
        playerTransform = playerRigidbody.GetComponent<Transform>();

        // Add an event listener to handle incoming match state data.
        gameManager.NakamaConnection.Socket.ReceivedMatchState += EnqueueOnReceivedMatchState;
    }

    /// <summary>
    /// Called by Unity after all Update calls have been made.
    /// </summary>
    /// 
 
    private void FixedUpdate()
    {
        if (!lerpPosition)
        {
            return;
        }

        // Interpolate the player's position based on the lerp timer progress.
        playerTransform.position = Vector3.Lerp(lerpFromPosition, lerpToPosition, lerpTimer / LerpTime);
        lerpTimer += Time.deltaTime;

        // If we have reached the end of the lerp timer, explicitly force the player to the last known correct position.
        if (lerpTimer >= LerpTime)
        {
            playerTransform.position = lerpToPosition;
            lerpPosition = false;
        }
    }

    /// <summary>
    /// Called when this GameObject is being destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.NakamaConnection.Socket.ReceivedMatchState -= EnqueueOnReceivedMatchState;
        }
    }

    /// <summary>
    /// Passes execution of the event handler to the main unity thread so that we can interact with GameObjects.
    /// </summary>
    /// <param name="matchState">The incoming match state data.</param>
    private void EnqueueOnReceivedMatchState(IMatchState matchState)
    {
        var mainThread = UnityMainThreadDispatcher.Instance();
        mainThread.Enqueue(() => OnReceivedMatchState(matchState));
    }

    /// <summary>
    /// Called when receiving match data from the Nakama server.
    /// </summary>
    /// <param name="matchState">The incoming match state data.</param>
    private void OnReceivedMatchState(IMatchState matchState)
    {
        // If the incoming data is not related to this remote player, ignore it and return early.
        if (matchState.UserPresence.UserId != NetworkData.User.UserId)
        {
            return;
        }

        // Decide what to do based on the Operation Code of the incoming state data as defined in OpCodes.
        switch (matchState.OpCode)
        {
            case OpCodes.VelocityAndPosition:
                UpdateVelocityAndPositionFromState(matchState.State);
                break;
            case OpCodes.Input:
                SetInputFromState(matchState.State);
              
                break;
            case OpCodes.Died:
               // playerMovementController.PlayDeathAnimation();
                break;
            default:
                break;
        }
    }


    private IDictionary<string, string> GetStateAsDictionary(byte[] state)
    {
        return Encoding.UTF8.GetString(state).FromJson<Dictionary<string, string>>();
    }

    /// <summary>
    /// Sets the appropriate input values on the PlayerMovementController and PlayerWeaponController based on incoming state data.
    /// </summary>
    /// <param name="state">The incoming state Dictionary.</param>
    private void SetInputFromState(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);
      //  Debug.Log("=========> " + stateDictionary.ToJson());
        playerMovementController.SetHorizontalMovement(float.Parse(stateDictionary["horizontalInput"]));
        playerMovementController.SetVeticalMovement(float.Parse(stateDictionary["verticalInput"]));
        playerMovementController.SetJump(bool.Parse(stateDictionary["jump"]));
        playerMovementController.SetJumpHeld(bool.Parse(stateDictionary["jumpHeld"]));
       
       

        //if (bool.Parse(stateDictionary["attack"]))
        //{
        //    playerWeaponController.Attack();
        //}
    }

    /// <summary>
    /// Updates the player's velocity and position based on incoming state data.
    /// </summary>
    /// <param name="state">The incoming state byte array.</param>
    private void UpdateVelocityAndPositionFromState(byte[] state)
    {
        var stateDictionary = GetStateAsDictionary(state);

      //  playerRigidbody.velocity = new Vector3(float.Parse(stateDictionary["velocity.x"]), float.Parse(stateDictionary["velocity.y"]), float.Parse(stateDictionary["velocity.z"]));
        var position = new Vector3(
            float.Parse(stateDictionary["position.x"]),
            float.Parse(stateDictionary["position.y"]),
            float.Parse(stateDictionary["position.z"]));
        Debug.Log(">> Position: " + stateDictionary.ToJson());

        // Begin lerping to the corrected position.
        lerpFromPosition = playerTransform.position;
        lerpToPosition = position;
        lerpTimer = 0;
        lerpPosition = true;
    }
}
