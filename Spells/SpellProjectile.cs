using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Entities;

namespace TJ.Spells
{
[System.Serializable] public enum ProjectileType  { SingleShot, AreaOfEffect }
[System.Serializable] public struct ProjectileStats {
    public SpellProjectile projectilePrefab;
    public float projectileMovementSpeed;
    public float projectileArcHeightMultiplier;
    public AnimationCurve projectileArc;
    public string fireSound;
    public Vector3 releasePointOffset;
    public ProjectileType projectileType;
}

public class SpellProjectile : MonoBehaviour
{
    [SerializeField] private float projectileLifeTime;
    [SerializeField] private Transform projectileArcTransform;
    [SerializeField] private Transform projectileModel;
    [SerializeField] private bool enableOnHitEffect;
    [SerializeField] private Transform onHitModel;
    [SerializeField] private string onHitSound;
    private ProjectileType projectileType;
    private float speed, critChance, critAmount, lifeTime, projectileArcMultiplier;
    private string tagToHit;
    readonly string groundTag = "Tile";
    private int projectilePiercing = 0;
    SquadMovementComponent activeTarget;
    DamageBufferElement damage;
    bool hasHit;
    public bool HasHit => hasHit;
    AnimationCurve archCurve;
    Vector3 releasePosition;
    TrailRenderer trailRenderer;
    Vector3 goalPosition;
    System.DateTime timer;
    float aoeLifetime, aoeRadius;
    bool overrideSpeed, hasCompletedArc;
    // int hitCount = 0;
    private void OnEnable() 
    {
        lifeTime = projectileLifeTime;
        distanceToTargetLastFrame = Mathf.Infinity;
        projectileModel.gameObject.SetActive(true);
        hasHit = false;
        //timer to track how long the projectile has been alive
        timer = System.DateTime.Now;
        aoeLifetime = speed;
        // hitCount = 0;
    }
    public void SetStats(ProjectileStats _projectileStats, SquadMovementComponent targetSquadEntity, DamageBufferElement _damage, int _projectilePiercing, float _critChance, 
                            float _critAmount, string _tagToHit, float _aoeRadius = 0f) {
                                
        activeTarget = targetSquadEntity;
        damage = _damage;
        speed = _projectileStats.projectileMovementSpeed;

        projectilePiercing = _projectilePiercing;
        critChance = _critChance;
        critAmount = _critAmount;
        tagToHit = _tagToHit;
        archCurve = _projectileStats.projectileArc;
        projectileArcMultiplier = _projectileStats.projectileArcHeightMultiplier;
        projectileType = _projectileStats.projectileType;
        aoeRadius = _aoeRadius;

        releasePosition = transform.position;

        //estimate the travel time based on the distance to the target
        float distance = Vector3.Distance(releasePosition, activeTarget.SquadCenter);
        float travelTime = distance / speed;

        if(projectileType == ProjectileType.AreaOfEffect){
            if(travelTime < 2f){
                overrideSpeed = true;
            }
        }

        gameObject.SetActive(true);
    }
    public void SetStatsNoTarget(Vector3 _goalPosition, DamageBufferElement _damage, float newSpeed, string _tagToHit, 
        AnimationCurve _animationCurve, float _projectileArcMultiplier, ProjectileType _projectileType, 
        float _aoeRadius)
    {
        hasHit = false;
        goalPosition = _goalPosition;
        speed = newSpeed;
        damage = _damage;
        tagToHit = _tagToHit;
        archCurve = _animationCurve;
        projectileArcMultiplier = _projectileArcMultiplier;
        projectileType = _projectileType;
        aoeRadius = _aoeRadius;
        releasePosition = transform.position;

        if(trailRenderer != null){
            trailRenderer.Clear(); // Clear the existing trail data
            trailRenderer.emitting = true; // Enable the trail renderer
        }
        gameObject.SetActive(true);
    }

