using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct UnitPrestigeSystemSetUpSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        EntityManager entityManager = state.EntityManager;
        EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var ( 
            MeleeAttack,
            MeleeDefense,
            UnitPrestigeSetUpTag,
            entity
        ) in SystemAPI.Query<
            RefRW<MeleeAttack>,
            RefRW<MeleeDefense>,
            RefRO<UnitPrestigeSetUpTag>
        >().WithEntityAccess()) {

            //check if ranged
            if(entityManager.HasComponent<RangedUnitTag>(entity)) {
                ShootAttack ShootAttack = entityManager.GetComponentData<ShootAttack>(entity);
                ShootAttack.Range += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
                ShootAttack.Accuracy += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;

                entityManager.SetComponentData(entity, ShootAttack);
            } else {
                MeleeAttack.ValueRW.MeleeAttackValue += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
                MeleeDefense.ValueRW.Value += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
            }
  
            entityCommandBuffer.RemoveComponent<UnitPrestigeSetUpTag>(entity);
        }
    }
}