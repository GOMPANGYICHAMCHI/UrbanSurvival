using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public enum PoolObjectType
{
    ParticleSystem,
    GameObject,
    PlayerBullet,
    Zombie
}

public class PoolAct
{
    ResourceManager rm;

    private GameObject OriginObject;
    public ObjectPool<GameObject> Object;

    PoolObjectType objectType;
    float DurTime;

    public ObjectPool<GameObject> ReturnPool()
    {
        return Object;
    }

    public void InitializeOnStart
        (GameObject input_object, PoolObjectType input_type, ResourceManager input_rm, int input_amount, float input_durTime)
    {
        rm = input_rm;
        OriginObject = input_object;
        objectType = input_type;
        DurTime = input_durTime;

        Object = new ObjectPool<GameObject>
        (
            CreatePoolObject,
            OnTakeFromPool,
            OnReturnToPool,
            OnDestroyObject, false,
            input_amount, 300
        );
    }

    GameObject CreatePoolObject()
    {
        GameObject instance = rm.InitObject(OriginObject);
        OnReturnToPool(instance);
        instance.gameObject.SetActive(false);
        instance.transform.position = rm.transform.position;

        return instance;
    }

    void OnTakeFromPool(GameObject input_return)
    {
        if(objectType != PoolObjectType.Zombie)
        {
            input_return.gameObject.SetActive(true);
        }

        if (objectType == PoolObjectType.ParticleSystem)
        {
            input_return.GetComponent<ParticleSystem>().Play();
        }

        rm.AutoReturn(input_return, Object, DurTime , objectType);
    }

    void OnReturnToPool(GameObject input_return)
    {
        input_return.gameObject.SetActive(false);
        input_return.gameObject.transform.localScale = OriginObject.transform.localScale;  
    }

    void OnDestroyObject(GameObject input_return)
    {
        rm.ObjectDestroy(input_return.gameObject);
    }
}

[Serializable]
public struct PoolSet
{
    public PoolAct Pool;

    //[Header("�������� ������Ʈ")]
    [SerializeField] GameObject OriginObject;
    //[Header("������Ʈ Ǯ�� �⺻ ����")]
    [SerializeField] int Amount;
    //[Header("������Ʈ ���� �ð�")]
    [SerializeField] float DurTime;
    //[Header("������Ʈ ��ƼŬ ����")]
    [SerializeField] PoolObjectType objectType;

    // ������Ʈ Ǯ
    public ObjectPool<GameObject> Object;

    public void InitOnAwake(ResourceManager input_rm)
    {
        Pool = new PoolAct();
        Pool.InitializeOnStart(OriginObject, objectType, input_rm, Amount, DurTime);

        Object = Pool.ReturnPool();
    }
}

public class ResourceManager : MonoBehaviour
{
    [Header("����")]
    public PoolSet Zombie_normal;
    public PoolSet Zombie_explode;
    public PoolSet Zombie_heavy;
    public PoolSet Zombie_fast;

    [Header("�Ϲ� ���� ���� ��ƼŬ")]
    public PoolSet Zp_die;
    public PoolSet Zp_hit;
    public PoolSet Zp_meat;

    [Header("���� �����̻� ���� ��ƼŬ")]
    public PoolSet Zp_Fire;
    public PoolSet Zp_IceHit;

    [Header("������ ���� ���� ��ƼŬ")]
    public PoolSet Zp_explosion;
    public PoolSet Zp_explosionTear;

    [Header("�÷��̾� źȯ")]
    public PoolSet P_Bullet;
    public PoolSet P_BulletCasing;
    public PoolSet P_BulletClip;

    [Header("���� ��ü ����")]
    public PoolSet[] Zpo_Explode;
    public PoolSet[] Zpo_Fast;
    public PoolSet[] Zpo_Heavy;
    public PoolSet[] Zpo_Normal;

    [Header("������")]
    public PoolSet FuelItem;
    public PoolSet HealthItem;

    [Header("���� ���� �̻� �ð���")]
    public float time_zombieBurning;
    public float time_zombieFreeze;
    public float time_zombieElectricshock;
    public float time_zombieStiff;

    void Awake()
    {
        Zombie_normal.InitOnAwake(this);
        Zombie_explode.InitOnAwake(this);
        Zombie_heavy.InitOnAwake(this);
        Zombie_fast.InitOnAwake(this);

        Zp_die.InitOnAwake(this);
        Zp_hit.InitOnAwake(this);
        Zp_meat.InitOnAwake(this);

        Zp_explosion.InitOnAwake(this);
        Zp_explosionTear.InitOnAwake(this);

        P_Bullet.InitOnAwake(this);
        P_BulletCasing.InitOnAwake(this);
        P_BulletClip.InitOnAwake(this);

        FuelItem.InitOnAwake(this);
        HealthItem.InitOnAwake(this);

        for (int i = 0; i < Zpo_Explode.Length; i++) 
        {
            Zpo_Explode[i].InitOnAwake(this);
            Zpo_Fast[i].InitOnAwake(this);
            Zpo_Heavy[i].InitOnAwake(this);
            Zpo_Normal[i].InitOnAwake(this);
        }
    }

    public void ObjectDestroy(GameObject temp_objct)
    {
        Destroy(temp_objct);
    }

    public GameObject InitObject(GameObject temp_object)
    {
        return Instantiate(temp_object, Vector3.zero, Quaternion.identity);
    }

    public void AutoReturn
        (GameObject temp_object, ObjectPool<GameObject> TargetPool, float DurTime, PoolObjectType input_type)
    {
        if(DurTime != -1)
        {
            StartCoroutine(ReturnActual(temp_object, TargetPool, DurTime, input_type));
        }
    }

    void ReturnActualInvoke
        (GameObject temp_object, ObjectPool<GameObject> TargetPool, float DurTime, PoolObjectType input_type)
    {
        TargetPool.Release(temp_object);
    }

    IEnumerator ReturnActual
        (GameObject temp_object, ObjectPool<GameObject> TargetPool, float DurTime, PoolObjectType input_type)
    {
        yield return new WaitForSeconds(DurTime);

        TargetPool.Release(temp_object);
    }
}
