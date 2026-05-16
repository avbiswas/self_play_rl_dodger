using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;


public class ShootRayCastAgent : Agent
{
    PlayerControl control;
    float stayAliveReward = 0f; // -0.0001f;
    float dodgeReward = 0f; // 0.0002f;
    
    HUDManager uIManager;
    public int team_id;
    int rewards_queued = 0;
    float startY, startX;
    float localStartX;
    Rigidbody2D rb;
    [SerializeField] int decisionInterval = 6;
    [SerializeField] int maxBulletsTracked = 4;
    [SerializeField] int maxBackBulletsTracked = 2;
    [SerializeField] int opponentBulletsTracked = 2;
    [SerializeField] Transform sensor;
    [SerializeField] bool maintainOffsetX = false;
    [SerializeField] bool maintainOffsetY = false;

    [SerializeField] Transform cameraSensor;
    Vector2 defaultCameraSensorScale;
    

    Vector3 sensorOffset, defaultSensorPosition;
    
    
    Vector2 facingCorrection; 
    [SerializeField] float dashPenalty;
    CapsuleCollider2D bodyCollider;
    int numSteps = 0;
    [SerializeField] Transform opponentTransform;
    Rigidbody2D opponentRb;
    PlayerControl opponentControl;
    ShootPlayerAgent opponentShooterAgent;

    EnvironmentManagerRayCast environmentManager;
    void Start(){
        startX = transform.localPosition.x;
        localStartX = transform.localPosition.x;
        startY = transform.localPosition.y;
        rb = GetComponent<Rigidbody2D>();
        control = GetComponent<PlayerControl>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        uIManager = FindObjectOfType<HUDManager>();
        environmentManager = GetComponentInParent<EnvironmentManagerRayCast>();
        facingCorrection = new Vector2(
            control.GetFacingDirection(), 1);
        opponentRb = opponentTransform.gameObject.GetComponent<Rigidbody2D>();
        opponentControl = opponentTransform.gameObject.GetComponent<PlayerControl>();
        opponentShooterAgent = opponentTransform.gameObject.GetComponent<ShootPlayerAgent>();
        team_id = GetComponent<BehaviorParameters>().TeamId;
        if (sensor != null){
            sensorOffset = (sensor.position - transform.position);
            defaultSensorPosition = sensor.position;
        }
        if (cameraSensor != null){
            defaultCameraSensorScale = cameraSensor.localScale; 
        }
    }

    public void FixedUpdate(){
        if (StepCount % decisionInterval == 0){
            RequestDecision();
        }
        RequestAction();
        if (!maintainOffsetX && !maintainOffsetY){
            if (sensor != null)
                sensor.position = transform.position;
        }
        else{
            float posX, posY;
            posX = maintainOffsetX?(transform.position.x + sensorOffset.x):transform.position.x;
            posY = maintainOffsetY?(transform.position.y + sensorOffset.y):defaultSensorPosition.y;
            sensor.position = new Vector3(posX, posY, sensor.position.z);
        }
        
        if (cameraSensor != null){
            if (control.IsCrouching()){
                cameraSensor.localScale = new Vector2(1, 0.5f) * defaultCameraSensorScale;
            }
            else{
                cameraSensor.localScale = new Vector2(1, 1f) * defaultCameraSensorScale;
            }
        }
    }
    public override void OnEpisodeBegin()
    {
        numSteps = 0;
        transform.localPosition = new Vector2(startX, startY);
        if (rb == null){
            Start();
        }
        rb.velocity = new Vector2(0, 0);
        control.SetAlive();
    }
    
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        actionMask.SetActionEnabled(4, 1, !control.IsCrouching());
    }


    public override void OnActionReceived(ActionBuffers actions){
        int moveInput = actions.DiscreteActions[0];
        int jumpControl = actions.DiscreteActions[1];
        int crouchControl = actions.DiscreteActions[2];
        int dashControl = actions.DiscreteActions[3];
        int shootControl = actions.DiscreteActions[4];

        if (shootControl == 1){
            control.FireBullet();
        }
        if (moveInput == 2){
            control.Move(-1 * (int) facingCorrection.x);
        }
        else if (moveInput == 1){
            control.Move(1  * (int) facingCorrection.x);
        }
        else{
            control.Move(0);
        }

        float distance_from_center = Mathf.Abs(localStartX - transform.localPosition.x);
        
        /*
        float energy = Vector2.Dot(rb.velocity, rb.velocity)/100;
        float energy_reward = 0;
        if (energy > 0.4){
            energy_reward = energyPenalty * Mathf.Exp(-energy);
            energy_reward = Mathf.Min(energyPenalty, Mathf.Max(energy_reward, 0));
        }
        AddReward(energy_reward);

        */
        if (jumpControl != 0){
            control.Jump();
        }
        else{
            control.JumpSpeedCancel();
        }
        
        control.Crouch(crouchControl);
        int dashDir = (moveInput == 1)? 1 : (moveInput == 0)? 0: -1;
        control.Dash(dashControl, dashDir);

        if (!control.IsAlive()){
            environmentManager.Resolve(team_id);
        }
        //else{
        //    AddReward(stayAliveReward);
        //}

        if (StepCount == MaxStep){
            Debug.Log("Total Reward Finished for Player " + team_id + " is : " + GetCumulativeReward());
            EpisodeInterrupted();
        }
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((transform.localPosition.x)/(control.maxX - control.minX) );
        sensor.AddObservation(transform.localPosition.y/3f);
        sensor.AddObservation(rb.velocity.x/control.MaxSpeed() * facingCorrection.x);
        
        sensor.AddObservation(rb.velocity.y/control.MaxSpeed());
        sensor.AddObservation(control.IsCrouching());
        sensor.AddObservation(control.CanDash());
        sensor.AddObservation(control.IsDashing());
        sensor.AddObservation(control.TimeToNextDash());
        sensor.AddObservation(control.GetNormalizedTimeSinceLastBullet());
        
        if (uIManager != null)
            uIManager.UpdateTimer(team_id, control.GetTimeRemainingTillNextShot());

        // DebugRayPerception();
    }

    void DebugRayPerception(){
        RayPerceptionSensorComponent2D rcp = GetComponentInChildren<RayPerceptionSensorComponent2D>();
        RayPerceptionInput spec = rcp.GetRayPerceptionInput();
        RayPerceptionOutput obs = RayPerceptionSensor.Perceive(spec);
        RayPerceptionOutput.RayOutput[] outs = obs.RayOutputs;
        for (int i = 0; i < outs.Length; i ++){
            if (outs[i].HitGameObject != null)
                Debug.Log(team_id + ": " + i + ", " + outs[i].HitFraction + outs[i].HitGameObject);
        }
        Debug.Break();
    }
    public override void Heuristic(in ActionBuffers actionsOut){
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        int movementControl = (int) Input.GetAxisRaw("Horizontal");
       
        if (movementControl == -1){
            movementControl = 2;
        }
        actions[0] = movementControl;
        numSteps += 1 ;

        actions[1] = (int) Input.GetAxisRaw("Jump");        
        float crouchInput = Input.GetAxisRaw("Vertical");
        float dashInput = Input.GetAxisRaw("Dash");
        
        if (crouchInput == 1){
            crouchInput = 0;
        }
        else if (crouchInput == -1){
            crouchInput = 1;
        }


        actions[2] = (int) crouchInput;
        actions[3] = (int) dashInput;
        actions[4] = Input.GetKey(KeyCode.K) ? 1 : 0;
    }
}
