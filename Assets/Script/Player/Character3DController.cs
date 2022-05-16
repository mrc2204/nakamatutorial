using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Character3DController : BaseCharacterController
{
    Rigidbody rigi;

    [SerializeField] protected float TimeSmooth, rotationSpeed;
    public float RunSpeed, JumpSpeed;
    protected float horizontalMovement, verticalMovement;
    
    [SerializeField] private Vector3 CurrenPos;
    [SerializeField] private Vector3 targetPosition;

    public Vector3 input;
    public Vector3 move;
    protected bool jump;
    private bool jumpHeld;

    protected override void Awake()
    {
        rigi = GetComponent<Rigidbody>();
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
    {
        if (rigi.velocity.y == 0)
        {
            ActiveIdle();
            return;
        }

    }


    protected override void OnRun()
    {
         input = new Vector3(horizontalMovement ,0,verticalMovement );

        CurrenPos = Vector3.SmoothDamp(CurrenPos, input, ref targetPosition, TimeSmooth);
        move = new Vector3(CurrenPos.x , 0, CurrenPos.z);
        rigi.velocity = move * RunSpeed;

    }

    protected override void ActiveJump()
    {
        base.ActiveJump();
        rigi.velocity += new Vector3(0f, transform.up.y * JumpSpeed, 0f);
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
