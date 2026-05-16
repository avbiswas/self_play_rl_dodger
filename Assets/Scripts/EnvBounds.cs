using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvBounds : MonoBehaviour
{
    List<BulletSpawner> spawners;
    // Start is called before the first frame update
    void Awake()
    {
        spawners = new List<BulletSpawner>();
        Transform[] transforms = GetComponentsInChildren<Transform>();
        foreach (Transform t in transforms){
            if (t.tag == "Spawner"){
                spawners.Add(t.gameObject.GetComponent<BulletSpawner>());
            }
        }
        
    }

    public BulletSpawner[] GetSpawner(){
        return spawners.ToArray();
    }

}
