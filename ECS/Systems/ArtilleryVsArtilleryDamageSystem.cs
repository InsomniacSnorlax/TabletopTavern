using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TJ.ApplyDamageSystem))]
partial struct ArtilleryVsArtilleryDamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BattleHasStarted>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var damageBuffer in SystemAPI.Query<DynamicBuffer<DamageBufferElement>>()
            .WithAll<ArtilleryUnit>())
        {
            for (int i = 0; i < damageBuffer.Length; i++)
            {
                if (!damageBuffer[i].SourceIsArtillery) continue;

                ref DamageBufferElement element = ref damageBuffer.ElementAt(i);
                
                // int originalDamage = element.AttackStrength;
                element.AttackStrength = (int)(element.AttackStrength * TabletopTavernConstants.ARTILLERY_VS_ARTILLERY_DAMAGE_MODIFIER);

                // Debug.Log($"ArtilleryVsArtilleryDamageSystem: took reduced artillery damage from squad {element.DamageSourceSquadId}: {originalDamage} -> {element.AttackStrength} (x{TabletopTavernConstants.ARTILLERY_VS_ARTILLERY_DAMAGE_MODIFIER})");
            }
        }
    }
}
