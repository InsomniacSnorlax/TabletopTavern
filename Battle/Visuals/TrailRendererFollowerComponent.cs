using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using System.Collections;

public class TrailRendererFollowerComponent : MonoBehaviour
{
    public Entity EntityToFollow;
    public EntityManager EntityManager;
    [SerializeField] private TrailRenderer trailRenderer;
    private bool _isStartingEmitter;

    private void Awake() {
        trailRenderer = GetComponent<TrailRenderer>();
    }
    private void OnEnable() {
        trailRenderer.enabled = false;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        _isStartingEmitter = false;
    }
    private void OnDisable() {
        trailRenderer.enabled = false;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        _isStartingEmitter = false;
    }

    void Update()
    {
        if (EntityManager.Exists(EntityToFollow)){
            if(EntityManager.HasComponent<Bullet>(EntityToFollow)){
                var bullet = EntityManager.GetComponentData<Bullet>(EntityToFollow);
                var actualEntity = bullet.bulletTrajectoryTransform;
                transform.position = EntityManager.GetComponentData<LocalToWorld>(actualEntity).Position;

                if(trailRenderer.enabled == false && !_isStartingEmitter){
                    _isStartingEmitter = true;
                    StartCoroutine(StartEmitting());
                }
            } else {
                gameObject.SetActive(false);
            }
        } else {
            gameObject.SetActive(false);
        }
    }
    private IEnumerator StartEmitting()
    {
        trailRenderer.Clear();
        yield return null;
        trailRenderer.emitting = true;
        trailRenderer.enabled = true;
        _isStartingEmitter = false;
    }
}
