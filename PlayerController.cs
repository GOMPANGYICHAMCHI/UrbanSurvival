using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;

// 조향 입력 상태
public enum RotateState
{
    Right,
    Left,
    Forward,
}

// 가속 상태
public enum AccelerationState
{
    Stop,
    Forward,
    Backward
}

public class PlayerController : MonoBehaviour
{
    public GameObject[] Arrow;
    public Transform[] GasStationTrans;

    public Text temp_text;

    public GameObject UI_CrossHair;

    ResourceManager rm;

    [Header("자신의 오브젝트들")]

    // 플레이어 카메라
    public Camera PlayerCamera;

    // 터렛 위치
    public GameObject TurretPos;

    // 플레이어 모델
    public GameObject PlayerModel;

    // 플레이어 터렛
    public Turret PlayerTurret;

    // 플레이어 차량
    public Vehicle PlayerVehicle;

    // 3D 크로스헤어
    public Transform CrossHair;

    // 리지드바디
    Rigidbody rigid;

    [Header("UI 에셋")]

    // 체력 슬라이더
    public Slider HealthSlider;

    // 연료 슬라이더
    public Slider FuelSlider;

    public Text HealthText;

    public Text FuelText;

    [Header("스탯 수치들")]

    // 현재 체력
    public float CurrentHealth;

    // 현재 연료
    public float CurrentFuel;


    [Header("가속 관련 수치들")]

    // 터렛이 바라볼 위치
    public Vector3 MouseRayHitPos;

    // 현재 속도
    public float CurrentMoveSpeed = 0;

    [HideInInspector]
    // 가속 상태
    public AccelerationState CurrentAccelState;

    [HideInInspector]
    // 현재 회전 방향
    public RotateState CurrentRotateState;

    PlayerMap PlayerInputMap;

    Vector3 CrossHairAdjustMent = new Vector3(0, 0.5f, 0);

    private void Awake()
    {
        PlayerInputSet();

        rigid = transform.GetComponent<Rigidbody>();

        PlayerCamera = Camera.main;
    }

    void PlayerInputSet()
    {
        PlayerInputMap = new PlayerMap();
        PlayerInputMap.Enable();
    }

    void Start()
    {
        rm = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();

        CurrentRotateState = RotateState.Forward;
        CurrentAccelState = AccelerationState.Stop;

        PlayerVehicle.SetPlayerController(this);

        CurrentHealth = PlayerVehicle.MaxHealth;
        CurrentFuel = PlayerVehicle.MaxFuel;

        UpdateFuelSlider();
        UpdateHealthSlider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "FuelItem")
        {
            CurrentFuel += 50f;
            CurrentFuel = Mathf.Clamp(CurrentFuel,0 , PlayerVehicle.MaxFuel);
            rm.FuelItem.Pool.ReturnPool().Release(other.gameObject);
            other.gameObject.SetActive(false);

            other.gameObject.GetComponent<FuelItem>().paretnStation.GenerateFuelbyDelay();

            Arrow[other.gameObject.GetComponent<FuelItem>().paretnStation.index].SetActive(false);
        }

        else if (other.gameObject.tag == "HealthItem")
        {

        }

        else if (other.gameObject.tag == "Zombie")
        {
            if (Mathf.Abs(CurrentMoveSpeed) > 5)
            {
                other.gameObject.GetComponent<IZombie>().GetDamage(PlayerVehicle.HitDamageToZombie * Mathf.Abs(CurrentMoveSpeed));
            }

            CurrentHealth -= PlayerVehicle.HitDamageToSelf;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, PlayerVehicle.MaxHealth);

            UpdateHealthSlider();

