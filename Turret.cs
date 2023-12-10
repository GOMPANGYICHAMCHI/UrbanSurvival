using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    [Header("����")]
    // �ѿ� ���� ����
    public bool IsBarrelHeatable;

    [Header("�ѱ� ���� ��ġ")]
    // �ѱ� �� ���� ��
    public int PenetrationAmount;
    // �Ѿ� ũ�� ����
    public float BulletSize;
    // �Ѿ� �ӵ�
    public float BulletSpeed;
    // �Ѿ� ������
    public float BulletDamage;
    // �ѱ� �߻� ������
    public float ShotDelayAmount;
    // �ѱ� ź���� ����
    public float ShotSpreadAmount;
    // �߻�� �Ѿ˰���
    public int BulletPerFire = 1;

    // �ִ� ��ź��
    public int BulletMaxCount;
    // ���� ��ź��
    public int CurrentBulletCount;
    // ������ �ð�
    public float ReloadTime;

    [Header("��ü���� ������Ʈ��")]
    // �ͷ� ��ġ Ʈ������
    public Transform TurretPos;
    // �ͷ� ���� ������Ʈ
    public GameObject TurretObj;
    // �ͷ� �ѿ� ������Ʈ
    public GameObject TurretBarrelObj;
    // �ͷ� �ѿ� ���׸���
    public Material TurretBarrelMaterial;
    // ź�� ���ⱸ ��ġ Ʈ������
    public Transform BulletCasingExtractPos;
    // �ѱ� �ݹ���
    public AudioSource GunFireSound;

    [Header("�ѱ�ȭ�� ���� ������Ʈ��")]
    // �ѱ� ��ġ Ʈ������
    public Transform MuzzlePos;
    // �ѱ� ȭ�� ������Ʈ
    public GameObject MuzzleFlashObj;
    // �ѱ� ȭ�� ����
    public Light MuzzleFlashLight;
    // �ѱ� ȭ�� ��������Ʈ ������Ʈ
    public GameObject MuzzleFlashSpriteObj;

    // �ݹ߽� �ѱ� ȭ�� ��ƼŬ��
    public ParticleSystem[] MuzzleFlashPatricle_atFire;
    // �ݹ����� �ѱ� ȭ�� ��ƼŬ��
    public ParticleSystem[] MuzzleFlashPatricle_afterFire;

    [HideInInspector]
    // �ݹ� ���� ����
    public bool iscanFire;

    // ���ҽ� �Ŵ���
    ResourceManager rm;

    GameObject temp_object;
    Bullet temp_bullet;

    float tmp_flt1;
    float tmp_flt2;

    Vector3 tmp_vec;

    void Awake()
    {
        rm = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
        GunFireSound = GetComponent<AudioSource>();

        // �ͷ� �跲 ���׸��� �ʱ�ȭ
        TurretBarrelMaterial = TurretBarrelObj.GetComponent<MeshRenderer>().material;
        TurretBarrelMaterial.EnableKeyword("_EMISSION");

        iscanFire = true;
    }

    // ������Ʈ
    virtual public void ActOnUpdate()
    {
        DecreaseBarrelMat();
    }

    // ���콺 Ŭ�� ���� ��
    virtual public void ActOnMouseUp()
    {
        InstAfterFireParticle();
    }

    // ���� �ݹ� ����
    public void FireActual()
    {
        if(iscanFire && CurrentBulletCount != 0)
        {
            iscanFire = false;

            // 1ȸ �߻� �з� ��ŭ �߻�
            for (int i = 0; i < BulletPerFire; i++)
            {
                // źȯ ����
                InstBullet();
            }

            // �ѱ�ȭ�� ����
            InstMuzzleFlash();
            // ź�� ����
            InstBulletCasing();
            // �ѿ� ���� ���׸��� ����
            IncreaseBarrelMat();

            StartCoroutine(NowCanFire(ShotDelayAmount));
        }
        else if(CurrentBulletCount == 0)
        {
            StartCoroutine(ReloadActual(ReloadTime));
        }
    }

    IEnumerator ReloadActual(float sec)
    {
        yield return new WaitForSeconds(sec);
        CurrentBulletCount = BulletMaxCount;
    }

    // �ݹ� ���� �۵� ��ƼŬ ����
    public void InstAfterFireParticle()
    {
        for (int i = 0; i < MuzzleFlashPatricle_afterFire.Length; i++)
        {
            MuzzleFlashPatricle_afterFire[i].Play();
        }
    }

    void DecreaseBarrelMat()
    {
        Color temp_color = TurretBarrelMaterial.GetColor("_EmissionColor");

        temp_color.r -= 0.05f * Time.deltaTime;

        if (temp_color.r < 0)
        {
            temp_color.r = 0;
        }

        TurretBarrelMaterial.SetColor("_EmissionColor", temp_color);
    }

    void IncreaseBarrelMat()
    {
        Color temp_color = TurretBarrelMaterial.GetColor("_EmissionColor");

        temp_color.r += 0.005f;

        if (temp_color.r > 0.3f)
        {
            temp_color.r = 0.3f;
        }

        TurretBarrelMaterial.SetColor("_EmissionColor", temp_color);
    }

    // ź�� ����
    virtual public void InstBulletCasing()
    {
        tmp_flt1 = UnityEngine.Random.Range(10, 12);
        tmp_flt2 = UnityEngine.Random.Range(150, 200);
        
        temp_object = rm.P_BulletCasing.Object.Get();
        temp_object.transform.position = BulletCasingExtractPos.position;
        temp_object.transform.rotation = temp_object.transform.rotation;
        temp_object.GetComponent<Rigidbody>().AddForce(-BulletCasingExtractPos.transform.forward * tmp_flt1);
        temp_object.GetComponent<Rigidbody>().AddTorque(0, tmp_flt2, 0);

        temp_object = rm.P_BulletClip.Object.Get();
        temp_object.transform.position = BulletCasingExtractPos.position;
        temp_object.transform.rotation = temp_object.transform.rotation;
        temp_object.GetComponent<Rigidbody>().AddForce(-BulletCasingExtractPos.transform.forward * tmp_flt1/4);
        temp_object.GetComponent<Rigidbody>().AddTorque(0, tmp_flt2/7, 0);
    }

    // źȯ ����
    virtual public void InstBullet()
    {
        GunFireSound.Play();

        tmp_flt1 = UnityEngine.Random.Range(-ShotSpreadAmount, ShotSpreadAmount);

        tmp_vec.x = 0;
        tmp_vec.y = tmp_flt1;
        tmp_vec.z = 0;

        temp_object = rm.P_Bullet.Object.Get();
        temp_bullet = temp_object.GetComponent<Bullet>();
        temp_bullet.DisableTrailRenderer();

        temp_object.transform.position = MuzzlePos.position;
        temp_object.transform.rotation = transform.rotation;
        
        //temp_bullet.rHeight = transform.position.y;
        temp_bullet.SetAtStart(BulletDamage, BulletSpeed, PenetrationAmount, BulletSize);
        temp_object.transform.Rotate(tmp_vec);

        temp_bullet.EnableTrailRenderer();
    }

    // �ѱ� ȭ�� ����
    virtual public void InstMuzzleFlash()
    {
        for(int i = 0; i < MuzzleFlashPatricle_atFire.Length;i++)
        {
            MuzzleFlashPatricle_atFire[i].Play();
        }

        MuzzleFlashObj.SetActive(true);
        MuzzleFlashLight.intensity = UnityEngine.Random.Range(1, 15);

        tmp_flt1 = UnityEngine.Random.Range(0.1f, 1);
        tmp_flt2 = UnityEngine.Random.Range(0, 360);

        MuzzleFlashSpriteObj.transform.localScale = new Vector3(tmp_flt1, 1, tmp_flt1 - 0.1f);
        MuzzleFlashSpriteObj.transform.localRotation = Quaternion.Euler(tmp_flt2, 90, -90);

        StartCoroutine(MuzzleFlashSpriteOff(0.05f));
    }

    IEnumerator MuzzleFlashSpriteOff(float sec)
    {
        yield return new WaitForSeconds(sec);   
        MuzzleFlashObj.SetActive(false);
    }

    IEnumerator NowCanFire(float sec)
    {
        yield return new WaitForSeconds(sec);
        iscanFire = true;
    }
}