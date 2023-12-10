using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    [Header("설정")]
    // 총열 과열 여부
    public bool IsBarrelHeatable;

    [Header("총기 관련 수치")]
    // 총기 적 관통 수
    public int PenetrationAmount;
    // 총알 크기 배율
    public float BulletSize;
    // 총알 속도
    public float BulletSpeed;
    // 총알 데미지
    public float BulletDamage;
    // 총기 발사 딜레이
    public float ShotDelayAmount;
    // 총기 탄퍼짐 정도
    public float ShotSpreadAmount;
    // 발사시 총알갯수
    public int BulletPerFire = 1;

    // 최대 장탄수
    public int BulletMaxCount;
    // 현재 장탄수
    public int CurrentBulletCount;
    // 재장전 시간
    public float ReloadTime;

    [Header("자체적인 오브젝트들")]
    // 터렛 위치 트랜스폼
    public Transform TurretPos;
    // 터렛 실질 오브젝트
    public GameObject TurretObj;
    // 터렛 총열 오브젝트
    public GameObject TurretBarrelObj;
    // 터렛 총열 메테리얼
    public Material TurretBarrelMaterial;
    // 탄피 사출구 위치 트랜스폼
    public Transform BulletCasingExtractPos;
    // 총기 격발음
    public AudioSource GunFireSound;

    [Header("총구화염 관련 오브젝트들")]
    // 총구 위치 트랜스폼
    public Transform MuzzlePos;
    // 총구 화염 오브젝트
    public GameObject MuzzleFlashObj;
    // 총구 화염 광원
    public Light MuzzleFlashLight;
    // 총구 화염 스프라이트 오브젝트
    public GameObject MuzzleFlashSpriteObj;

    // 격발시 총구 화염 파티클들
    public ParticleSystem[] MuzzleFlashPatricle_atFire;
    // 격발이후 총구 화염 파티클들
    public ParticleSystem[] MuzzleFlashPatricle_afterFire;

    [HideInInspector]
    // 격발 가능 여부
    public bool iscanFire;

    // 리소스 매니저
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

        // 터렛 배럴 메테리얼 초기화
        TurretBarrelMaterial = TurretBarrelObj.GetComponent<MeshRenderer>().material;
        TurretBarrelMaterial.EnableKeyword("_EMISSION");

        iscanFire = true;
    }

    // 업데이트
    virtual public void ActOnUpdate()
    {
        DecreaseBarrelMat();
    }

    // 마우스 클릭 해제 시
    virtual public void ActOnMouseUp()
    {
        InstAfterFireParticle();
    }

    // 실제 격발 로직
    public void FireActual()
    {
        if(iscanFire && CurrentBulletCount != 0)
        {
            iscanFire = false;

            // 1회 발사 분량 만큼 발사
            for (int i = 0; i < BulletPerFire; i++)
            {
                // 탄환 생성
                InstBullet();
            }

            // 총구화염 생성
            InstMuzzleFlash();
            // 탄피 생성
            InstBulletCasing();
            // 총열 과열 메테리얼 적용
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

    // 격발 이후 작동 파티클 동작
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

    // 탄피 생성
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

    // 탄환 생성
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

    // 총구 화염 생성
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