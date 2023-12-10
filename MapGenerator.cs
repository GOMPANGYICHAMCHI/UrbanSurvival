using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform Road;

    public GameObject[] BuildingSet;

    public List<Transform> BuildingPos;

    void GatherMapData()
    {
        float chilcount = Road.childCount;
        Transform temp_trans;

        for (int i = 0; i < chilcount; i++) 
        {
            temp_trans = Road.GetChild(i);
            BuildingPos.Add(temp_trans.GetChild(0));
            BuildingPos.Add(temp_trans.GetChild(1));
        }
    }

    public void GenerateMap()
    {
        int temp_rand;
        GameObject temp_obj;

        for (int i = 0; i < BuildingPos.Count; i++) 
        {
            if(BuildingPos[i].childCount != 0)
            {
                Destroy(BuildingPos[i].GetChild(0).gameObject);
            }

            temp_rand = UnityEngine.Random.Range(0, BuildingSet.Length);
            temp_obj = Instantiate(BuildingSet[temp_rand]);
            temp_obj.transform.SetParent(BuildingPos[i]);
            temp_obj.transform.localPosition = new Vector3(0, 0, 0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GatherMapData();
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
