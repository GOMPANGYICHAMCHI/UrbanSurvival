using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct ZombieSpawnData
{
    // 타입 
    public ZombieType type;
    // 스폰 확률
    public float SpawnPercent;
}

public class ZombieSpawner : MonoBehaviour
{
    // 리소스 매니저
    ResourceManager rm;

    // 좀비 스폰 확률
    [SerializeField]
    public List<ZombieSpawnData> ZombieSpawnPersentData;

    // 원거리 좀비
    public Queue<GameObject> FarDistanceZombies = new Queue<GameObject>();

    // 플레이어와 멀어서 비활성화 된 좀비 수
    public int FarDeathCount = 0;

    // 스포너 트랜스폼
    public List<Transform> Spawners;

    // 시간당 생성 좀비 수
    public int SpawnAmountPerTime;

    // 현재 생성된 좀비 수
    public int CurrentSpawnAmount = 0;

    // 최대 좀비 수
    public int MaxZombieCount = 100;

    // 스폰 위치에서의 위치 선정 랜덤 정도
    public int RandomSpawnRadius;
    
    // 좀비 스폰 시간 간격
    public float SpawnTime = 1f;

    // 좀비 스폰 체크 시간
    float TimeCheck = 0;

    int temp_amount;
    float temp_randnum;
    GameObject temp_zombie;
    float allPercent = 0;
    float temp_posX;
    float temp_posZ;
    Vector3 temp_Pos;

    // 전체 확률 합산
    void GetAllPercent()
    {
        for (int i = 0; i < ZombieSpawnPersentData.Count; i++)
        {
            allPercent += ZombieSpawnPersentData[i].SpawnPercent;
        }
    }

    void Start()
    {
        // 리소스 매니저 할당
        rm = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();

        GetAllPercent();
    }

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.GetComponent<MeshRenderer>().enabled = true;
        Spawners.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<MeshRenderer>().enabled = false;
        Spawners.Remove(other.transform);
    }

    // 확률 반환 함수
    int CheckPercent()
    {
        float temp_addpercent = 0;

        for(int i = 0; i < ZombieSpawnPersentData.Count; i++)
        {
            temp_addpercent += ZombieSpawnPersentData[i].SpawnPercent;

            if(temp_randnum <= temp_addpercent)
            {
                return i;
            }
        }

        return 0;
    }

    // 현재 생성 되는 좀비 설정
    void SetCurrentZombie()
    {
        // 현재 좀비 인덱스 확률 반환
        int CurrentIndex = CheckPercent();

        switch(CurrentIndex)
        {
            case 0:
                temp_zombie = rm.Zombie_explode.Object.Get();
                break;

            case 1:
                temp_zombie = rm.Zombie_fast.Object.Get();
                break;

            case 2:
                temp_zombie = rm.Zombie_heavy.Object.Get();
                break;

            case 3:
                temp_zombie = rm.Zombie_normal.Object.Get();
                break;
        }
    }

    // 좀비 생성
    void SpawnZombieActual()
    {
        if(Spawners.Count == 0)
        {
            return;
        }

        int amountactual;

        int focuspoint_amount = SpawnAmountPerTime / 3;
        
        temp_amount = focuspoint_amount * 2 / Spawners.Count;

        int currentfocuspoint_index = 0;
        float Mostsightcorrect = 0;

        for (int i = 0; i < Spawners.Count; i++)
        {   
            // 첫번째 인덱스 일때
            if(i == 0)
            {
                // 가장 일치된 시선 정도 설정
                Mostsightcorrect = Vector3.Dot(transform.forward, Spawners[i].transform.position);
                currentfocuspoint_index = i;
            }

            // 현재 플레이어와의 시선 일치 정도
            float temp_sight = Vector3.Dot(transform.forward, Spawners[i].transform.position);

            // 현재 시선 일치 정도가 가장 일치할때
            if (temp_sight > Mostsightcorrect)
            {
                // 가장 일치된 시선 정도 설정
                Mostsightcorrect = temp_sight;
                currentfocuspoint_index = i;
            }
        }

        for (int i = 0; i < Spawners.Count; i++)
        {
            if (i == currentfocuspoint_index) 
            {
                amountactual = focuspoint_amount;

                amountactual += FarDeathCount / 3;
                FarDeathCount -= FarDeathCount / 3;
            }
            else
            {
                amountactual = temp_amount;

                amountactual += FarDeathCount / 3 * 2 / Spawners.Count;
                FarDeathCount -= FarDeathCount / 3 * 2 / Spawners.Count;
            }

            for (int a = 0; a < amountactual; a++)
            {
                // NULL 체크
                if (Spawners[i] == null)
                    break;

                temp_randnum = UnityEngine.Random.Range(0, allPercent);

                if(FarDistanceZombies.Count != 0)
                {
                    temp_zombie = FarDistanceZombies.Dequeue();
                }
                else
                {
                    SetCurrentZombie();
                }

                // 위치 랜덤 설정
                temp_posX = UnityEngine.Random.Range(-RandomSpawnRadius, RandomSpawnRadius);
                temp_posZ = UnityEngine.Random.Range(-RandomSpawnRadius, RandomSpawnRadius);

                // 스포너로 부터 랜덤 위치 더하기
                temp_Pos = Spawners[i].transform.position;

                temp_Pos.x += temp_posX;
                temp_Pos.z += temp_posZ;

                temp_zombie.transform.position = temp_Pos;
                temp_zombie.SetActive(true);
                temp_zombie.GetComponent<NavMeshAgent>().enabled = true;
                temp_zombie.GetComponent<IZombie>().zs = this;
                temp_zombie.GetComponent<IZombie>().OnDeployed();
                CurrentSpawnAmount++;
            }
        }
    }

    void Update()
    {
        if(CurrentSpawnAmount < MaxZombieCount)
        {
            TimeCheck += Time.deltaTime;
        }

        if(TimeCheck >= SpawnTime)
        {
            TimeCheck = 0;
            SpawnZombieActual();
        }
    }
}
