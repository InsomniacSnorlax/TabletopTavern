using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MoveOverrideSystem : ISystem 
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;
    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRO<MoveOverride> moveOverride,
            EnabledRefRW<MoveOverride> moveOverrideEnabled,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRO<MoveOverride>,
                EnabledRefRW<MoveOverride>,
                RefRW<UnitMover>>()) {
                    
            if (math.distancesq(localTransform.ValueRO.Position, moveOverride.ValueRO.targetPosition) > REACHED_TARGET_POSITION_DISTANCE_SQ) {
                // Move closer
                unitMover.ValueRW.targetPosition = moveOverride.ValueRO.targetPosition;
            } else {
                // Reached the move override position
                moveOverrideEnabled.ValueRW = false;
                UnityEngine.Debug.Log($"MoveOverrideSystem: Reached move override position at {moveOverride.ValueRO.targetPosition}");
            }
        }
    }
}