using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class SmartBulletSpawner : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    List<GameObject> bullets;
    [SerializeField] int currentNumBullets = 5;
    [SerializeField] int maxBullets = 8;
    Coroutine coroutine; 
    Coroutine bulletResetCoroutine;
    [SerializeField] float bulletSpawnMinTime = 0.5f;
    [SerializeField] float bulletSpawnMaxTime = 1.5f;
    [SerializeField] float bulletSpeed = 20;
    [SerializeField] float slowBulletSpeed = 10;
    [SerializeField] float slowBulletHeight = 1;
    [SerializeField] bool shootPlayerControlled = false;

    FixedWaitTimeManager fixedWaitTimeManager;    
    PlayerControl control;
    float timeLastBulletWasFired;
    float minX, maxX;
    bool tryFiring;
    

    // Start is called before the first frame update
    void Awake(){

       
        minX = -35; //Camera.main.ViewportToWorldPoint(new Vector2(0, 0)).x - 1f;
        maxX = 35f; //Camera.main.ViewportToWorldPoint(new Vector2(1, 0)).x + 1f;
        fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();

    }
    void init_bullets(){
        bullets = new List<GameObject>();
        for (int idx = 0; idx < maxBullets; idx ++){
            GameObject temp = Instantiate(bullet, transform.position, 
                Quaternion.Euler(0, 0, -90));
            temp.SetActive(false);
            bullets.Add(temp);
        }
        tryFiring = true;
    }

    public void UpdateNumBullets(int x){
        currentNumBullets = x; 
    }

    public float GetNormalizedTimeSinceLastBullet(){
        return Mathf.Min(GetTimeSinceLastBullet()/bulletSpawnMinTime, 1);
    }
    public float GetTimeSinceLastBullet(){
        return Time.fixedTime - timeLastBulletWasFired;
    }

    public bool CanShoot(){
        if (GetTimeSinceLastBullet() > bulletSpawnMinTime){
            for (int idx = 0; idx < bullets.Count; idx ++){
                if (!bullets[idx].activeInHierarchy){
                    return true;
                }
            }
        }

        return false;
    }

    public void Reset(){
        if (fixedWaitTimeManager == null){
            fixedWaitTimeManager = gameObject.GetComponent<FixedWaitTimeManager>();
            if (fixedWaitTimeManager == null){
                fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();
            }
        }
        fixedWaitTimeManager.FinishAllCoroutines(true);
        timeLastBulletWasFired = Time.fixedTime;
        if (bullets == null){
            init_bullets();
        }
        if (control == null){
            control = GetComponentInParent<PlayerControl>();
        }
        for (int idx = 0; idx < maxBullets; idx ++){
            bullets[idx].tag = control.bulletTag;
            ResetBullet(bullets[idx]);
        }
    }

    public Collider2D[] GetBulletColliders(){
        List<Collider2D> collider2Ds = new List<Collider2D>();
        for (int idx = 0; idx < maxBullets; idx ++){
            if (bullets[idx].activeInHierarchy){
                collider2Ds.Add(bullets[idx].GetComponent<CapsuleCollider2D>());
            }
        } 
        return collider2Ds.ToArray();
    }

    public void FireBullet(){
        if (shootPlayerControlled && (GetTimeSinceLastBullet() < bulletSpawnMinTime)){
            return;
        }
        float distance = control.GetDistanceFromGround();
        float distanceRatio = distance/slowBulletHeight;
        bool firingFastBullet = distanceRatio < 0.5;

        float newBulletSpeed = Mathf.Lerp(bulletSpeed, slowBulletSpeed, 
                    distanceRatio);
        newBulletSpeed = Mathf.Clamp(newBulletSpeed, slowBulletSpeed, bulletSpeed);
   
        for (int idx = 0; idx < currentNumBullets; idx ++){
            if (idx >= maxBullets){
                break;
            }
            if (!bullets[idx].activeInHierarchy){
                bullets[idx].GetComponent<Bullet>().SetPosition(transform.position);
                bullets[idx].GetComponent<Bullet>().SetSpeed(newBulletSpeed);
                bullets[idx].GetComponent<Bullet>().SetDirection(control.GetFacingDirection());
                bullets[idx].SetActive(true);  
                timeLastBulletWasFired = Time.fixedTime;              
                break;
            }
        }
    }
    void FixedUpdate(){
        if (bullets == null){
            init_bullets();
        }
        if (control == null){
            control = GetComponentInParent<PlayerControl>();
        }
        if (fixedWaitTimeManager == null)
            fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();

        for (int idx = 0; idx < bullets.Count; idx ++){
            float xPosition = bullets[idx].transform.position.x;
            if (xPosition < minX || xPosition > maxX){
                ResetBullet(bullets[idx]);
            }
        }
        if (!shootPlayerControlled){
            if (tryFiring){
                FireBullet();
                tryFiring = false;
                fixedWaitTimeManager.StartFixedCoroutine("nextSpawnWaitTime", Random.Range(bulletSpawnMinTime, 
                    bulletSpawnMaxTime), EnableFiring);
            }
        }
    }


    // Update is called once per frame
    void EnableFiring(){
        tryFiring = true;
    }

    void ResetBullet(GameObject b){
        if (b.activeInHierarchy){
            b.SetActive(false);
            //b.GetComponent<SpriteRenderer>().enabled = false;
            //b.GetComponent<TrailRenderer>().enabled = false;
            //b.transform.position = transform.position;
        }
        
    }
}
