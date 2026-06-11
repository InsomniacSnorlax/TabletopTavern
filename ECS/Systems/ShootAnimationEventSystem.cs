using Unity.Burst;
using Unity.Entities;
using GPUECSAnimationBaker.Engine.AnimatorSystem;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
partial struct ShootAnimationEventSystem : ISystem {
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        foreach ((RefRO<ShootAttack> shootAttack, Entity entity) in SystemAPI.Query<RefRO<ShootAttack>>().WithEntityAccess()) {

            if (shootAttack.ValueRO.onShoot.isTriggered) {
                RefRO<AnimationDataHolder> gpuEcsAnimatorAspect = SystemAPI.GetComponentRO<AnimationDataHolder>(entity);
                RefRW<GpuEcsAnimatorControlComponent> controlComp = SystemAPI.GetComponentRW<GpuEcsAnimatorControlComponent>(gpuEcsAnimatorAspect.ValueRO.gpuEcsAnimatorEntity);
                controlComp.ValueRW.animatorInfo.animationID = gpuEcsAnimatorAspect.ValueRO.attackanimationId;

                RefRW<GpuEcsAnimatorControlStateComponent> controlStateComp = SystemAPI.GetComponentRW<GpuEcsAnimatorControlStateComponent>(gpuEcsAnimatorAspect.ValueRO.gpuEcsAnimatorEntity);
                controlStateComp.ValueRW.state = GpuEcsAnimatorControlStates.Start;
            }

        }
    }


}