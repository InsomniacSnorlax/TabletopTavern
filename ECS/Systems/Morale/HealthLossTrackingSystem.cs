using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using TJ.Morale;
using Unity.Burst;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct HealthLossTrackingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        double currentTime = SystemAPI.Time.ElapsedTime;
        // var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (SquadStateComponent, squadDamageComponent, previousHealthRef, previousDamageDealtRef, buffer, damageBuffer, percentRef) in
            SystemAPI.Query<RefRO<SquadStateComponent>, RefRW<SquadDamageComponent>, RefRW<PreviousHealth>, RefRW<PreviousDamageDealt>, DynamicBuffer<HealthLossEvent>, DynamicBuffer<DamageDealtEvent>, RefRW<HealthLossPercent>>())
        {
            var health = SquadStateComponent.ValueRO.CurrentHealthValue;
            var damageDealt = squadDamageComponent.ValueRO.DamageDealt;
            var previousHealth = previousHealthRef.ValueRO.Value;
            var previousDamageDealt = previousDamageDealtRef.ValueRO.Value;

            // Detect health change
            if (health != previousHealth)
            {
                int loss = previousHealth - health;
                if (loss > 0)
                {
                    buffer.Add(new HealthLossEvent { Time = currentTime, Loss = loss });
                }
                previousHealthRef.ValueRW.Value = health;
            }

            //Detect damage dealt change
            if (damageDealt != previousDamageDealt)
            {
                int loss = damageDealt - previousDamageDealt;
                if (loss > 0)                {
                    damageBuffer.Add(new DamageDealtEvent { Time = currentTime, Damage = loss });
                }
                previousDamageDealtRef.ValueRW.Value = damageDealt;
            }

            // Prune old events and calculate total loss over past 5 seconds
            int totalLoss = 0;
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                var evt = buffer[i];
                if (currentTime - evt.Time > 5.0)
                {
                    buffer.RemoveAt(i);
                }
                else
                {
                    totalLoss += evt.Loss;
                }
            }
            // Prune old damage dealt events and calculate total damage dealt over past 5 seconds
            int totalDamageDealt = 0;
            for (int i = damageBuffer.Length - 1; i >= 0; i--)
            {
                var evt = damageBuffer[i];
                if (currentTime - evt.Time > 5.0)
                {
                    damageBuffer.RemoveAt(i);
                }
                else
                {
                    totalDamageDealt += evt.Damage;
                }
            }

            // Calculate percentage
            float maxHealth = SquadStateComponent.ValueRO.MaxHealthValue;
            percentRef.ValueRW.Value = (totalLoss / maxHealth) * 100f;

            // Calculate discrepancy between damage dealt and health loss for winning/losing combat
            int discrepancy = totalDamageDealt - totalLoss;
            percentRef.ValueRW.RecentDamageDiscrepency = discrepancy;

            //if greater than 250, likely winning, if less than -250, likely losing, otherwise likely even
            if (discrepancy > 250)
            {
                percentRef.ValueRW.CombatStatus = CombatStatus.Winning;
            }
            else if (discrepancy < -250)
            {
                percentRef.ValueRW.CombatStatus = CombatStatus.Losing;
            }
            else
            {
                percentRef.ValueRW.CombatStatus = CombatStatus.None;
            }
        }
    }
}