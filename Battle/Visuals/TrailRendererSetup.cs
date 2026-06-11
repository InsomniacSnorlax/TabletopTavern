using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Transforms;
using Unity.Mathematics;

public class TrailRendererSetup : MonoBehaviour
{
    [SerializeField] private GameObject trailPrefab, smokeExplosionPrefab, flamingArrowTrailPrefab;
    private List<GameObject> smokeExplosions = new();
    private List<TrailRendererFollowerComponent> trailRendererFollowers = new();
    private List<TrailRendererFollowerComponent> flamingArrowTrailFollowers;
    int poolSize = 100;
    private void Awake()
    {
        for (int i = 0; i < poolSize; i++){
            GameObject trailInstance = Instantiate(trailPrefab, this.transform);
            trailInstance.SetActive(false);
            trailRendererFollowers.Add(trailInstance.GetComponent<TrailRendererFollowerComponent>());

            GameObject smokeInstance = Instantiate(smokeExplosionPrefab, this.transform);
            smokeInstance.SetActive(false);
            smokeExplosions.Add(smokeInstance);
        }
    }
    public void SetupTrail(Entity entity, EntityManager entityManager, bool smokeExplosionOnStart, bool flamingAmmo)
    {
        if(flamingAmmo)
        {
            SetupFlamingArrowTrail(entity, entityManager);
            return;
        }
        
        if(trailRendererFollowers.Count == 0){
            return;
        }
        TrailRendererFollowerComponent followerComponent = GetInactiveTrailRenderer();
        if (smokeExplosionOnStart)
        {
            float3 newPosition = entityManager.GetComponentData<LocalToWorld>(entity).Position;
            if(float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y) || float.IsNaN(newPosition.z)){
                return;
            }
            
            GameObject smokeExplosion = GetInactiveSmokeExplosion();
            smokeExplosion.transform.position = newPosition;

            StartCoroutine(DisableSmokeCoroutine(smokeExplosion));
        }
        followerComponent.EntityToFollow = entity;
        followerComponent.EntityManager = entityManager;
        BattleManager.Instance.SquadManager.stuffToDestroy.Add(followerComponent.gameObject);
    }
    private TrailRendererFollowerComponent GetInactiveTrailRenderer()
    {
        for (int i = 0; i < trailRendererFollowers.Count; i++)
        {
            if (trailRendererFollowers[i].gameObject.activeSelf == false)
            {
                trailRendererFollowers[i].gameObject.SetActive(true);
                return trailRendererFollowers[i];
            }
        }
        TrailRendererFollowerComponent trailInstance = Instantiate(trailPrefab, this.transform).GetComponent<TrailRendererFollowerComponent>();
        trailRendererFollowers.Add(trailInstance.GetComponent<TrailRendererFollowerComponent>());
        return trailInstance;
    }
    private GameObject GetInactiveSmokeExplosion()
    {
        for (int i = 0; i < smokeExplosions.Count; i++)
        {
            if (smokeExplosions[i].activeSelf == false)
            {
                smokeExplosions[i].SetActive(true);
                return smokeExplosions[i];
            }
        }
        GameObject smokeInstance = Instantiate(smokeExplosionPrefab, this.transform);
        smokeExplosions.Add(smokeInstance);
        return smokeInstance;
    }
    public void SetupFlamingArrowTrail(Entity entity, EntityManager entityManager)
    {
        flamingArrowTrailFollowers ??= new List<TrailRendererFollowerComponent>();

        TrailRendererFollowerComponent followerComponent = GetInactiveFlamingArrowTrailRenderer();
        followerComponent.EntityToFollow = entity;
        followerComponent.EntityManager = entityManager;
        BattleManager.Instance.SquadManager.stuffToDestroy.Add(followerComponent.gameObject);
    }
    private TrailRendererFollowerComponent GetInactiveFlamingArrowTrailRenderer()
    {
        for (int i = 0; i < flamingArrowTrailFollowers.Count; i++)
        {
            if (!flamingArrowTrailFollowers[i].gameObject.activeSelf)
            {
                flamingArrowTrailFollowers[i].gameObject.SetActive(true);
                return flamingArrowTrailFollowers[i];
            }
        }
        TrailRendererFollowerComponent trailInstance = Instantiate(flamingArrowTrailPrefab, transform).GetComponent<TrailRendererFollowerComponent>();
        flamingArrowTrailFollowers.Add(trailInstance);
        return trailInstance;
    }
    private IEnumerator DisableSmokeCoroutine(GameObject smokeExplosion)
    {
        yield return new WaitForSeconds(2f);
        smokeExplosion.SetActive(false);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}