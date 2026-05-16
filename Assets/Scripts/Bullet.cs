using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] float speed = 20f;
    float direction = -1;
    // Start is called before the first frame update

    public void SetSpeed(float speed_){
        speed = speed_;
    }
    public void SetDirection(float direction_){
        direction = direction_;
    }

    public void SetPosition(Vector2 position){
        transform.position = position;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate(){
        rb.velocity = new Vector2(direction * speed, 0);   
    }
}
