using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsController : MonoBehaviour
{
    int num_bullets;
    float bullet_min_time;
    float dash_cooldown_choices; 
    PlayerControl pc;
    // Start is called before the first frame update
    void Start()
    {
        pc = GetComponent<PlayerControl>();
    }

    void UpdateNumBullets(int x){
        pc.UpdateNumBullets(x);        
    }

    void UpdateMinTime(float minTime){
        
    }

    void UpdateDashCooldown(float dashCooldownTime){

    }


}
