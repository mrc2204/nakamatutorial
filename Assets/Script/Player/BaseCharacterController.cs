using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BaseCharacterController : MonoBehaviour
{
    public abstract Vector3 Direction
    {
        set;
    }
    public enum Character
    {

        RUN,
        Idle,
        JUMP
    }
    public Character characterController = Character.Idle;


    Animator animator;
    protected virtual void Awake()
    {
   
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected void Update()
    {
        switch (characterController)
        {
      
            case Character.RUN:
                OnRun();
                break;
            case Character.Idle:
                OnIdle();
                break;
            case Character.JUMP:
                Onjump();
                break;

        }
    }

    protected abstract void OnIdle();
    protected abstract void OnRun();
    protected abstract void Onjump();
      protected virtual void ActiveIdle()
    {
        characterController = Character.Idle;
       animator.Play("Idle");
     //   Debug.Log("vao 2");

    }
    protected virtual void ActiveRun()
    {

        characterController = Character.RUN;
        animator.Play("Run");
      //  Debug.Log("vao 1");

    }


    protected virtual void ActiveJump()
    {

        characterController = Character.JUMP;
       animator.Play("Jump");
     //   Debug.Log("vao 3");

    }
}
