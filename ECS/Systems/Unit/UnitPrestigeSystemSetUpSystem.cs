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
            unit,
            entity
        ) in SystemAPI.Query<
            RefRW<MeleeAttack>,
            RefRW<MeleeDefense>,
            RefRO<UnitPrestigeSetUpTag>,
            RefRO<Unit>
        >().WithEntityAccess()) {

            bool isPureRanged = !TabletopTavernConstants.UsesMeleePrestige(unit.ValueRO.unitName)
                             && entityManager.HasComponent<RangedFireModeUnitComponent>(entity);
            if(isPureRanged) {
                ShootAttack ShootAttack = entityManager.GetComponentData<ShootAttack>(entity);
                ShootAttack.Range += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
                ShootAttack.Accuracy += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;

                entityManager.SetComponentData(entity, ShootAttack);
            } else {
                MeleeAttack.ValueRW.MeleeAttackValue += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
                MeleeDefense.ValueRW.Value += TabletopTavernConstants.PRESTIGE_BONUS * UnitPrestigeSetUpTag.ValueRO.PrestigeLevel;
            }

            // Granted trait tags (ArmorPiercingTag/AntiInfantryTag/AntiLargeTag) and gear bonuses that
            // key off them (e.g. Glaives) are now applied in UnitSetUpSystem, which merges GrantedTrait
            // into SquadAttributes before computing gear/attribute bonuses. Doing it here instead was too
            // late: this system only runs once MeleeAttack exists (i.e. after UnitSetUpSystem already
            // baked WeaponStrength), so gear bonuses gated on the granted trait were silently dropped.

            entityCommandBuffer.RemoveComponent<UnitPrestigeSetUpTag>(entity);
        }
    }
}