    void Update()
    {
        if(hasHit) return;

        HandleDistanceCheck();
        HandleMovement();
    }
    private void HandleMovement()
    {
        if(projectileType == ProjectileType.SingleShot)
            HandleMovementSingleShot();
        else if(projectileType == ProjectileType.AreaOfEffect)
            HandleMovementAreaOfEffect();
    }
    private void HandleMovementSingleShot()
    {
        transform.LookAt(goalPosition);
        float percentageTraveled = Vector3.Distance(releasePosition, transform.position) / Vector3.Distance(releasePosition, goalPosition);
        float archY = archCurve.Evaluate(percentageTraveled) * projectileArcMultiplier;
        projectileArcTransform.localPosition = new Vector3(0, archY, 0);

        //get the angle at the slope of the curve at the percentage traveled
        float angle = AngleAtPoint(percentageTraveled) * projectileArcMultiplier / 2f;
        projectileArcTransform.localRotation = Quaternion.Euler(-angle, 0, 0);

        transform.Translate(speed * Time.deltaTime * Vector3.forward);
    }
    private void HandleMovementAreaOfEffect()
    {
        aoeLifetime -= Time.deltaTime;

        float percentageTraveled = Vector3.Distance(releasePosition, transform.position) / Vector3.Distance(releasePosition, goalPosition);
        if(overrideSpeed) percentageTraveled = (speed - aoeLifetime) / speed;

        //look at the goal position in the xz plane
        transform.LookAt(new Vector3(goalPosition.x, transform.position.y, goalPosition.z));

        float archY = archCurve.Evaluate(percentageTraveled) * projectileArcMultiplier;
        projectileArcTransform.localPosition = new Vector3(0, archY, 0);

        //get the angle at the slope of the curve at the percentage traveled
        float angle = AngleAtPoint(percentageTraveled) * projectileArcMultiplier / 2f;
        projectileArcTransform.localRotation = Quaternion.Euler(-angle, 0, 0);
        
        //move from release position to goal position should land in the maount of time specified by speed
        if(overrideSpeed)
            transform.position = Vector3.Lerp(releasePosition, goalPosition, percentageTraveled);
        else
            transform.position = Vector3.MoveTowards(transform.position, goalPosition, speed * Time.deltaTime);

    }
    private float AngleAtPoint(float point)
    {
        // Function to calculate the derivative of the curve equation at a given point
        float DerivativeAtPoint(float point) {
            float epsilon = 0.001f; // Small value for calculating the derivative
            float x1 = point - epsilon;
            float x2 = point + epsilon;
            float y1 = archCurve.Evaluate(x1);
            float y2 = archCurve.Evaluate(x2);

            // Approximate the derivative using the slope formula (change in y divided by change in x)
            return (y2 - y1) / (x2 - x1);
        }

        float derivative = DerivativeAtPoint(point);
        return Mathf.Rad2Deg * Mathf.Atan(derivative);
    }
    float distanceToTargetLastFrame = Mathf.Infinity;
    private void HandleDistanceCheck()
    {
        if(activeTarget.SelfEntity == Entity.Null){
            HandleFinishArc();
            return;
        }

        goalPosition = activeTarget.SquadCenter;

        float distanceToTarget = Vector3.Distance(transform.position, goalPosition);
        //check to fix arrow going back and forth
        if(distanceToTarget < 0.1f || distanceToTarget > distanceToTargetLastFrame){
            HandleHit();
        }
        distanceToTargetLastFrame = distanceToTarget;
    }
    private void HandleFinishArc()
    {
        if(hasCompletedArc){
            //move forward until we hit the new goal position
            if(Vector3.Distance(transform.position, goalPosition) < 0.1f){
                HandleHit();
            }
        }

        if(Vector3.Distance(transform.position, goalPosition) < 0.1f){
            hasCompletedArc = true;

            //if AOE, we are already at the ground, so just hit
            if(projectileType == ProjectileType.AreaOfEffect){
                HandleHit();
            }
            //if not AOE, we need to just move forward until we hit the ground, so new goal position is the hit point
            else {
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100f, LayerMask.GetMask(groundTag))) {
                    goalPosition = hit.point;
                } else {
                    //probably flying off the island, just turn off 
                    Invoke(nameof(TurnOff), 1f);
                    // Debug.LogError("No ground found");
                }
            }
            return;
        }
    }
    private void HandleHit()
    {
        // Debug.Log($"Travel Time: {(System.DateTime.Now - timer).TotalMilliseconds:n0}ms");
        hasHit = true;
        // Runtime.Instance.IAudioRequester.PlaySFXAtPoint(onHitSound, transform.position);
        projectileModel.gameObject.SetActive(false);
        Invoke(nameof(TurnOff), 3f);
        if(enableOnHitEffect) onHitModel.gameObject.SetActive(true);

        if(projectileType == ProjectileType.SingleShot)
            SingleTargetHit();
        if(projectileType == ProjectileType.AreaOfEffect)
            AOEHit();
    }
    private void SingleTargetHit()
    {
        if(activeTarget.SelfEntity == null) return;
        
        // IDamageable damageable = activeTarget.GetComponent<IDamageable>();
        // TakeDamage(damageable);
    }
    private void AOEHit()
    {
        Collider[] colliders = new Collider[10];

        // instantiate a sphere collider at the hit point
        // Physics.OverlapSphereNonAlloc(transform.position, aoeRadius, colliders, LayerMask.GetMask(tagToHit));
        // foreach(Collider collider1 in colliders){
        //     if(collider1 == null) continue;
        //     IDamageable damageable = collider1.GetComponent<IDamageable>();
        //     TakeDamage(damageable);
        // }
  
            // Runtime.Instance.CameraManager.ShakeBasedOnDistance(transform.position);

    }
    // private void TakeDamage(IDamageable damageable)
    // {
    //     damageable.TakeDamage(damage);
    // }
    private void TurnOff()
    {
        if(turnoffDisabled) return;

        gameObject.SetActive(false);
        projectileModel.gameObject.SetActive(false);
        if(enableOnHitEffect) onHitModel.gameObject.SetActive(false);
    }
    bool turnoffDisabled;
    public void DisableTurnOff()
    {
        turnoffDisabled = true;
    }
}
}