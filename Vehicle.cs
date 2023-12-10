using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("-차량 관련 수치-")]

    [Header("조향 및 가속")]
    // 차량 회전각도
    public float RotateSpeed;
    // 차량 감속도
    public float SpeedDecreaseAmount;
    // 차량 최대 속도
    public float MaxMoveSpeed;
    // 차량 가속도
    public float AccelerationAmount;
    // 브레이크 정도
    public float BrakeAmount;

    [Header("충돌")]
    // 좀비 충돌 데미지
    public float HitDamageToZombie;
    // 셀프 충돌 데미지
    public float HitDamageToSelf;
    // 충돌시 감속정도
    public float HitSpeedDeceaseAmount;

    [Header("체력 및 연료")]
    // 최대체력 
    public float MaxHealth;
    // 최대 연료
    public float MaxFuel;
    // 연료 사용량
    public float FuelUsePerAccel;


    [Header("-차량 내부 오브젝트들-")]
    // 리어램프 오브젝트
    public GameObject RearLampObj;

    [Header("-차량 바퀴 오브젝트들-")]
    // 후륜
    public GameObject[] RWheelJointObj;
    // 전륜 오른쪽
    public GameObject FWheelRightObj;
    // 전륜 왼쪽
    public GameObject FWheelLeftObj;

    // 전륜 회전 속도
    float FWRotation_x;

    PlayerController mPC;

    public void SetPlayerController(PlayerController input_controller)
    {
        mPC = input_controller;
    }

    public void ActOnUpdate()
    {
        UpdateWheels();
    }

    public void UpdateWheels()
    {
        for (int i = 0; i < RWheelJointObj.Length; i++)
        {
            RWheelJointObj[i].transform.Rotate(600 * mPC.CurrentMoveSpeed / MaxMoveSpeed * Time.deltaTime, 0, 0);
        }
        
        FWRotation_x += 600 * mPC.CurrentMoveSpeed / MaxMoveSpeed * Time.deltaTime;
        
        Quaternion rotation;
        
        if (mPC.CurrentRotateState == RotateState.Left)
        {
            rotation = Quaternion.Euler(FWRotation_x, -40, 0);
        }
        else if (mPC.CurrentRotateState == RotateState.Right)
        {
            rotation = Quaternion.Euler(FWRotation_x, 40, 0);
        }
        else
        {
            rotation = Quaternion.Euler(FWRotation_x, 0, 0);
        }
        
        FWheelLeftObj.transform.rotation = transform.rotation * rotation;
        FWheelRightObj.transform.rotation = transform.rotation * rotation;
    }

    public void RearLampSet(bool input_isOn)
    {
        RearLampObj.SetActive(input_isOn);
    }
}
