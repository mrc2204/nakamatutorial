using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Character3DController : BaseCharacterController
{
    Rigidbody rigi;
    [SerializeField] private float TimeSmooth, rotationSpeed;
    public float RunSpeed, JumpSpeed;
    protected float horizontalMovement, verticalMovement;
    
    [SerializeField] private Vector3 CurrenPos;
    [SerializeField] private Vector3 targetPosition;

    public Vector3 input;
    public Vector3 move;
    private bool jump;
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

        Quaternion toRotation = Quaternion.LookRotation(input, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        // Debug.Log("ss " + transform.rotation + " toRo:" + toRotation);
        /*        float tarGet = Mathf.Atan2(input.x,input.y);
                Quaternion To = Quaternion.Euler(0, tarGet, 0);
                transform.rotation = Quaternion.Lerp(transform.rotation, To, rotationSpeed * Time.deltaTime);*/


        CurrenPos = Vector3.SmoothDamp(CurrenPos, input, ref targetPosition, TimeSmooth);
        move = new Vector3(CurrenPos.x * RunSpeed, 0, CurrenPos.z * RunSpeed);
        rigi.velocity = move;

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
