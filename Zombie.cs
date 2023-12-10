using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

public enum ZombieType
{
    Explode,
    Fast,
    Heavy,
    Normal
}

public enum AttackType
{ 
    Burning,
    Freeze,
    Stiff,
    Electricshock
}

public class Zombie : MonoBehaviour
{
    ResourceManager rm;
    public ZombieSpawner zs;

    Transform Player;

    [Header("상태관련 수치들")]
    public bool isBurning;
    public bool isFreeze;
    public bool isElectricshock;
    public bool isStiff;

    Coroutine burningCor;
    Coroutine freezeCor;
    Coroutine electricshockCor;
    Coroutine stiffCor;

    ParticleSystem Particle_Fire;
    ParticleSystem Particle_Freeze;

    Dictionary<AttackType, Action<bool>> Setstateeffectfuncmap 
        = new Dictionary<AttackType, Action<bool>>();
    Dictionary<AttackType, bool> Setstatefuncmap 
        = new Dictionary<AttackType, bool>();

    [Header("좀비관련 수치들")]
    public ZombieType mType;
    public float MoveSpeed;
    public float MaxHealth;
    public float RotateSpeed;
    public float Weight;
    public float SelfDeathDistance;
    public float SimpleFollowDistance;

    // 파티클 스케일 ( 파티클의 스케일 값에 곱해집니다 )
    public float ParticleScale;

    public float ExplodeRadius;
    public float ExplodeDamage;

    public LayerMask explodeLayer;

    [Header("폭발여부")]
    public bool isExplosion;

    [Header("셀프 컴포넌트")]
    public MeshRenderer[] selfRenderer;

    bool isDead = false;

    NavMeshAgent self_agent;
    Color originColor;
    Color originEmmisionColor;
    float CurrentHealth;

    GameObject temp_object;
    Rigidbody temp_rigid;
    float temp_float1;
    float temp_float2;
    float temp_float3;
    Vector3 temp_vec1;
    Vector3 temp_vec2;

    PoolSet[] SelfZPOPoolSet;
    PoolSet SelfZombiePoolSet;

    GameObject temp_Item;
    Action<bool> temp_action;
    Action dead_func;
    bool temp_bool;

    private void Awake()
    {
        rm = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        Player = GameObject.Find("Player").transform;

        SetSelfPoolSet();

        self_agent = transform.GetComponent<NavMeshAgent>();
        originColor = selfRenderer[0].material.color;
        originEmmisionColor = selfRenderer[0].material.GetColor("_EmissionColor");

        CurrentHealth = MaxHealth;
        self_agent.speed = MoveSpeed;
        self_agent.acceleration = MoveSpeed;
        self_agent.angularSpeed = RotateSpeed;

        Setstateeffectfuncmap.Add(AttackType.Burning, SetBurning);
        Setstateeffectfuncmap.Add(AttackType.Freeze, SetFreeze);
        Setstateeffectfuncmap.Add(AttackType.Electricshock, SetElectricshock);
        Setstateeffectfuncmap.Add(AttackType.Stiff, SetStiff);

        Setstatefuncmap.Add(AttackType.Burning, isBurning);
        Setstatefuncmap.Add(AttackType.Freeze, isFreeze);
        Setstatefuncmap.Add(AttackType.Electricshock, isElectricshock);
        Setstatefuncmap.Add(AttackType.Stiff, isStiff);

        // 폭발형 일 경우
        if (isExplosion)
        {
            dead_func = ExplosionDeath;
        }
        // 아닐 경우
        else
        {
            dead_func = NormalDeath;
        }

        Particle_Fire = transform.GetChild(3).GetComponent<ParticleSystem>();
    }

    public void OnDeployed()
    {
        isDead = false;
        CurrentHealth = MaxHealth;
    }

    // 데미지 적용 함수
    public void GetDamage(float Damage)
    {
        if (!isDead)
        {
            // 체력 감소
            CurrentHealth -= Damage;

            // 히트 파티클 배치
            temp_object = rm.Zp_hit.Object.Get();
            temp_object.transform.position = transform.position;
            temp_object.transform.localScale *= ParticleScale;

            // 히트시 히트 메테리얼 잠시 적용
            MatHit();

            // 죽음 판별
            if (CurrentHealth <= 0)
            {
                isDead = true;
                Death(false);
            }
        }
    }

