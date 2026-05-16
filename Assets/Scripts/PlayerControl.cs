using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
public class PlayerControl : MonoBehaviour
{
    Rigidbody2D rb;
    CapsuleCollider2D collider2D;
    [SerializeField] float speed;
    [SerializeField] float jumpHeight=6;
    [SerializeField] float jumpForce=6;
    [SerializeField] bool allowCrouchFire = false;
    [SerializeField] ParticleSystem canDashEffect;

    Animator animator;
    
    public enum DeathReasons {
        fall,
        inactivity,
        bullet,

        alive
    };

    public DeathReasons deathReasons;
    public float minX;
    public float maxX;
    float score;
    bool isJumping = false;
    bool isAlive = true;
    bool canCancelJump = false;
    bool isCrouching = false;
    public bool isJumpUpGravityOn = false;
    bool jumpQueued = false;
    bool canDash = true;
    bool isInvulnerable = false;
    bool isDashing = false;
    float defaultGravity = 1;
    [SerializeField] int start_num_lives = 3;
    int num_lives;
    float dangerHeight = -6f;
    [SerializeField] float crouchGravity = 10f;
    [SerializeField] float fallDownGravity = 2f;
    [SerializeField] float jumpCancelTime = 0.2f;
    [SerializeField] float jumpGravityOffTime=0.15f;
    [SerializeField] float dashTime = 0.1f;
    
    [SerializeField] float dashCooldownTime = 1f;
    [SerializeField] float dashForce = 20f;
    [SerializeField] Sprite[] jumpAnimation;

    [SerializeField] GameObject runStopDust;
    
    [SerializeField] GameObject left;
    [SerializeField] GameObject right;
    [SerializeField] GameObject jump;
    [SerializeField] GameObject canShoot;
    
    
    [SerializeField] bool autoClamp = true;  
    List<Collider2D> bulletColliders;
    float timeDashed;
    float maxY;
    public string bulletTag;
    public string adversarialBulletTag;
    [SerializeField] float facing = 0f;
    TrailRenderer trailRenderer;
    float colliderStandHeight, colliderCrouchHeight, colliderStandCenter, colliderCrouchCenter;

    [SerializeField] bool isDummy = false;
    FixedWaitTimeManager fixedWaitTimeManager;
    [SerializeField] Transform groundTransfom;
    SmartBulletSpawner shootPoint;
    Transform shooter;
    Vector2 shooter_pos;
    [SerializeField] float maxTimeSinceLastBullet = 6f;
    float maxHeightSoFar = -0.1f;
    string groundLayer;

