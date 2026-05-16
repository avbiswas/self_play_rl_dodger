using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateScores : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] int which_team;
    [SerializeField] string bullet_to_catch = "";
    EnvironmentManagerRayCast erc;

    void Awake(){
        erc = GetComponentInParent<EnvironmentManagerRayCast>();
    }
    public void OnTriggerEnter2D(Collider2D other){
        if (other.tag == bullet_to_catch){
            erc.register_bullet_dodge(which_team);
        }
    }
}
