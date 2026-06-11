using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;

namespace TJ
{
public class UnitDebugSetUp : MonoBehaviour
{
    [SerializeField] private GameObject debugCanvasPrefab, debugSquadCanvasPrefab;
    private List<UnitDebugUnitPosition> unitDebugCanvases = new ();
    private List<SquadEntityDebugPosition> debugSquadCanvases = new ();
    private void Awake()
    {
        if(!BattleManager.Instance.Debug) return;

        for (int i = 0; i < 100; i++){
            GameObject unitDebugCanvas = Instantiate(debugCanvasPrefab, this.transform);
            unitDebugCanvas.SetActive(false);
            unitDebugCanvases.Add(unitDebugCanvas.GetComponent<UnitDebugUnitPosition>());
        }
        for (int i = 0; i < 10; i++){
            GameObject squadDebugCanvas = Instantiate(debugSquadCanvasPrefab, this.transform);
            squadDebugCanvas.SetActive(false);
            debugSquadCanvases.Add(squadDebugCanvas.GetComponent<SquadEntityDebugPosition>());
        }
    }
    public void SetUpPositionDebug(Entity entity, EntityManager entityManager)
    {
        if(!BattleManager.Instance.Debug) return;

        // Debug.Log($"Setting up debug for entity {entity.Index}");
        if(unitDebugCanvases.Count == 0){
            return;
        }
        UnitDebugUnitPosition followerComponent = GetInactiveUnitDebug();
        followerComponent.EntityToFollow = entity;
        followerComponent.EntityManager = entityManager;
    }
    public void SetUpSquadDebug(Entity entity, EntityManager entityManager)
    {
        if(!BattleManager.Instance.Debug) return;

        if(debugSquadCanvases.Count == 0){
            return;
        }
        SquadEntityDebugPosition followerComponent = GetInactiveSquadRenderer();
        followerComponent.Entity = entity;
        followerComponent.EntityManager = entityManager;
    }
    private UnitDebugUnitPosition GetInactiveUnitDebug()
    {
        for(int i = 0; i < unitDebugCanvases.Count; i++){
            if(unitDebugCanvases[i].gameObject.activeSelf == false){
                unitDebugCanvases[i].gameObject.SetActive(true);
                return unitDebugCanvases[i];
            }
        }
        UnitDebugUnitPosition trailInstance = Instantiate(debugCanvasPrefab, this.transform).GetComponent<UnitDebugUnitPosition>();
        unitDebugCanvases.Add(trailInstance.GetComponent<UnitDebugUnitPosition>());
        return trailInstance;
    }
    private SquadEntityDebugPosition GetInactiveSquadRenderer()
    {
        for(int i = 0; i < debugSquadCanvases.Count; i++){
            if(debugSquadCanvases[i].gameObject.activeSelf == false){
                debugSquadCanvases[i].gameObject.SetActive(true);
                return debugSquadCanvases[i];
            }
        }
        SquadEntityDebugPosition trailInstance = Instantiate(debugSquadCanvasPrefab, this.transform).GetComponent<SquadEntityDebugPosition>();
        debugSquadCanvases.Add(trailInstance.GetComponent<SquadEntityDebugPosition>());
        return trailInstance;
    }
}
}