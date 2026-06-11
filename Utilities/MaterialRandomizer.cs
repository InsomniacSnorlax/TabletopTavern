// using System.Collections.Generic;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Entities.Hybrid.Baking;
// using Unity.Mathematics;
// using Unity.Rendering;
// using UnityEngine;


// public struct OverrideBakingCleaner : IComponentData{}

// [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
// [BurstCompile]
// public partial struct BakingSystem : ISystem
// {
//     public void OnUpdate(ref SystemState state)
//     {
//         var ecb = new EntityCommandBuffer(Allocator.TempJob);
//         foreach (var (bakeData,additionalEntities, entity) in SystemAPI.Query<ColorOverride, DynamicBuffer<AdditionalEntitiesBakingData>>().WithNone<OverrideBakingCleaner>().WithEntityAccess())
//         {
//             foreach (var rendererEntity in additionalEntities.AsNativeArray())
//             {
//                 if (state.EntityManager.HasComponent<RenderMeshUnmanaged>(rendererEntity.Value))
//                 {
//                     ecb.AddComponent(rendererEntity.Value, new URPMaterialPropertyBaseColor() { Value = bakeData.Value });
//                 }
//             }
//             ecb.AddComponent(entity, new OverrideBakingCleaner());
//         }
      
      
//         var cleanUpQuery= SystemAPI.QueryBuilder()
//                                                  .WithAll<OverrideBakingCleaner>()
//                                                  .WithNone<ColorOverride>()
//                                                  .Build();
//         ecb.RemoveComponent<OverrideBakingCleaner>(cleanUpQuery, EntityQueryCaptureMode.AtPlayback);
//         ecb.RemoveComponent<URPMaterialPropertyBaseColor>(cleanUpQuery, EntityQueryCaptureMode.AtPlayback);
//         ecb.Playback(state.EntityManager);
//         ecb.Dispose();
//     }
// }
// public class BakerScript : MonoBehaviour
// {
//     public Color color = Color.white;
//     private class HighlightedAuthoringBaker : Baker<BakerScript>
//     {
//         public override void Bake(BakerScript authoring)
//         {
//             var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
//             float4 colour = (Vector4) authoring.color;
//             AddComponent(entity, new ColorOverride()
//             {
//                 Value = colour
//             });
//         }
//     }
// }

// [TemporaryBakingType]
// public struct ColorOverride : IComponentData
// {
//     public float4 Value;
// }
