using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TJ
{
    public partial struct SwordSetUpSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
                
            foreach (var (sword, SwordEntity) in SystemAPI
                .Query<SwordSetUpEntity>()
                .WithEntityAccess())
            {
                // Get the parent of the spawned entity
                if (SystemAPI.HasComponent<Parent>(SwordEntity))
                {
                    var parentComponent = SystemAPI.GetComponent<Parent>(SwordEntity);
                    Entity parentEntity = parentComponent.Value;

                    if (SystemAPI.HasComponent<Parent>(parentEntity))
                    {
                        var grandparentComponent = SystemAPI.GetComponent<Parent>(parentEntity);
                        Entity grandparentEntity = grandparentComponent.Value;
                        if (SystemAPI.HasComponent<Parent>(grandparentEntity))
                        {
                            var greatGrandparentComponent = SystemAPI.GetComponent<Parent>(grandparentEntity);
                            grandparentEntity = greatGrandparentComponent.Value;

                            if (SystemAPI.HasComponent<RangedMeleeConverter>(grandparentEntity))
                            {
                                RangedMeleeConverter RangedMeleeConverter = SystemAPI.GetComponent<RangedMeleeConverter>(grandparentEntity);

                                RangedMeleeConverter.SwordEntity = SwordEntity;
                                if (SystemAPI.HasComponent<LocalTransform>(SwordEntity))
                                {
                                    LocalTransform swordTransform = SystemAPI.GetComponent<LocalTransform>(SwordEntity);
                                    swordTransform.Scale = 0f;
                                    ecb.SetComponent(SwordEntity, swordTransform);
                                    ecb.SetComponent(grandparentEntity, RangedMeleeConverter);
                                    ecb.RemoveComponent<SwordSetUpEntity>(SwordEntity);
                                }
                            }
                        }
                    }
                }
            }
            ecb.Playback(state.EntityManager);
        }
    }
}