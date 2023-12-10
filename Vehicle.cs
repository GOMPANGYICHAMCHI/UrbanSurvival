using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("-���� ���� ��ġ-")]

    [Header("���� �� ����")]
    // ���� ȸ������
    public float RotateSpeed;
    // ���� ���ӵ�
    public float SpeedDecreaseAmount;
    // ���� �ִ� �ӵ�
    public float MaxMoveSpeed;
    // ���� ���ӵ�
    public float AccelerationAmount;
    // �극��ũ ����
    public float BrakeAmount;

    [Header("�浹")]
    // ���� �浹 ������
    public float HitDamageToZombie;
    // ���� �浹 ������
    public float HitDamageToSelf;
    // �浹�� ��������
    public float HitSpeedDeceaseAmount;

    [Header("ü�� �� ����")]
    // �ִ�ü�� 
    public float MaxHealth;
    // �ִ� ����
    public float MaxFuel;
    // ���� ��뷮
    public float FuelUsePerAccel;


    [Header("-���� ���� ������Ʈ��-")]
    // ����� ������Ʈ
    public GameObject RearLampObj;

    [Header("-���� ���� ������Ʈ��-")]
    // �ķ�
    public GameObject[] RWheelJointObj;
    // ���� ������
    public GameObject FWheelRightObj;
    // ���� ����
    public GameObject FWheelLeftObj;

    // ���� ȸ�� �ӵ�
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
