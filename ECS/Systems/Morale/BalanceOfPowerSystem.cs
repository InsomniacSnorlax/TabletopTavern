using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

namespace TJ.Morale
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BalanceOfPowerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalanceOfPower>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // if BattlePhase exists, return
            if(SystemAPI.HasSingleton<BattlePhase>())
                return;

            if (!SystemAPI.TryGetSingletonRW<BalanceOfPower>(out var balanceOfPowerRW))
                return;

            ref var balanceOfPower = ref balanceOfPowerRW.ValueRW;

            var maxHealthOfPlayer = new NativeHashMap<int, int>(16, Allocator.Temp);
            var maxHealthOfEnemy = new NativeHashMap<int, int>(16, Allocator.Temp);

            foreach (var (squad, squadMovementComponent) in
                SystemAPI.Query<RefRO<SquadEntity>, RefRO<SquadStateComponent>>())
            {
                if(squad.ValueRO.Team == Team.Player)
                {
                    maxHealthOfPlayer[squad.ValueRO.SquadId] = squadMovementComponent.ValueRO.MaxHealthValue;
                }
                else
                {
                    maxHealthOfEnemy[squad.ValueRO.SquadId] = squadMovementComponent.ValueRO.MaxHealthValue;
                }
            }
            
            int totalPlayerHealth = 0;
            int totalEnemyHealth = 0;

            foreach(var kvp in maxHealthOfPlayer)
                totalPlayerHealth += kvp.Value;

            foreach(var kvp in maxHealthOfEnemy)
                totalEnemyHealth += kvp.Value;
            
            balanceOfPower.PlayerMaxHealth = totalPlayerHealth;
            balanceOfPower.EnemyMaxHealth = totalEnemyHealth;

            maxHealthOfPlayer.Dispose();
            maxHealthOfEnemy.Dispose();
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct BalanceOfPowerSetUpSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BalanceOfPower>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW<BalanceOfPower>(out var balanceOfPowerRW))
                return;

            ref var balanceOfPower = ref balanceOfPowerRW.ValueRW;

            var currentHealthOfPlayer = new NativeHashMap<int, int>(16, Allocator.Temp);
            var currentHealthOfEnemy = new NativeHashMap<int, int>(16, Allocator.Temp);

            foreach (var (squad, squadMovementComponent) in
                SystemAPI.Query<RefRO<SquadEntity>, RefRO<SquadStateComponent>>().WithNone<BrokenSquadTag>())
            {
                if(squad.ValueRO.Team == Team.Player)
                {
                    currentHealthOfPlayer[squad.ValueRO.SquadId] = squadMovementComponent.ValueRO.CurrentHealthValue;
                }
                else
                {
                    currentHealthOfEnemy[squad.ValueRO.SquadId] = squadMovementComponent.ValueRO.CurrentHealthValue;
                }
            }
            
            int totalPlayerHealth = 0;
            int totalEnemyHealth = 0;

            foreach(var kvp in currentHealthOfPlayer)
                totalPlayerHealth += kvp.Value;

            foreach(var kvp in currentHealthOfEnemy)
                totalEnemyHealth += kvp.Value;

            balanceOfPower.PlayerCurrentHealth = totalPlayerHealth;
            balanceOfPower.EnemyCurrentHealth = totalEnemyHealth;

            currentHealthOfPlayer.Dispose();
            currentHealthOfEnemy.Dispose();
        }
    }
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct ArmyLossesTriggeredSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BattlePhase>();
            state.RequireForUpdate<BalanceOfPower>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityManager entityManager = state.EntityManager;
            EntityCommandBuffer entityCommandBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            if (SystemAPI.TryGetSingletonEntity<ArmyLossesTriggeredPlayer>(out var armyLossesTriggeredPlayer))
            {
                foreach (var (squad, entity) in
                SystemAPI.Query<RefRO<PlayerSquad>>().WithDisabled<ArmyLossesPenaltyTag>().WithEntityAccess())
                {
                    entityManager.SetComponentEnabled<ArmyLossesPenaltyTag>(entity, true);
                }
                entityCommandBuffer.DestroyEntity(armyLossesTriggeredPlayer);
            }

            if (SystemAPI.TryGetSingletonEntity<ArmyLossesTriggeredEnemy>(out var armyLossesTriggeredEnemy))
            {
                foreach (var (squad, entity) in
                SystemAPI.Query<RefRO<EnemySquad>>().WithDisabled<ArmyLossesPenaltyTag>().WithEntityAccess())
                {
                    entityManager.SetComponentEnabled<ArmyLossesPenaltyTag>(entity, true);
                }
                entityCommandBuffer.DestroyEntity(armyLossesTriggeredEnemy);
            }
        }
    }
}