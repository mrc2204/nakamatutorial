using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Character3DController : BaseCharacterController
{
    Rigidbody rigi;
    CharacterController character;
    [SerializeField] protected float TimeSmooth, rotationSpeed;
    public float RunSpeed, JumpSpeed, Gravity;
    public float horizontalMovement, verticalMovement;
    
    [SerializeField] private Vector3 CurrenPos;
    [SerializeField] private Vector3 targetPosition;
    public Vector3 input;
    public Vector3 move;

    protected bool jump;
    private bool jumpHeld;

    protected override void Awake()
    {
        character = GetComponent<CharacterController>();
        base.Awake();
    }
    public override Vector3 Direction
    {
        set
        {
            transform.forward = value;
        }

    }
    protected  override void Onjump()
    {/*
        if (rigi.velocity.y == 0)
        {
            ActiveIdle();
            return;
        }*/

    }

    protected override void OnIdle()
    {
          if(!character.isGrounded)
        {
            move.y -= Gravity * Time.deltaTime;
            character.Move(move * RunSpeed * Time.deltaTime);

        }

    }
    protected override void OnRun()
    {
   

        if (character.isGrounded)
        {
             input = new Vector3(horizontalMovement, 0, verticalMovement);
            CurrenPos = Vector3.SmoothDamp(CurrenPos, input, ref targetPosition, TimeSmooth);
            move = new Vector3(CurrenPos.x, 0, CurrenPos.z);
            move.y = 0;
        }   
     
        else
        {
            move.y -= Gravity * Time.deltaTime;
        }
        character.Move(move * RunSpeed * Time.deltaTime);


    }
    protected override void ActiveJump()
    {
        base.ActiveJump();
    }


    public void SetHorizontalMovement(float value)
    {
        horizontalMovement = value;
    }

    internal void SetVeticalMovement(float value)
    {
        verticalMovement = value;
    }

    public void SetJump(bool value)
    {
        jump = value;
    }

    public void SetJumpHeld(bool value)
    {
        jumpHeld = value;
    }
}
