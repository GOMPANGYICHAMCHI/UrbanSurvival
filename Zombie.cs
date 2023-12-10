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

    [Header("���°��� ��ġ��")]
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

    [Header("������� ��ġ��")]
    public ZombieType mType;
    public float MoveSpeed;
    public float MaxHealth;
    public float RotateSpeed;
    public float Weight;
    public float SelfDeathDistance;
    public float SimpleFollowDistance;

    // ��ƼŬ ������ ( ��ƼŬ�� ������ ���� �������ϴ� )
    public float ParticleScale;

    public float ExplodeRadius;
    public float ExplodeDamage;

    public LayerMask explodeLayer;

    [Header("���߿���")]
    public bool isExplosion;

    [Header("���� ������Ʈ")]
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

        // ������ �� ���
        if (isExplosion)
        {
            dead_func = ExplosionDeath;
        }
        // �ƴ� ���
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

    // ������ ���� �Լ�
    public void GetDamage(float Damage)
    {
        if (!isDead)
        {
            // ü�� ����
            CurrentHealth -= Damage;

            // ��Ʈ ��ƼŬ ��ġ
            temp_object = rm.Zp_hit.Object.Get();
            temp_object.transform.position = transform.position;
            temp_object.transform.localScale *= ParticleScale;

            // ��Ʈ�� ��Ʈ ���׸��� ��� ����
            MatHit();

            // ���� �Ǻ�
            if (CurrentHealth <= 0)
            {
                isDead = true;
                Death(false);
            }
        }
    }

    // �Ϲ� ����
    void NormalDeath()
    {
        // ���� ���� ��ƼŬ ��ġ
        temp_object = rm.Zp_die.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // ���� ���� ��ƼŬ ��ġ
        temp_object = rm.Zp_meat.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;
    }

    // ������ ���� ����
    void ExplosionDeath()
    {
        // ���� ���� ��ƼŬ ��ġ
        temp_object = rm.Zp_explosion.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // ���� ���� ��ƼŬ ��ġ
        temp_object = rm.Zp_explosionTear.Object.Get();
        temp_object.transform.position = transform.position;
        temp_object.transform.localScale *= ParticleScale;

        // ����
        Explode();
    }

    // ��� �Լ�
    void Death(bool isFarDeath)
    {
        dead_func();

        ReapBodyActual(SelfZPOPoolSet);

        zs.CurrentSpawnAmount--;  

        SelfZombiePoolSet.Pool.ReturnPool().Release(gameObject);

        gameObject.SetActive(false);
    }

    // ��� �� ���� ���� ������Ʈ ��ġ �Լ�
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

    // ������ ���� ��� �� ���� ����
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

    // �ǰ� �� ���׸��� ����
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

    // �ǰ� �� �������� ���׸��� ����
    void MatOri()
    {
        for (int i = 0; i < selfRenderer.Length; i++)
        {
            selfRenderer[i].material.color = originColor;
            selfRenderer[i].material.SetColor("_EmissionColor", originEmmisionColor);
        }
    }

    // ������̼� Ÿ�� ������Ʈ
    void UpdateTarget()
    {
        timer += Time.deltaTime;

        if (self_agent.isOnNavMesh && timer >= 0.1f)
        {
            self_agent.SetDestination(Player.position);
            timer = 0;
        }
    }

    // ������Ʈ �� ���� ����
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

    // �÷��̿��� �Ÿ� ����
    void CheckSelfDistanceWithPlayer()
    {
        UpdateTarget();

        // �ڻ� �Ÿ� ���� üũ ( �÷��̾�� �ʹ� �־����� ��� )
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

    // ȭ�� �����̻� ����
    void SetBurning(bool isOn)
    {
        // Ȱ��ȭ ��
        if(isOn) 
        {
            // ���� ���� �� ���,
            if (isFreeze)
            {
                // ���� ���� ��Ȱ��ȭ
                SetStateEffect(AttackType.Freeze, false);
                // ���� �ڷ�ƾ ����
                StopCoroutine(freezeCor);
            }

            // �̹� ȭ�� Ȱ��ȭ ��
            else if (isBurning)
            {
                // ���� ȭ�� �ڷ�ƾ ����
                StopCoroutine(burningCor);
            }

            // ȭ�� �ڷ�ƾ ����
            StartCoroutine(StateTimer(AttackType.Burning,rm.time_zombieBurning));
            // ��ƼŬ Ȱ��ȭ
            Particle_Fire.gameObject.SetActive(true);
        }

        // ��Ȱ��ȭ ��
        else
        {
            // ȭ�� �����̻� Ȱ��ȭ ��
            if(isBurning)
            {
                // ��ƼŬ ��Ȱ��ȭ
                Particle_Fire.gameObject.SetActive(false);
            }

            else
            {
                return;
            }
        }

        isBurning = isOn;
    }

    // ���� ���� �̻� ����
    void SetFreeze(bool isOn)
    {
        // Ȱ��ȭ �� ���
        if (isOn)
        {
            // �̹� ȭ�� ���� �̻� �� ���
            if (isBurning)
            {
                // ȭ�� ���� �̻� ����
                SetStateEffect(AttackType.Burning, false);
                // ȭ�� ���� �̻� �ڷ�ƾ ����
                StopCoroutine(burningCor);
            }

            // �̹� ���� ���� �� ���
            else if (isFreeze)
            {
                // ���� ���� ���� �̻� �ڷ�ƾ ����
                StopCoroutine(freezeCor);
            }

            // ���� �ڷ�ƾ ����
            freezeCor = StartCoroutine(StateTimer(AttackType.Freeze, rm.time_zombieFreeze));
            // ���� ��ƼŬ Ȱ��ȭ
            Particle_Freeze.gameObject.SetActive(true);
        }

        // ��Ȱ��ȭ �� ���
        else
        {
            // ������ Ȱ��ȭ �Ǿ� ���� ���
            if (isFreeze)
            {
                // ���� ��ƼŬ ��Ȱ��ȭ
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
        // Ȱ��ȭ �� ���
        if (isOn)
        {
            // �̹� ������ Ȱ��ȭ ���� �϶�
            if(isStiff)
            {
                // ���� ���� �ڷ�ƾ ����
                StopCoroutine(stiffCor);
            }
            else
            {
                // �ӵ� ����

            }
        }

        // ��Ȱ��ȭ �� ���
        else
        {
            // ������ Ȱ��ȭ ���� �϶�
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

    // ���� �Է� �� ���� ȿ�� �Լ� ȣ��
    void SetStateEffect(AttackType inputType, bool isOn)
    {
        Setstateeffectfuncmap.TryGetValue(inputType, out temp_action);
        temp_action(isOn);
    }

    // ���� �Է� �� ���� bool �� ��ȯ �Լ�
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
