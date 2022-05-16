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

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A component that handles all player movement.
/// </summary>
public class PlayerMovementController : Character3DController
{
    [HideInInspector]
    public UnityEvent CollidedWithProjectile;



    protected override void OnRun()
    {
        base.OnRun();

        if (!ChangeDirection())
        {
            ActiveIdle();

            return;
        }
        if (jump == true)
        {
            ActiveJump();
            return;
        }
    }
 
    protected override void OnIdle()
    {   

        if (Mathf.Abs(horizontalMovement) != 0 || Mathf.Abs(verticalMovement) != 0)
        {
            ActiveRun();
        }
        if (jump == true)
        {
            ActiveJump();
            return;
        }
    }
    protected override void Onjump()
    {
        base.Onjump();
  
        if (ChangeDirection())
        {
             base.OnRun();
        }

    }
    bool ChangeDirection()
    {
        /*
                if (JoytickMoveController.Intansce.direction.x == 0f && JoytickMoveController.Intansce.direction.y == 0f) return false;

                Direction = new Vector3(JoytickMoveController.Intansce.direction.x , 0, JoytickMoveController.Intansce.direction.y);*/
        // Joytick
        if (horizontalMovement == 0f && verticalMovement == 0f) return false;

        Quaternion toRotation = Quaternion.LookRotation(input, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        ///  Direction = new Vector3(input.x, 0, input.z); 
        return true;
    }
   
}
