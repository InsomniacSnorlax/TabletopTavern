using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Shapes;
using System.Collections.Generic;
using TJ.Shapes;

public class BattlefieldBonusGameObject : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private BattlefieldBonus battlefieldBonus;
    public BattlefieldBonus BattlefieldBonus => battlefieldBonus;
    [SerializeField] private Disc disc1, disc2;
    [SerializeField] private Color playerColor, enemyColor, neutralColor;
    [SerializeField] private Color playerInnerColor, enemyInnerColor, neutralInnerColor;
    [SerializeField] private Transform colliderTransform;
    Entity entity;

    private void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<EntitiesReferences>());
        EntitiesReferences entitiesReferences = query.GetSingleton<EntitiesReferences>();
        entity  = entityManager.Instantiate(entitiesReferences.battlefieldBonusPrefabEntity);
        BattlefieldBonusApplicator bonusApplicator = entityManager.GetComponentData<BattlefieldBonusApplicator>(entity);

        battlefieldBonus.OriginationPoint = transform.position;
        battlefieldBonus.Range = UnityEngine.Random.Range(0.9f, 1.2f) * range;
        battlefieldBonus.Guid = System.Guid.NewGuid(); // Ensure each bonus has a unique Guid

        bonusApplicator.BattlefieldBonus = battlefieldBonus;
        
        entityManager.SetComponentData(entity, bonusApplicator);
        SetDisplayOfBonus(battlefieldBonus);
        query.Dispose();
    }
    private void SetDisplayOfBonus(BattlefieldBonus _battlefieldBonus)
    {
        Color color = _battlefieldBonus.Team switch
        {
            Team.Player => playerColor,
            Team.Enemy => enemyColor,
            Team.Neutral => neutralColor,
            _ => Color.white
        };
        disc1.GetComponent<ShapesBloom>().Bloom(color);
        color = _battlefieldBonus.Team switch
        {
            Team.Player => playerInnerColor,
            Team.Enemy => enemyInnerColor,
            Team.Neutral => neutralInnerColor,
            _ => Color.white
        };
        disc2.ColorOuter = color;
        disc1.Radius = battlefieldBonus.Range;
        disc2.Radius = battlefieldBonus.Range;
        colliderTransform.localScale = new Vector3(battlefieldBonus.Range, battlefieldBonus.Range, 1f);

        if(_battlefieldBonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Rain || _battlefieldBonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Fog || _battlefieldBonus.BattlefieldBonusEnum == BattlefieldBonusEnum.Snow)
        {
            UnityEngine.MeshCollider meshCollider = colliderTransform.GetComponent<UnityEngine.MeshCollider>();
            meshCollider.enabled = false;
        }
    }
    public void OnDestroy()
    {
        if (entity == Entity.Null) return;
        World world = World.DefaultGameObjectInjectionWorld;
        if (world == null || !world.IsCreated) return;
        if (!world.EntityManager.Exists(entity)) return;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.DestroyEntity(entity);
    }
}