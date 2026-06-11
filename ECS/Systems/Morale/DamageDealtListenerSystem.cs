using Unity.Entities;
using TJ.Morale;
using Unity.Burst;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct DamageDealtListenerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattlePhase>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<SquadDamageBufferElement>(out var damageBuffer))
            return;

        // Use a temp map to sum damage per SquadId
        var damagePerSquad = new NativeHashMap<int, int>(16, Allocator.Temp);

        foreach (var element in damageBuffer)
        {
            if (!damagePerSquad.TryGetValue(element.SquadId, out var current))
                current = 0;

            damagePerSquad[element.SquadId] = current + element.DamageAmount;
        }

        // Now apply totals to SquadDamageComponent entities
        foreach (var (squadDamage, entity) in 
            SystemAPI.Query<RefRW<SquadDamageComponent>>()
                     .WithAll<SquadDamageComponent>()
                     .WithEntityAccess())
        {
            if (damagePerSquad.TryGetValue(squadDamage.ValueRO.SquadId, out var totalDamage))
            {
                squadDamage.ValueRW.DamageDealt += totalDamage;
                // Optional: cap, reset, or trigger morale here
            }
        }

        // Clear buffer for next frame
        damageBuffer.Clear();

        damagePerSquad.Dispose();
    }
}