    void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        init();
        
    }  
    void init(){

        if (groundTransfom != null){
            minX = groundTransfom.position.x - groundTransfom.localScale.x/2;
            maxX = groundTransfom.position.x + groundTransfom.localScale.x/2;
            maxY = groundTransfom.position.y + groundTransfom.localScale.y/2;
            groundLayer = LayerMask.LayerToName(groundTransfom.gameObject.layer);
        }
        if (facing == 0){
            facing = transform.parent.localScale.x;
        }
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;
        collider2D = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        isAlive = true;
        colliderStandHeight = collider2D.size.y;
        colliderStandCenter = collider2D.offset.y;
        colliderCrouchHeight = colliderStandHeight/1.6f;
        colliderCrouchCenter = colliderStandCenter - (colliderStandHeight - colliderCrouchHeight)/2;
        shootPoint = GetComponentInChildren<SmartBulletSpawner>();
        shooter = shootPoint.gameObject.GetComponent<Transform>();
        shooter_pos = shooter.localPosition;
        num_lives = start_num_lives;

    }
    
    void Start(){
        fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();
        Bullet[] bullets = FindObjectsOfType<Bullet>(true);
        bulletColliders = new List<Collider2D>();
        foreach (Bullet bullet in bullets){
            if (bullet.gameObject.tag == adversarialBulletTag){
                bulletColliders.Add(bullet.GetComponent<Collider2D>());
            }
        }
    }

    public int NumLives(){
        return num_lives;
    }

    public void FireBullet(){
        if (isCrouching){
            if (allowCrouchFire)
                shootPoint.FireBullet();
        }
        else{
            shootPoint.FireBullet();
        }
    }

    public void UpdateNumBullets(int num_bullets){
        shootPoint.UpdateNumBullets(num_bullets);
    }

    public float GetNormalizedTimeSinceLastBullet(){
        return shootPoint.GetNormalizedTimeSinceLastBullet();
    }

    
    public float GetTimeRemainingTillNextShot(){
        return maxTimeSinceLastBullet - shootPoint.GetTimeSinceLastBullet();
    }


    void SpawnDustEffect(GameObject dust, float dustXOffset = 0f)
    {
        if (dust != null)
        {
            // Set dust spawn position
            //float y_pos = -4f;
            //Vector2 dustSpawnPosition = new Vector2(transform.position.x, y_pos);
            //GameObject newDust = Instantiate(dust, dustSpawnPosition, Quaternion.identity) as GameObject;
            // Turn dust in correct X direction
            //newDust.transform.localScale = newDust.transform.localScale.x * new Vector3(transform.localPosition.x * facing, 1, 1);
        }
    }

    public void SetAlive(){
        isAlive = true;
        deathReasons = DeathReasons.alive;
        canCancelJump = false;
        isCrouching = false;
        canDash = true;
        jumpQueued = false;
        isInvulnerable = false;
        isDashing = false;
        if (rb == null){
            init();
            Start();
        }
        rb.velocity = Vector2.zero;
        if (shootPoint != null){
            shootPoint.Reset();
        }
    }

    public bool IsCrouching(){
        return isCrouching;
    }
    public bool CanDash(){
        return canDash;
    }
    public bool IsDashing(){
        return isDashing;
    }
    public float TimeToNextDash(){
        if (canDash){
            return 0;
        }
        else{
            return 1 - (Time.unscaledTime - timeDashed)/(dashCooldownTime + dashTime);
        }
    }
    

    public float GetFacingDirection(){
        return facing;
    }

    public float GetDistanceFromGround(){
        return (transform.position.y - transform.localScale.y/2) - maxY;
    }

    public void Move(int horizontalInput){
        if (isDashing){
            return;
        }
        if (horizontalInput == 0){
            //left.SetActive(false);
            right.SetActive(false);
            // transform.localScale = new Vector2(facing * 1, transform.localScale.y);  
        }
        else if (horizontalInput == 1){
            //left.SetActive(false);
            right.SetActive(true);  
            // transform.localScale = new Vector2(facing * 1, transform.localScale.y);  
        }
        else if (horizontalInput == -1){
            //left.SetActive(true);
            right.SetActive(true);
            // transform.localScale = new Vector2(facing * -1, transform.localScale.y);
        }

        rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);        
    }

    public void PlayRunDust(){
        if (!isJumping){
            SpawnDustEffect(runStopDust, 0.6f);
        }
    }

    public float GetCrouchableHeight(){
        return colliderCrouchCenter + colliderCrouchHeight/2;
    }

    public void Update(){
        if (canShoot != null){
            canShoot.SetActive(shootPoint.CanShoot());
        }
        if (Input.GetKeyDown(KeyCode.K)){
            Debug.Break();
        }
    }

    public Vector2 GetDistanceFromPlatformEdges(){
        float towardX, awayX;
        if (facing == -1){
            awayX = maxX;
            towardX = minX;
        }
        else{
            awayX = minX;
            towardX = maxX;
        }        
        return new Vector2((transform.position.x - towardX) * facing, 
            (transform.position.x - awayX) * facing);
    }

    public void FixedUpdate(){
        if (isDummy){
            return;
        }
        if (fixedWaitTimeManager == null){
            fixedWaitTimeManager = gameObject.AddComponent<FixedWaitTimeManager>();
        }

        CheckCollisions();
        CheckFallDeath();
        if (Academy.Instance.IsCommunicatorOn)
            CheckInactivityDeath();
        if (autoClamp){
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, minX, maxX),
                transform.position.y);
        }
        if (collider2D.IsTouchingLayers(LayerMask.GetMask(groundLayer))){
            isJumping = false;
            canCancelJump = false;
            if (!isDashing){
                if (trailRenderer != null)
                    trailRenderer.emitting = false;
            }
        
            if (jumpQueued){
                Jump();
            }
        }

        if (isJumpUpGravityOn){
            rb.gravityScale = 0;
        }
        else if (isCrouching){
            rb.gravityScale = crouchGravity;
            collider2D.size = new Vector2(collider2D.size.x, colliderCrouchHeight);
            collider2D.offset = new Vector2(collider2D.offset.x, colliderCrouchCenter);
        }
        else{
            collider2D.size = new Vector2(collider2D.size.x, colliderStandHeight);
            collider2D.offset = new Vector2(collider2D.offset.x, colliderStandCenter);
            if ((rb.velocity.y < 0)){
                rb.gravityScale = fallDownGravity;
            }
            else if (rb.velocity.y > 0 && GetDistanceFromGround() > (jumpHeight)){
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y/4);
                //rb.gravityScale = fallDownGravity;
            }
            else{
                rb.gravityScale = defaultGravity;
            }
        }
        UpdateShooterHeight();
        UpdateAnimation();
       
    }

    void UpdateShooterHeight(){
        if (isCrouching){
            shooter.localPosition = new Vector2(shooter.localPosition.x, shooter.localPosition.y/2);
        }
        else{
            shooter.localPosition = shooter_pos;
        }
    }
    void CheckInactivityDeath(){
        if (shootPoint != null){
            if (shootPoint.GetTimeSinceLastBullet() > maxTimeSinceLastBullet){
                Die(DeathReasons.inactivity);
            }

        }
    }
    void CheckFallDeath(){
        if (transform.localPosition.y < -5){
            Die(DeathReasons.fall);
        }

        /*if (collider2D.IsTouchingLayers(LayerMask.GetMask("FallenDown"))){
            if (collider2D.IsTouching(fallDownCollider)){
                Debug.Log("Current: " + deathReasons + ", pos: " + transform.position);
                
                Die(DeathReasons.fall);

            }
        }*/
    }

    void Die(DeathReasons reason){
        deathReasons = reason;
        isAlive = false;
        fixedWaitTimeManager.FinishAllCoroutines();
        num_lives -= 1;

        Debug.Log("Num Lives" + num_lives + reason);
    }

    public float Remap (float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    void PlayAnimation(string animationName){
        animator.enabled = true;
        if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == animationName){
            return;
        }
        animator.Play(animationName);
        

    }
    void UpdateAnimation(){
        if (isDashing){
            PlayAnimation("Dash");
        }  
        else if (isCrouching){
            animator.enabled = true;
            if (rb.velocity.x != 0){
                PlayAnimation("CrouchMove");   
            }
            else{
                PlayAnimation("Crouch");
            }
        }
        else if (!collider2D.IsTouchingLayers(LayerMask.GetMask(groundLayer))){
            
            animator.enabled = false;
            
            int remapped = Mathf.Clamp(
                Mathf.FloorToInt(Remap(-rb.velocity.y, -jumpHeight, jumpHeight, 
                0, jumpAnimation.Length)),
                0, jumpAnimation.Length - 1);
            if (!isDummy){
                GetComponent<SpriteRenderer>().sprite = jumpAnimation[remapped];
            }
        }
        else {
            animator.enabled = true;
            // is standing
            if (rb.velocity.x != 0){
                PlayAnimation("Run");   
            }
            else{
                PlayAnimation("Idle");
            }
        }

        if (canDash && !canDashEffect.isEmitting){
            canDashEffect.Play();
        }
        else if (!canDash && canDashEffect.isEmitting){
            canDashEffect.Stop();
        }
    }

    public void CheckCollisions(){
        if (isInvulnerable){
            return;
        }
        if (collider2D.IsTouchingLayers(LayerMask.GetMask(adversarialBulletTag))){
            foreach (Collider2D bullet in bulletColliders){
                if (collider2D.IsTouching(bullet)){
                    Die(DeathReasons.bullet);
                    animator.SetLayerWeight(1, 1.0f);
                    animator.Play("Hurt", 1);
                }
            }
        }
    }
    public void OnCollisionEnter2D(Collision2D other){
        
        if (collider2D.IsTouchingLayers(LayerMask.GetMask(adversarialBulletTag))){
            if (other.gameObject.tag == adversarialBulletTag){
                if (!isInvulnerable){
                    Die(DeathReasons.bullet);
                    animator.SetLayerWeight(1, 1.0f);
                    animator.Play("Hurt", 1);
                }
            }
        }
    }

    public void DisableHurtLayer(){
        animator.SetLayerWeight(1, 0.0f);
    }
    
    public float ttc(GameObject bullet, float maxTime = 5.0f)
    {
        CapsuleCollider2D bulletCollider = bullet.GetComponent<CapsuleCollider2D>();
        float rad = collider2D.size.y/2 + bulletCollider.size.x/2;
        
        Vector2 dx = ((Vector2) transform.position + collider2D.offset) - (Vector2) bullet.transform.position;
        float c = dx.x * dx.x + dx.y * dx.y - rad * rad;

        if (c < 0)
        {
            return 0;
        }

        Vector2 actual_dv = rb.velocity - bullet.GetComponent<Rigidbody2D>().velocity;

        float a = actual_dv.sqrMagnitude;
        float b = Vector2.Dot(dx, actual_dv);

        if (b > 0)
        {
            return maxTime;
        }
        float dis = b * b - a * c;
        if (dis <= 0)
        {
            return maxTime;
        }
        float tau = c / (-b + (float)Mathf.Sqrt(dis));
        if (tau < 0)
        {   
            return maxTime;
        }
        tau = (tau > maxTime)?maxTime: tau;
        return tau;
    }

    public float ttc_to_ttd(float ttc, float maxTime=5.0f){
        if (ttc > maxTime){
            return 0;
        }
        return Mathf.Exp(1 - 1/(1 + 0.00001f - ttc/maxTime));
    }

    public void Dash(int value, int direction=1)
    {
        if (value == 1 & canDash){
            BeginDash(direction);
        }
        animator.SetBool("Dashing", isDashing);
        if (isDashing){
            if (trailRenderer != null){
                trailRenderer.emitting = true;
            }
        }
        
    }

    void BeginDash(int direction){
        canDash = false;
        isInvulnerable = true;
        isDashing = true;
        Physics2D.IgnoreLayerCollision(gameObject.layer, 
            LayerMask.NameToLayer(adversarialBulletTag), true);
        timeDashed = Time.unscaledTime;
        // rb.velocity = new Vector2(facing * dashForce, rb.velocity.y);
        rb.AddForce(new Vector2(facing * dashForce * direction, 0), ForceMode2D.Impulse);
        //transform.localScale = new Vector2(facing, transform.localScale.y); 

        //StartCoroutine(StopDash(dashTime));
        fixedWaitTimeManager.StartFixedCoroutine("dashBegin", dashTime, StopDash, false);
        
    }

    void StopDash(){
        Physics2D.IgnoreLayerCollision(gameObject.layer, 
            LayerMask.NameToLayer(adversarialBulletTag), false);
        
        isInvulnerable = false;
        isDashing = false;
        fixedWaitTimeManager.StartFixedCoroutine("dashStop", dashCooldownTime, ResetDash, false);
    }

    void ResetDash(){
        canDash = true;
    }

    public void Crouch(int value){
        if (isCrouching && value == 0){
            //fixedWaitTimeManager.StartFixedCoroutine("crouchGetUp", 0.1f, GetUpFromCrouch, false);
            GetUpFromCrouch();
        }
        else{
            isCrouching = (value == 1);

        }
        animator.SetBool("Crouch", isCrouching);        
    }
    public void GetUpFromCrouch(){
        isCrouching = false;
    }

    public bool IsJumping(){
        return isJumping;
    }
    public bool IsAlive(){
        return isAlive;
    }

    public DeathReasons DeathReason(){
        return deathReasons;
    }

    public void Jump(){
        if (isCrouching){
            return;
        }
        jump.SetActive(true);
        if (collider2D.IsTouchingLayers(LayerMask.GetMask(groundLayer)) && !isJumping){
            // rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isJumping = true;
            canCancelJump = false;
            jumpQueued = false;
            isJumpUpGravityOn = true;
            
            fixedWaitTimeManager.StartFixedCoroutine("turnOffJumpGravity", jumpGravityOffTime, TurnOffJumpGravity, false);
            fixedWaitTimeManager.StartFixedCoroutine("jumpCancelTime", jumpCancelTime, EnableJumpCancel, false);
            
        }
        else if (rb.velocity.y < 0) {
            jumpQueued = true;
            fixedWaitTimeManager.StartFixedCoroutine("resetJumpQueued", 0.1f, ResetJumpQueued, false);
        }

    }
    void TurnOffJumpGravity(){
        isJumpUpGravityOn = false;
    }

    void ResetJumpQueued(){
        jumpQueued = false;
    }
    void EnableJumpCancel(){
        canCancelJump = true;
    }
    public void JumpSpeedCancel(){
        jump.SetActive(false);
        if (rb.velocity.y > 0 && canCancelJump){
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.05f);
        }
    }
    public float MaxSpeed(){
        return speed;
    }

    public void AddScore(int addScore){
        score += addScore;
    }
}
