using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    List<GameObject> bullets;
    [SerializeField] int numBullets = 5;
    Coroutine coroutine; 
    Coroutine bulletResetCoroutine;
    [SerializeField] float bulletSpawnMinTime = 0.5f;
    [SerializeField] float bulletSpawnMaxTime = 1.5f;
    [SerializeField] float facing = 1;
    
    FixedWaitTimeManager fixedWaitTimeManager;
    float minX, maxX;
    bool tryFiring;
    void Awake(){
        fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();
        minX = Camera.main.ScreenToWorldPoint(new Vector2(-1, 0)).x - 1f;
        maxX = Camera.main.ScreenToWorldPoint(new Vector2(1, 0)).x + 1f;
    }
    // Start is called before the first frame update
    void Start(){
        tryFiring = true;
        bullets = new List<GameObject>();
        for (int idx = 0; idx < numBullets; idx ++){
            GameObject temp = Instantiate(bullet, transform.position, 
                Quaternion.Euler(0, 0, -90));
            temp.SetActive(false);
            bullets.Add(temp);
        }
    }


    public void Reset(){
        // if (coroutine != null){
        StopAllCoroutines();
        for (int idx = 0; idx < numBullets; idx ++){
            ResetBullet(bullets[idx]);
        }
        // StartCoroutine(SpawnBullet());
        fixedWaitTimeManager.StartFixedCoroutine("spawnBullet", 0.1f, SpawnBullet);
    }

    // Update is called once per frame
    void SpawnBullet(){
        for (int idx = 0; idx < bullets.Count; idx ++){
            if (!bullets[idx].activeInHierarchy){
                bullets[idx].SetActive(true);
                bullets[idx].GetComponent<Bullet>().SetPosition(transform.position);
                bullets[idx].GetComponent<Bullet>().SetDirection(facing);
                break;
            }
        }
    }

    void FixedUpdate(){
        for (int idx = 0; idx < bullets.Count; idx ++){
            if (bullets[idx].activeInHierarchy){
                float xPosition = bullets[idx].transform.position.x;
                if (xPosition < minX && xPosition > maxX){
                    ResetBullet(bullets[idx]);
                }
            }
        }
        if (tryFiring){
            SpawnBullet();
            tryFiring = false;
            float nextSpawnWaitTime = Random.Range(bulletSpawnMinTime, 
                bulletSpawnMaxTime);
            fixedWaitTimeManager.StartFixedCoroutine("tryFiringAgain", nextSpawnWaitTime, TryFiringAgain);
        }
    }
    void TryFiringAgain(){
        tryFiring = true;
    }

    void ResetBullet(GameObject b){
        if (b.activeInHierarchy){
            b.SetActive(false);
            b.transform.position = transform.position;
        }
        
    }
}
