﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PlayerController : MonoBehaviour
{
    private int hp = 3;
    public int Hp {
        set { hp = value; }
        get { return hp; }
    }
    private float fevertime = 5;

    private const float LANE_DISTANCE = 6.0f;
    private const float TURN_SPEED = 0.025f;

    private bool _isRunning = false;

    //Movement (임시)
    private CharacterController Controller;
    private float JumpForce = 8.0f;
    private float Gravity = 12.0f;
    private float VerticalVelocity;
    [SerializeField]
    private float MoveSpeed = 5.24f;
    public float moveSpeed {
        set { MoveSpeed = value; }
        get { return MoveSpeed; }
    }

    private float MoveSpeedIncreaseLastTick;
    private float MoveSpeedIncreaseTime = 15.0f;     //속도 증가 쿨타임
    private float MoveSpeedIncreaseAmount = 1.2f;    //가속도
    
    private float LaneMoveSpeed = 30.0f;
    private int DesiredLane = 1; // 0 = 좌측, 1 = 중앙, 2 = 우측

    private void Start()
    {
        Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        GameStart();
    }

    public void StartRunning()
    {
        _isRunning = true;
    }

    private void StartSliding()
    {
        //애니메이션
        Controller.height /= 2;
        Controller.center = new Vector3(Controller.center.x , Controller.center.y / 2, Controller.center.z);
    }
    
    private void StopSliding()
    {
        //애니메이션
        Controller.height *= 2;
        Controller.center = new Vector3(Controller.center.x , Controller.center.y * 2, Controller.center.z);
    }

    private void Crash()
    {
        //anim.SetTrigger("Hit"); 
        //만약 HP가 0으로 되었다면 Death 애니메이션 출력과 동시에 
        //bool _isRunning 을 false로 바꿔준다. 
        hp--;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //장애물은 Tag를 Obstacle 로 지정하겠습니다.
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                if (ItemManager.Instance.isShield == true)
                    Crash();
                break;
            case "Item":
                ItemManager.Instance.CurrentItem(hit.gameObject.name);
                Destroy(hit.gameObject);
                break;
        }
        
        //switch (collision.gameObject.tag)
        //{
        //    case "Obstacle":
        //        if (ItemManager.Instance.isShield == true)
        //            Crash();
        //        break;
        //    case "Item":
        //        ItemManager.Instance.CurrentItem(collision.gameObject.name);
        //        Destroy(collision.gameObject);
        //        break;
        //}
    }

    private void GameStart()
    {
        if (_isRunning == false)
        {
            return;
        }

        if (Time.time - MoveSpeedIncreaseLastTick > MoveSpeedIncreaseTime)
        {
            MoveSpeedIncreaseLastTick = Time.time;
            MoveSpeed += MoveSpeedIncreaseAmount;
        }
        
        if (MobileInput.Instance.tap && _isRunning == false)
        {
            _isRunning = true;
        }
        
        if (MobileInput.Instance.swipeLeft)
        {
            MoveLane(false);
        }
        if (MobileInput.Instance.swipeRight)
        {
            MoveLane(true);
        }

        var TargetPosition = transform.position.z * Vector3.forward;
        switch (DesiredLane)
        {
            case 0:
                TargetPosition += Vector3.left * LANE_DISTANCE;
                break;
            case 2:
                TargetPosition += Vector3.right * LANE_DISTANCE;
                break;
        }

        var MoveVector = Vector3.zero;
        MoveVector.x = (TargetPosition - transform.position).normalized.x * LaneMoveSpeed;

        if (_isGrounded()) 
        {
            VerticalVelocity = -0.1f;
            if (MobileInput.Instance.swipeUp)
            {
                // Jump
                VerticalVelocity = JumpForce;
            }
            else if (MobileInput.Instance.swipeDown)
            {
                //Slide
                StartSliding();
                Invoke("StopSliding", 1.0f);
            }
        }
        else
        {
            VerticalVelocity -= (Gravity * Time.deltaTime);
            
            //빠르게 떨어지기
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                VerticalVelocity = -JumpForce;
            }
            
        }
        
        MoveVector.y = VerticalVelocity;
        MoveVector.z = MoveSpeed;

        Controller.Move(MoveVector * Time.deltaTime);
        
        var Dir = Controller.velocity;
        if (Dir != Vector3.zero)
        {
            Dir.y = 0;
            transform.forward = Vector3.Lerp(transform.forward, Dir, TURN_SPEED);
        }
    }

    private void MoveLane(bool GoingRight)
    {
        DesiredLane += (GoingRight) ? 1 : -1;
        DesiredLane = Mathf.Clamp(DesiredLane, 0, 2);
    }

    private bool _isGrounded()
    {
        Ray GroundRay = new Ray(new Vector3(Controller.bounds.center.x,
                (Controller.bounds.center.y - Controller.bounds.extents.y) + 0.2f,
                Controller.bounds.center.z), Vector3.down);
        Debug.DrawRay(GroundRay.origin, GroundRay.direction, Color.cyan, 1.0f);

        if (Physics.Raycast(GroundRay, 0.2f + 0.1f))
        {
            return true;
        }

        return false;
    }
}