    // 일반 죽음
    void NormalDeath()
    {
        // 좀비 파편 파티클 배치
        temp_object = rm.Zp_die.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // 좀비 죽음 파티클 배치
        temp_object = rm.Zp_meat.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;
    }

    // 폭발형 좀비 죽음
    void ExplosionDeath()
    {
        // 좀비 폭발 파티클 배치
        temp_object = rm.Zp_explosion.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // 좀비 죽음 파티클 배치
        temp_object = rm.Zp_explosionTear.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // 폭발
        Explode();
    }

    // 사망 함수
    void Death(bool isFarDeath)
    {
        dead_func();

        ReapBodyActual(SelfZPOPoolSet);

        zs.CurrentSpawnAmount--;  

        SelfZombiePoolSet.Pool.ReturnPool().Release(gameObject);

        gameObject.SetActive(false);
    }

    // 사망 시 좀비 파편 오브젝트 배치 함수
    void ReapBodyActual(PoolSet[] input_poolSet)
    {
        for (int i = 0; i < input_poolSet.Length; i++)
        {
            temp_object = input_poolSet[i].Object.Get();
            temp_rigid = temp_object.GetComponent<Rigidbody>();

            temp_vec1 = new Vector3(transform.position.x, transform.position.y / 2, transform.position.z);
            temp_object.transform.position = temp_vec1;
            temp_object.transform.rotation = transform.rotation;

            temp_float1 = UnityEngine.Random.Range(-150, 150);
            temp_float2 = UnityEngine.Random.Range(50, 80);
            temp_float3 = UnityEngine.Random.Range(-150, 150);

            temp_vec1 = new Vector3(temp_float1, -temp_float2, temp_float3);

            temp_rigid.AddForce(temp_vec1);

            temp_float1 = UnityEngine.Random.Range(-40, 40);
            temp_float2 = UnityEngine.Random.Range(-10, 10);
            temp_float3 = UnityEngine.Random.Range(-40, 40);

            temp_vec1 = new Vector3(temp_float1, temp_float2, temp_float3);

            temp_rigid.AddTorque(temp_vec1);
        }
    }

    void SetSelfPoolSet()
    {
        switch (mType)
        {
            case ZombieType.Explode:
                {
                    SelfZPOPoolSet = rm.Zpo_Explode;
                    SelfZombiePoolSet = rm.Zombie_explode;
                    break;
                }
            case ZombieType.Fast:
                {
                    SelfZPOPoolSet = rm.Zpo_Fast;
                    SelfZombiePoolSet = rm.Zombie_fast;
                    break;
                }
            case ZombieType.Heavy:
                {
                    SelfZPOPoolSet = rm.Zpo_Heavy;
                    SelfZombiePoolSet = rm.Zombie_heavy;
                    break;
                }
            case ZombieType.Normal:
                {
                    SelfZPOPoolSet = rm.Zpo_Normal;
                    SelfZombiePoolSet = rm.Zombie_normal;
                    break;
                }
        }
    }

