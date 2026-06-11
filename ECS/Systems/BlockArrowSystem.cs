using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;

namespace TJ
{
public partial struct BlockArrowSystem : ISystem
{
    private Unity.Mathematics.Random _random;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        _random = new Unity.Mathematics.Random(1);
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
        foreach (var (shield, blockedArrowTag, unitEntity) in SystemAPI
                        .Query<RefRO<Shield>, RefRO<BlockedArrowTag>>()
                        .WithEntityAccess()
                        .WithOptions(EntityQueryOptions.FilterWriteGroup))
        {
            // Debug.Log($"BlockArrowSystem: Blocking arrow");
            ecb.RemoveComponent<BlockedArrowTag>(unitEntity);
            Entity arrowImpactModel = state.EntityManager.Instantiate(entitiesReferences.arrowImpactPrefabEntity);
            
            ecb.AddComponent(arrowImpactModel, new Parent { Value = shield.ValueRO.shieldEntity });

            //add a random offset to the arrow impact model
            // float3 targetPosition = SystemAPI.GetComponent<LocalTransform>(shield.ValueRO.shieldEntity).Position;
            LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(arrowImpactModel);
            // localTransform.Scale = 1f;
            quaternion shieldRotation = localTransform.Rotation;
            // float3 targetPosition = SystemAPI.GetComponent<LocalTransform>(arrowImpactModel).Position;
            float3 randomOffset = new (_random.NextFloat(-0.25f, 0.25f), _random.NextFloat(-0.25f, 0.25f), _random.NextFloat(-0.1f, 0.1f));
            quaternion randomRotation = quaternion.Euler(_random.NextFloat(-math.PI, math.PI), _random.NextFloat(-math.PI, math.PI), _random.NextFloat(-math.PI, math.PI));
            //should be a small offset
            randomRotation = math.mul(shieldRotation, randomRotation);
            //make this much smaller
            randomRotation = math.slerp(quaternion.identity, randomRotation, 0.15f);
            ecb.SetComponent(arrowImpactModel, new LocalTransform { Position = localTransform.Position + randomOffset, Rotation = randomRotation, Scale = 1f });
            


            // ecb.SetComponent(arrowImpactModel, LocalTransform.FromPosition(localTransform.Position + randomOffset));
        }
        
        ecb.Playback(state.EntityManager);
    }
}
}