            if (CurrentMoveSpeed < 0)
            {
                CurrentMoveSpeed += PlayerVehicle.HitSpeedDeceaseAmount;
            }
            else if (CurrentMoveSpeed > 0)
            {
                CurrentMoveSpeed -= PlayerVehicle.HitSpeedDeceaseAmount;
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag != "Ground")
        {
            if(other.gameObject.tag == "Obstacle")
            {
                CurrentMoveSpeed -= CurrentMoveSpeed / 10;
            }
        }
    }

    void FixedUpdate()
    {
        if(CurrentHealth != 0 || CurrentFuel != 0)
        {
            UpdateMove();
        }
    }

    void Update()
    {
        if (CurrentHealth > 0 && CurrentFuel > 0)
        {
            ProcessInputs();
            ProcessTargetPos();

            PlayerTurret.ActOnUpdate();
            PlayerVehicle.ActOnUpdate();
        }

        // 임시 
        UpdateArrow();

        HealthText.text = (Mathf.Floor(CurrentHealth * 10f) / 10f).ToString();
        FuelText.text = (Mathf.Floor(CurrentFuel * 10f) / 10f).ToString();
    }

    void UpdateArrow()
    {
        for(int i = 0; i < 6; i ++)
        {
            Arrow[i].transform.LookAt(GasStationTrans[i].position);
            Transform temp = Arrow[i].transform.GetChild(0);
            float disc = Vector3.Distance(GasStationTrans[i].position, transform.position);
            float tempflt = 300 / disc;

            tempflt = Mathf.Clamp(tempflt , 0.1f , 0.8f);

            temp.localScale = new Vector3(tempflt,tempflt,tempflt);
        }
    }

    void UpdateTurretRot()
    {
        Vector3 tgtp = MouseRayHitPos;
        tgtp.y = TurretPos.transform.position.y;

        TurretPos.transform.LookAt(tgtp);
    }

    // 이동 업데이트 함수
    void UpdateMove()
    {
        if(CurrentMoveSpeed != 0)
        {
            if (CurrentMoveSpeed > 0)
            {
                // 조향 
                if (CurrentRotateState == RotateState.Right)
                {
                    PlayerModel.transform.Rotate(new Vector3(0, 1, 0) * PlayerVehicle.RotateSpeed * Mathf.Clamp(Mathf.Abs(2 * (CurrentMoveSpeed / PlayerVehicle.MaxMoveSpeed)), 0, 1) * Time.deltaTime);
                }
                else if (CurrentRotateState == RotateState.Left)
                {
                    PlayerModel.transform.Rotate(new Vector3(0, -1, 0) * PlayerVehicle.RotateSpeed * Mathf.Clamp(Mathf.Abs(2 * (CurrentMoveSpeed / PlayerVehicle.MaxMoveSpeed)), 0, 1) * Time.deltaTime);
                }
            }
            else if (CurrentMoveSpeed < 0)
            {
                // 조향 
                if (CurrentRotateState == RotateState.Right)
                {
                    PlayerModel.transform.Rotate(new Vector3(0, -1, 0) * PlayerVehicle.RotateSpeed * Mathf.Clamp(Mathf.Abs(2 * (CurrentMoveSpeed / PlayerVehicle.MaxMoveSpeed)), 0, 1) * Time.deltaTime);
                }
                else if (CurrentRotateState == RotateState.Left)
                {
                    PlayerModel.transform.Rotate(new Vector3(0, 1, 0) * PlayerVehicle.RotateSpeed * Mathf.Clamp(Mathf.Abs(2 * (CurrentMoveSpeed / PlayerVehicle.MaxMoveSpeed)), 0, 1) * Time.deltaTime);
                }
            }

            // 실질 이동
            rigid.velocity = (PlayerModel.transform.forward * CurrentMoveSpeed);

            // 가속 및 후진 입력 없을 시 자동 감속
            if (CurrentAccelState == AccelerationState.Stop)
            {
                if (CurrentMoveSpeed > 0) 
                {
                    CurrentMoveSpeed -= PlayerVehicle.SpeedDecreaseAmount * Time.deltaTime;
                    if (CurrentMoveSpeed < 0)
                    {
                        CurrentMoveSpeed = 0;
                    }
                }
                else if(CurrentMoveSpeed < 0)
                {
                    CurrentMoveSpeed += PlayerVehicle.SpeedDecreaseAmount * Time.deltaTime;
                    if (CurrentMoveSpeed > 0)
                    {
                        CurrentMoveSpeed = 0;
                    }
                }
                
            }
        }
    }

    // 입력 처리 함수
    void ProcessInputs()
    {
        // 사격
        if(PlayerInputMap.Player.Fire.IsPressed())
        {
            PlayerTurret.FireActual();
        }
        // 사격 해제
        if (PlayerInputMap.Player.Fire.WasReleasedThisFrame())
        {
            PlayerTurret.ActOnMouseUp();
        }
        // 재장전
        if (PlayerInputMap.Player.Reload.IsPressed())
        {
            PlayerTurret.CurrentBulletCount = 0;
            Invoke("ReloadActual", PlayerTurret.ReloadTime);
        }

        if (PlayerInputMap.Player.Move_Hor.IsPressed())
        {
            // 오른쪽
            if(PlayerInputMap.Player.Move_Hor.ReadValue<float>() > 0)
            {
                CurrentRotateState = RotateState.Right;
            }
            //왼쪽
            else
            {
                CurrentRotateState = RotateState.Left;
            }
        }
        // 조향 해제
        if (PlayerInputMap.Player.Move_Hor.WasReleasedThisFrame())
        {
            CurrentRotateState = RotateState.Forward;
        }

        if (PlayerInputMap.Player.Move_ver.IsPressed())
        {
            // 가속
            if (PlayerInputMap.Player.Move_ver.ReadValue<float>() > 0)
            {
                CurrentAccelState = AccelerationState.Forward;

                CurrentFuel -= PlayerVehicle.FuelUsePerAccel * Time.deltaTime;
                UpdateFuelSlider();

                if (CurrentMoveSpeed <0)
                {
                    CurrentMoveSpeed += PlayerVehicle.AccelerationAmount * 9 * Time.deltaTime;
                }
                else
                {
                    CurrentMoveSpeed += PlayerVehicle.AccelerationAmount * Time.deltaTime;
                }
            
                if (CurrentMoveSpeed > PlayerVehicle.MaxMoveSpeed)
                {
                    CurrentMoveSpeed = PlayerVehicle.MaxMoveSpeed;
                }
            }
            // 감속
            else
            {
                PlayerVehicle.RearLampSet(true);

                CurrentAccelState = AccelerationState.Stop;

                if (CurrentMoveSpeed <= 0)
                {
                    CurrentAccelState = AccelerationState.Backward;
                    CurrentMoveSpeed -= PlayerVehicle.AccelerationAmount * Time.deltaTime;

                    if (CurrentMoveSpeed < -PlayerVehicle.MaxMoveSpeed) 
                    {
                        CurrentMoveSpeed = -PlayerVehicle.MaxMoveSpeed;
                    }
                }
                else
                {
                    CurrentMoveSpeed -= PlayerVehicle.BrakeAmount * Time.deltaTime;

                    if (CurrentMoveSpeed < 0)
                    {
                        CurrentMoveSpeed = 0;
                    }
                }
            }
        }
        // 가감속 해제
        if (PlayerInputMap.Player.Move_ver.WasReleasedThisFrame())
        {
            CurrentAccelState = AccelerationState.Stop;
            PlayerVehicle.RearLampSet(false);
        }
    }

    void UpdateFuelSlider()
    {
        FuelSlider.value = CurrentFuel / PlayerVehicle.MaxFuel;
    }

    void UpdateHealthSlider()
    {
        HealthSlider.value = CurrentHealth / PlayerVehicle.MaxHealth;
    }

    Vector2 temp_tgt = new Vector2();

    // 터렛 타겟 위치 처리 함수
    void ProcessTargetPos()
    {
        if (PlayerInputMap.Player.TargetPos_Pad.IsPressed())
        {
            if(PlayerInputMap.Player.TargetPos_Pad.ReadValue<Vector2>().x > 0)
            {

            }
            else
            {

            }

            if (PlayerInputMap.Player.TargetPos_Pad.ReadValue<Vector2>().y > 0)
            {

            }
            else
            {

            }
        }

        else if(PlayerInputMap.Player.TargetPos_Mouse.IsPressed())
        {
            temp_tgt = PlayerInputMap.Player.TargetPos_Mouse.ReadValue<Vector2>();
            UI_CrossHair.transform.position = temp_tgt;

            Vector3 temp_PlayerAim = UI_CrossHair.transform.position;

            Ray ray = PlayerCamera.ScreenPointToRay(temp_PlayerAim);
            int layerMask = 1 << LayerMask.NameToLayer("Ground");
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100000, layerMask))
            {
                MouseRayHitPos = hit.point;
            }

            CrossHair.position = MouseRayHitPos + CrossHairAdjustMent;
            CrossHair.Rotate(new Vector3(0, 0, 100 * Time.deltaTime));


            temp_tgt = PlayerCamera.WorldToScreenPoint(MouseRayHitPos + new Vector3(0, 3, 0));
            UI_CrossHair.transform.position = temp_tgt;

            UpdateTurretRot();

        }
    }

    public void FuelGenerated(int GasStationIndex)
    {
        Arrow[GasStationIndex].SetActive(true);
    }
}
