using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct ZombieSpawnData
{
    // Ÿ�� 
    public ZombieType type;
    // ���� Ȯ��
    public float SpawnPercent;
}

public class ZombieSpawner : MonoBehaviour
{
    // ���ҽ� �Ŵ���
    ResourceManager rm;

    // ���� ���� Ȯ��
    [SerializeField]
    public List<ZombieSpawnData> ZombieSpawnPersentData;

    // ���Ÿ� ����
    public Queue<GameObject> FarDistanceZombies = new Queue<GameObject>();

    // �÷��̾�� �־ ��Ȱ��ȭ �� ���� ��
    public int FarDeathCount = 0;

    // ������ Ʈ������
    public List<Transform> Spawners;

    // �ð��� ���� ���� ��
    public int SpawnAmountPerTime;

    // ���� ������ ���� ��
    public int CurrentSpawnAmount = 0;

    // �ִ� ���� ��
    public int MaxZombieCount = 100;

    // ���� ��ġ������ ��ġ ���� ���� ����
    public int RandomSpawnRadius;
    
    // ���� ���� �ð� ����
    public float SpawnTime = 1f;

    // ���� ���� üũ �ð�
    float TimeCheck = 0;

    int temp_amount;
    float temp_randnum;
    GameObject temp_zombie;
    float allPercent = 0;
    float temp_posX;
    float temp_posZ;
    Vector3 temp_Pos;

    // ��ü Ȯ�� �ջ�
    void GetAllPercent()
    {
        for (int i = 0; i < ZombieSpawnPersentData.Count; i++)
        {
            allPercent += ZombieSpawnPersentData[i].SpawnPercent;
        }
    }

    void Start()
    {
        // ���ҽ� �Ŵ��� �Ҵ�
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

    // Ȯ�� ��ȯ �Լ�
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

    // ���� ���� �Ǵ� ���� ����
    void SetCurrentZombie()
    {
        // ���� ���� �ε��� Ȯ�� ��ȯ
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

    // ���� ����
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
            // ù��° �ε��� �϶�
            if(i == 0)
            {
                // ���� ��ġ�� �ü� ���� ����
                Mostsightcorrect = Vector3.Dot(transform.forward, Spawners[i].transform.position);
                currentfocuspoint_index = i;
            }

            // ���� �÷��̾���� �ü� ��ġ ����
            float temp_sight = Vector3.Dot(transform.forward, Spawners[i].transform.position);

            // ���� �ü� ��ġ ������ ���� ��ġ�Ҷ�
            if (temp_sight > Mostsightcorrect)
            {
                // ���� ��ġ�� �ü� ���� ����
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
                // NULL üũ
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

                // ��ġ ���� ����
                temp_posX = UnityEngine.Random.Range(-RandomSpawnRadius, RandomSpawnRadius);
                temp_posZ = UnityEngine.Random.Range(-RandomSpawnRadius, RandomSpawnRadius);

                // �����ʷ� ���� ���� ��ġ ���ϱ�
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