    // 폭발형 좀비 사망 시 폭발 로직
    void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, ExplodeRadius, explodeLayer);

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.tag == "Zombie")
            {
                colliders[i].GetComponent<IZombie>().GetDamage(ExplodeDamage);
            }
        }
    }

    // 피격 시 메테리얼 적용
    void MatHit()
    {
        for(int i = 0; i < selfRenderer.Length;i++)
        {
            selfRenderer[i].material.EnableKeyword("_EMISSION");

            selfRenderer[i].material.color = Color.red;
            selfRenderer[i].material.SetColor("_EmissionColor", Color.red);
            Invoke("MatOri", 0.1f);
        }
    }

    // 피격 후 오리지널 메테리얼 적용
    void MatOri()
    {
        for (int i = 0; i < selfRenderer.Length; i++)
        {
            selfRenderer[i].material.color = originColor;
            selfRenderer[i].material.SetColor("_EmissionColor", originEmmisionColor);
        }
    }

    // 내비게이션 타겟 업데이트
    void UpdateTarget()
    {
        timer += Time.deltaTime;

        if (self_agent.isOnNavMesh && timer >= 0.1f)
        {
            self_agent.SetDestination(Player.position);
            timer = 0;
        }
    }

    // 스테이트 별 상태 적용
    void ManageState()
    {
        if(isBurning)
        {

        }
        if (isFreeze)
        {

        }
        if (isStiff)
        {

        }
        if (isElectricshock)
        {

        }
    }

    // 플레이와의 거리 측정
    void CheckSelfDistanceWithPlayer()
    {
        UpdateTarget();

        // 자살 거리 인지 체크 ( 플레이어와 너무 멀어졌을 경우 )
        if (SelfDeathDistance < Vector3.Distance(transform.position , Player.position))
        {
            zs.FarDeathCount += 1;
            zs.FarDistanceZombies.Enqueue(this.gameObject);
        
            gameObject.SetActive(false);
            self_agent.enabled = false;
        
            zs.CurrentSpawnAmount -= 1;
        }
    }

    float timer = 0;

    IEnumerator StateTimer(AttackType inputType, float WaitTime)
    {
        yield return new WaitForSeconds(WaitTime);

        SetStateEffect(inputType, false);
    }

    // 화염 상태이상 설정
    void SetBurning(bool isOn)
    {
        // 활성화 시
        if(isOn) 
        {
            // 빙결 상태 일 경우,
            if (isFreeze)
            {
                // 빙결 상태 비활성화
                SetStateEffect(AttackType.Freeze, false);
                // 빙결 코루틴 중지
                StopCoroutine(freezeCor);
            }

            // 이미 화염 활성화 시
            else if (isBurning)
            {
                // 기존 화염 코루틴 중지
                StopCoroutine(burningCor);
            }

            // 화염 코루틴 시작
            StartCoroutine(StateTimer(AttackType.Burning,rm.time_zombieBurning));
            // 파티클 활성화
            Particle_Fire.gameObject.SetActive(true);
        }

        // 비활성화 시
        else
        {
            // 화염 상태이상 활성화 시
            if(isBurning)
            {
                // 파티클 비활성화
                Particle_Fire.gameObject.SetActive(false);
            }

            else
            {
                return;
            }
        }

        isBurning = isOn;
    }

    // 빙결 상태 이상 설정
    void SetFreeze(bool isOn)
    {
        // 활성화 일 경우
        if (isOn)
        {
            // 이미 화염 상태 이상 일 경우
            if (isBurning)
            {
                // 화염 상태 이상 종료
                SetStateEffect(AttackType.Burning, false);
                // 화염 상태 이상 코루틴 종료
                StopCoroutine(burningCor);
            }

            // 이미 빙결 상태 일 경우
            else if (isFreeze)
            {
                // 기존 빙결 상태 이상 코루틴 종료
                StopCoroutine(freezeCor);
            }

            // 빙결 코루틴 시작
            freezeCor = StartCoroutine(StateTimer(AttackType.Freeze, rm.time_zombieFreeze));
            // 빙결 파티클 활성화
            Particle_Freeze.gameObject.SetActive(true);
        }

        // 비활성화 일 경우
        else
        {
            // 빙결이 활성화 되어 있을 경우
            if (isFreeze)
            {
                // 빙결 파티클 비활성화
                Particle_Freeze.gameObject.SetActive(false);
            }

            else
            {
                return;
            }
        }

        isFreeze = isOn;
    }

    void SetStiff(bool isOn)
    {
        // 활성화 일 경우
        if (isOn)
        {
            // 이미 경직이 활성화 상태 일때
            if(isStiff)
            {
                // 기존 경직 코루틴 중지
                StopCoroutine(stiffCor);
            }
            else
            {
                // 속도 감소

            }
        }

        // 비활성화 일 경우
        else
        {
            // 경직이 활성화 상태 일때
            if (isStiff)
            {

            }

            else
            {
                return;
            }
        }

        isStiff = isOn;
    }

    void SetElectricshock(bool isOn)
    {
        if (isOn)
        {
            
        }

        else
        {
            if (isElectricshock)
            {

            }

            else
            {
                return;
            }
        }

        isElectricshock = isOn;
    }

    // 상태 입력 후 상태 효과 함수 호출
    void SetStateEffect(AttackType inputType, bool isOn)
    {
        Setstateeffectfuncmap.TryGetValue(inputType, out temp_action);
        temp_action(isOn);
    }

    // 상태 입력 후 상태 bool 값 변환 함수
    void SetStateBool(AttackType inputType , bool isOn)
    {
        Setstatefuncmap.TryGetValue(inputType, out temp_bool);
        temp_bool = isOn;
    }
    
    void FixedUpdate()
    {
        CheckSelfDistanceWithPlayer();
        ManageState();
    }
}
